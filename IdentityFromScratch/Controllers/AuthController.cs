using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using IdentityFromScratch.Identity;
using Microsoft.AspNetCore.Identity;
using System.Threading.Tasks;

namespace IdentityFromScratch.Controllers;

[ApiController]
[Route("/[controller]")]
public class AuthController : ControllerBase
{
    private readonly SignInManager<IdentityUser> _signInManager;
    private readonly ILogger<AuthController> _logger;

    public AuthController(SignInManager<IdentityUser> signInManager, ILogger<AuthController> logger)
    {
        _signInManager = signInManager ?? throw new ArgumentNullException(nameof(signInManager));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    // Login
    [Route("/login")]
    [ProducesResponseType(typeof(IdentityResult), StatusCodes.Status200OK)]
    [HttpPost]
    public async Task<IActionResult> Login([FromBody] LoginRequestModel data)
    {
        _signInManager.AuthenticationScheme = JwtBearerDefaults.AuthenticationScheme;

        // Convert Base64 password to plain password.

        var result = await _signInManager.PasswordSignInAsync(data.Username, data.Password, false, false);

        if (!result.Succeeded)
        {
            return Problem(result.ToString(), statusCode: StatusCodes.Status401Unauthorized);
        }

        // All authentication and authorization works are done in SignInManager..., but we have to create a JWT token manually...
        return Ok();
    }

    // Logout

    // Sign Up

    // Confirm Email

    // Update user information

    // Password Reset

    // Delete User Account
}
