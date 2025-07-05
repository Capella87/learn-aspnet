using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace IdentityFromScratch.Identity;

public class LoginDto
{
    // It can be encoded with Base64
    [JsonPropertyName("username")]
    [Required(ErrorMessage = "A valid username is required.")]
    public string Username { get; set; }

    [JsonPropertyName("password")]
    [Required(ErrorMessage = "Password is required.")]
    [DataType(DataType.Password)]
    public string Password { get; set; }
}
