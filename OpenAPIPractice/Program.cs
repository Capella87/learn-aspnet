using Microsoft.AspNetCore.HttpLogging;
using System.Collections.Concurrent;
using Microsoft.OpenApi.Models;
using Microsoft.AspNetCore.OpenApi;
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

// Add OpenAPI tool, Swagger related services
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddSwaggerGen(x =>
    x.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "KoreanCityAPI",
        Description = "An API for Korean cities and counties",
        Version = "v1"
    }));

var app = builder.Build();

app.UseHsts();
app.UseHttpsRedirection();
app.UseHttpLogging();
app.UseAntiforgery();
app.UseStatusCodePages();

// OpenAPI
app.UseSwagger();
app.UseSwaggerUI();

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
app.MapGet("/city/", () => _city.Values)
    .WithName("GetAllCityEntries")
    .WithTags("city")
    .Produces<ICollection<City>>()
    .WithSummary("Fetches all city entries")
    .WithDescription("Fetches all registered city entries as a list, or returns a blank list if there's no entry")
    .WithOpenApi();

app.MapGet("/city/{id}", (string id) =>
    _city.TryGetValue(id, out var city) ? TypedResults.Ok(city) : (IResult)TypedResults.Problem(statusCode: 404))
    .WithName("GetCityEntry")
    .WithTags("city")
    .Produces<City>()
    .ProducesProblem(statusCode: 404) // Description for the API; It will be shown in Swagger API explorer.
    .WithSummary("Fetches a city in Korea")
    .WithDescription("Fetches a city entry by id, or returns 404 if there's no city entry with the ID exists")
    // Parameter description via overloading WithOpenApi() method
    .WithOpenApi(o =>
    {
        o.Parameters[0].Description = "The id of the city entry to fetch";
        o.Summary = "Fetches a city";
        return o;
    });

app.MapPost("/city/{id}", (string id, City city) =>
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
    }))
    .WithName("AddCity")
    .WithTags("city")
    .Produces<City>(statusCode: 201)
    .ProducesValidationProblem();

app.Run();

record City(string Name, string Province, int Population);
