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
        // Convert to a list with GameViewModel objects
        var result = await _context.Games
            .AsNoTracking()
            .OrderBy(g => g.UrlName)
            .Include(g => g.Genres)
            .Include(g => g.Developers)
            .Include(g => g.Publishers)
            .Select(g => new GameViewModel(g))
            .ToListAsync();

        return result;
    }

    public async Task<bool> IsUrlNameExist(string urlName)
    {
        return await _context.Games
            .AsNoTracking()
            .AnyAsync(g => g.UrlName == urlName);
    }

    public async Task<GameViewModel?> GetGameByUrlName(string urlName)
    {
        var rt = await _context.Games
            .AsNoTracking()
            .Include(g => g.Genres)
            .Include(g => g.Developers)
            .Include(g => g.Publishers)
            .FirstOrDefaultAsync(g => g.UrlName == urlName);

        return rt == null ? null : new GameViewModel(rt);
    }

    /// <summary>
    /// Add a new game to the database.
    /// </summary>
    /// <param name="urlName">Another primary key of Game entity. Used in URL.</param>
    /// <param name="cmd">Command class to generate Game entity.</param>
    /// <returns>Returns the generated entity or null if the system failed to add the entity.</returns>
    public async Task<GameViewModel?> AddGame(string urlName, GameCreateCommand cmd)
    {
        var game = cmd.Create();

        // Search genres by string that GameCreateCommand given.
        if (cmd.Genres != null)
        {
            var genres = await _context.Genres
                .Where(g => cmd.Genres.Contains(g.Name))
                .ToListAsync();

            // Add existing genres to game
            foreach (var genre in genres)
            {
                game.Genres.Add(genre);
            }

            var notFound = cmd.Genres
                .Except(genres.Select(g => g.Name))
                .ToList();

            foreach (var name in notFound)
            {
                var g = new Genre() { Name = name };
                _context.Genres.Add(g);
                game.Genres.Add(g);
            }
        }

        if (cmd.Developers != null)
        {
            var d = await _context.Companies
                    .Where(c => cmd.Developers.Contains(c.Name))
                    .ToListAsync();

            foreach (var comp in d)
            {
                game.Developers.Add(comp);
            }

            var notFound = cmd.Developers.Except(d.Select(c => c.Name))
                .ToList();

            foreach (var comp in notFound)
            {
                var c = new Company() { Name = comp };
                _context.Companies.Add(c);
                game.Developers.Add(c);
            }
        }

        if (cmd.Publishers != null)
        {
            var p = await _context.Companies
                    .Where(c => cmd.Publishers.Contains(c.Name))
                    .ToListAsync();

            foreach (var comp in p)
            {
                game.Developers.Add(comp);
            }

            var notFound = cmd.Publishers.Except(p.Select(c => c.Name))
                .ToList();

            foreach (var comp in notFound)
            {
                var c = new Company() { Name = comp };
                _context.Companies.Add(c);
                game.Publishers.Add(c);
            }
        }

        var entity = _context.Games.Add(entity: game)?.Entity;
        ArgumentNullException.ThrowIfNull(entity, $"Failed to add the game with id '{urlName}'.");
        await _context.SaveChangesAsync();

        return new GameViewModel(game);
    }

    public async Task DeleteGame(string urlName)
    {
        // Is it a good idea to throw an exception if the game is not found????
        // Answer : You should not use exceptions as regular flow of control.
        // https://stackoverflow.com/questions/76647772/when-should-i-throw-exception-vs-return-error-actionresult-in-asp-net-core-web
        var game = await _context.Games.FirstOrDefaultAsync(g => g.UrlName == urlName)
            ?? throw new Exception($"Game with Id '{urlName}' not found.");

        var result = _context.Games.Remove(game)?.Entity;
        ArgumentNullException.ThrowIfNull(result, "Failed to delete the game.");
        await _context.SaveChangesAsync();
    }

    public async Task<GameViewModel?> UpdateGame(string urlName, GameUpdateCommand cmd)
    {
        var game = await _context.Games
            .Include(g => g.Genres)
            .Include(g => g.GameDevelopers)
            .Include(g => g.GamePublishers)
            .FirstOrDefaultAsync(g => g.UrlName == urlName);

        if (game == null) throw new Exception($"Game with id '{urlName}' not found.");

        cmd.Update(game);

        if (cmd.Genres != null)
        {
            // Get genres except existing genres in the game
            // https://learn.microsoft.com/en-us/ef/core/querying/client-eval
            var genres = await _context.Genres
                .Where(g => cmd.Genres.Contains(g.Name))
                .ToListAsync();

            // Add existing genres to game
            foreach (var genre in genres.Except(game.Genres))
            {
                game.Genres.Add(genre);
            }

            var notFound = cmd.Genres
                .Except(genres.Select(g => g.Name))
                .ToList();

            foreach (var name in notFound)
            {
                var g = new Genre() { Name = name };
                _context.Genres.Add(g);
                game.Genres.Add(g);
            }
        }

        if (cmd.Developers != null)
        {
            var d = await _context.Companies
                    .Where(c => cmd.Developers.Contains(c.Name))
                    .ToListAsync();

            foreach (var comp in d.Except(game.Developers))
            {
                game.Developers.Add(comp);
            }

            var notFound = cmd.Developers
                .Except(d.Select(c => c.Name))
                .ToList();

            foreach (var comp in notFound)
            {
                var c = new Company() { Name = comp };
                _context.Companies.Add(c);
                game.Developers.Add(c);
            }
        }

        if (cmd.Publishers != null)
        {
            var p = await _context.Companies
                    .Where(c => cmd.Publishers.Contains(c.Name))
                    .ToListAsync();

            foreach (var comp in p.Except(game.Publishers))
            {
                game.Publishers.Add(comp);
            }

            var notFound = cmd.Publishers
                .Except(p.Select(c => c.Name))
                .ToList();

            foreach (var comp in notFound)
            {
                var c = new Company() { Name = comp };
                _context.Companies.Add(c);
                game.Publishers.Add(c);
            }
        }

        var result = _context.Update(game)?.Entity;
        ArgumentNullException.ThrowIfNull(result, $"Failed to update the game");
        await _context.SaveChangesAsync();

        return new GameViewModel(game);
    }
}
