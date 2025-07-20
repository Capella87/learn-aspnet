using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion.Internal;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System.Globalization;

namespace IdentityFromScratch.Identity.JwtToken;

// Source: https://github.com/dotnet/aspnetcore/blob/main/src/Security/Authentication/JwtBearer/src/JwtBearerConfigureOptions.cs#L13
// Almost the same as JwtBearerConfigureOptions, but that class is for internal use only.

internal class JwtBearerSignInConfigureOptions : IConfigureNamedOptions<JwtBearerOptions>
{
    private readonly IAuthenticationConfigurationProvider _authenticationConfigurationProvider;
    private static readonly Func<string, TimeSpan> _invariantTimeSpanParse = (string timespanString) => TimeSpan.Parse(timespanString, CultureInfo.InvariantCulture);

    public JwtBearerSignInConfigureOptions(IAuthenticationConfigurationProvider authenticationConfigurationProvider)
    {
        _authenticationConfigurationProvider = authenticationConfigurationProvider;
    }

    public void Configure(string? name, JwtBearerOptions options)
    {
        if (string.IsNullOrEmpty(name))
        {
            return;
        }

        var configSection = _authenticationConfigurationProvider.GetSchemeConfiguration(name);

        if (configSection is null || !configSection.GetChildren().Any())
        {
            return;
        }

        // Get important options for TokenValidationParameters from the configuration section that the host has provided. (IConfiguration) Wherever possible.
        var validateIssuer = ParseValueOrDefault(configSection[nameof(TokenValidationParameters.ValidateIssuer)],
            bool.Parse,
            options.TokenValidationParameters.ValidateIssuer);
        var tokenIssuer  = configSection[nameof(TokenValidationParameters.ValidIssuer)];
        var tokenIssuers = configSection.GetSection(nameof(TokenValidationParameters.ValidIssuers))
            .GetChildren()
            .Select(c => c.Value)
            .ToList();
        var validateAudience = ParseValueOrDefault(configSection[nameof(TokenValidationParameters.ValidateAudience)],
            bool.Parse, options.TokenValidationParameters.ValidateAudience);
        var audience = configSection[nameof(TokenValidationParameters.ValidAudience)];
        var audiences = configSection.GetSection(nameof(TokenValidationParameters.ValidAudiences))
            .GetChildren()
            .Select(aud => aud.Value)
            .ToList();
        var validateIssuerSigningKey = ParseValueOrDefault(configSection[nameof(TokenValidationParameters.ValidateIssuerSigningKey)],
            bool.Parse, options.TokenValidationParameters.ValidateAudience);

        // Set properties in the options.
        options.Authority = configSection[nameof(options.Authority)] ?? options.Authority;
        options.BackchannelTimeout = ParseValueOrDefault(configSection[nameof(options.BackchannelTimeout)],
            _invariantTimeSpanParse, options.BackchannelTimeout);
        options.Challenge = configSection[nameof(options.Challenge)] ?? options.Challenge;
        options.ForwardAuthenticate = configSection[nameof(options.ForwardAuthenticate)] ?? options.ForwardAuthenticate;
        options.ForwardChallenge = configSection[nameof(options.ForwardChallenge)] ?? options.ForwardChallenge;
        options.ForwardDefault = configSection[nameof(options.ForwardDefault)] ?? options.ForwardDefault;
        options.ForwardForbid = configSection[nameof(options.ForwardForbid)] ?? options.ForwardForbid;
        options.ForwardSignIn = configSection[nameof(options.ForwardSignIn)] ?? options.ForwardSignIn;
        options.ForwardSignOut = configSection[nameof(options.ForwardSignOut)] ?? options.ForwardSignOut;
        options.IncludeErrorDetails = ParseValueOrDefault(configSection[nameof(options.IncludeErrorDetails)],
            bool.Parse, options.IncludeErrorDetails);
        options.MapInboundClaims = ParseValueOrDefault(configSection[nameof(options.MapInboundClaims)],
            bool.Parse, options.MapInboundClaims);
        options.MetadataAddress = configSection[nameof(options.MetadataAddress)] ?? options.MetadataAddress;

        options.RefreshInterval = ParseValueOrDefault(configSection[nameof(options.RefreshInterval)],
            _invariantTimeSpanParse, options.RefreshInterval);
        options.RefreshOnIssuerKeyNotFound = ParseValueOrDefault(configSection[nameof(options.RefreshOnIssuerKeyNotFound)],
            bool.Parse, options.RefreshOnIssuerKeyNotFound);
        options.RequireHttpsMetadata = ParseValueOrDefault(configSection[nameof(options.RequireHttpsMetadata)],
            bool.Parse, options.RequireHttpsMetadata);

        options.SaveToken = ParseValueOrDefault(configSection[nameof(options.SaveToken)],
            bool.Parse, options.SaveToken);

        options.RequireHttpsMetadata = ParseValueOrDefault(configSection[nameof(options.RequireHttpsMetadata)],
            bool.Parse, options.RequireHttpsMetadata);

        options.TokenValidationParameters = new()
        {
            ValidateIssuer = validateIssuer,
            ValidIssuers = tokenIssuers,
            ValidIssuer = tokenIssuer,
            ValidateAudience = validateAudience,
            ValidAudiences = audiences,
            ValidAudience = audience,
            ValidateIssuerSigningKey = validateIssuerSigningKey,
            IssuerSigningKeys = GetIssuerSigningKeys(configSection, [tokenIssuer, ..tokenIssuers]),
        };

        throw new NotImplementedException();
    }

    public void Configure(JwtBearerOptions options)
    {
        Configure(Options.DefaultName, options);
    }

    private static T ParseValueOrDefault<T>(string? strValue, Func<string, T> parser, T defaultValue)
    {
        if (string.IsNullOrEmpty(strValue))
        {
            return defaultValue;
        }

        return parser(strValue);
    }

    private static IEnumerable<SecurityKey> GetIssuerSigningKeys(IConfiguration config, List<string?> issuers)
    {
        foreach (var issuer in issuers)
        {
            var signingKey = config.GetSection("SigningKeys")
                .GetChildren()
                .FirstOrDefault(key => key["Issuer"] == issuer);
            if (signingKey is not null && signingKey["Key"] is string keyValue)
            {
                yield return new SymmetricSecurityKey(Convert.FromBase64String(keyValue));
            }
        }
    }
}
