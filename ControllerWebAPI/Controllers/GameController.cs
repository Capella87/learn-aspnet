using Microsoft.AspNetCore.Mvc;

using ControllerWebAPI.Models;
using ControllerWebAPI.Services;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.EntityFrameworkCore;

namespace ControllerWebAPI.Controllers;

[ApiController]
// Controller is a class that handles HTTP requests, and it is a collection of actions.
// ControllerBase is a base class for an MVC controller without view support. So, it is used to create Web APIs.
public class GameController : ControllerBase
{
    private readonly GameService _gameService;
    private readonly ILogger _logger;

    // Dependency Injection for logging as well
    public GameController(GameService gameService, ILogger<GameController> logger)
    {
        _gameService = gameService;
        _logger = logger;
    }

    [HttpGet("/games")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> Index()
    {
        var allGames = await _gameService.GetAllGames();
        var rt = new
        {
            Count = allGames.Count,
            Data = allGames
        };

        return Ok(rt);
    }

    // These are all actions
    [HttpGet("game/{urlName}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<GameViewModel>> Get(string urlName)
    {
        // If found
        return (await _gameService.GetGameByUrlName(urlName) switch
        {
            GameViewModel g => Ok(g),
            _ => NotFound()
        });
    }

    [HttpHead("game/{urlName}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<GameViewModel>> HeadGame(string urlName)
    {
        return (await _gameService.IsUrlNameExist(urlName) switch
        {
            true => Ok(),
            _ => NotFound()
        });
    }

    [HttpPost("/game")]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<Game>> Add([FromBody] GameCreateCommand newEntity)
    {
        //if (_gameService.GetGameByUrlNam.Any(_games => _games.Id == newGame.Id))
        //{
        //    // Return bad request by Problem method with ProblemDetails type
        //    // If Problem method is invoked with ProblemDetails related parameters,
        //    // it will return a decent ProblemDetails object with supplied parameters and default values for other properties.
        //    return Problem(detail: $"Game with id {newGame.Id} already exists.", statusCode: StatusCodes.Status400BadRequest);
        //}

        // TODO : You should check by its name also...

        if (await _gameService.IsUrlNameExist(newEntity.UrlName))
        {
            return Problem(detail: $"Game with id '{newEntity.UrlName}' already exists.", statusCode: StatusCodes.Status400BadRequest);
        }

        var result = await _gameService.AddGame(newEntity.UrlName, newEntity);

        return result == null ?
            Problem(detail: $"Failed to add a new Game with id '{newEntity.UrlName}'.", statusCode: StatusCodes.Status500InternalServerError)
            : CreatedAtAction(nameof(Get), new { urlName = result.UrlName }, result);
    }

    [HttpDelete("/game/{urlName}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult> Delete([FromRoute] string urlName)
    {
        if (!await _gameService.IsUrlNameExist(urlName))
        {
            return Problem(detail: $"Game with id '{urlName}' does not exist.", statusCode: StatusCodes.Status404NotFound);
        }

        try
        {
            await _gameService.DeleteGame(urlName);
        }
        catch (Exception ex)
        {
            // Log the exception
            _logger.LogError(ex, ex.Message);
            // Return 500 Internal Server Error with ProblemDetails format
            return Problem(detail: ex.Message, statusCode: StatusCodes.Status500InternalServerError);
        }

        return Ok();
    }

    [HttpPut("/game/{urlName}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult> Update([FromRoute] string urlName, [FromBody] GameUpdateCommand cmd)
    {
        // Validation
        if (!await _gameService.IsUrlNameExist(urlName))
        {
            return Problem(detail: $"Game with id '{urlName}' does not exist.", statusCode: StatusCodes.Status404NotFound);
        }

        try
        {
            var result = await _gameService.UpdateGame(urlName, cmd);

            // Returns 200 OK
            return Ok(result);
        }
        catch (DbUpdateConcurrencyException ex)
        {
            _logger.LogError(ex, ex.Message);
            return Problem(detail: ex.Message, statusCode: StatusCodes.Status409Conflict);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, ex.Message);
            return Problem(detail: ex.Message, statusCode: StatusCodes.Status500InternalServerError);
        }
    }
}
