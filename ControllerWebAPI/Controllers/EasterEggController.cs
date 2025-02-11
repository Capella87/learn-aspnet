using Microsoft.AspNetCore.Mvc;

namespace ControllerWebAPI.Controllers;

public class EasterEggController : ControllerBase
{
    [HttpGet("/teapot")]
    public async Task<IActionResult> Index()
    {
        return await Task.FromResult(StatusCode(StatusCodes.Status418ImATeapot, "I'm a teapot"));
    }
}
