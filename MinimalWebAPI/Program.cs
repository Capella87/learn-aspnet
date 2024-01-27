using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.HttpLogging;
using System.Collections.Concurrent;

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

// Should NOT be set in production
builder.Environment.EnvironmentName = "Development";

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
    new("Martin", "Odersky"),        // Inventor of Scala
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

// Lambda expression
app.MapGet("/fruit", () => _fruit);

// Define fruit-related endpoints
app.MapGet("/fruit/{id}", (string id) =>
        _fruit.TryGetValue(id, out var fruit)
        ? TypedResults.Ok(fruit)
        : Results.NotFound());
app.MapPost("/fruit/{id}", (string id, Fruit fruit) =>
        _fruit.TryAdd(id, fruit)
        ? TypedResults.Created($"/fruit/{id}", fruit)
        : Results.BadRequest(new { id = "A fruit with this id is already exist" }));

// Handler for request can be both static and instantiated.
app.MapPut("/fruit/{id}", (string id, Fruit fruit) =>
{
    _fruit[id] = fruit;
    return Results.NoContent();
});

app.MapDelete("/fruit/{id}", (string id) =>
{
    _fruit.TryRemove(id, out _);
    return Results.NoContent(); // 204 NO CONTENT : Server has successfully processed and not returning any content.
});

app.Run();

// Data Model definitions
public record Person(string FirstName, string LastName);

record struct Fruit(string Name, int Stock);
