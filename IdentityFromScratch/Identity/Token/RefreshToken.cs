using System.ComponentModel.DataAnnotations;
using System.Security.Claims;

namespace IdentityFromScratch.Identity.Token;

public class RefreshToken : IToken
{
    [Key]
    public string Token { get; set; }

    public DateTimeOffset? ValidTo { get; set; }

    public DateTimeOffset? ValidFrom { get; set; } = DateTimeOffset.UtcNow;


    // TODO: Apply Options pattern for configuration

    public RefreshToken(string token, DateTimeOffset? validTo = null, DateTimeOffset? validFrom = null)
    {
        Token = token;
        ArgumentException.ThrowIfNullOrEmpty(token, nameof(token));
        ValidTo = validTo ?? DateTimeOffset.UtcNow.AddDays(30); // Default to 30 days if not specified
    }
}
