using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace IdentityFromScratch.Identity.Token;

public class JwtToken : IToken
{
    [JsonPropertyName("access_token")]
    [Required]
    public string AccessToken { get; set; }

    [JsonPropertyName("refresh_token")]
    [Required]
    public string RefreshToken { get; set; }

    public JwtToken(string accessToken, string refreshToken)
    {
        AccessToken = accessToken ?? throw new ArgumentNullException(nameof(accessToken));
        RefreshToken = refreshToken ?? throw new ArgumentNullException(nameof(refreshToken));
    }
}
