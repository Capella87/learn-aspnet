using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using IdentityFromScratch.Identity;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using IdentityFromScratch.Identity.Token;
using IdentityFromScratch.Identity.JwtToken;
using Microsoft.AspNetCore.Authorization;
using System.ComponentModel.DataAnnotations;
using Microsoft.IdentityModel.JsonWebTokens;

namespace IdentityFromScratch.Controllers;

[ApiController]
[Route("/auth")]
public class AuthController : ControllerBase
{
    private readonly SignInManager<User<int>> _signInManager;
    private readonly ILogger<AuthController> _logger;
    private readonly AppDbContext _dbContext;
    private readonly ITokenService<JsonWebToken> _tokenService;

    private readonly EmailAddressAttribute _emailAttr = new();
    private readonly PhoneAttribute _phoneAttr = new();

    public AuthController(SignInManager<User<int>> signInManager, ILogger<AuthController> logger,
        AppDbContext dbContext, ITokenService<JsonWebToken> tokenService)
    {
        _signInManager = signInManager ?? throw new ArgumentNullException(nameof(signInManager));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _dbContext = dbContext; // ?? throw new ArgumentNullException(nameof(dbContext));
        _tokenService = tokenService ?? throw new ArgumentNullException(nameof(tokenService));
    }

    // Login
    [HttpPost("login")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(JwtTokenResponse), StatusCodes.Status200OK)]
    public async Task<IActionResult> Login([FromBody] LoginRequestModel data)
    {
        if (HttpContext.User.Identity!.IsAuthenticated)
        {
            return Problem("You're already authenticated.", statusCode: StatusCodes.Status400BadRequest);
        }

        _signInManager.AuthenticationScheme = JwtBearerDefaults.AuthenticationScheme;

        // Convert Base64 password to plain password.

        var result = await _signInManager.PasswordSignInAsync(data.Username, data.Password, false, false);
        if (!result.Succeeded)
        {
            return Problem(result.ToString(), statusCode: StatusCodes.Status401Unauthorized);
        }

        // All authentication and authorization works are done in SignInManager..., but we have to create a JWT token manually...
        // In the referred code, they returns TypedResults.Empty.
        return new EmptyResult();
    }

    // Logout
    [HttpPost("logout")]
    [Authorize]
    public async Task<IActionResult> Logout()
    {
        if (!HttpContext.User.Identity!.IsAuthenticated)
        {
            return Problem("You're not authenticated.", statusCode: StatusCodes.Status400BadRequest);
        }
        // Sign out the user
        // TODO: If the user has refresh token not expired, we should remove the refresh token from the database.
        await _signInManager.SignOutAsync();
        // Return a success response
        return Ok(new { Message = "Logged out successfully." });
    }

    // Sign Up
    [HttpPost("signup")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(IdentityResult), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> SignUp([FromBody] SignUpRequestModel data, [FromServices] IServiceProvider sp)
    {
        var userManager = sp.GetRequiredService<UserManager<User<int>>>();
        var userStore = sp.GetRequiredService<IUserStore<User<int>>>();
        var emailStore = (IUserEmailStore<User<int>>)userStore;
        var email = data.EmailAddress;

        if (string.IsNullOrEmpty(email) || !_emailAttr.IsValid(email))
        {
            var validationProblemDetails = CreateValidationProblemDetails(IdentityResult.Failed(userManager.ErrorDescriber.InvalidEmail(email)));
            return BadRequest(validationProblemDetails); // Explicitly return BadRequest with ValidationProblemDetails
        }

        var newUser = new User<int>()
        {
            FirstName = data.FirstName,
            LastName = data.LastName,
        };

        if (data.PhoneNumber is not null && !_phoneAttr.IsValid(data.PhoneNumber))
        {
            if (!_phoneAttr.IsValid(data.PhoneNumber))
            {
                // TODO: Apply IdentityResult.Failed is used to create an IdentityResult with errors.
                return BadRequest(new ProblemDetails
                {
                    Title = "Invalid Phone Number",
                    Detail = "Invalid phone number format. Place a well-formed phone number.",
                });
            }
            else
            {
                newUser.PhoneNumber = data.PhoneNumber;
            }
        }

        await userStore.SetUserNameAsync(newUser, email, CancellationToken.None);
        await emailStore.SetEmailAsync(newUser, email, CancellationToken.None);
        var result = await userManager.CreateAsync(newUser, data.Password);

        if (!result.Succeeded)
        {
            var validationProblemDetails = CreateValidationProblemDetails(result);
            return BadRequest(validationProblemDetails); // Explicitly return BadRequest with ValidationProblemDetails
        }

        // TODO : Email confirmation logic can be added here.

        return CreatedAtAction(nameof(SignUp), result);
    }

    // Confirm Email

    // Update user information

    // Password Reset (For Logged user)
    [HttpPost("change-password")]
    [Authorize]
    [ProducesResponseType(typeof(IdentityResult), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordRequest request)
    {
        if (!ModelState.IsValid)
        {
            return Unauthorized(new ValidationProblemDetails(ModelState));
        }

        // Get the current user from JWT Bearer token
        var user = await _signInManager.UserManager.GetUserAsync(User);
        if (user is null)
        {
            return Unauthorized(new ProblemDetails
            {
                Title = "Unauthorized",
                Detail = "Invalid user.",
            });
        }

        // Validation
        // Check if the old password is correct
        var checkPasswordResult = await _signInManager.UserManager.CheckPasswordAsync(user, request.OldPassword);
        if (!checkPasswordResult)
        {
            return Unauthorized(new ProblemDetails
            {
                Title = "Unauthorized",
                Detail = "Invalid old password. Please check your password.",
            });
        }

        // Check the old password and the new password is the same
        var oldHash = _signInManager.UserManager.PasswordHasher.HashPassword(user, request.OldPassword);
        var newHash = _signInManager.UserManager.PasswordHasher.HashPassword(user, request.NewPassword);
        if (oldHash == newHash)
        {
            return Unauthorized(new ProblemDetails
            {
                Title = "Unauthorized",
                Detail = "The new password cannot be the same as the old password.",
            });
        }

        // Change the password; Check the existing password
        var result = await _signInManager.UserManager.ChangePasswordAsync(user, request.OldPassword,
            request.NewPassword);

        if (!result.Succeeded)
        {
            return Unauthorized(new ProblemDetails
            {
                Title = "Unauthorized",
                Detail = "Invalid password. Please check your password.",
            });
        }

        return Ok(new { Message = "Password changed successfully." });
    }

    private static ValidationProblemDetails CreateValidationProblemDetails(IdentityResult result)
    {
        var errorDict = new Dictionary<string, string[]>(1);

        foreach (var error in result.Errors)
        {
            string[] newStatements;

            if (errorDict.TryGetValue(error.Code, out var descs))
            {
                newStatements = new string[descs.Length + 1];
                Array.Copy(descs, newStatements, descs.Length);
                newStatements[descs.Length] = error.Description;
            }
            else
            {
                newStatements = [error.Description];
            }

            errorDict[error.Code] = newStatements;
        }

        return new ValidationProblemDetails(errorDict);
    }
}
