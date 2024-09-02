using IdentityRazorPages.Data;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Serilog;
using Serilog.Events;
using System.Text.Json;

var configuration = new ConfigurationBuilder()
        .SetBasePath(Directory.GetCurrentDirectory())
        .AddEnvironmentVariables()
        .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
        .AddJsonFile($"appsettings.{Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Development"}.json",
            optional: true, reloadOnChange: true)
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

    builder.Services.Configure<RouteOptions>(o =>
    {
        o.LowercaseUrls = true;
        o.AppendTrailingSlash = true;
        o.LowercaseQueryStrings = true;
    });

    builder.Services.ConfigureHttpJsonOptions(o =>
    {
        o.SerializerOptions.AllowTrailingCommas = false;
        o.SerializerOptions.ReadCommentHandling = JsonCommentHandling.Skip;
        o.SerializerOptions.PropertyNameCaseInsensitive = true;
    });

    builder.Host.UseDefaultServiceProvider(o =>
    {
        o.ValidateScopes = true;
        o.ValidateOnBuild = true;
    });

    builder.Services.AddAntiforgery();
    builder.Services.AddAuthentication();
    builder.Services.AddAuthorization();
    builder.Services.AddProblemDetails();
    builder.Services.AddHealthChecks();

    // Add services to the container.
    var connectionString = builder.Configuration.GetConnectionString("DefaultConnection") ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");
    builder.Services.AddDbContext<ApplicationDbContext>(options =>
        options.UseSqlServer(connectionString));
    builder.Services.AddDatabaseDeveloperPageExceptionFilter();

    // Add ASP.NET Core Identity and its services
    builder.Services.AddDefaultIdentity<IdentityUser>(options => options.SignIn.RequireConfirmedAccount = true)
        .AddEntityFrameworkStores<ApplicationDbContext>();
    builder.Services.AddRazorPages();

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
    if (app.Environment.IsDevelopment())
    {
        app.UseMigrationsEndPoint();
        app.UseDeveloperExceptionPage();
    }
    else
    {
        app.UseExceptionHandler("/Error");
        // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
        app.UseHsts();
    }
    app.UseHttpsRedirection();
    app.UseAntiforgery();
    app.UseRouting();
    app.UseAuthentication();
    app.UseStaticFiles();
    app.UseAuthorization();
    app.MapRazorPages();

    app.Run();

}
// https://github.com/dotnet/efcore/issues/29923#issuecomment-2092619682
catch (Exception ex) when (ex is not HostAbortedException && ex.Source != "Microsoft.EntityFrameworkCore.Design")
{
    Log.Fatal(ex, "WebApplication unexpectedly terminated...");
}
catch (Exception ex) when (ex is HostAbortedException && ex.Source == "Microsoft.EntityFrameworkCore.Design")
{
    Log.Information(messageTemplate: "Maybe you've migrated your database with Entity Framework Core.");
    Log.Information("Closing WebApplication...");
}
finally
{
    Log.CloseAndFlush();
}
