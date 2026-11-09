using MealPlanner.Domain.Meals;

namespace MealPlanner.Application.Meals;

public record PaginatedMeal
{
    public required Guid Id { get; set; }

    public required string Name { get; set; }

    public MealType Type { get; set; }

    public CuisineType? Cuisine { get; set; }
}