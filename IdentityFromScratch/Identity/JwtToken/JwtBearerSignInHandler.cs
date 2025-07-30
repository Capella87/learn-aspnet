using IdentityFromScratch.Identity.Token;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Microsoft.Net.Http.Headers;
using Npgsql.Internal;
using System.Security.Claims;
using System.Text.Encodings.Web;
using System.Text.Json;

namespace IdentityFromScratch.Identity.JwtToken;

// Source: https://github.com/dotnet/aspnetcore/blob/main/src/Security/Authentication/BearerToken/src/BearerTokenHandler.cs
// Source: https://github.com/dotnet/aspnetcore/blob/main/src/Security/Authentication/JwtBearer/src/JwtBearerHandler.cs


public class JwtBearerSignInHandler : JwtBearerHandler, IAuthenticationSignInHandler
{
    private ITokenService _tokenService => Context.RequestServices.GetRequiredService<ITokenService>();

    private readonly IOptionsMonitor<JsonSerializerOptions> _serializerOptions;

    public JwtBearerSignInHandler(IOptionsMonitor<JwtBearerOptions> options, ILoggerFactory logger, UrlEncoder encoder, IOptionsMonitor<JsonSerializerOptions> serializerOptions) : base(options, logger, encoder)
    {
        _serializerOptions = serializerOptions;
    }

    public virtual Task SignInAsync(ClaimsPrincipal user, AuthenticationProperties? properties)
    {
        // ForwardSignIn is used to forward the sign-in request to another authentication handler.
        var target = ResolveTarget(Options.ForwardSignIn);

        // If a target is not specified, we handle the sign-in ourselves with JWT.
        return (target != null)
            ? Context.SignInAsync(target, user, properties)
            : HandleSignInAsync(user, properties);
    }

    public virtual Task SignOutAsync(AuthenticationProperties? properties)
    {
        var target = ResolveTarget(Options.ForwardSignOut);

        return (target != null)
            ? Context.SignOutAsync(target, properties)
            : HandleSignOutAsync(properties ?? new AuthenticationProperties());
    }

    protected override Task HandleChallengeAsync(AuthenticationProperties properties)
    {
        Response.Headers.Append(HeaderNames.WWWAuthenticate, "Bearer");
        return base.HandleChallengeAsync(properties);
    }

    protected virtual async Task HandleSignInAsync(ClaimsPrincipal user, AuthenticationProperties? properties)
    {
        var utcNow = DateTime.UtcNow;

        // We have to create a new token manually.
        try
        {
            var token = _tokenService.GenerateAccessToken(user.Claims);
            var refreshToken = _tokenService.GenerateRefreshToken();

            var tokenResponse = JwtTokenResponse.CreateTokenResponse(token as JwtToken ?? throw new ArgumentNullException(nameof(token)),
    refreshToken as RefreshToken);
            await Context.Response.WriteAsJsonAsync(tokenResponse, _serializerOptions.Get("NoNullSerialization"));
        }
        catch (ArgumentNullException ex)
        {
            Logger.LogError(ex, "An error occurred while generating the JWT token.");
            Context.Response.StatusCode = StatusCodes.Status500InternalServerError;
            // Write an error response with ProblemDetails or a custom error message.

            var tokenProblemDetails = new ProblemDetails
            {
                Status = StatusCodes.Status500InternalServerError,
                Title = "Sign-In Error",
                Detail = "An error occurred while generating the JWT token.",
            };
            await Context.Response.WriteAsJsonAsync(tokenProblemDetails, _serializerOptions.Get("NoNullSerialization"));
        }
    }

    protected virtual Task HandleSignOutAsync(AuthenticationProperties? properties) => Task.CompletedTask;
}
