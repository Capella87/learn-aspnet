using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.JsonWebTokens;
using Microsoft.IdentityModel.Tokens;
using System.Security.Claims;

namespace IdentityFromScratch.Identity.Token;

public class JwtTokenService : ITokenService
{
    private readonly IConfiguration _config;
    private readonly ILogger<JwtTokenService> _logger;

    // TODO: Options patterns for refresh token (access token is already handled by other settings...?)

    // For JWT token (access token) generation and validation
    private readonly JsonWebTokenHandler _jwtHandler;

    ILogger<ITokenService> ITokenService.Logger => _logger;

    public JwtTokenService(IConfiguration config, ILogger<JwtTokenService> logger, JsonWebTokenHandler jwtHandler)
    {
        _config = config;
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _jwtHandler = jwtHandler ?? throw new ArgumentNullException(nameof(jwtHandler));
    }

    IToken<JsonWebToken> ITokenService.GenerateAccessToken(IEnumerable<Claim> claims)
    {
        throw new NotImplementedException();
    }

    IToken ITokenService.GenerateRefreshToken()
    {
        throw new NotImplementedException();
    }

    public ClaimsPrincipal? GetPrincipalFromExpiredToken(string token)
    {
        throw new NotImplementedException();
    }

    public IToken IssueToken(string accessToken, string? refreshToken)
    {
        throw new NotImplementedException();
    }
}
