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

namespace IdentityFromScratch.Controllers;

[ApiController]
[Route("/[controller]")]
public class AuthController : ControllerBase
{
    private readonly SignInManager<User<int>> _signInManager;
    private readonly ILogger<AuthController> _logger;
    private readonly AppDbContext _dbContext;
    private readonly ITokenService _tokenService;

    private readonly EmailAddressAttribute _emailAttr = new();
    private readonly PhoneAttribute _phoneAttr = new();

    public AuthController(SignInManager<User<int>> signInManager, ILogger<AuthController> logger,
        AppDbContext dbContext, ITokenService tokenService)
    {
        _signInManager = signInManager ?? throw new ArgumentNullException(nameof(signInManager));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _dbContext = dbContext; // ?? throw new ArgumentNullException(nameof(dbContext));
        _tokenService = tokenService ?? throw new ArgumentNullException(nameof(tokenService));
    }

    // Login
    [Route("/login")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(JwtTokenResponse), StatusCodes.Status200OK)]
    [HttpPost]
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

    // Sign Up
    [HttpPost("/signup")]
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

    // Password Reset

    // Delete User Account

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
