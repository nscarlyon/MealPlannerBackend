namespace MealPlanner.Domain.Meals;

public class Meal
{
    public Guid Id { get; set; }

    public required string Name { get; set; }

    public string? Description { get; set; }

    public int NumberOfServings { get; set; }
    public bool IsDeleted { get; set; }

    public MealType Type { get; set; } = MealType.None;

    public CuisineType Cuisine { get; set; } = CuisineType.None;

    public List<MealIngredient> Ingredients { get; set; } = [];
}