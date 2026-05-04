using db_final_proj.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace db_final_proj.Data;

public class GameDbContext : DbContext
{
    public GameDbContext(DbContextOptions<GameDbContext> options) : base(options)
    {
    }

    public DbSet<Game> Games { get; set; }
    public DbSet<User> Users { get; set; }
    public DbSet<Company> Companies { get; set; }
    public DbSet<Platform> Platforms { get; set; }
    public DbSet<Website> Websites { get; set; }
    public DbSet<Review> Reviews { get; set; }
    public DbSet<Bookmark> Bookmarks { get; set; }
    public DbSet<SupportedPlatform> SupportedPlatforms { get; set; }
    public DbSet<InvolvedIn> InvolvedIn { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        base.OnConfiguring(optionsBuilder);
        optionsBuilder.EnableSensitiveDataLogging();
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Game configuration
        modelBuilder.Entity<Game>(entity =>
        {
            entity.HasKey(e => e.GameId);
            entity.ToTable("game");
            entity.Property(e => e.GameId).HasColumnName("game_id");
            entity.Property(e => e.GameTitle).HasColumnName("game_title").IsRequired().HasMaxLength(128);
            entity.Property(e => e.GameSummary).HasColumnName("game_summary");
            entity.Property(e => e.CoverUrl).HasColumnName("cover_url").HasMaxLength(256);
            entity.Property(e => e.ReleaseDate).HasColumnName("release_date");
        });

        // User configuration
        modelBuilder.Entity<User>(entity =>
        {
            entity.HasKey(e => e.UserId);
            entity.ToTable("user");

            entity.Property(e => e.UserId).HasColumnName("user_id");
            entity.Property(e => e.Username).HasColumnName("username").IsRequired().HasMaxLength(32);
            entity.Property(e => e.UserEmail).HasColumnName("user_email").IsRequired().HasMaxLength(128);
            entity.Property(e => e.PassHash).HasColumnName("pass_hash").IsRequired();
        });

        // Company configuration
        modelBuilder.Entity<Company>(entity =>
        {
            entity.HasKey(e => e.CompanyId);
            entity.ToTable("company");

            entity.Property(e => e.CompanyId).HasColumnName("company_id").ValueGeneratedNever();
            entity.Property(e => e.CompanyName).HasColumnName("company_name").HasMaxLength(128);
        });

        // Platform configuration
        modelBuilder.Entity<Platform>(entity =>
        {
            entity.HasKey(e => e.PlatformId);
            entity.ToTable("platform");

            entity.Property(e => e.PlatformId).HasColumnName("platform_id");
            entity.Property(e => e.PlatformName).HasColumnName("platform_name").IsRequired().HasMaxLength(128);
        });

        // Website configuration
        modelBuilder.Entity<Website>(entity =>
        {
            entity.HasKey(e => e.WebsiteId);
            entity.ToTable("website");

            entity.Property(e => e.WebsiteId).HasColumnName("website_id").ValueGeneratedNever();
            entity.Property(e => e.GameId).HasColumnName("game_id");
            entity.Property(e => e.WebsiteUrl).HasColumnName("website_url").IsRequired().HasMaxLength(256);

            entity.HasOne(e => e.Game)
                .WithMany(g => g.Websites)
                .HasForeignKey(e => e.GameId);
        });

        // Review configuration
        modelBuilder.Entity<Review>(entity =>
        {
            entity.HasKey(e => e.ReviewId);
            entity.ToTable("review");

            entity.Property(e => e.ReviewId).HasColumnName("review_id").ValueGeneratedOnAdd();
            entity.Property(e => e.UserId).HasColumnName("user_id");
            entity.Property(e => e.GameId).HasColumnName("game_id");
            entity.Property(e => e.Rating).HasColumnName("rating");
            entity.Property(e => e.ReviewComment).HasColumnName("review_comment");
            entity.Property(e => e.DateReviewed).HasColumnName("date_reviewed");

            entity.HasOne(e => e.User)
                .WithMany(u => u.Reviews)
                .HasForeignKey(e => e.UserId);

            entity.HasOne(e => e.Game)
                .WithMany(g => g.Reviews)
                .HasForeignKey(e => e.GameId);
        });

        // Bookmark configuration
        modelBuilder.Entity<Bookmark>(entity =>
        {
            entity.HasKey(e => new { e.UserId, e.GameId });
            entity.ToTable("bookmark");

            entity.Property(e => e.UserId).HasColumnName("user_id");
            entity.Property(e => e.GameId).HasColumnName("game_id");
            entity.Property(e => e.DateBookmarked).HasColumnName("date_bookmarked");

            entity.HasOne(e => e.User)
                .WithMany(u => u.Bookmarks)
                .HasForeignKey(e => e.UserId);

            entity.HasOne(e => e.Game)
                .WithMany(g => g.Bookmarks)
                .HasForeignKey(e => e.GameId);
        });

        // SupportedPlatform configuration
        modelBuilder.Entity<SupportedPlatform>(entity =>
        {
            entity.HasKey(e => new { e.GameId, e.PlatformId });
            entity.ToTable("supported_platforms");

            entity.Property(e => e.GameId).HasColumnName("game_id");
            entity.Property(e => e.PlatformId).HasColumnName("platform_id");
            entity.Property(e => e.DownloadPageUrl).HasColumnName("download_page_url");

            entity.HasOne(e => e.Game)
                .WithMany(g => g.SupportedPlatforms)
                .HasForeignKey(e => e.GameId);

            entity.HasOne(e => e.Platform)
                .WithMany(p => p.SupportedPlatforms)
                .HasForeignKey(e => e.PlatformId);
        });

        // InvolvedIn configuration
        modelBuilder.Entity<InvolvedIn>(entity =>
        {
            entity.HasKey(e => new { e.GameId, e.CompanyId });
            entity.ToTable("involved_in");

            entity.Property(e => e.GameId).HasColumnName("game_id");
            entity.Property(e => e.CompanyId).HasColumnName("company_id");
            entity.Property(e => e.IsDeveloper).HasColumnName("is_developer");
            entity.Property(e => e.IsPublisher).HasColumnName("is_publisher");

            entity.HasOne(e => e.Game)
                .WithMany(g => g.InvolvedCompanies)
                .HasForeignKey(e => e.GameId);

            entity.HasOne(e => e.Company)
                .WithMany(c => c.InvolvedGames)
                .HasForeignKey(e => e.CompanyId);
        });
    }
}
