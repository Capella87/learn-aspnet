using Microsoft.AspNetCore.HttpLogging;

// Create a 'builder' prior to creating WebApplication object for configuration.
var builder = WebApplication.CreateBuilder(args);

// Middlewares

// Use development environment logging; Not to be used in production
builder.Services.AddHttpLogging(opts =>
    opts.LoggingFields = HttpLoggingFields.RequestProperties);
builder.Logging.AddFilter("Microsoft.AspNetCore.HttpLogging", LogLevel.Debug);

// WebApplication class is available in .NET 6 or later with modern ways.
var app = builder.Build();

app.UseHsts();

if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
    app.UseHttpLogging();
}

app.UseStaticFiles();
app.UseRouting();

// Defining endpoints
app.MapGet("/", () => "Hello World!");
app.MapGet("/person", () => new Person("John", "Doe"));

app.Run();

public record Person(string FirstName, string LastName);
