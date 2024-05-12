using EFCorePractice;
using FluentAssertions.Formatting;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel;
using System.Runtime.InteropServices;

public class RecipeService
{
    readonly AppDbContext _context;
    readonly ILogger _logger;
    
    public RecipeService(AppDbContext context, ILoggerFactory loggerFactory)
    {
        _context = context;
        _logger = loggerFactory.CreateLogger<RecipeService>();
    }

    public async Task<int> CreateRecipe(CreateRecipeCommand cmd)
    {
        var recipe = cmd.ToRecipe();
        _context.Add(recipe);
        await _context.SaveChangesAsync();

        return recipe.RecipeId;
    }

    public async Task<ICollection<RecipeSummaryViewModel>> GetRecipes()
    {
        return await _context.Recipes
            .Where(result => !result.IsDeleted)
            .Select(result => RecipeSummaryViewModel.FromRecipe(result))
            .ToListAsync();
    }

    public async Task<RecipeDetailViewModel?> GetRecipeDetail(int id)
    {
        return await _context.Recipes
            .Where(x => x.RecipeId == id)
            .Where(x => !x.IsDeleted)
            .Select(x => new RecipeDetailViewModel
            {
                Id = x.RecipeId,
                Name = x.Name,
                Method = x.Method,
                Ingredients = x.Ingredients
                .Select(item => new RecipeDetailViewModel.Item
                {
                    Name = item.Name,
                    Quantity = $"{item.Quantity} {item.Unit}"
                })
            })
            .SingleOrDefaultAsync();
    }

    public async Task<UpdateRecipeCommand?> GetRecipeForUpdate(int id)
    {
        return await _context.Recipes
            .Where(x => x.RecipeId == id)
            .Where(x => !x.IsDeleted)
            .Select(x => new UpdateRecipeCommand
            {
                Name = x.Name,
                Method = x.Method,
                IsVegan = x.IsVegan,
                IsVegetarian = x.IsVegetarian,
                TimeToCookHours = x.TimeToCook.Hours,
                TimeToCookMinutes = x.TimeToCook.Minutes
            })
            .SingleOrDefaultAsync();
    }

    public async Task UpdateRecipe(UpdateRecipeCommand cmd)
    {
        var recipe = await _context.Recipes.FindAsync(cmd.Id);
        if (recipe == null) throw new Exception("Unable to find the recipe");
        if (recipe.IsDeleted) throw new Exception("Unable to update a deleted recipe");

        cmd.UpdateRecipe(recipe);
        // Save the updated entity to database.
        await _context.SaveChangesAsync();
    }

    public async Task DeleteRecipe(int recipeId)
    {
        var recipe = await _context.Recipes.FindAsync(recipeId);
        if (recipe is not null)
        {
            recipe.IsDeleted = true;
            await _context.SaveChangesAsync();
        }
    }
    
    public async Task<bool> IsAvailableForUpdate(int recipeId)
    {
        return await _context.Recipes
            .Where(x => x.RecipeId == recipeId)
            .Where(x => !x.IsDeleted)
            .AnyAsync();
    }
}
