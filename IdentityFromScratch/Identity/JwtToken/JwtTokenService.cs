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

public class JwtTokenService : ITokenService<JsonWebToken>
{
    private readonly IConfiguration _config;
    private readonly ILogger<JwtTokenService> _logger;
    private readonly JwtSettings _settings;

    // TODO: Options patterns for refresh token (access token is already handled by other settings...?)

    // For JWT token (access token) generation and validation
    private readonly JsonWebTokenHandler _jwtHandler;

    ILogger<ITokenService<JsonWebToken>> ITokenService<JsonWebToken>.Logger => _logger;

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

    async Task<IToken<JsonWebToken>> ITokenService<JsonWebToken>.GenerateAccessTokenAsync(IEnumerable<Claim> claims)
    {
        var descriptor = GetTokenDescriptor(claims);

        var strToken = _jwtHandler.CreateToken(descriptor);
        // Replace this line:
        // var jwt = new JsonWebToken(strToken);

        // With this line:
        var jwt = _jwtHandler.ReadJsonWebToken(strToken);

        return (IToken<JsonWebToken>)await Task.FromResult(new JwtToken(strToken, jwt));
    }

    async Task<IToken> ITokenService<JsonWebToken>.GenerateRefreshTokenAsync()
    {
        var currentTime = DateTime.UtcNow;
        var rt = new byte[32];
        var rng = RandomNumberGenerator.Create();
        rng.GetBytes(rt);

        return await Task.FromResult(new RefreshToken(Convert.ToBase64String(rt), currentTime.AddDays(10), currentTime));
    }

    async Task<ClaimsPrincipal?> ITokenService<JsonWebToken>.GetPrincipalFromExpiredTokenAsync(string token)
    {
        var validationParameters = new TokenValidationParameters
        {
            ValidateAudience = true,
            ValidateIssuer = true,
            ValidateLifetime = false,
            ClockSkew = TimeSpan.Zero,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_settings.AccessToken.SecretKey ?? throw new SecurityTokenInvalidSigningKeyException("Secret key is not found."))),
            ValidateIssuerSigningKey = true,
            ValidAudiences = _settings.AccessToken.Audiences ?? throw new SecurityTokenInvalidAudienceException("Valid audiences are not found"),
            ValidIssuers = _settings.AccessToken.Issuers ?? throw new SecurityTokenInvalidIssuerException("Valid issuers are not found"),
        };

        var validationResult = await _jwtHandler.ValidateTokenAsync(token, validationParameters) ?? throw new InvalidOperationException("Token validation result is null");

        if (!validationResult!.IsValid)
        {
            throw new SecurityTokenException("Failed to validate the token", validationResult.Exception!);
        }

        var securityJwt = validationResult.SecurityToken as JsonWebToken;
        if (securityJwt == null || !securityJwt.Alg.Equals(_settings.AccessToken.SigningAlgorithm 
            ?? SecurityAlgorithms.HmacSha256, StringComparison.CurrentCultureIgnoreCase))
        {
            throw new SecurityTokenException("Invalid token signing algorithm or token is invalid");
        }

        return new ClaimsPrincipal(validationResult.ClaimsIdentity);
    }

    private SecurityTokenDescriptor GetTokenDescriptor(IEnumerable<Claim> claims)
    {
        var issuedAt = DateTime.UtcNow;

        var descriptor = new SecurityTokenDescriptor
        {
            Expires = issuedAt.AddMinutes(_settings.AccessToken.ExpiresInMinutes ?? 15),
            IssuedAt = issuedAt,
            Issuer = _settings.AccessToken.Issuers!.First(),
            Subject = new ClaimsIdentity(claims),

            // Source: https://stackoverflow.com/questions/71449622/add-multiple-audiences-in-token-descriptor
            Claims = new Dictionary<string, object>
            {
                { Microsoft.IdentityModel.JsonWebTokens.JwtRegisteredClaimNames.Aud, _settings.AccessToken.Audiences! }
            },
            SigningCredentials = new SigningCredentials(
                new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_settings.AccessToken.SecretKey ?? throw new SecurityTokenInvalidSigningKeyException("Secret key is not found."))),
                _settings.AccessToken.SigningAlgorithm)
        };

        return descriptor;
    }
}
