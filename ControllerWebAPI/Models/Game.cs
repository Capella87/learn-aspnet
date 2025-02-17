using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
namespace ControllerWebAPI.Models;

[Index(nameof(UrlName), IsUnique = true)]
public class Game : IEntity
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public Guid Id { get; set; }

    [Required]
    public string UrlName { get; set; }

    public required string Name { get; set; }

    public DateOnly? ReleaseDate { get; set; }

    public ICollection<Genre> Genres { get; } = [];
    public ICollection<GameGenre> GameGenres { get; } = [];

    public ICollection<Company> Publishers { get; } = [];
    public ICollection<GamePublisher> GamePublishers { get; } = [];

    public ICollection<Company> Developers { get; } = [];
    public ICollection<GameDeveloper> GameDevelopers { get; } = [];

    public string? Description { get; set; }

    public Game(string UrlName, string Name)
    {
        this.UrlName = UrlName;
        this.Name = Name;
    }

    public Game()
    {
    }
}
