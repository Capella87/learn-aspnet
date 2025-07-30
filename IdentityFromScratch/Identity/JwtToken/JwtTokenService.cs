using IdentityFromScratch.Identity.Token;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.JsonWebTokens;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Threading.Tasks;

namespace IdentityFromScratch.Identity.JwtToken;

public class JwtTokenService : ITokenService
{
    private readonly IConfiguration _config;
    private readonly JwtBearerOptions _jwtBearerOptions;
    private readonly ILogger<JwtTokenService> _logger;

    // TODO: Options patterns for refresh token (access token is already handled by other settings...?)

    // For JWT token (access token) generation and validation
    private readonly JsonWebTokenHandler _jwtHandler;

    ILogger<ITokenService> ITokenService.Logger => _logger;

    public JwtTokenService(IConfiguration config, ILogger<JwtTokenService> logger, IOptions<JwtBearerOptions> options)
    {
        _config = config;
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _jwtBearerOptions = options.Value;
        _jwtHandler = _jwtBearerOptions.TokenHandlers.FirstOrDefault() as JsonWebTokenHandler ?? new JsonWebTokenHandler()
        {
            MapInboundClaims = JwtSecurityTokenHandler.DefaultMapInboundClaims,
        };
    }

    public IToken<JsonWebToken> GenerateAccessToken(IEnumerable<Claim> claims)
    {
        var isseudAt = DateTime.UtcNow;
        var descriptor = new SecurityTokenDescriptor
        {
            Expires = isseudAt.AddMinutes(_config.GetValue<int?>("JwtSettings:AccessToken:ExpiresInMinutes") ?? 15),
            IssuedAt = isseudAt,
            Issuer = _jwtBearerOptions.TokenValidationParameters.ValidIssuer,
            Subject = new ClaimsIdentity(claims),

            // Source: https://stackoverflow.com/questions/71449622/add-multiple-audiences-in-token-descriptor
            Claims = new Dictionary<string, object>
            {
                { Microsoft.IdentityModel.JsonWebTokens.JwtRegisteredClaimNames.Aud, _jwtBearerOptions.TokenValidationParameters.ValidAudiences }
            },
            SigningCredentials = new SigningCredentials(
                _jwtBearerOptions.TokenValidationParameters.IssuerSigningKey,
                SecurityAlgorithms.HmacSha256)
        };

        var strToken = _jwtHandler.CreateToken(descriptor);
        var jwt = new JsonWebToken(strToken);

        return new JwtToken(strToken, jwt);
    }

    IToken ITokenService.GenerateRefreshToken()
    {
        throw new NotImplementedException("Refresh token generation is not implemented yet.");
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
