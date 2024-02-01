using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.HttpLogging;
using System.Collections.Concurrent;
using System.Net.Mime;

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

// Add Problem Details for Exception Middleware
// AddProblemDetails is adding an implementation of IProblemDetailsService
// In default, ProblemDetails responds exception in two ways,
// If the request user is available to render HTML, it returns exception page with HTML formatted instead of Problem details.
// If not, it returns Problem details as IETF standardized.
builder.Services.AddProblemDetails();

// Should NOT be set in production
// builder.Environment.EnvironmentName = "Development";

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
    app.UseExceptionHandler();
}

// Enable to return error response with problem details in non-exception errors
app.UseStatusCodePages();

app.UseStaticFiles();
app.UseRouting();

var people = new List<Person>()
{
    new("Linus", "Torvalds"),        // Inventor of Linux
    new("Bill", "Joy"),              // Inventor of Vi and csh
    new("Kilnam", "Chon"),           // The Contributor of TCP/IP and the Internet in Republic of Korea
    new("Tim", "Berners-Lee"),       // Inventor of the World Wide Web
    new("Ken", "Thompson"),          // Inventor of UNIX, Go, UTF-8, B, and Belle
    new("Dennis", "Ritchie"),        // Inventor of C, UNIX, and B
    new("Bjarne", "Stroustrup"),     // Inventor of C++
    new("Anders", "Hejlsberg"),      // Inventor of C# and TypeScript
    new("Douglas", "Engelbart"),     // Inventor of the computer mouse
    new("Yukihiro", "Matsumoto"),    // Inventor of Ruby
    new("Guido", "van Rossum"),      // Inventor of Python
    new("James", "Gosling"),         // Inventor of Java
    new("John", "Backus"),           // Inventor of FORTRAN
    new("Grace", "Hopper"),          // Inventor of COBOL
    new("Ada", "Lovelace"),          // The First programmer
    new("Alan", "Turing"),           // Inventor of Turing Machine
    new("Brandon", "Eich"),          // Inventor of JavaScript
    new("Brian", "Kernighan"),       // Inventor of AWK
    new("Richard", "Stallman"),      // Free Software Foundation
    new("Larry", "Wall"),            // Inventor of Perl
    new("Martin", "Odersky"),        // Inventor of Scalaxc
    new("Rasmus", "Lerdorf"),        // Inventor of PHP
    new("Roberto", "Ierusalimschy"), // Inventor of Lua
    new("Simon", "Peyton Jones"),    // Inventor of Haskell
    new("Stephen", "Kleene"),        // Inventor of Regular Expression and Kleene's Recursion Theorem
    new("Thomas", "Kurtz"),          // Inventor of BASIC
    new("Maurice", "Wilkes"),        // Inventor of Microprogramming
    new("Bram", "Moolenaar"),        // Developer of Vim
    new("John", "McCarthy"),         // Inventor of LISP
    new("Claude", "Shannon"),        // Inventor of Information Theory
    new("Donald", "Knuth"),          // Inventor of TeX
    new("Miguel", "de Icaza"),       // Inventor of GNOME and Mono
    new("Matthias", "Ettrich"),      // Inventor of KDE
    new("John", "von Neumann"),      // Inventor of von Neumann Architecture
    new("Stephen", "Bourne"),        // Inventor of Bourne Shell
    new("Mitchell", "Baker"),        // Founder of Mozilla Foundation
    new("Brewster", "Kahle"),        // Founder of Internet Archive
};

// Thread-Safe dictionary
var _fruit = new ConcurrentDictionary<string, Fruit>();

// Defining endpoints
app.MapGet("/", () => "Hello World!");
// Search and return people whose first name starts with the given name
app.MapGet("/person/{name}", (string name) => people.Where(p => p.FirstName.StartsWith(name)));
app.MapGet("/error", () => "Sorry, something went wrong!");

app.MapGet("/teapot", (HttpResponse response) =>
{
    response.StatusCode = 418;
    response.ContentType = MediaTypeNames.Text.Plain;

    return response.WriteAsync("I'm a teapot!");
});

app.MapGet("/excpt", void () => throw new Exception("Test Exception for learning."));
app.MapGet("/nonexcpt", () => Results.NotFound());

// Lambda expression
app.MapGet("/fruit", () => _fruit);

// Define fruit-related endpoints
app.MapGet("/fruit/{id}", (string id) =>
    _fruit.TryGetValue(id, out var fruit)
        ? TypedResults.Ok(fruit)
        // Methods such as Results.NotFound() provides default responses.
        : Results.Problem(statusCode: 404))

    // Add Endpoint-level filter; Likes an onion!
    .AddEndpointFilter(ValidationHelper.ValidateId)
    
    // Another one; Just printing log information
    .AddEndpointFilter(async (context, next) =>
     {
         app.Logger.LogInformation("Executing filter...");
         object? result = await next(context);

         app.Logger.LogInformation($"Handler result: {result}");
         return result;
     });

// Results.Problem() and Results.ValidationProblem() are both returning problem details JSON format.
// The former returns 500 Internal Server Error in default.
// The latter returns 400 Bad Request and requires passing something to validation error dictionary.
// The dictionary enumerates errors in response.

app.MapPost("/fruit/{id}", (string id, Fruit fruit) =>
        _fruit.TryAdd(id, fruit)
        ? TypedResults.Created($"/fruit/{id}", fruit)
        : Results.ValidationProblem(new Dictionary<string, string[]>
        {
            {"id", new[] {"A fruit with this id already exists"} }
        }));

// Handler for request can be both static and instantiated.
app.MapPut("/fruit/{id}", (string id, Fruit fruit) =>
{
    _fruit[id] = fruit;
    return TypedResults.NoContent();
});

app.MapDelete("/fruit/{id}", (string id) =>
{
    _fruit.TryRemove(id, out _);
    return TypedResults.NoContent(); // 204 NO CONTENT : Server has successfully processed and not returning any content.
});

app.Run();

// Data Model definitions
public record Person(string FirstName, string LastName);

record struct Fruit(string Name, int Stock);

class ValidationHelper
{
    internal static async ValueTask<object?> ValidateId(EndpointFilterInvocationContext context,
        EndpointFilterDelegate next) // Second one is the next endpoint filter or endpoint middleware. It is a delegate type.
    {
        // Retrieve the method arguments from the context
        var id = context.GetArgument<string>(0);

        // Return error response if filtered
        if (string.IsNullOrEmpty(id) || !id.StartsWith('f'))
        {
            return Results.ValidationProblem(
                new Dictionary<string, string[]>
                {
                    {"id", new[]{"Invalid format. Id must start with 'f'"}}
                });
        }

        // Invoke next one if there's no trouble.
        return await next(context);
    }
}
