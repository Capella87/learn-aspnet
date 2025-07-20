using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace IdentityFromScratch.Identity;

public class SignUpRequestModel
{
    [Required(ErrorMessage = "Username is required.")]
    [JsonPropertyName("username")]
    public string Username { get; set; }

    [Required(ErrorMessage = "Password is required.")]
    [DataType(DataType.Password)]
    [JsonPropertyName("password")]
    public string Password { get; set; }

    [EmailAddress(ErrorMessage = "Invalid email address.")]
    [JsonPropertyName("email")]
    public string? EmailAddress { get; set; }

    [JsonPropertyName("first_name")]
    public string? FirstName { get; set; }

    [JsonPropertyName("last_name")]
    public string? LastName { get; set; }

    [JsonPropertyName("middle_name")]
    public string? MiddleName { get; set; }

    [JsonPropertyName("birthday")]
    public DateOnly? Birthday { get; set; }
}
