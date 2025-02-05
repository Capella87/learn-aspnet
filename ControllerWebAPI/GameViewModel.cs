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

    public string? Publisher { get; set; }

    public string? Developer { get; set; }

    public string? Description { get; set; }

    public GameViewModel(Game game)
    {
        UrlName = game.UrlName;
        Name = game.Name;
        ReleaseDate = game.ReleaseDate;
        Genres = game.Genres;
        Publisher = game.Publisher;
        Developer = game.Developer;
        Description = game.Description;
    }
}
