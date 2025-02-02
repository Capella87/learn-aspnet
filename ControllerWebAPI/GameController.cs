using Microsoft.AspNetCore.Mvc;

using ControllerWebAPI.Models;
using ControllerWebAPI.Services;
using Microsoft.AspNetCore.Mvc.Infrastructure;

namespace ControllerWebAPI.Controllers;

[ApiController]
// Controller is a class that handles HTTP requests, and it is a collection of actions.
// ControllerBase is a base class for an MVC controller without view support. So, it is used to create Web APIs.
public class GameController : ControllerBase
{
    private readonly GameService _gameService;

    public GameController(GameService gameService)
    {
        _gameService = gameService;
    }

    [HttpGet("/games")]
    public async Task<IEnumerable<Game>> Index()
    {
        return await Task.FromResult(_gameService.Games);
    }


    // These are all actions
    [HttpGet("game/{id}")]
    public async Task<ActionResult<Game>> Show(string id)
    {
        var game = _gameService.GetGameById(id);
        // If found
        return game != null ? await Task.FromResult(game) : NotFound();
    }

    [HttpPost("/game")]
    public async Task<ActionResult<Game>> Add([FromBody] Game newGame)
    {
        if (!ModelState.IsValid) return BadRequest(new ValidationProblemDetails(ModelState));

        if (_gameService.Games.Any(_games => _games.Id == newGame.Id))
        {
            // Return bad request by Problem method with ProblemDetails type
            // If Problem method is invoked with ProblemDetails related parameters,
            // it will return a decent ProblemDetails object with supplied parameters and default values for other properties.
            return Problem(detail: $"Game with id {newGame.Id} already exists.", statusCode: StatusCodes.Status400BadRequest);
        }

        _gameService.AddGame(newGame);
        return await Task.FromResult(CreatedAtAction(nameof(Show), new { id = newGame.Id }, newGame));
    }

    [HttpDelete("/game/{id}")]
    public async Task<ActionResult> Delete([FromRoute] string id)
    {
        var result = _gameService.GetGameById(id);
        if (result == null)
            return NotFound();
        _gameService.DeleteGame(id);

        return await Task.FromResult(NoContent());
    }
}
