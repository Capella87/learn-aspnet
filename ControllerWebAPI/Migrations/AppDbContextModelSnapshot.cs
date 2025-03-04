// <auto-generated />
using System;
using ControllerWebAPI.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

#nullable disable

namespace ControllerWebAPI.Migrations
{
    [DbContext(typeof(AppDbContext))]
    partial class AppDbContextModelSnapshot : ModelSnapshot
    {
        protected override void BuildModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .UseCollation("Korean_100_CI_AS_KS_WS_SC_UTF8")
                .HasAnnotation("ProductVersion", "9.0.2")
                .HasAnnotation("Relational:MaxIdentifierLength", 128);

            SqlServerModelBuilderExtensions.UseIdentityColumns(modelBuilder);

            modelBuilder.Entity("ControllerWebAPI.Models.Company", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uniqueidentifier")
                        .HasDefaultValueSql("NEWID()");

                    b.Property<string>("Name")
                        .IsRequired()
                        .IsUnicode(true)
                        .HasColumnType("varchar(max)")
                        .UseCollation("Korean_100_CI_AS_KS_WS_SC_UTF8");

                    b.HasKey("Id");

                    b.ToTable("Companies");
                });

            modelBuilder.Entity("ControllerWebAPI.Models.Game", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uniqueidentifier")
                        .HasDefaultValueSql("NEWID()");

                    b.Property<string>("Description")
                        .IsUnicode(true)
                        .HasColumnType("varchar(max)")
                        .UseCollation("Korean_100_CI_AS_KS_WS_SC_UTF8");

                    b.Property<string>("Name")
                        .IsRequired()
                        .IsUnicode(true)
                        .HasColumnType("varchar(max)")
                        .UseCollation("Korean_100_CI_AS_KS_WS_SC_UTF8");

                    b.Property<DateOnly?>("ReleaseDate")
                        .HasColumnType("date");

                    b.Property<string>("UrlName")
                        .IsRequired()
                        .HasColumnType("nvarchar(450)");

                    b.HasKey("Id");

                    b.HasIndex("UrlName")
                        .IsUnique();

                    b.ToTable("Games");
                });

            modelBuilder.Entity("ControllerWebAPI.Models.Genre", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("Id"));

                    b.Property<string>("Name")
                        .IsRequired()
                        .IsUnicode(true)
                        .HasColumnType("varchar(max)")
                        .UseCollation("Korean_100_CI_AS_KS_WS_SC_UTF8");

                    b.HasKey("Id");

                    b.ToTable("Genres");
                });

            modelBuilder.Entity("GameDeveloper", b =>
                {
                    b.Property<Guid>("DeveloperId")
                        .HasColumnType("uniqueidentifier");

                    b.Property<Guid>("GameId")
                        .HasColumnType("uniqueidentifier");

                    b.HasKey("DeveloperId", "GameId");

                    b.HasIndex("GameId");

                    b.ToTable("GameDeveloper");
                });

            modelBuilder.Entity("GameGenre", b =>
                {
                    b.Property<Guid>("GameId")
                        .HasColumnType("uniqueidentifier");

                    b.Property<int>("GenreId")
                        .HasColumnType("int");

                    b.HasKey("GameId", "GenreId");

                    b.HasIndex("GenreId");

                    b.ToTable("GameGenre");
                });

            modelBuilder.Entity("GamePublisher", b =>
                {
                    b.Property<Guid>("GameId")
                        .HasColumnType("uniqueidentifier");

                    b.Property<Guid>("PublisherId")
                        .HasColumnType("uniqueidentifier");

                    b.HasKey("GameId", "PublisherId");

                    b.HasIndex("PublisherId");

                    b.ToTable("GamePublisher");
                });

            modelBuilder.Entity("GameDeveloper", b =>
                {
                    b.HasOne("ControllerWebAPI.Models.Company", null)
                        .WithMany("GameDevelopers")
                        .HasForeignKey("DeveloperId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("ControllerWebAPI.Models.Game", null)
                        .WithMany("GameDevelopers")
                        .HasForeignKey("GameId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();
                });

            modelBuilder.Entity("GameGenre", b =>
                {
                    b.HasOne("ControllerWebAPI.Models.Game", null)
                        .WithMany("GameGenres")
                        .HasForeignKey("GameId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("ControllerWebAPI.Models.Genre", null)
                        .WithMany("GameGenres")
                        .HasForeignKey("GenreId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();
                });

            modelBuilder.Entity("GamePublisher", b =>
                {
                    b.HasOne("ControllerWebAPI.Models.Game", null)
                        .WithMany("GamePublishers")
                        .HasForeignKey("GameId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("ControllerWebAPI.Models.Company", null)
                        .WithMany("GamePublishers")
                        .HasForeignKey("PublisherId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();
                });

            modelBuilder.Entity("ControllerWebAPI.Models.Company", b =>
                {
                    b.Navigation("GameDevelopers");

                    b.Navigation("GamePublishers");
                });

            modelBuilder.Entity("ControllerWebAPI.Models.Game", b =>
                {
                    b.Navigation("GameDevelopers");

                    b.Navigation("GameGenres");

                    b.Navigation("GamePublishers");
                });

            modelBuilder.Entity("ControllerWebAPI.Models.Genre", b =>
                {
                    b.Navigation("GameGenres");
                });
#pragma warning restore 612, 618
        }
    }
}
