namespace ControllerWebAPI.Models;

public class Game
{
    public required string Id { get; set; }

    public required string Name { get; set; }

    public DateOnly? ReleaseDate { get; set; }

    public IEnumerable<string>? Genres { get; set; }

    public string? Publisher { get; set; }

    public string? Developer { get; set; }

    public string? Description { get; set; }

    public Game(string Id, string Name)
    {
        this.Id = Id;
        this.Name = Name;
    }

    public Game()
    {
    }
}
