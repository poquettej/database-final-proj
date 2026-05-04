namespace db_final_proj.Models;

public class User
{
    public int UserId { get; set; }
    public string Username { get; set; } = string.Empty;
    public string UserEmail { get; set; } = string.Empty;
    public string PassHash { get; set; } = string.Empty;

    // Navigation properties
    public ICollection<Review> Reviews { get; set; } = new List<Review>();
    public ICollection<Bookmark> Bookmarks { get; set; } = new List<Bookmark>();
}
