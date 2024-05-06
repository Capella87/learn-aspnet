namespace EFCorePractice
{
    public class Recipe
    {
        public int RecipeId { get; set; }
        public required string Name { get; set; }
        public TimeSpan TimeToCook { get; set; }
        public bool IsDeleted { get; set; }
        public required string Method { get; set; }
        public required ICollection<Ingredient> Ingredients { get; set; }
    }
}
