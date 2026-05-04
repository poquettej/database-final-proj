using db_final_proj.Data;
using db_final_proj.Models;
using Microsoft.EntityFrameworkCore;
using System.Linq;

namespace db_final_proj.Services;

public class GameService
{
    private readonly GameDbContext _dbContext;
    private readonly IGDBService _igdbService;
    private readonly ILogger<GameService> _logger;

    public GameService(GameDbContext dbContext, IGDBService igdbService, ILogger<GameService> logger)
    {
        _dbContext = dbContext;
        _igdbService = igdbService;
        _logger = logger;
    }

    public async Task<List<Game>> SearchGamesAsync(string query, int offset = 0, int pageSize = 10, List<int>? uninclude = null)
    {
        try
        {
            _logger.LogInformation($"Excluding {uninclude?.Count} titles from search results.");
            var cachedGames = await _dbContext.Games
                .AsNoTracking()
                .Where(g => (EF.Functions.Like(g.GameTitle, $"%{query}%") || 
                           EF.Functions.Like(g.GameSummary, $"%{query}%")) && 
                           (uninclude == null || !uninclude.Contains(g.GameId)))
                .ToListAsync();

            _logger.LogInformation($"Found {cachedGames.Count} games in cache for query: {query}");
            if (uninclude == null)
            {
                uninclude = new List<int>();
            }
            uninclude.AddRange(cachedGames.Select(g => g.GameId));
            // If we need more results than what's cached, fetch from IGDB
            if (cachedGames.Count < pageSize)
            {
                _logger.LogInformation($"Insufficient cached results. Fetching from IGDB with offset {offset}...");
                var igdbGames = await _igdbService.SearchGamesAsync(query, uninclude);
                _logger.LogInformation($"Fetched {igdbGames.Count} games from IGDB for query: {query}");

                if (igdbGames.Count > 0)
                {
                    // Cache the new games
                    foreach (var game in igdbGames)
                    {
                        var existingGame = await _dbContext.Games
                            .AsNoTracking()
                            .FirstOrDefaultAsync(g => g.GameId == game.GameId);
                        
                        if (existingGame == null)
                        {
                            _logger.LogInformation($"Caching new game: {game.GameTitle} (ID: {game.GameId})");
                            
                            foreach (var supportedPlatform in game.SupportedPlatforms)
                            {
                                var dbPlatform = await _dbContext.Platforms.FindAsync(supportedPlatform.PlatformId);
                                if (dbPlatform != null)
                                {
                                    supportedPlatform.Platform = dbPlatform;
                                }
                            }

                            foreach (var involvedIn in game.InvolvedCompanies)
                            {
                                var dbCompany = await _dbContext.Companies.FindAsync(involvedIn.CompanyId);
                                if (dbCompany != null)
                                {
                                    involvedIn.Company = dbCompany;
                                }
                            }

                            _dbContext.Games.Add(game);
                            await _dbContext.SaveChangesAsync();
                            _dbContext.ChangeTracker.Clear();
                            
                            cachedGames.Add(game);
                        } else {
                            _logger.LogInformation($"Game already exists in cache: {game.GameTitle} (ID: {game.GameId})");
                        }
                    }

                    _logger.LogInformation($"Cached {igdbGames.Count} games from IGDB");
                }
            }

            // Return paginated slice
            var result = cachedGames.Take(pageSize).ToList();
            
            _logger.LogInformation($"Returning {result.Count} games for offset {offset}");
            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError($"Error searching games: {ex.Message}");
            return new List<Game>();
        }
    }


    public async Task<Game?> GetGameByIdAsync(int id)
    {
        return await _dbContext.Games
            .Include(g => g.Websites)
            .Include(g => g.InvolvedCompanies)
                .ThenInclude(ic => ic.Company)
            .Include(g => g.SupportedPlatforms)
                .ThenInclude(sp => sp.Platform)
            .Include(g => g.Reviews)
                .ThenInclude(r => r.User)
            .FirstOrDefaultAsync(g => g.GameId == id);
    }

    public async Task<List<Game>> GetAllGamesAsync()
    {
        return await _dbContext.Games.ToListAsync();
    }

    public async Task<bool> IsGameBookmarkedAsync(int userId, int gameId)
    {
        return await _dbContext.Bookmarks.AnyAsync(b => b.UserId == userId && b.GameId == gameId);
    }

    public async Task<List<Game>> GetBookmarkedGamesAsync(int userId)
    {
        try
        {
            return await _dbContext.Bookmarks
                .Where(b => b.UserId == userId)
                .Include(b => b.Game)
                    .ThenInclude(g => g.Reviews) // Needed for rating display on HomeGameCard
                .Select(b => b.Game!)
                .AsNoTracking()
                .ToListAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError($"Error getting bookmarked games for user {userId}: {ex.Message}");
            return new List<Game>();
        }
    }

    public async Task AddBookmarkAsync(int userId, int gameId)
    {
        var exists = await _dbContext.Bookmarks.AnyAsync(b => b.UserId == userId && b.GameId == gameId);
        if (!exists)
        {
            _dbContext.Bookmarks.Add(new Bookmark 
            { 
                UserId = userId, 
                GameId = gameId, 
                DateBookmarked = DateTime.UtcNow 
            });
            await _dbContext.SaveChangesAsync();
        }
    }

    public async Task<List<Game>> GetDatabaseGamesAsync(int offset = 0, int pageSize = 10)
    {
        try
        {
            return await _dbContext.Games
                .AsNoTracking()
                .Include(g => g.Reviews) // Needed for the rating display on cards
                .Skip(offset)
                .Take(pageSize)
                .ToListAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError($"Error getting database games: {ex.Message}");
            return new List<Game>();
        }
    }

    public async Task RemoveBookmarkAsync(int userId, int gameId)
    {
        var bookmark = await _dbContext.Bookmarks.FirstOrDefaultAsync(b => b.UserId == userId && b.GameId == gameId);
        if (bookmark != null)
        {
            _dbContext.Bookmarks.Remove(bookmark);
            await _dbContext.SaveChangesAsync();
        }
    }

    public async Task AddReviewAsync(int userId, int gameId, int rating, string? comment)
    {
        _dbContext.Reviews.Add(new Review
        {
            UserId = userId,
            GameId = gameId,
            Rating = rating,
            ReviewComment = comment,
            DateReviewed = DateTime.UtcNow
        });
        await _dbContext.SaveChangesAsync();
    }

    public async Task RemoveReviewAsync(int reviewId)
    {
        var review = await _dbContext.Reviews.FindAsync(reviewId);
        if (review != null)
        {
            _dbContext.Reviews.Remove(review);
            await _dbContext.SaveChangesAsync();
        }
    }
}
