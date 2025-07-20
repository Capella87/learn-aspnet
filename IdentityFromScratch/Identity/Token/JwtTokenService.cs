using Microsoft.IdentityModel.JsonWebTokens;
using System.Security.Claims;

namespace IdentityFromScratch.Identity.Token;

public class JwtTokenService : ITokenService
{
    private readonly IConfiguration _config;

    public JwtTokenService(IConfiguration config)
    {
        _config = config;
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
