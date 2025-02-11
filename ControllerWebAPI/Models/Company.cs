using System.ComponentModel.DataAnnotations;

namespace ControllerWebAPI.Models;

public class Company : IEntity
{
    [Key]
    public required Guid Id { get; set; }

    public required string Name { get; set; }

    public ICollection<Game> DevelopedGames { get; } = [];
    public ICollection<GameDeveloper> GameDevelopers { get; } = [];

    public ICollection<Game> PublishedGames { get; } = [];
    public ICollection<GamePublisher> GamePublishers { get; } = [];
}

public class GameDeveloper
{
    public Guid GameId { get; set; }
    public Guid DeveloperId { get; set; }
}

public class GamePublisher
{
    public Guid GameId { get; set; }
    public Guid PublisherId { get; set; }
}
