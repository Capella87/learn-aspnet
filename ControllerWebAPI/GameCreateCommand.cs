using ControllerWebAPI.Models;

namespace ControllerWebAPI.Commands;

public interface ICreateCommand<T>
{
    public T Create();
}

public class GameCreateCommand
{
    public required string Name { get; set; }

    public required string UrlName { get; set; }

    public DateOnly? ReleaseDate { get; set; }

    public IEnumerable<string>? Genres { get; set; }

    public string? Publisher { get; set; }

    public string? Developer { get; set; }

    public string? Description { get; set; }

    public virtual Game Create()
    {
        return new Game(UrlName, Name)
        {
            ReleaseDate = ReleaseDate,
            Genres = Genres,
            Publisher = Publisher,
            Developer = Developer,
            Description = Description
        };
    }
}
