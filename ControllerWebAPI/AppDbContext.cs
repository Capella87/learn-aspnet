using ControllerWebAPI.Models;
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

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        base.OnConfiguring(optionsBuilder);
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // You have to explicitly specify the collation of the database in SQL Server
        modelBuilder.UseCollation("Korean_100_CI_AS_KS_WS_SC_UTF8");

        // Apply entity configurations
        modelBuilder.ApplyConfiguration<Genre>(new GenreConfiguration())
                    .ApplyConfiguration<Company>(new CompanyConfiguration())
                    .ApplyConfiguration<Game>(new GameConfiguration());
    }
}
