using Microsoft.AspNetCore.Mvc;

using ControllerWebAPI.Models;
using ControllerWebAPI.Services;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using ControllerWebAPI.Commands;

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
        return await _gameService.GetAllGames();
    }


    // These are all actions
    [HttpGet("game/{urlName}")]
    public async Task<ActionResult<Game>> Get(string urlName)
    {
        // If found
        return await _gameService.GetGameByUrlName(urlName) switch
        {
            Game game => game,
            _ => NotFound()
        };
    }

    [HttpPost("/game")]
    public async Task<ActionResult<Game>> Add([FromBody] GameCreateCommand newEntity)
    {
        //if (_gameService.GetGameByUrlNam.Any(_games => _games.Id == newGame.Id))
        //{
        //    // Return bad request by Problem method with ProblemDetails type
        //    // If Problem method is invoked with ProblemDetails related parameters,
        //    // it will return a decent ProblemDetails object with supplied parameters and default values for other properties.
        //    return Problem(detail: $"Game with id {newGame.Id} already exists.", statusCode: StatusCodes.Status400BadRequest);
        //}

        var result = await _gameService.AddGame(newEntity.UrlName, newEntity);

        return result == null ?
            Problem(detail: $"Game with id {newEntity.UrlName} already exists.", statusCode: StatusCodes.Status400BadRequest)
            : CreatedAtAction(nameof(Get), new { urlName = result.UrlName }, result);
    }

    [HttpDelete("/game/{urlName}")]
    public async Task<ActionResult> Delete([FromRoute] string urlName)
    {
        var result = await _gameService.DeleteGame(urlName);
        if (result == null)
        {
            return Problem(detail: $"Game with id {urlName} does not exist.", statusCode: StatusCodes.Status404NotFound);
        }

        return await Task.FromResult(NoContent());
    }

    [HttpPut("/game/{urlName}")]
    public async Task<ActionResult> Update([FromRoute] string urlName, [FromBody] UpdateGameCommand cmd)
    {
        // Validation
    }

}
