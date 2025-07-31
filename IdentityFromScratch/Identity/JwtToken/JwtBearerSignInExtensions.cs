using IdentityFromScratch.Identity.Token;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.JsonWebTokens;

namespace IdentityFromScratch.Identity.JwtToken;

public static class JwtBearerSignInExtensions
{
    public static AuthenticationBuilder AddJwtBearerSignIn(this AuthenticationBuilder builder)
        => builder.AddJwtBearerSignIn(JwtBearerDefaults.AuthenticationScheme, options => { });

    public static AuthenticationBuilder AddJwtBearerSignIn(this AuthenticationBuilder builder, string authenticationScheme)
        => builder.AddJwtBearerSignIn(authenticationScheme, _ => { });

    public static AuthenticationBuilder AddJwtBearerSignIn(this AuthenticationBuilder builder, Action<JwtBearerOptions> configureOptions)
        => builder.AddJwtBearerSignIn(JwtBearerDefaults.AuthenticationScheme, configureOptions);

    public static AuthenticationBuilder AddJwtBearerSignIn(this AuthenticationBuilder builder, string authenticationScheme,
        Action<JwtBearerOptions> configureOptions)
        => builder.AddJwtBearerSignIn(authenticationScheme, null, configureOptions);

    public static AuthenticationBuilder AddJwtBearerSignIn(this AuthenticationBuilder builder,
        string authenticationScheme,
        string? displayName,
        Action<JwtBearerOptions> configureOptions)
    {
        ArgumentNullException.ThrowIfNull(builder);
        ArgumentNullException.ThrowIfNull(authenticationScheme);
        ArgumentNullException.ThrowIfNull(configureOptions);

        builder.Services.TryAddScoped<ITokenService, JwtTokenService>();
        builder.Services.AddSingleton<JsonWebTokenHandler>();

        builder.Services.TryAddEnumerable(ServiceDescriptor.Singleton<IConfigureOptions<JwtBearerOptions>, JwtBearerSignInConfigureOptions>());
        builder.Services.TryAddEnumerable(ServiceDescriptor.Singleton<IPostConfigureOptions<JwtBearerOptions>, JwtBearerPostConfigureOptions>());
        return builder.AddScheme<JwtBearerOptions, JwtBearerSignInHandler>(authenticationScheme, displayName, configureOptions);
    }
}
