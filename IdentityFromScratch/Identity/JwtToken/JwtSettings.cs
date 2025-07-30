using Microsoft.Extensions.Options;
using System.Text.Json.Serialization;

namespace IdentityFromScratch.Identity.JwtToken;

public class JwtSettings
{
    [JsonPropertyName("AccessToken")]
    public AccessTokenSettings AccessToken { get; set; }

    [JsonPropertyName("RefreshToken")]
    public RefreshTokenSettings RefreshToken { get; set; }
}


public class  AccessTokenSettings
{
    public bool? ValidateIssuer { get; set; }

    public bool? ValidateIssuerSigningKey { get; set; }

    public IEnumerable<string>? Issuers { get; set; }

    public bool? ValidateAudience { get; set; }

    public IEnumerable<string>? Audiences { get; set; }

    public string SecretKey { get; set; }

    public long ExpiresInMinutes { get; set; }

    public string? SigningAlgorithm { get; set; } = "HS256";
}

public class RefreshTokenSettings
{
    public long ExpiresInMinutes { get; set; }
}

// TODO : Sliding Token Support
