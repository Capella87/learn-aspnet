using Microsoft.AspNetCore.HttpLogging;
using System.Collections.Concurrent;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Mvc;
using System.Text.Json;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

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
builder.Services.AddHealthChecks();

builder.Services.AddRazorPages();

// Much simpler.. with extension method.
builder.Services.AddEmailSender();

builder.Services.AddScoped<IMessageSender, NewEmailSender>();

// An implementation added via TryAddScoped will only be used when there's no implementation registered previously
builder.Services.TryAddScoped<IMessageSender, SmsSender>();

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

app.MapGet("/", () => "Hello Dependency Injection!");

// Directly access registered services from Program.cs at the ouside the context of a request..
LinkGenerator links = app.Services.GetRequiredService<LinkGenerator>();

app.MapGet("/register/{username}", RegisterUser);
app.MapGet("/message/single/{username}", SendSingleMessage);
app.MapGet("/message/multi/{username}", SendMultiMessage);

app.Run();

// Multiple dependencies without Dependency Injection
// Endpoint handler dedicated for /register/{username}

string RegisterUserWithoutDI(string username)
{
    // Create a EmailSender object with extra dependent objects at the same time
    // No external new objects..
    // This is implicit dependency, which should be avoided in practice.
    // Those are internally initialized and used, so those cannot be used in outside of the class.
    var emailSender = new EmailSender(
        new MessageFactory(),
        new NetworkClient(
            new EmailServerSettings("asdf.asdf.com", 25)));
    emailSender.SendEmail(username);
    return $"Email sent to {username}!";
}

// With Dependency injection. This handler just invokes method or delegate. Because needed object is provided, not created inside the handler.
// It has a single responsibility of SOLID principle.
string RegisterUser(string username, IEmailSender emailSender)
{
    emailSender.SendEmail(username);
    return $"Email sent to {username}!";
}

string SendSingleMessage(string username, IMessageSender sender)
{
    sender.SendMessage($"Hello {username}!");
    return "Check the application logs to see what was called..";
}

string SendMultiMessage(string username, IEnumerable<IMessageSender> senders)
{
    foreach (var sender in senders)
    {
        sender.SendMessage($"Hello {username}!");
    }

    return "Check the application logs to see what were called..";
}

static string RowCounts(DIDataContext db, DIRepository repository)
{
    int dbCount = db.RowCount;
    int repositoryCount = repository.RowCount;

    return $"DataContext: {dbCount}, Repository: {repositoryCount}";
}

public static class EmailSenderServiceCollectionExtensions
{
    public static IServiceCollection AddEmailSender(this IServiceCollection services)
    {
        services.AddScoped<IEmailSender, EmailSender>();
        services.AddSingleton<NetworkClient>();
        services.AddScoped<MessageFactory>();
        services.AddSingleton(
            new EmailServerSettings
            (
                Host: "smtp.asdf.asdf",
                Port: 25
            ));

        return services;
    }
}

// Interface for extensibility
public interface IMessageSender
{
    public void SendMessage(string message);
}

public interface IEmailSender
{
    public void SendEmail(string username);
}

// Newer classes which implement IMessageSender
public class NewEmailSender : IMessageSender
{
    public void SendMessage(string message)
    {
        Console.WriteLine($"Sending Email message: {message}");
    }
}

public class SmsSender : IMessageSender
{
    public void SendMessage(string message)
    {
        Console.WriteLine($"Sending SMS message: {message}");
    }
}

// EmailSender class with explicit dependencies
public class EmailSender : IEmailSender
{
    private readonly NetworkClient _client;
    private readonly MessageFactory _factory;

    // This is explicit dependency because 2 dependent classes must be created at initialization
    public EmailSender(MessageFactory factory, NetworkClient client)
    {
        _factory = factory;
        _client = client;
    }

    public void SendEmail(string username)
    {
        // Create Email object with EmailFactory class. This is factory pattern
        var email = _factory.Create(username);
        _client.SendEmail(email);

        Console.WriteLine($"Email sent to {username}!");
    }
}

public class NetworkClient
{
    private readonly EmailServerSettings _settings;

    public NetworkClient(EmailServerSettings settings)
    {
        _settings = settings;
    }

    public void SendEmail(Email email)
    {
        Console.WriteLine($"Connecting to server {_settings.Host}:{_settings.Port}");
        Console.WriteLine($"Email sent to {email.Address}: {email.Message}");
    }
}

// Factory class; Uses default constructor
public class MessageFactory
{
    public Email Create(string emailAddress)
        => new Email(emailAddress, "Thanks for signing up!");
}

public record Email(string Address, string Message);
public record EmailServerSettings(string Host, int Port);

public class DIDataContext
{
    public int RowCount { get; } = Random.Shared.Next(1, 1_000_000_000);

}

public class DIRepository
{
    private readonly DIDataContext _dataContext;
    public DIRepository(DIDataContext dataContext)
    {
        _dataContext = dataContext;
    }

    public int RowCount => _dataContext.RowCount;
}
