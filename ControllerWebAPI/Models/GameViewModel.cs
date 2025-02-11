namespace ControllerWebAPI.Models;

public interface IViewModel<T> where T : class, IEntity
{
}

public class GameViewModel : IViewModel<Game>
{
    public string UrlName { get; set; }

    public string Name { get; set; }

    public DateOnly? ReleaseDate { get; set; }

    public IEnumerable<string>? Genres { get; set; }

    public IEnumerable<string>? Publishers { get; set; }

    public IEnumerable<string>? Developers { get; set; }

    public string? Description { get; set; }

    public GameViewModel(Game game)
    {
        UrlName = game.UrlName;
        Name = game.Name;
        ReleaseDate = game.ReleaseDate;

        // TODO : Cache these...
        Genres = game?.Genres
            .OrderBy(item => item.Name)
            .Select(item => item.Name)
            .ToArray();
        Publishers = game?.Publishers
            .OrderBy(item => item.Name)
            .Select(item => item.Name)
            .ToArray();
        Developers = game?.Developers
            .OrderBy(item => item.Name)
            .Select(item => item.Name)
            .ToArray();

        Description = game?.Description;
    }
}
