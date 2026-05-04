namespace db_final_proj.Models;

public class Company
{
    public int CompanyId { get; set; }
    public string CompanyName { get; set; } = string.Empty;

    // Navigation properties
    public ICollection<InvolvedIn> InvolvedGames { get; set; } = new List<InvolvedIn>();
}
