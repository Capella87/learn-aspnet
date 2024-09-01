using Microsoft.AspNetCore.Mvc.ApplicationModels;
using RazorPageHandlerPractice.Services;
using System.Text.Json;
using System.Text.RegularExpressions;
using Serilog.Events;
using Microsoft.Extensions.Logging.Configuration;
using Serilog;

var configuration = new ConfigurationBuilder()
    .SetBasePath(Directory.GetCurrentDirectory())
    .AddEnvironmentVariables()
    .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
    .AddJsonFile($"appsettings.{Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Development"}.json",
        optional: true,
        reloadOnChange: true)
    .Build();

Log.Logger = new LoggerConfiguration()
    .ReadFrom.Configuration(configuration)
    .CreateLogger();

try
{
    Log.Information("Starting WebApplication...");
    var builder = WebApplication.CreateBuilder(args);
    builder.Configuration.AddConfiguration(configuration);
    builder.Services.AddSerilog();

    // Add services to the container.
    // Set page routing configuration regarding kebab-case
    builder.Services.AddRazorPages()
        .AddRazorPagesOptions(opts =>
        {
            opts.Conventions.Add(
                new PageRouteTransformerConvention(
                    new KebabCaseParameterTransformer()));
            opts.Conventions.AddPageRoute(
                "/Search/Products/StartSearch", "/search-products");
        });

    // Routing configuration
    // Kebab-case, lower-case, and trailing slash are common in these days.
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

    // Show details according to RFC9110
    builder.Services.AddProblemDetails();

    builder.Services.AddHealthChecks();
    builder.Services.AddRazorPages();
    builder.Services.AddMvc();
    builder.Services.AddSingleton<SearchService>();

    var app = builder.Build();
    app.UseSerilogRequestLogging((opts) =>
    {
        opts.MessageTemplate = "{Protocol} {RequestMethod} {RequestPath} responded {StatusCode} in {Elapsed:0.0000} ms";
        opts.GetMessageTemplateProperties = (HttpContext httpContext, string requestPath, double elapsedMs, int statusCode) =>
        [
            new LogEventProperty("Protocol", new ScalarValue(httpContext.Request.Protocol)),
            new LogEventProperty("RequestMethod", new ScalarValue(httpContext.Request.Method)),
            new LogEventProperty("RequestPath", new ScalarValue(requestPath)),
            new LogEventProperty("StatusCode", new ScalarValue(statusCode)),
            new LogEventProperty("Elapsed", new ScalarValue(elapsedMs))
        ];
    });

    // Configure the HTTP request pipeline.
    if (!app.Environment.IsDevelopment())
    {
        app.UseExceptionHandler("/Error");
    }
    else
    {
        app.UseDeveloperExceptionPage();
    }

    app.UseHsts();
    app.UseHttpsRedirection();
    app.UseStaticFiles();
    app.UseAntiforgery();
    app.UseStatusCodePagesWithReExecute("/Errors/{0}");
    app.UseAuthorization();

    app.MapGet("/Category/", () => (IResult)TypedResults.Redirect("/Category/Game"));
    app.MapRazorPages();

    app.Run();
}
catch (Exception ex)
{
    Log.Fatal(ex, "Host application unexpectedly terminated");
}
finally
{
    Log.CloseAndFlush();
}

public class KebabCaseParameterTransformer : IOutboundParameterTransformer
{
    // Convert PascalCase to kebab-case with lowercase. Using regular expression
    public string TransformOutbound(object? value)
    {
        if (value is null)
            return null;

        return Regex.Replace(value.ToString(),
            "([a-z])([A-Z])", "$1-$2").ToLower();
    }
}
