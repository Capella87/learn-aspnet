using ControllerWebAPI.Models;
using ControllerWebAPI.Services;
using Microsoft.EntityFrameworkCore;

namespace ControllerWebAPI.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
    }

    public DbSet<Game> Games { get; set; }
    public DbSet<Company> Companies { get; set; }
    public DbSet<Genre> Genres { get; set; }

    // Many-to-Many Relationship join tables
    public DbSet<GameDeveloper> GameDevelopers { get; set; }
    public DbSet<GamePublisher> GamePublishers { get; set; }
    public DbSet<GameGenre> GameGenres { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // Create a new company entity when user specified the name on JSON, but no existing one.
        // Many-To-Many Relationship
        modelBuilder.Entity<Game>()
            .HasMany(g => g.Genres)
            .WithMany(g => g.Games)
            .UsingEntity<GameGenre>(
                "GameGenre",
                l => l.HasOne<Genre>()
                    .WithMany(e => e.GameGenres)
                    .HasForeignKey(e => e.GenreId)
                    .HasPrincipalKey(e => e.Id),
                r => r.HasOne<Game>()
                    .WithMany(e => e.GameGenres)
                    .HasForeignKey(e => e.GameId)
                    .HasPrincipalKey(e => e.Id));

        modelBuilder.Entity<Game>()
            .HasMany(g => g.Developers)
            .WithMany(g => g.DevelopedGames)
            .UsingEntity<GameDeveloper>(
                "GameDeveloper",
                l => l.HasOne<Company>()
                    .WithMany(e => e.GameDevelopers)
                    .HasForeignKey(e => e.DeveloperId)
                    .HasPrincipalKey(e => e.Id),
                r => r.HasOne<Game>()
                    .WithMany(e => e.GameDevelopers)
                    .HasForeignKey(e => e.GameId)
                    .HasPrincipalKey(e => e.Id));

        modelBuilder.Entity<Game>()
            .HasMany(g => g.Publishers)
            .WithMany(g => g.PublishedGames)
            .UsingEntity<GamePublisher>(
                "GamePublisher",
                l => l.HasOne<Company>()
                    .WithMany(e => e.GamePublishers)
                    .HasForeignKey(e => e.PublisherId)
                    .HasPrincipalKey(e => e.Id),
                r => r.HasOne<Game>()
                    .WithMany(e => e.GamePublishers)
                    .HasForeignKey(e => e.GameId)
                    .HasPrincipalKey(e => e.Id));
    }
}
