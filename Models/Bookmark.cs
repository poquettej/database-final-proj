namespace db_final_proj.Models;

public class Bookmark
{
    public int UserId { get; set; }
    public int GameId { get; set; }
    public DateTime? DateBookmarked { get; set; }

    // Navigation properties
    public User? User { get; set; }
    public Game? Game { get; set; }
}
