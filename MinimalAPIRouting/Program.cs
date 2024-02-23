using Microsoft.AspNetCore.HttpLogging;
using System.Collections.Concurrent;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Mvc;

var builder = WebApplication.CreateBuilder(args);

builder.WebHost.ConfigureKestrel((context, options) =>
{
    options.ListenAnyIP(2010, listenOptions =>
    {
        listenOptions.Protocols = Microsoft.AspNetCore.Server.Kestrel.Core.HttpProtocols.Http1AndHttp2AndHttp3;
        listenOptions.UseHttps();
    });
});

// Services

// Add routing options regarding URL-case, trailing slash and query string case.
builder.Services.Configure<RouteOptions>(o =>
{
    o.LowercaseUrls = true;
    o.AppendTrailingSlash = true;
    o.LowercaseQueryStrings = false;
});

builder.Services.AddAntiforgery();

// Use development environment logging; Not to be used in production
builder.Services.AddHttpLogging(opts =>
    opts.LoggingFields = HttpLoggingFields.RequestProperties);
builder.Logging.AddFilter("Microsoft.AspNetCore.HttpLogging", LogLevel.Debug);
builder.Services.AddProblemDetails();
builder.Services.AddRazorPages();
builder.Services.AddHealthChecks();

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

// Routing

app.MapGet("/HealthCheck", () => Results.Ok()).WithName("healthcheck");
app.MapGet("/products/{id}/paged",
    ([FromRoute] int id,
    [FromQuery] int page,
    [FromHeader(Name = "PageSize")] int pageSize)
    => $"Received id {id}, page {page}, pageSize {pageSize}");
app.MapGet("/legacy/{name}", (string name) => name).WithName("prod");

// We can use WithName for reference
app.MapGet("/", (LinkGenerator links) =>
new[]
{
    // Override RouteOptions by LinkOptions for purposes such as legacy support
    links.GetPathByName("healthcheck",
        options: new LinkOptions
        {
            LowercaseUrls = false,
            AppendTrailingSlash = false,
        }),
    links.GetPathByName("prod", new { Name = "Big-Widget", Q = "Test" })
})
    .WithName("home");

// Literal segments are not case-sensitive.
// Endpoint names are case-sensitive, but route templates (URL) are NOT case-sensitive.
// But in URL generation, we need to set rules whether sensitive to case or not.
// LinkGenerator can create an URL to endpoint.
app.MapGet("/links", (LinkGenerator links) =>
{
    // Create a link dynamically (in runtime)
    string link = links.GetPathByName("prod",
        new { name = "big-widget" });

    return $"View the project at {link}";
});

// But in ASP.NET Core Razor, redirection to generated link is more widely used..
// Results.RedirectToRoute returns 302 Found response code in default,
// But we can permanent and preserveMethod parameters to change response code.
app.MapGet("/redirect", () => Results.RedirectToRoute("home"));

// Results.Redirect() takes a URL instead of route name instead
app.MapGet("/redirect2", () => Results.Redirect("/"));

app.Run();
