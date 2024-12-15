using Microsoft.AspNetCore.HttpLogging;
using System.Collections.Concurrent;
using Microsoft.OpenApi.Models;
using Microsoft.AspNetCore.OpenApi;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc;
using Scalar.AspNetCore;

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
    o.LowercaseQueryStrings = true;
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

// Add OpenAPI tool, Swagger related services
//builder.Services.AddEndpointsApiExplorer();
//builder.Services.AddSwaggerGen(x =>
//    x.SwaggerDoc("v1", new OpenApiInfo
//    {
//        Title = "KoreanCityAPI",
//        Description = "An API for Korean cities and counties",
//        Version = "v1"
//    }));
builder.Services.AddOpenApi(options =>
{
    options.AddDocumentTransformer((document, _, _) =>
    {
        document.Info.Title = "KoreanCityAPI";
        document.Info.Description = "An API for Korean cities and counties";
        document.Info.Version = "v1";

        document.Info.Contact = new OpenApiContact()
        {
            Name = "Capella87",
            Url = new Uri("https://github.com/Capella87"),
        };

        document.Info.License = new OpenApiLicense()
        {
            Name = "MIT",
            Url = new Uri("https://opensource.org/licenses/MIT"),
        };

        return Task.CompletedTask;
    });
});

var app = builder.Build();

app.UseHsts();
app.UseHttpsRedirection();
app.UseHttpLogging();
app.UseAntiforgery();
app.UseStatusCodePages();

// OpenAPI
app.MapOpenApi();
app.MapScalarApiReference();

if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
}
else
{
    app.UseExceptionHandler();
}

app.MapRazorPages();

var _city = new ConcurrentDictionary<string, City>();

app.MapGet("/", () => "Hello World!")
    .WithName("Index");

// app.MapGroup("/city/", )
app.MapGet("/city/",
    [EndpointName("GetAllCityEntries")]
    [Tags("city")]
    [ProducesResponseType(typeof(ICollection<City>), 200)]
    [EndpointSummary("Fetches all city entries")]
    [EndpointDescription("Fetches all registered city entries in ICollection type, or returns a blank [] if there's no entry")]
    () => _city.Values)
    .WithOpenApi();

app.MapGet("/city/{id}",
    [EndpointName("GetCityEntry")]
    [Tags("city")]
    [EndpointDescription("Fetches a city entry by id, or returns 404 if there's no city entry with the ID exists")]
    [EndpointSummary("Fetches a city in Korea")]
    [ProducesResponseType(typeof(City), 200, "application/json")]
    [ProducesResponseType(typeof(HttpValidationProblemDetails), 404, "application/problem+json")]
    (string id) =>
        _city.TryGetValue(id, out var city)
            ? TypedResults.Ok(city)
            : (IResult)TypedResults.Problem(statusCode: 404))
    // Parameter description via overloading WithOpenApi() method
    .WithOpenApi(o =>
    {
        o.Parameters[0].Description = "The id of the city entry to fetch";
        o.Summary = "Fetches a city";
        return o;
    });

app.MapPost("/city/{id}",
    [EndpointName("AddCity")]
    [EndpointSummary("Add a new city")]
    [EndpointDescription("Add a new city entry with its name, province and population. Returns 404 if the city entry already exists with id")]
    [Tags("city")]
    [ProducesResponseType(typeof(City), 201)]
    [ProducesResponseType(typeof(HttpValidationProblemDetails), 400)]
    (string id, City city) =>
        _city.TryAdd(id, city)
        ? TypedResults.Created($"/city/{id}", city)
        : (IResult)TypedResults.ValidationProblem(new Dictionary<string, string[]>
        {
            {
                "id", new[]
                {
                    "A city with this id already exists"
                }
            }
    })).WithOpenApi();

app.Run();

record City(string Name, string Province, int Population);
