using Microsoft.AspNetCore.HttpLogging;

// Create a 'builder' prior to creating WebApplication object for configuration.
var builder = WebApplication.CreateBuilder(args);

// Configure HTTP/3 for Kestrel (Available on Windows 11 22000.0 or later)
builder.WebHost.ConfigureKestrel((context, options) =>
{
    // Set port to 2010 now, but need to integrate with appsettings.json later
    options.ListenAnyIP(2010, listenOptions =>
    {
        listenOptions.Protocols = Microsoft.AspNetCore.Server.Kestrel.Core.HttpProtocols.Http1AndHttp2AndHttp3;
        listenOptions.UseHttps();
    });
});

// Middlewares

// Use development environment logging; Not to be used in production
builder.Services.AddHttpLogging(opts =>
    opts.LoggingFields = HttpLoggingFields.RequestProperties);
builder.Logging.AddFilter("Microsoft.AspNetCore.HttpLogging", LogLevel.Debug);

// WebApplication class is available in .NET 6 or later with modern ways.
var app = builder.Build();

app.UseHsts();
app.UseHttpLogging();

if (app.Environment.IsDevelopment())
{
    // In development environment, it includes detailed error information.
    app.UseDeveloperExceptionPage();
}
else
{
    // Exception handler for production. Does not leak sensitive information.
    // When the exception is thrown in endpoint middleware, it is propagated up the pipeline.
    // The exception handler middleware catches it.
    // It changes the response path to /error and sends the request down the middleware pipeline again.
    // The pipeline executes the new error path and a response as usual.
    // The exception handler middleware updates the new response's status code to 500.
    app.UseExceptionHandler("/error");
}

app.UseStaticFiles();
app.UseRouting();

// Defining endpoints
app.MapGet("/", () => "Hello World!");
app.MapGet("/person", () => new Person("John", "Doe"));
app.MapGet("/error", () => "Sorry, something went wrong!");

app.Run();

public record Person(string FirstName, string LastName);
