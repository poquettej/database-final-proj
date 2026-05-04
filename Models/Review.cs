namespace db_final_proj.Models;

public class Review
{
    public int ReviewId { get; set; }
    public int UserId { get; set; }
    public int GameId { get; set; }
    public int Rating { get; set; }
    public string? ReviewComment { get; set; }
    public DateTime? DateReviewed { get; set; }

    // Navigation properties
    public User? User { get; set; }
    public Game? Game { get; set; }
}
