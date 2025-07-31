using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace IdentityFromScratch.Controllers;

[ApiController]
[Route("/admin")]
public class AdminController : ControllerBase
{
    [HttpGet]
    [Produces("application/json")]
    [ProducesResponseType(typeof(string), StatusCodes.Status200OK)]
    [Authorize(Roles = "Admin")]
    public IActionResult Index()
    {
        return new OkObjectResult(new { Message = "Welcome, Admin!" });
    }
}
