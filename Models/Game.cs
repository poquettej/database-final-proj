using System.ComponentModel.DataAnnotations.Schema;

namespace db_final_proj.Models;

public class Game
{
    public int GameId { get; set; }
    public string GameTitle { get; set; } = string.Empty;
    public string? GameSummary { get; set; }
    public string? CoverUrl { get; set; }
    public DateTime? ReleaseDate { get; set; }

    // Navigation properties
    public ICollection<Website> Websites { get; set; } = new List<Website>();
    public ICollection<Review> Reviews { get; set; } = new List<Review>();
    public ICollection<Bookmark> Bookmarks { get; set; } = new List<Bookmark>();
    public ICollection<SupportedPlatform> SupportedPlatforms { get; set; } = new List<SupportedPlatform>();
    public ICollection<InvolvedIn> InvolvedCompanies { get; set; } = new List<InvolvedIn>();

    // Helper properties for easier access (not mapped to database)
    [NotMapped]
    public IEnumerable<Company> Companies => InvolvedCompanies.Select(ic => ic.Company).OfType<Company>();
    
    [NotMapped]
    public IEnumerable<InvolvedIn> Developers => InvolvedCompanies.Where(ic => ic.IsDeveloper);
    
    [NotMapped]
    public IEnumerable<InvolvedIn> Publishers => InvolvedCompanies.Where(ic => ic.IsPublisher);
    
    [NotMapped]
    public IEnumerable<Platform> Platforms => SupportedPlatforms.Select(sp => sp.Platform).OfType<Platform>();
}
