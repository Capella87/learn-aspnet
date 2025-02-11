using Microsoft.IdentityModel.Tokens;

namespace ControllerWebAPI.Models;

public interface IUpdate<T> where T : IEntity
{
    public void Update(T target);
}

public class GameUpdateCommand : IUpdate<Game>
{
    public string? Name { get; set; }

    public string? NewUrlName { get; set; }

    public DateOnly? ReleaseDate { get; set; }

    public IEnumerable<string>? Genres { get; set; }

    public IEnumerable<string>? Publishers { get; set; }

    public IEnumerable<string>? Developers { get; set; }

    public string? Description { get; set; }

    public virtual void Update(Game game)
    {
        game.Name = Name ?? game.Name;
        game.UrlName = NewUrlName ?? game.UrlName;
        game.ReleaseDate = ReleaseDate ?? game.ReleaseDate;
        //game.Genres = Genres ?? game.Genres;
        //game.Publisher = Publisher ?? game.Publisher;
        //game.Developer = Developer ?? game.Developer;
        game.Description = Description ?? game.Description;
    }
}
