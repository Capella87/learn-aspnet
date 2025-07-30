using Microsoft.IdentityModel.JsonWebTokens;
using System.ComponentModel.DataAnnotations;
using System.Runtime.InteropServices;
using System.Text.Json.Serialization;

namespace IdentityFromScratch.Identity.JwtToken;

public class JwtTokenResponse
{
    [Required]
    [JsonPropertyName("access_token")]
    public string AccessToken { get; set; }

    [JsonPropertyName("expires_in")]
    public DateTimeOffset? Expires { get; set; }

    [JsonPropertyName("refresh_token")]
    public string? RefreshToken { get; set; }

    [JsonPropertyName("refresh_expires_in")]
    public DateTimeOffset? RefreshExpiresIn { get; set; }

    public static JwtTokenResponse CreateTokenResponse(JwtToken jwtToken, RefreshToken? refreshToken)
    {
        return new JwtTokenResponse
        {
            AccessToken = jwtToken.Token,
            Expires = jwtToken.SecurityToken?.ValidTo,
            RefreshToken = refreshToken?.Token,
            RefreshExpiresIn = refreshToken?.ValidTo
        };
    }
}
