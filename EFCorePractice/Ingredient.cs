namespace EFCorePractice
{
    public class Ingredient
    {
        public int IngredientId { get; set; }
        
        // This represents Many-To-One relationship between Ingredient and Recipe classes
        public int RecipeId { get; set; }
        // This property is for fully defined relationships
        // Source: https://www.learnentityframeworkcore5.com/relationship-in-ef-core
        public Recipe Recipe { get; set; }
        public required string Name { get; set; }
        public decimal Quantity { get; set; }
        public required string Unit { get; set; }
    }
}
