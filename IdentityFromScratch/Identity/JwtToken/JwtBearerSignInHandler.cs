using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.Options;
using System.Security.Claims;
using System.Text.Encodings.Web;

using IdentityFromScratch.Identity.Token;
using Microsoft.IdentityModel.Tokens;

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
        var target = ResolveTarget(Options.ForwardSignIn);
        return (target != null)
            ? Context.SignInAsync(target, user, properties)
            : HandleSignInAsync(user, properties);
    }

    public virtual Task SignOutAsync(AuthenticationProperties? properties) => Task.CompletedTask;

    protected virtual async Task HandleSignInAsync(ClaimsPrincipal user, AuthenticationProperties? properties)
    {
        var utcNow = DateTime.UtcNow;

        // We have to create a new token manually.
        var accessToken = _tokenService.GenerateAccessToken(user.Claims);
        var refreshToken = _tokenService.GenerateRefreshToken();

        // Getting the access token from the context.
        var token = await Context.GetTokenAsync("access_token") ?? throw new InvalidOperationException("Access token is not available in the context.");
        var isTimeRetrieved = long.TryParse(Context.User.FindFirst("exp")?.Value, out var expiresAt);

        if (!isTimeRetrieved)
        {
            throw new InvalidOperationException("Expiration time could not be retrieved.");
        }

        await Task.CompletedTask;
        // await Context.Response.WriteAsJsonAsync(new Token.JwtToken(token, expiresAt));
    }

    protected virtual Task HandleSignOutAsync(ClaimsPrincipal user, AuthenticationProperties? properties) => Task.CompletedTask;
}
