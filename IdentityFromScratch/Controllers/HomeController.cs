using Microsoft.AspNetCore.Mvc;

namespace IdentityFromScratch.Controllers;

[ApiController]
public class HomeController : ControllerBase
{
    private readonly ILogger<HomeController> _logger;

    public HomeController(ILogger<HomeController> logger)
    {
        _logger = logger;
    }

    [HttpGet("/")]
    [Produces("application/json")]
    [ProducesResponseType(typeof(string), StatusCodes.Status200OK)]
    public IActionResult Index()
    {
        return new OkObjectResult(new { Message = "Welcome!" });
    }
}
