using Microsoft.IdentityModel.JsonWebTokens;
using Microsoft.IdentityModel.Tokens;
using System.Security.Claims;

namespace IdentityFromScratch.Identity.Token;

public interface ITokenService<T> where T : SecurityToken
{
    protected ILogger<ITokenService<T>> Logger { get; }

    public Task<IToken<T>> GenerateAccessTokenAsync(IEnumerable<Claim> claims);

    public Task<IToken> GenerateRefreshTokenAsync();

    public Task<ClaimsPrincipal?> GetPrincipalFromExpiredTokenAsync(string token);
}
