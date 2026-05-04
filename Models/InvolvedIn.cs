namespace db_final_proj.Models;

public class InvolvedIn
{
    public int GameId { get; set; }
    public int CompanyId { get; set; }
    public bool IsDeveloper { get; set; }
    public bool IsPublisher { get; set; }

    // Navigation properties
    public Game? Game { get; set; }
    public Company? Company { get; set; }
}
