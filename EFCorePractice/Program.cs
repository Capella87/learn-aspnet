using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.HttpLogging;
using System.Text.Json;

var builder = WebApplication.CreateBuilder(args);

builder.Configuration.Sources.Clear();
builder.Configuration
    .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
    .AddJsonFile($"appsettings.{builder.Environment.EnvironmentName}.json", optional: true);

// User Secrets; We should use this provider method only in development.
// In production, environment variables or key vaults such as Azure Key Vault are strongly recommended.
if (builder.Environment.IsDevelopment())
{
    builder.Configuration.AddUserSecrets<Program>();
}

// Routing configuration
builder.Services.Configure<RouteOptions>(o =>
{
    o.LowercaseUrls = true;
    o.AppendTrailingSlash = true;
    o.LowercaseQueryStrings = false;
});

// JSON configuration
builder.Services.ConfigureHttpJsonOptions(o =>
{
    o.SerializerOptions.AllowTrailingCommas = false;
    o.SerializerOptions.ReadCommentHandling = JsonCommentHandling.Skip;
    o.SerializerOptions.PropertyNameCaseInsensitive = true;
});

// Enable Scope Validation always (By default, it is only enabled in development)
builder.Host.UseDefaultServiceProvider(o =>
{
    o.ValidateScopes = true;
    o.ValidateOnBuild = true;
});

builder.Services.AddAntiforgery();
builder.Services.AddHttpLogging(opts =>
   opts.LoggingFields = HttpLoggingFields.RequestProperties);
builder.Logging.AddFilter("Microsoft.AspNetCore.HttpLogging", LogLevel.Debug);
builder.Services.AddProblemDetails();
builder.Services.AddHealthChecks();
builder.Services.AddRazorPages();

builder.Services.AddMvc();

var app = builder.Build();

app.UseHsts();
app.UseHttpsRedirection();
app.UseHttpLogging();
app.UseAntiforgery();
app.UseStatusCodePages();

if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
}
else
{
    app.UseExceptionHandler();
}

app.MapRazorPages();

app.MapGet("/", () => "Hello, EF Core and ASP.NET Core!");

app.Run();
