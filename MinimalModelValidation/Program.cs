using Microsoft.AspNetCore.HttpLogging;
using System.Collections.Concurrent;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Mvc;
using System.Text.Json;
using System.ComponentModel.DataAnnotations;

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


// Routing

// /users binding 
app.MapPost("/users", (CreateUserModel user) => user.ToString());

// /minimal/users binding; Requires MinimalApis.Extensions for WithParameterValidation();
app.MapPost("/minimal/users", (UserModel user) => user.ToString()).WithParameterValidation();

app.Run();

public record class UserModel
{
    // Validation with System.ComponentModel.DataAnnotations, but it can only check format of input.
    // Moreover, dependable properties are hard to be validated..
    // These attributes are derived from ValidationAttribute, so we can implement custom attributes by inheritance.
    [Required]
    [StringLength(100)]
    [Display(Name = "First Name")]
    public string FirstName { get; set; }

    [Required]
    [StringLength(100)]
    [Display(Name = "Last Name")]
    public string LastName { get; set; }

    [Required]
    [EmailAddress]
    public string EmailAddress { get; set; }

    [Phone]
    [Display(Name = "Phone Number")]
    public string PhoneNumber { get; set; }
}

public record class CreateUserModel : IValidatableObject
{
    [EmailAddress]
    public string EmailAddress { get; set; }

    [Phone]
    public string PhoneNumber { get; set; }

    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        if (string.IsNullOrEmpty(EmailAddress) && string.IsNullOrEmpty(PhoneNumber))
        {
            yield return new ValidationResult("You must provide an EmailAddress or a PhoneNumber",
                [nameof(EmailAddress), nameof(PhoneNumber)]); // This expression is called collection expressions. Requires .NET 8 (C# 12) or later.
        }
    }
}
