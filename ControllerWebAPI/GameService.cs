namespace ControllerWebAPI.Services;

using ControllerWebAPI;
using ControllerWebAPI.Data;
using ControllerWebAPI.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Collections;

public class GameService
{
    private readonly AppDbContext _context;
    private readonly ILogger _logger;

    public GameService(AppDbContext context, ILoggerFactory loggerFactory)
    {
        _context = context;
        _logger = loggerFactory.CreateLogger<GameService>();
    }

    public async Task<ICollection<GameViewModel>> GetAllGames()
    {
        // Return all games in the database with async
        // Returns id anyway...
        // TODO : Check Performance of this code.
        var result = await _context.Games
            .OrderBy(g => g.UrlName)
            .ToListAsync();

        return result.ConvertAll(g => new GameViewModel(g));
    }

    public async Task<bool> IsUrlNameExist(string urlName)
    {
        return await _context.Games.AnyAsync(g => g.UrlName == urlName);
    }

    public async Task<Game?> GetGameByUrlName(string urlName)
    {
        // TODO : threading issue
        return await _context.Games.FirstOrDefaultAsync(g => g.UrlName == urlName);
    }

    /// <summary>
    /// Add a new game to the database.
    /// </summary>
    /// <param name="urlName">Another primary key of Game entity. Used in URL.</param>
    /// <param name="cmd">Command class to generate Game entity.</param>
    /// <returns>Returns the generated entity or null if the system failed to add the entity.</returns>
    public async Task<GameViewModel?> AddGame(string urlName, GameCreateCommand cmd)
    {
        if (await _context.Games.AnyAsync(x => x.UrlName == urlName))
        {
            throw new Exception($"Id '{urlName}' already exists.");
        }

        var game = cmd.Create();
        var entity = _context.Games.Add(game)?.Entity;
        ArgumentNullException.ThrowIfNull(entity, $"Failed to add the game with id '{urlName}'.");
        await _context.SaveChangesAsync();

        return new GameViewModel(game);
    }

    public async Task DeleteGame(string urlName)
    {
        // Is it a good idea to throw an exception if the game is not found????
        // Answer : You should not use exceptions as regular flow of control.
        // https://stackoverflow.com/questions/76647772/when-should-i-throw-exception-vs-return-error-actionresult-in-asp-net-core-web
        var game = await _context.Games.FirstOrDefaultAsync(g => g.UrlName == urlName);
        if (game == null) throw new Exception($"Game with Id '{urlName}' not found.");

        var result = _context.Games.Remove(game)?.Entity;
        ArgumentNullException.ThrowIfNull(result, "Failed to delete the game.");
        await _context.SaveChangesAsync();
    }

    public async Task<GameViewModel?> UpdateGame(string urlName, GameUpdateCommand cmd)
    {
        var game = await _context.Games.FirstOrDefaultAsync(g => g.UrlName == urlName);
        if (game == null) throw new Exception($"Game with id '{urlName}' not found.");

        cmd.Update(game);
        var result = _context.Update(game)?.Entity;
        ArgumentNullException.ThrowIfNull(result, "Failed to update the game.");
        await _context.SaveChangesAsync();

        return new GameViewModel(game);
    }

    // public async AddNewGame()

    //private readonly List<Game> _games =
    //[
    //    new Game { Id = "aoe2", Name = "Age of Empires 2: Definitive Edition" },
    //    new Game { Id = "aoe3", Name = "Age of Empires 3: Definitive Edition" },
    //    new Game { Id = "aom", Name = "Age of Mythology: Retold" },
    //    new Game { Id = "sdv", Name = "Stardew Valley" },
    //    new Game { Id = "ts3", Name = "The Sims 3" },
    //    new Game { Id = "ts2", Name = "The Sims 2" },
    //    new Game { Id = "ets", Name = "Euro Truck Simulator 2" },
    //    new Game { Id = "f4", Name = "Fallout 4" },
    //    new Game { Id = "f3", Name = "Fallout 3" },
    //    new Game { Id = "fnv", Name = "Fallout: New Vegas" },
    //    new Game { Id = "tropico4", Name = "Tropico 4" },
    //    new Game { Id = "halo", Name = "Halo: Master Chief Collection" },
    //];
}
