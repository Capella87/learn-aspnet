using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace IdentityFromScratch.Identity;

public class ChangePasswordRequest
{
    [Required]
    [PasswordPropertyText]
    [JsonPropertyName("old_password")]
    public required string OldPassword { get; set; }

    [Required]
    [PasswordPropertyText]
    [JsonPropertyName("new_password")]
    public required string NewPassword { get; set; }
}
