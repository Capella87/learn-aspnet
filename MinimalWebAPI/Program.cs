using Microsoft.AspNetCore.HttpLogging;

// Create a 'builder' prior to creating WebApplication object for configuration.
var builder = WebApplication.CreateBuilder(args);

// Middlewares

// Use development environment logging; Not to be used in production
builder.Services.AddHttpLogging(opts =>
    opts.LoggingFields = HttpLoggingFields.RequestProperties);
builder.Logging.AddFilter("Microsoft.AspNetCore.HttpLogging", LogLevel.Debug);

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
    app.UseHttpLogging();
}

app.UseHsts();

// Mapping endpoints (So routing middleware is added automatically in WebApplication.)
app.MapGet("/", () => "Hello World!");
app.MapGet("/person", () => new Person("John", "Doe"));

app.Run();

public record Person(string FirstName, string LastName);
