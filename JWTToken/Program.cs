using Serilog;
using Microsoft.Extensions.Configuration;
using System.Security.Claims;
using Serilog.Events;

var configuration = new ConfigurationBuilder()
    .SetBasePath(Directory.GetCurrentDirectory())
    .AddEnvironmentVariables()
    .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
    .AddJsonFile($"appsettings.{Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Production"}.json", optional: true, reloadOnChange: true)
    .Build();

Log.Logger = new LoggerConfiguration()
    .ReadFrom.Configuration(configuration)
    .CreateLogger();
try
{
    Log.Information("Starting WebApplication...");
    var builder = WebApplication.CreateBuilder(args);
    builder.Configuration.AddConfiguration(configuration);
    builder.Services.AddSerilog();

    builder.Services.Configure<RouteOptions>(o =>
    {
        o.LowercaseUrls = true;
        o.AppendTrailingSlash = true;
        o.LowercaseQueryStrings = true;
    });

    // JSON configuration
    builder.Services.ConfigureHttpJsonOptions(o =>
    {
        o.SerializerOptions.AllowTrailingCommas = false;
        o.SerializerOptions.ReadCommentHandling = System.Text.Json.JsonCommentHandling.Skip;
        o.SerializerOptions.PropertyNameCaseInsensitive = true;
    });

    // Enable Scope Validation always (By default, it is only enabled in development)
    builder.Host.UseDefaultServiceProvider(o =>
    {
        o.ValidateScopes = true;
        o.ValidateOnBuild = true;
    });
    builder.Services.AddAntiforgery();
    builder.Services.AddProblemDetails();
    builder.Services.AddHealthChecks();

    builder.Services.AddAuthorization();
    builder.Services.AddAuthentication("Bearer")
        .AddJwtBearer();

    var app = builder.Build();
    app.UseSerilogRequestLogging((opts) =>
    {
        opts.MessageTemplate = "{Protocol} {RequestMethod} {RequestPath} responded {StatusCode} in {Elapsed:0.0000} ms";
        opts.GetMessageTemplateProperties = (HttpContext httpContext, string requestPath, double elapsedMs, int statusCode) =>
        [
            new LogEventProperty("Protocol", new ScalarValue(httpContext.Request.Protocol)),
            new LogEventProperty("RequestMethod", new ScalarValue(httpContext.Request.Method)),
            new LogEventProperty("RequestPath", new ScalarValue(requestPath)),
            new LogEventProperty("StatusCode", new ScalarValue(statusCode)),
            new LogEventProperty("Elapsed", new ScalarValue(elapsedMs))
        ];
    });

    if (!app.Environment.IsDevelopment())
    {
        app.UseExceptionHandler("/Error");
    }
    else
    {
        app.UseDeveloperExceptionPage();
    }

    app.UseHsts();
    app.UseHttpsRedirection();
    app.UseStaticFiles();
    app.UseAuthorization();
    app.UseAntiforgery();

    app.MapGet("/", () => "Hello World!");
    app.MapGet("/secret", (ClaimsPrincipal user) => $"Hello {user.Identity.Name}!")
        .RequireAuthorization();
    app.MapGet("/secret2", (ClaimsPrincipal user) => $"Hello {user.Identity.Name}! Welcome to another secret!")
        .RequireAuthorization(p => p.RequireClaim("scope", "jwttoken:anothersecret"));

    app.Run();
}
catch (Exception ex)
{
    Log.Fatal(ex, "Application unexpectedly terminated.");
}
finally
{
    Log.CloseAndFlush();
}
