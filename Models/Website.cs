namespace db_final_proj.Models;

public class Website
{
    public int WebsiteId { get; set; }
    public int GameId { get; set; }
    public string WebsiteUrl { get; set; } = string.Empty;

    // Navigation property
    public Game? Game { get; set; }
}
