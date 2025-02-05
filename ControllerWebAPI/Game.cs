using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
namespace ControllerWebAPI.Models;

[Index(nameof(UrlName), IsUnique = true)]
public class Game : IEntity
{
    [Key]
    public required Guid Id { get; set; }

    public required string UrlName { get; set; }

    public required string Name { get; set; }

    public DateOnly? ReleaseDate { get; set; }

    public IEnumerable<string>? Genres { get; set; }

    public string? Publisher { get; set; }

    public string? Developer { get; set; }

    public string? Description { get; set; }

    public Game(string UrlName, string Name)
    {
        this.Id = new Guid();
        this.UrlName = UrlName;
        this.Name = Name;
    }

    public Game()
    {
    }
}
