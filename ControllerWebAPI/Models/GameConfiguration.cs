using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ControllerWebAPI.Models;

public class GameConfiguration : IEntityTypeConfiguration<Game>
{
    public void Configure(EntityTypeBuilder<Game> builder)
    {
        builder.Property(p => p.Id)
            .HasDefaultValueSql("NEWID()");

        builder.Property(p => p.Name)
            .HasColumnType("varchar(max)")
            .IsRequired()
            .IsUnicode()
            .UseCollation("Korean_100_CI_AS_KS_WS_SC_UTF8");

        builder.Property(p => p.Description)
            .HasColumnType("varchar(max)")
            .IsUnicode()
            .UseCollation("Korean_100_CI_AS_KS_WS_SC_UTF8");

        builder.HasMany(g => g.Genres)
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

        builder.HasMany(g => g.Developers)
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

        builder.HasMany(g => g.Publishers)
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
