using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using IdentityFromScratch.Identity;

namespace IdentityFromScratch.Controllers;

[ApiController]
[Route("/auth")]
public class UserController : ControllerBase
{
    private readonly IUserService _userService;
    private readonly ILogger<UserController> _logger;

    public UserController(IUserService userService, ILogger<UserController> logger)
    {
        _userService = userService ?? throw new ArgumentNullException(nameof(userService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    // Login
    [Route("login")]
    [HttpPost]
    public IActionResult Login([FromBody] LoginDto data)
    {
        return Unauthorized();
    }

    // Logout

    // Sign Up

    // Confirm Email

    // Update user information

    // Password Reset

    // Delete User Account
}
