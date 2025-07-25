using Microsoft.AspNetCore.Identity;
using Scalar.AspNetCore;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics.CodeAnalysis;
using System.Text.Json.Serialization;

namespace IdentityFromScratch.Identity;

public class User<TKey> : IdentityUser<TKey>, IDisposable
    where TKey : IEquatable<TKey>
{
    public User()
    {
    }

    public User(string userName)
    {
        UserName = userName;
    }

    [JsonPropertyName("secondary_email")]
    [EmailAddress]
    public string? SecondaryEmail { get; set; }

    public string? NormalizedSecondaryEmail { get; set; }

    [JsonPropertyName("first_name")]
    public string? FirstName { get; set; }

    [JsonPropertyName("last_name")]
    public string? LastName { get; set; }

    [JsonPropertyName("baptismal_name")]
    public string? BaptismalName { get; set; }

    public DateTime? JoinedDate { get; set; }

    public override string ToString()
    {
        return UserName;
    }

    public void Dispose()
    {
        // Implement any necessary cleanup logic here
        // For example, if you had any disposable resources, you would dispose of them here.
        // In this case, there are no disposable resources, so this method can be empty;
    }
}
