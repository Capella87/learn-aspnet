using Microsoft.IdentityModel.JsonWebTokens;
using System.Security.Claims;

namespace IdentityFromScratch.Identity.Token;

public interface ITokenService
{
    public IToken<JsonWebToken> GenerateAccessToken(IEnumerable<Claim> claims);

    public IToken GenerateRefreshToken();

    public ClaimsPrincipal? GetPrincipalFromExpiredToken(string token);
}
