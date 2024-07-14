using Microsoft.AspNetCore.HttpLogging;
using Microsoft.AspNetCore.Mvc.ApplicationModels;
using System.Text.Json;
using System.Text.RegularExpressions;
using ToDoList;

var builder = WebApplication.CreateBuilder(args);

builder.Configuration.Sources.Clear();
builder.Configuration.AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
    .AddJsonFile($"appsettings.{builder.Environment.EnvironmentName}.json"
    , optional: true
    , reloadOnChange: true);


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
builder.Services.AddSingleton<ToDoService>();

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
builder.Services.AddHttpLogging(opts =>
   opts.LoggingFields = HttpLoggingFields.RequestProperties);
builder.Logging.AddFilter("Microsoft.AspNetCore.HttpLogging", LogLevel.Debug);
builder.Services.AddProblemDetails();
builder.Services.AddHealthChecks();
builder.Services.AddRazorPages();
builder.Services.AddMvc();

var app = builder.Build();

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
app.UseHttpLogging();
app.UseAntiforgery();
app.UseStatusCodePages();

app.UseRouting();
app.UseAuthorization();

app.MapGet("/Category/", () => (IResult)TypedResults.Redirect("/Category/Game"));
app.MapRazorPages();

app.Run();

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
