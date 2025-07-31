using IdentityFromScratch.Identity.Token;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.JsonWebTokens;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace IdentityFromScratch.Identity.JwtToken;

public class JwtTokenService : ITokenService
{
    private readonly IConfiguration _config;
    private readonly ILogger<JwtTokenService> _logger;
    private readonly JwtSettings _settings;

    // TODO: Options patterns for refresh token (access token is already handled by other settings...?)

    // For JWT token (access token) generation and validation
    private readonly JsonWebTokenHandler _jwtHandler;

    ILogger<ITokenService> ITokenService.Logger => _logger;

    public JwtTokenService(IConfiguration config, ILogger<JwtTokenService> logger, JsonWebTokenHandler? handler, IOptions<JwtSettings> settings)
    {
        _config = config;
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _jwtHandler = handler ?? new JsonWebTokenHandler()
        {
            MapInboundClaims = JwtSecurityTokenHandler.DefaultMapInboundClaims,
        };
        _settings = settings.Value;
    }

    public IToken<JsonWebToken> GenerateAccessToken(IEnumerable<Claim> claims)
    {
        var isseudAt = DateTime.UtcNow;

        var descriptor = new SecurityTokenDescriptor
        {
            Expires = isseudAt.AddMinutes(_settings.AccessToken.ExpiresInMinutes ?? 15),
            IssuedAt = isseudAt,
            Issuer = _settings.AccessToken.Issuers!.First(),
            Subject = new ClaimsIdentity(claims),

            // Source: https://stackoverflow.com/questions/71449622/add-multiple-audiences-in-token-descriptor
            Claims = new Dictionary<string, object>
            {
                { Microsoft.IdentityModel.JsonWebTokens.JwtRegisteredClaimNames.Aud, _settings.AccessToken.Audiences! }
            },
            SigningCredentials = new SigningCredentials(
                new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_settings.AccessToken.SecretKey)),
                SecurityAlgorithms.HmacSha256)
        };

        var strToken = _jwtHandler.CreateToken(descriptor);
        var jwt = new JsonWebToken(strToken);

        return new JwtToken(strToken, jwt);
    }

    IToken ITokenService.GenerateRefreshToken()
    {
        var currentTime = DateTime.UtcNow;
        var rt = new byte[32];
        var rng = RandomNumberGenerator.Create();
        rng.GetBytes(rt);

        return new RefreshToken(Convert.ToBase64String(rt), currentTime.AddDays(10), currentTime);
    }

    public ClaimsPrincipal? GetPrincipalFromExpiredToken(string token)
    {
        throw new NotImplementedException();
    }

    ClaimsPrincipal? ITokenService.GetPrincipalFromExpiredToken(string token)
    {
        throw new NotImplementedException();
    }
}
