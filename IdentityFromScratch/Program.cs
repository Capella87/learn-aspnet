using Microsoft.AspNetCore.Mvc.ApplicationParts;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Net.Http.Headers;
using Microsoft.AspNetCore.Rewrite;
using Microsoft.OpenApi;
using Scalar.AspNetCore;
using Serilog;
using Serilog.Events;

var configuration = new ConfigurationBuilder()
    .SetBasePath(AppContext.BaseDirectory)
    .AddEnvironmentVariables()
    .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
    .AddJsonFile($"appsettings.{Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Development"}.json",
    optional: true,
    reloadOnChange: true)
    .AddUserSecrets<Program>()
    .Build();

Log.Logger = new LoggerConfiguration()
    .ReadFrom.Configuration(configuration)
    .CreateLogger();

try
{
    Log.Information("Starting the host...");
    var builder = WebApplication.CreateBuilder(args);
    builder.Configuration.AddConfiguration(configuration);
    builder.Services.AddSerilog();

    builder.Services.Configure<RouteOptions>(o =>
    {
        o.LowercaseUrls = true;
        o.AppendTrailingSlash = true;
        o.LowercaseQueryStrings = true;
    });

    builder.Services.AddProblemDetails();
    builder.Services.AddAntiforgery();
    builder.Services.AddCors();

    if (builder.Environment.IsDevelopment())
    {
        builder.Services.AddHealthChecks();
    }

    // Identity configurations

    // Add services to the container.

    // Note: This is Controller-based MVC Web API.
    // For Minimal APIs, you should set JSON options with `builder.Services.ConfigureHttpJsonOptions`
    builder.Services.AddControllers()
        .AddJsonOptions(options =>
        {
            options.JsonSerializerOptions.AllowTrailingCommas = false;
            options.AllowInputFormatterExceptionMessages = true;
            options.JsonSerializerOptions.AllowOutOfOrderMetadataProperties = false;
            options.JsonSerializerOptions.ReadCommentHandling = System.Text.Json.JsonCommentHandling.Skip;
            options.JsonSerializerOptions.PropertyNameCaseInsensitive = true;
        });

    // Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
    builder.Services.AddEndpointsApiExplorer();
    builder.Services.AddOpenApi(options =>
    {
        options.OpenApiVersion = OpenApiSpecVersion.OpenApi3_0;
    });

    var app = builder.Build();
    app.UseRouting();
    app.UseSerilogRequestLogging((opts) =>
    {
        opts.MessageTemplate = "{Protocol} {RequestMethod} {RequestPath} responded {StatusCode} in {Elapsed:0.0000} ms";
        opts.GetMessageTemplateProperties = (HttpContext httpContext, string requestPath, double elapsedMs, int statusCode) =>
        [
            new LogEventProperty("Protocol", new ScalarValue(httpContext.Request.Protocol)),
            new LogEventProperty("RequestMethod", new ScalarValue(httpContext.Request.Method)),
            new LogEventProperty("RequestPath", new ScalarValue(requestPath)),
            new LogEventProperty("StatusCode", new ScalarValue(statusCode)),
            new LogEventProperty("Elapsed", new ScalarValue(elapsedMs)),
            new LogEventProperty("UserAgent", new ScalarValue(httpContext.Request.Headers[HeaderNames.UserAgent].ToString())),
            new LogEventProperty("ContentType", new ScalarValue(httpContext.Request.ContentType)),
        ];
    });

    // Use redirection to non-www URLs with 301 Moved Permanently status code.
    app.UseRewriter(new RewriteOptions()
        .AddRedirectToNonWwwPermanent());

    // Configure the HTTP request pipeline.
    if (app.Environment.IsDevelopment())
    {
        app.MapOpenApi();
        app.MapScalarApiReference("openapi/scalar", config =>
        {
            config.Theme = ScalarTheme.BluePlanet;
            config.HideModels = true;
            config.HideDarkModeToggle = false;
        });
        app.UseDeveloperExceptionPage();
        app.MapHealthChecks("/healthcheck");
    }
    else
    {
        app.UseExceptionHandler("/Error");
        app.UseHsts();
    }

    app.UseHttpsRedirection();
    app.UseCors();
    app.UseAntiforgery();
    app.UseStatusCodePages();
    app.UseStaticFiles();

    app.UseAuthentication();
    app.UseAuthorization();

    app.MapControllers();
    app.Run();
}
catch (Exception ex) when (ex is not HostAbortedException && ex.Source != "Microsoft.EntityFrameworkCore.Design")
{
    Log.Fatal(ex, "The WebApplication host terminated unexpectedly...");
}
catch (HostAbortedException ex) when (ex.Source != "Microsoft.EntityFrameworkCore.Design")
{
    Log.Fatal(ex, "The WebApplication host is aborted...");
}
finally
{
    Log.Information("Closing the logger...");
    Log.CloseAndFlush();
}
