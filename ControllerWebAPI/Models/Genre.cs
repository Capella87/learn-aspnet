using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ControllerWebAPI.Models;

public class Genre
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int Id { get; set; }

    public required string Name { get; set; }

    public ICollection<Game> Games { get; set; } = [];
    public ICollection<GameGenre> GameGenres { get; } = [];
}

public class GameGenre
{
    public Guid GameId { get; set; }
    public int GenreId { get; set; }
}
