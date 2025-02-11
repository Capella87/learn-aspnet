using System.ComponentModel.DataAnnotations;

namespace ControllerWebAPI.Models;

public class Genre
{
    [Key]
    public required int Id { get; set; }

    public required string Name { get; set; }

    public ICollection<Game> Games { get; } = [];
    public ICollection<GameGenre> GameGenres { get; } = [];
}

public class GameGenre
{
    public int GameId { get; set; }
    public Guid GenreId { get; set; }
}
