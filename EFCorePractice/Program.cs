using EFCorePractice;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.HttpLogging;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Text.Json;
using Microsoft.OpenApi.Models;
using Microsoft.AspNetCore.OpenApi;
using Microsoft.EntityFrameworkCore.Metadata;
using Swashbuckle.AspNetCore.Swagger;
using Microsoft.OpenApi.Writers;
using Microsoft.OpenApi.Extensions;

var builder = WebApplication.CreateBuilder(args);

builder.Configuration.Sources.Clear();
builder.Configuration
    .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
    .AddJsonFile($"appsettings.{builder.Environment.EnvironmentName}.json", optional: true);

// User Secrets; We should use this provider method only in development.
// In production, environment variables or key vaults such as Azure Key Vault are strongly recommended.
if (builder.Environment.IsDevelopment())
{
    builder.Configuration.AddUserSecrets<Program>();
}

// Routing configuration
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

// Retrieve database connection configuration and register DbContext to DI container
var connectionString = builder.Configuration.GetConnectionString("DefaultConnectionWithSqlServer");
// In this case, we connect to SQL Server.
builder.Services.AddDbContext<AppDbContext>(options => options.UseSqlServer(connectionString));

// EFCore services on DI container
builder.Services.AddScoped<RecipeService>();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
    options.SwaggerDoc("v1", new OpenApiInfo { Title = "Recipe App", Version = "v1" }));

builder.Services.AddAntiforgery();
builder.Services.AddHttpLogging(opts =>
   opts.LoggingFields = HttpLoggingFields.RequestProperties);
builder.Logging.AddFilter("Microsoft.AspNetCore.HttpLogging", LogLevel.Debug);
builder.Services.AddProblemDetails();
builder.Services.AddHealthChecks();
builder.Services.AddRazorPages();

builder.Services.AddMvc();

var app = builder.Build();

app.UseHsts();
app.UseHttpsRedirection();
app.UseHttpLogging();
app.UseAntiforgery();
app.UseStatusCodePages();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
    app.UseDeveloperExceptionPage();
}
else
{
    app.UseExceptionHandler();
}

app.MapRazorPages();

app.MapGet("/", () => "Hello, EF Core and ASP.NET Core!");

var cookingRoute = app.MapGroup("/recipes")
    .WithParameterValidation()
    .WithOpenApi()
    .WithTags("Recipes");

cookingRoute.MapGet("/", async (RecipeService service) =>
{
    return await service.GetRecipes();
})
    .WithSummary("List all recipes")
    .Produces<ICollection<RecipeSummaryViewModel>>(statusCode: StatusCodes.Status200OK);

cookingRoute.MapPost("/", async (CreateRecipeCommand input, RecipeService service) =>
{
    var id = await service.CreateRecipe(input);
    return TypedResults.CreatedAtRoute("view-recipe", new { id });
})
    .WithSummary("Create recipe")
    .Produces(StatusCodes.Status201Created)
    .ProducesProblem(StatusCodes.Status400BadRequest);

cookingRoute.MapGet("/{id}", async (int id, RecipeService service) =>
{
    var recipe = await service.GetRecipeDetail(id);

    return recipe is null
        ? (IResult)TypedResults.NotFound()
        : (IResult)TypedResults.Ok(recipe);
})
    .WithName("view-recipe")
    .WithSummary("Get recipe")
    .ProducesProblem(404)
    .Produces<RecipeDetailViewModel>();

cookingRoute.MapPut("/{id}", async (int id, UpdateRecipeCommand input, RecipeService service) =>
{
    if (await service.IsAvailableForUpdate(id))
    {
        await service.UpdateRecipe(id, input);
        return (IResult)TypedResults.NoContent();
    }

    return TypedResults.NotFound();
})
    .WithSummary("Update recipe")
    .ProducesProblem(404)
    .Produces(204);

cookingRoute.MapDelete("/{id}", async (int id, RecipeService service) =>
{
    await service.DeleteRecipe(id);
    return TypedResults.NoContent();
})
    .WithSummary("Delete recipe")
    .ProducesProblem(404)
    .Produces(204);

app.Run();

public class AppDbContext : DbContext
{
    public DbSet<Recipe> Recipes { get; set; }

    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
    }
}

public class EditRecipeBase
{
    [Required, StringLength(100)]
    public required string Name { get; set; }
    [Range(0, 23), DisplayName("Time to cook (hours)")]
    public int TimeToCookHours { get; set; }
    [Range(0, 59), DisplayName("Time to cook (minutes)")]
    public int TimeToCookMinutes { get; set; }
    [Required]
    public required string Method { get; set; }
    [DisplayName("Vegetarian?")]
    public bool IsVegetarian { get; set; }
    [DisplayName("Vegan?")]
    public bool IsVegan { get; set; }
}

public class CreateRecipeCommand : EditRecipeBase
{
    public IList<CreateIngredientCommand> Ingredients { get; set; } = new List<CreateIngredientCommand>();

    public Recipe ToRecipe()
    {
        return new Recipe
        {
            Name = Name,
            TimeToCook = new TimeSpan(TimeToCookHours, TimeToCookMinutes, 0),
            Method = Method,
            IsVegetarian = IsVegetarian,
            IsVegan = IsVegan,
            Ingredients = Ingredients.Select(x => x.ToIngredient()).ToList()
        };
    }
}

public class CreateIngredientCommand
{
    [Required, StringLength(100)]
    public required string Name { get; set; }
    [Range(0, int.MaxValue)]
    public decimal Quantity { get; set; }
    [Required, StringLength(20)]
    public required string Unit { get; set; }

    // Will be used in such as LINQ
    // Command Pattern; Store data and use them to create an object..
    public Ingredient ToIngredient()
    {
        return new Ingredient
        {
            Name = Name,
            Quantity = Quantity,
            Unit = Unit
        };
    }
}

public class UpdateRecipeCommand : EditRecipeBase
{
    public int Id { get; set; }

    public void UpdateRecipe(Recipe recipe)
    {
        recipe.Name = Name;
        recipe.TimeToCook = new TimeSpan(TimeToCookHours, TimeToCookMinutes, 0);
        recipe.Method = Method;
        recipe.IsVegetarian = IsVegetarian;
        recipe.IsVegan = IsVegan;
    }
}

public class RecipeDetailViewModel
{
    public int Id { get; set; }
    public required string Name { get; set; }
    public required string Method { get; set; }

    public required IEnumerable<Item> Ingredients { get; set; }

    public class Item
    {
        public required string Name { get; set; }
        public required string Quantity { get; set; }
    }
}

public class RecipeSummaryViewModel
{
    public int Id { get; set; }
    public required string Name { get; set; }
    public required string TimeToCook { get; set; }
    public int NumberOfIngredients { get; set; }

    public static RecipeSummaryViewModel FromRecipe(Recipe recipe)
    {
        return new RecipeSummaryViewModel
        {
            Id = recipe.RecipeId,
            Name = recipe.Name,
            TimeToCook = $"{recipe.TimeToCook.Hours}hrs {recipe.TimeToCook.Minutes}mins",
        };
    }
}
