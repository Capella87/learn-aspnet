using Microsoft.Net.Http.Headers;
using Microsoft.AspNetCore.Rewrite;
using Microsoft.OpenApi;
using Scalar.AspNetCore;
using Serilog;
using Serilog.Events;
using IdentityFromScratch.Identity;
using Microsoft.IdentityModel.Tokens;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.EntityFrameworkCore;
using IdentityFromScratch;
using IdentityFromScratch.Identity.JwtToken;
using System.Text;
using IdentityFromScratch.Identity.Token;
using System.Text.Json;
using System.Text.Json.Serialization;

var configuration = new ConfigurationBuilder()
    .SetBasePath(AppContext.BaseDirectory)
    .AddEnvironmentVariables()
    .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
    .AddJsonFile($"appsettings.{Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Development"}.json",
    optional: true,
    reloadOnChange: true)
    .AddUserSecrets<Program>()
    .Build();

Log.Logger = new LoggerConfiguration()
    .ReadFrom.Configuration(configuration)
    .CreateLogger();

try
{
    Log.Information("Starting the host...");
    var builder = WebApplication.CreateBuilder(args);
    builder.Configuration.AddConfiguration(configuration);
    builder.Services.AddSerilog();

    builder.Services.Configure<RouteOptions>(o =>
    {
        o.LowercaseUrls = true;
        o.AppendTrailingSlash = true;
        o.LowercaseQueryStrings = true;
    });

    builder.Services.AddProblemDetails();
    builder.Services.AddAntiforgery();
    builder.Services.AddCors();

    if (builder.Environment.IsDevelopment())
    {
        builder.Services.AddHealthChecks();
    }

    // Add services to the container.

    // Configure DbContext with PostgreSQL
    builder.Services.AddDbContext<AppDbContext>(options =>
    {
        options.UseNpgsql(builder.Configuration.GetConnectionString(builder.Configuration["ConnectionStringProfile"] ?? "DefaultConnection"),
            npgsqlOptions =>
            {
                npgsqlOptions.EnableRetryOnFailure();
            });
    });

    // Identity configurations
    builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
        .AddJwtBearerSignIn(JwtBearerDefaults.AuthenticationScheme, options =>
        {
            options.TokenValidationParameters = new Microsoft.IdentityModel.Tokens.TokenValidationParameters
            {
                ValidateIssuer = builder.Configuration.GetValue<bool?>("JwtSettings:AccessToken:ValidateIssuer") ?? true,
                ValidateAudience = builder.Configuration.GetValue<bool?>("JwtSettings:AccessToken:ValidateAudience") ?? true,
                ValidateLifetime = true,
                ValidateIssuerSigningKey = builder.Configuration.GetValue<bool?>("JwtSettings:AccessToken:ValidateIssuerSigningKey") ?? true,
                ValidIssuers = builder.Configuration.GetSection("JwtSettings:AccessToken:Issuers").Get<string[]?>()
                    ?? throw new SecurityTokenInvalidIssuerException("Valid issuers are not found"),
                ValidAudiences = builder.Configuration.GetSection("JwtSettings:AccessToken:Audiences").Get<string[]?>()
                    ?? throw new SecurityTokenInvalidAudienceException("Valid audiences are not found"),

                // Note: You should hide the secret key in a secure location, such as Azure Key Vault or AWS Secrets Manager in Production level.
                IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration["JwtSettings:AccessToken:SecretKey"]
                ?? throw new SecurityTokenSignatureKeyNotFoundException("Signing key is not found")))
            };
        });

    builder.Services.TryAddScoped<IRoleValidator<IdentityRole<int>>, RoleValidator<IdentityRole<int>>>();
    builder.Services.TryAddScoped<ITokenService, JwtTokenService>();

    // Source: https://github.com/dotnet/aspnetcore/blob/main/src/Identity/UI/src/IdentityServiceCollectionUIExtensions.cs
    // Source: https://github.com/dotnet/aspnetcore/blob/main/src/Identity/Core/src/IdentityBuilderExtensions.cs#L28
    // Source: https://github.com/dotnet/aspnetcore/blob/main/src/Identity/UI/src/IdentityBuilderUIExtensions.cs

    builder.Services.AddIdentity<User<int>, IdentityRole<int>>(options =>
    {
        options.User.RequireUniqueEmail = true;
        options.Password.RequireDigit = true;
        options.Password.RequireLowercase = true;
        options.Password.RequireUppercase = true;
        options.Password.RequiredLength = 15;
        options.Password.RequireNonAlphanumeric = false;
    })
        .AddEntityFrameworkStores<AppDbContext>()
        .AddDefaultTokenProviders();

    // Email Verification and password reset
    builder.Services.TryAddTransient<IEmailSender, NoOpEmailSender>();

    // Note: This is Controller-based MVC Web API.
    // For Minimal APIs, you should set JSON options with `builder.Services.ConfigureHttpJsonOptions`
    builder.Services.AddControllers()
        .AddJsonOptions(options =>
        {
            options.JsonSerializerOptions.AllowTrailingCommas = false;
            options.AllowInputFormatterExceptionMessages = true;
            options.JsonSerializerOptions.AllowOutOfOrderMetadataProperties = false;
            options.JsonSerializerOptions.ReadCommentHandling = JsonCommentHandling.Skip;
            options.JsonSerializerOptions.PropertyNameCaseInsensitive = true;
        });

    // Exclude null values from JSON serialization in some services or controllers.
    // Named Options
    builder.Services.Configure<JsonSerializerOptions>("NoNullSerialization", options =>
    {
        options.AllowTrailingCommas = false;
        options.AllowOutOfOrderMetadataProperties = false;
        options.ReadCommentHandling = JsonCommentHandling.Skip;
        options.PropertyNameCaseInsensitive = true;
        options.DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull;
    });

    // Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
    builder.Services.AddEndpointsApiExplorer();
    builder.Services.AddOpenApi(options =>
    {
        options.OpenApiVersion = OpenApiSpecVersion.OpenApi3_0;
    });

    var app = builder.Build();
    app.UseRouting();
    app.UseSerilogRequestLogging((opts) =>
    {
        opts.MessageTemplate = "{Protocol} {RequestMethod} {RequestPath} responded {StatusCode} in {Elapsed:0.0000} ms";
        opts.GetMessageTemplateProperties = (HttpContext httpContext, string requestPath, double elapsedMs, int statusCode) =>
        [
            new LogEventProperty("Protocol", new ScalarValue(httpContext.Request.Protocol)),
            new LogEventProperty("RequestMethod", new ScalarValue(httpContext.Request.Method)),
            new LogEventProperty("RequestPath", new ScalarValue(requestPath)),
            new LogEventProperty("StatusCode", new ScalarValue(statusCode)),
            new LogEventProperty("Elapsed", new ScalarValue(elapsedMs)),
            new LogEventProperty("UserAgent", new ScalarValue(httpContext.Request.Headers[HeaderNames.UserAgent].ToString())),
            new LogEventProperty("ContentType", new ScalarValue(httpContext.Request.ContentType)),
        ];
    });

    // Use redirection to non-www URLs with 301 Moved Permanently status code.
    app.UseRewriter(new RewriteOptions()
        .AddRedirectToNonWwwPermanent());

    // Configure the HTTP request pipeline.
    if (app.Environment.IsDevelopment())
    {
        app.MapOpenApi();
        app.MapScalarApiReference("openapi/scalar", config =>
        {
            config.Theme = ScalarTheme.BluePlanet;
            config.HideModels = true;
            config.HideDarkModeToggle = false;
        });
        app.UseDeveloperExceptionPage();
        app.MapHealthChecks("/healthchecks");
    }
    else
    {
        app.UseExceptionHandler("/error");
        app.UseHsts();
    }

    app.UseHttpsRedirection();
    app.UseCors();
    app.UseAntiforgery();
    app.UseStatusCodePages();
    app.UseStaticFiles();

    app.UseAuthentication();
    app.UseAuthorization();

    app.MapControllers();
    app.Run();
}
catch (Exception ex) when (ex is not HostAbortedException && ex.Source != "Microsoft.EntityFrameworkCore.Design")
{
    Log.Fatal(ex, "The WebApplication host terminated unexpectedly...");
}
catch (HostAbortedException ex) when (ex.Source != "Microsoft.EntityFrameworkCore.Design")
{
    Log.Fatal(ex, "The WebApplication host is aborted...");
}
finally
{
    Log.Information("Closing the logger...");
    Log.CloseAndFlush();
}
