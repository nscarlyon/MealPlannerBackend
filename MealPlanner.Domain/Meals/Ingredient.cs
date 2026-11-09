namespace MealPlanner.Domain.Meals;

public class Ingredient
{
    public Guid Id { get; set; }

    public required string Name { get; set; }

    public List<MealIngredient> MealIngredients { get; set; } = [];
}