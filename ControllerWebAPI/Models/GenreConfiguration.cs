using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ControllerWebAPI.Models;

public class GenreConfiguration : IEntityTypeConfiguration<Genre>
{
    public void Configure(EntityTypeBuilder<Genre> builder)
    {
        builder.Property(p => p.Name)
            .HasColumnType("varchar(max)")
            .IsRequired()
            .IsUnicode()
            .UseCollation("Korean_100_CI_AS_KS_WS_SC_UTF8");
    }
}
