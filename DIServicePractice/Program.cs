using Microsoft.AspNetCore.HttpLogging;
using System.Collections.Concurrent;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Mvc;
using System.Text.Json;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Identity.UI.Services;

var builder = WebApplication.CreateBuilder(args);

// Kestrel configuration for HTTP/2 and HTTP/3. This can be replaced to appsettings.json related..
builder.WebHost.ConfigureKestrel((context, options) =>
{
    options.ListenAnyIP(2010, listenOptions =>
    {
        listenOptions.Protocols = Microsoft.AspNetCore.Server.Kestrel.Core.HttpProtocols.Http1AndHttp2AndHttp3;
        listenOptions.UseHttps();
    });
});

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

builder.Services.AddAntiforgery();
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

app.MapGet("/", () => "Hello Dependency Injection!");

app.MapGet("/register/{username}", RegisterUser);

app.Run();

string RegisterUser(string username)
{
    var emailSender = new EmailSender();
    emailSender.SendEmail(username);
    return $"Email sent to {username}!";
}

public class EmailSender
{
    public void SendEmail(string username)
    {
        Console.WriteLine($"Email sent to {username}!");
    }
}
