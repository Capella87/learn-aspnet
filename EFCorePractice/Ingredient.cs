namespace EFCorePractice
{
    public class Ingredient
    {
        public int IngredientId { get; set; }
        
        // This represents Many-To-One relationship between Ingredient and Recipe classes
        public int RecipeId { get; set; }
        public required string Name { get; set; }
        public decimal Quantity { get; set; }
        public required string Unit { get; set; }
    }
}
