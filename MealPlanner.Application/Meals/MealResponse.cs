using MealPlanner.Domain.Meals;

namespace MealPlanner.Application.Meals;

public record MealResponse
{
    public required Guid Id { get; init; }

    public required string Name { get; init; }

    public string? Description { get; init; }

    public int NumberOfServings { get; init; }

    public MealType Type { get; init; }

    public CuisineType Cuisine { get; init; } = CuisineType.None;

    public List<MealIngredient> Ingredients { get; init; } = [];
}

public record MealIngredient
{
    public Guid IngredientId { get; init; }

    public required string Name { get; init; }

    public required int Quantity { get; init; }

    public required UnitOfMeasurement Unit { get; init; } = UnitOfMeasurement.None;
}