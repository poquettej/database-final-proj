using System.Text.Json;
using db_final_proj.Models;
using Microsoft.Extensions.Options;

namespace db_final_proj.Services;

public class IGDBSettings
{
    public string BaseUrl { get; set; } = string.Empty;
    public string ClientId { get; set; } = string.Empty;
    public string AccessToken { get; set; } = string.Empty;
}

public class IGDBService
{
    private readonly HttpClient _httpClient;
    private readonly IGDBSettings _settings;
    private readonly ILogger<IGDBService> _logger;

    public IGDBService(HttpClient httpClient, IOptions<IGDBSettings> settings, ILogger<IGDBService> logger)
    {
        _httpClient = httpClient;
        _settings = settings.Value;
        _logger = logger;
    }

    public async Task<List<Game>> SearchGamesAsync(string query, List<int>? uninclude = null)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(_settings.ClientId) || string.IsNullOrWhiteSpace(_settings.AccessToken))
            {
                _logger.LogWarning("IGDB credentials not configured. Returning empty results.");
                return new List<Game>();
            }

            string unincludeFilter = "";
            if (uninclude != null && uninclude.Count > 0)
            {
                unincludeFilter = $"& id != ({string.Join(",", uninclude)})";
            }

            var request = new HttpRequestMessage(HttpMethod.Post, $"{_settings.BaseUrl}/games")
            {
                Content = new StringContent($"fields id, name, cover.url, first_release_date, summary, platforms.id, platforms.name, involved_companies.publisher, involved_companies.developer, involved_companies.company.id, involved_companies.company.name, websites.id, websites.url; where name ~ *\"{query}\"* & rating_count > 9 {unincludeFilter}; limit 10;")
            };

            _logger.LogInformation($"IGDB API Request Body: {await request.Content.ReadAsStringAsync()}");

            request.Headers.Add("Client-ID", _settings.ClientId);
            request.Headers.Add("Authorization", $"Bearer {_settings.AccessToken}");

            var response = await _httpClient.SendAsync(request);
            response.EnsureSuccessStatusCode();

            var json = await response.Content.ReadAsStringAsync();
            var games = ParseIGDBResponse(json);
            return games;
        }
        catch (Exception ex)
        {
            _logger.LogError($"Error searching IGDB: {ex.Message}");
            return new List<Game>();
        }
    }

    private List<Game> ParseIGDBResponse(string json)
    {
        var games = new List<Game>();

        try
        {
            using (JsonDocument doc = JsonDocument.Parse(json))
            {
                var root = doc.RootElement;

                if (root.ValueKind == JsonValueKind.Array)
                {
                    foreach (var gameElement in root.EnumerateArray())
                    {
                        var game = ParseGameFromJson(gameElement);
                        if (game != null)
                        {
                            games.Add(game);
                        }
                    }
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogError($"Error parsing IGDB response: {ex.Message}");
        }

        return games;
    }

    private Game? ParseGameFromJson(JsonElement gameElement)
    {
        try
        {
            var game = new Game
            {
                GameId = gameElement.TryGetProperty("id", out var idProp) ? idProp.GetInt32() : 0,
                GameTitle = gameElement.TryGetProperty("name", out var nameProp) ? nameProp.GetString() ?? string.Empty : string.Empty,
                GameSummary = gameElement.TryGetProperty("summary", out var summaryProp) ? summaryProp.GetString() : null,
            };

            if (gameElement.TryGetProperty("first_release_date", out var dateProp))
            {
                if (long.TryParse(dateProp.GetRawText(), out var timestamp))
                {
                    game.ReleaseDate = UnixTimeStampToDateTime(timestamp);
                }
            }

            if (gameElement.TryGetProperty("cover", out var coverProp) && coverProp.TryGetProperty("url", out var urlProp))
            {
                var coverUrl = urlProp.GetString();
                if (!string.IsNullOrEmpty(coverUrl))
                {
                    game.CoverUrl = "https:" + coverUrl;
                }
            }

            // Parse platforms
            if (gameElement.TryGetProperty("platforms", out var platformsProp) && platformsProp.ValueKind == JsonValueKind.Array)
            {
                foreach (var platformElement in platformsProp.EnumerateArray())
                {
                    if (platformElement.ValueKind == JsonValueKind.Object)
                    {
                        var platformId = platformElement.TryGetProperty("id", out var platformIdProp) ? platformIdProp.GetInt32() : 0;
                        var platformName = platformElement.TryGetProperty("name", out var platformNameProp) ? platformNameProp.GetString() : null;

                        if (platformId > 0 && !string.IsNullOrEmpty(platformName))
                        {
                            var supportedPlatform = new SupportedPlatform
                            {
                                GameId = game.GameId,
                                PlatformId = platformId,
                                Platform = new Platform { PlatformId = platformId, PlatformName = platformName }
                            };
                            game.SupportedPlatforms.Add(supportedPlatform);
                        }
                    }
                }
            }

            // Parse companies
            if (gameElement.TryGetProperty("involved_companies", out var companiesProp) && companiesProp.ValueKind == JsonValueKind.Array)
            {
                foreach (var companyElement in companiesProp.EnumerateArray())
                {
                    var isPublisher = companyElement.TryGetProperty("publisher", out var publisherProp) && publisherProp.GetBoolean();
                    var isDeveloper = companyElement.TryGetProperty("developer", out var developerProp) && developerProp.GetBoolean();

                    if (companyElement.TryGetProperty("company", out var companyProp))
                    {
                        var companyId = companyProp.TryGetProperty("id", out var companyIdProp) ? companyIdProp.GetInt32() : 0;
                        var companyName = companyProp.TryGetProperty("name", out var companyNameProp) ? companyNameProp.GetString() : null;

                        if (companyId > 0 && !string.IsNullOrEmpty(companyName))
                        {
                            // Check if this company is already involved in this game (e.g., as both developer and publisher)
                            var existingInvolvement = game.InvolvedCompanies.FirstOrDefault(ic => ic.CompanyId == companyId);
                            
                            if (existingInvolvement != null)
                            {
                                // Company already added, merge the roles
                                existingInvolvement.IsDeveloper = existingInvolvement.IsDeveloper || isDeveloper;
                                existingInvolvement.IsPublisher = existingInvolvement.IsPublisher || isPublisher;
                            }
                            else
                            {
                                // New company involvement
                                var involvedIn = new InvolvedIn
                                {
                                    GameId = game.GameId,
                                    CompanyId = companyId,
                                    IsPublisher = isPublisher,
                                    IsDeveloper = isDeveloper,
                                    Company = new Company 
                                    { 
                                        CompanyId = companyId, 
                                        CompanyName = companyName
                                    }
                                };
                                game.InvolvedCompanies.Add(involvedIn);
                            }
                        }
                    }
                }
            }

            // Parse websites
            if (gameElement.TryGetProperty("websites", out var websitesProp) && websitesProp.ValueKind == JsonValueKind.Array)
            {
                var websiteIndex = 0;
                foreach (var websiteElement in websitesProp.EnumerateArray())
                {
                    var websiteId = websiteElement.TryGetProperty("id", out var websiteIdProp) ? websiteIdProp.GetInt32() : websiteIndex;
                    var websiteUrl = websiteElement.TryGetProperty("url", out var websiteUrlProp) ? websiteUrlProp.GetString() : null;

                    if (!string.IsNullOrEmpty(websiteUrl))
                    {
                        var website = new Website
                        {
                            WebsiteId = websiteId,
                            GameId = game.GameId,
                            WebsiteUrl = websiteUrl
                        };
                        game.Websites.Add(website);
                    }
                    websiteIndex++;
                }
            }

            return game;
        }
        catch (Exception ex)
        {
            _logger.LogError($"Error parsing individual game: {ex.Message}");
            return null;
        }
    }

    private DateTime UnixTimeStampToDateTime(long unixTimeStamp)
    {
        var dateTime = new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc);
        dateTime = dateTime.AddSeconds(unixTimeStamp).ToLocalTime();
        return dateTime;
    }
}
