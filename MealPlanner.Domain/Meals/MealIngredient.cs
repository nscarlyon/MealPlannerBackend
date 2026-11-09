namespace MealPlanner.Domain.Meals;

public class MealIngredient
{
    public Guid MealId { get; set; }

    public Meal Meal { get; set; } = null!;

    public Guid IngredientId { get; set; }
    public Ingredient Ingredient { get; set; } = null!;

    public int Quantity { get; set; }

    public UnitOfMeasurement Unit { get; set; } = UnitOfMeasurement.None;
}