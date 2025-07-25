using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using IdentityFromScratch.Identity;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using IdentityFromScratch.Identity.Token;
using IdentityFromScratch.Identity.JwtToken;

namespace IdentityFromScratch.Controllers;

[ApiController]
[Route("/[controller]")]
public class AuthController : ControllerBase
{
    private readonly SignInManager<IdentityUser> _signInManager;
    private readonly ILogger<AuthController> _logger;
    private readonly AppDbContext _dbContext;
    private readonly ITokenService _tokenService;

    public AuthController(SignInManager<IdentityUser> signInManager, ILogger<AuthController> logger,
        AppDbContext dbContext, ITokenService tokenService)
    {
        _signInManager = signInManager ?? throw new ArgumentNullException(nameof(signInManager));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _dbContext = dbContext; // ?? throw new ArgumentNullException(nameof(dbContext));
        _tokenService = tokenService ?? throw new ArgumentNullException(nameof(tokenService));
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
