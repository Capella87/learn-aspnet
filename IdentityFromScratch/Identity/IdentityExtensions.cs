using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using System.Diagnostics.CodeAnalysis;

using IdentityFromScratch.Identity.JwtToken;
using IdentityFromScratch.Identity.Token;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using Microsoft.IdentityModel.Protocols.Configuration;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.JsonWebTokens;

namespace IdentityFromScratch.Identity;

public static class IdentityExtensions
{
    // Source: https://github.com/dotnet/aspnetcore/blob/main/src/Identity/Core/src/IdentityServiceCollectionExtensions.cs
    /// <summary>
    /// Adds and configures the identity system for the specified User and Role types with JWT token system.
    /// </summary>
    /// <typeparam name="TUser"></typeparam>
    /// <typeparam name="TRole"></typeparam>
    /// <param name="services"></param>
    /// <param name="setupAction"></param>
    /// <returns></returns>
    /// <exception cref="SecurityTokenInvalidIssuerException"></exception>
    /// <exception cref="SecurityTokenInvalidAudienceException"></exception>
    /// <exception cref="SecurityTokenSignatureKeyNotFoundException"></exception>
    public static IdentityBuilder AddTokenIdentity<TUser, [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors)] TRole>(
        this IServiceCollection services,
        Action<IdentityOptions> setupAction)
        where TUser : class
        where TRole : class
    {
        services.AddAuthentication(options =>
        {
            options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
            options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            options.DefaultSignInScheme = JwtBearerDefaults.AuthenticationScheme;
        })
        .AddJwtBearerSignIn(JwtBearerDefaults.AuthenticationScheme, options =>
        {
            // Get JwtSettings
            var jwtSettings = services.BuildServiceProvider().GetRequiredService<IConfiguration>().GetSection("JwtSettings").Get<JwtSettings>()
            ?? new JwtSettings();
            options.TokenValidationParameters = new Microsoft.IdentityModel.Tokens.TokenValidationParameters
            {
                ValidateIssuer = jwtSettings.AccessToken.ValidateIssuer ?? true,
                ValidateAudience = jwtSettings.AccessToken.ValidateAudience ?? true,
                ValidateLifetime = true,
                ValidateIssuerSigningKey = jwtSettings.AccessToken.ValidateIssuerSigningKey ?? true,
                ValidIssuers = jwtSettings.AccessToken.Issuers
                    ?? throw new SecurityTokenInvalidIssuerException("Valid issuers are not found"),
                ValidAudiences = jwtSettings.AccessToken.Audiences
                    ?? throw new SecurityTokenInvalidAudienceException("Valid audiences are not found"),

                // Note: You should hide the secret key in a secure location, such as Azure Key Vault or AWS Secrets Manager in Production level.
                IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSettings.AccessToken.SecretKey
                ?? throw new SecurityTokenSignatureKeyNotFoundException("Signing key is not found")))
            };
        })
        .AddCookie(IdentityConstants.ExternalScheme, o =>
        {
            o.Cookie.Name = IdentityConstants.ExternalScheme;
            o.ExpireTimeSpan = TimeSpan.FromMinutes(5);
        })
        .AddCookie(IdentityConstants.TwoFactorRememberMeScheme, o =>
        {
            o.Cookie.Name = IdentityConstants.TwoFactorRememberMeScheme;
            o.Events = new CookieAuthenticationEvents
            {
                OnValidatePrincipal = SecurityStampValidator.ValidateAsync<ITwoFactorSecurityStampValidator>
            };
        })
        .AddCookie(IdentityConstants.TwoFactorUserIdScheme, o =>
        {
            o.Cookie.Name = IdentityConstants.TwoFactorUserIdScheme;
            o.Events = new CookieAuthenticationEvents
            {
                OnRedirectToReturnUrl = _ => Task.CompletedTask
            };
            o.ExpireTimeSpan = TimeSpan.FromMinutes(5);
        });

        // Hosting doesn't add IHttpContextAccessor by default
        services.AddHttpContextAccessor();
        // Identity services
        services.TryAddScoped<IUserValidator<TUser>, UserValidator<TUser>>();
        services.TryAddScoped<IPasswordValidator<TUser>, PasswordValidator<TUser>>();
        services.TryAddScoped<IPasswordHasher<TUser>, PasswordHasher<TUser>>();
        services.TryAddScoped<ILookupNormalizer, UpperInvariantLookupNormalizer>();
        services.TryAddScoped<IRoleValidator<TRole>, RoleValidator<TRole>>();
        // No interface for the error describer so we can add errors without rev'ing the interface
        services.TryAddScoped<IdentityErrorDescriber>();
        services.TryAddScoped<ISecurityStampValidator, SecurityStampValidator<TUser>>();
        services.TryAddEnumerable(ServiceDescriptor.Singleton<IPostConfigureOptions<SecurityStampValidatorOptions>, PostConfigureSecurityStampValidatorOptions>());
        services.TryAddScoped<ITwoFactorSecurityStampValidator, TwoFactorSecurityStampValidator<TUser>>();
        services.TryAddScoped<IUserClaimsPrincipalFactory<TUser>, UserClaimsPrincipalFactory<TUser, TRole>>();
        services.TryAddScoped<IUserConfirmation<TUser>, DefaultUserConfirmation<TUser>>();
        services.TryAddScoped<UserManager<TUser>>();
        services.TryAddScoped<SignInManager<TUser>>();
        services.TryAddScoped<RoleManager<TRole>>();

        services.TryAddScoped<IRoleValidator<IdentityRole<int>>, RoleValidator<IdentityRole<int>>>();
        services.TryAddScoped<ITokenService<JsonWebToken>, JwtTokenService>();

        // Email Verification and password reset
        services.TryAddTransient<IEmailSender, NoOpEmailSender>();

        if (setupAction != null)
        {
            services.Configure(setupAction);
        }

        return new IdentityBuilder(typeof(TUser), typeof(TRole), services);
    }

    private sealed class PostConfigureSecurityStampValidatorOptions : IPostConfigureOptions<SecurityStampValidatorOptions>
    {
        public PostConfigureSecurityStampValidatorOptions(TimeProvider? timeProvider = null)
        {
            // We could assign this to "timeProvider ?? TimeProvider.System", but
            // SecurityStampValidator already has system clock fallback logic.
            TimeProvider = timeProvider;
        }
        private TimeProvider? TimeProvider { get; }

        public void PostConfigure(string? name, SecurityStampValidatorOptions options)
        {
            options.TimeProvider ??= TimeProvider;
        }
    }
}
