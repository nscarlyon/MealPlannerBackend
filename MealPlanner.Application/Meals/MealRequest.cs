using System.ComponentModel.DataAnnotations;

using MealPlanner.Domain.Meals;

namespace MealPlanner.Application.Meals;

public record MealRequest
{
    public Guid Id { get; init; }

    [StringLength(100)] public required string Name { get; init; }

    public string? Description { get; init; }

    [Range(0, 100)] public int NumberOfServings { get; init; }

    public MealType Type { get; init; }

    public CuisineType Cuisine { get; init; } = CuisineType.None;

    public List<MealIngredient> Ingredients { get; init; } = [];
}