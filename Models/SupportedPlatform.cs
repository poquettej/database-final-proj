namespace db_final_proj.Models;

public class SupportedPlatform
{
    public int GameId { get; set; }
    public int PlatformId { get; set; }
    public string? DownloadPageUrl { get; set; }

    // Navigation properties
    public Game? Game { get; set; }
    public Platform? Platform { get; set; }
}
