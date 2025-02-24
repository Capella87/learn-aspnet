namespace ControllerWebAPI.Models;

public interface ICreate<T> where T : IEntity
{
    public T Create();
}

public class GameCreateCommand : ICreate<Game>
{
    public required string Name { get; set; }

    public required string UrlName { get; set; }

    public DateOnly? ReleaseDate { get; set; }

    public IEnumerable<string>? Genres { get; set; }

    public IEnumerable<string>? Publishers { get; set; }

    public IEnumerable<string>? Developers { get; set; }

    public string? Description { get; set; }

    public virtual Game Create()
    {
        return new Game()
        {
            UrlName = UrlName,
            Name = Name,
            ReleaseDate = ReleaseDate,
            // Find genres first, then try to create that name
            Description = Description
        };
    }
}
