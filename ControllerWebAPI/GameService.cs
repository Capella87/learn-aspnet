namespace ControllerWebAPI.Services;

using ControllerWebAPI;
using ControllerWebAPI.Models;

public class GameService
{
    private readonly List<Game> _games =
    [
        new Game { Id = "aoe2", Name = "Age of Empires 2: Definitive Edition" },
        new Game { Id = "aoe3", Name = "Age of Empires 3: Definitive Edition" },
        new Game { Id = "aom", Name = "Age of Mythology: Retold" },
        new Game { Id = "sdv", Name = "Stardew Valley" },
        new Game { Id = "ts3", Name = "The Sims 3" },
        new Game { Id = "ts2", Name = "The Sims 2" },
        new Game { Id = "ets", Name = "Euro Truck Simulator 2" },
        new Game { Id = "f4", Name = "Fallout 4" },
        new Game { Id = "f3", Name = "Fallout 3" },
        new Game { Id = "fnv", Name = "Fallout: New Vegas" },
        new Game { Id = "tropico4", Name = "Tropico 4" },
        new Game { Id = "halo", Name = "Halo: Master Chief Collection" },
    ];

    public IEnumerable<Game> Games => _games;

    public IEnumerable<Game> GetAllGames() => _games;

    public Game? GetGameById(string id) => _games.FirstOrDefault(g => g.Id == id);

    public void AddGame(Game newGame) => _games.Add(newGame);

    public void DeleteGame(string id) => _games.Remove(_games.First(g => g.Id == id));
}
