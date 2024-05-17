using Microsoft.AspNetCore.HttpLogging;
using System.Text.Json;
using ToDoList;

var builder = WebApplication.CreateBuilder(args);

builder.Configuration.Sources.Clear();
builder.Configuration.AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
    .AddJsonFile($"appsettings.{builder.Environment.EnvironmentName}.json"
    , optional: true
    , reloadOnChange: true);


// Add services to the container.
builder.Services.AddRazorPages();
builder.Services.AddSingleton<ToDoService>();

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
