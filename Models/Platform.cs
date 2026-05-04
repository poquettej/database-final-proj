namespace db_final_proj.Models;

public class Platform
{
    public int PlatformId { get; set; }
    public string PlatformName { get; set; } = string.Empty;

    // Navigation properties
    public ICollection<SupportedPlatform> SupportedPlatforms { get; set; } = new List<SupportedPlatform>();
}
