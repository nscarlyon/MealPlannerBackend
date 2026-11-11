using System.ComponentModel.DataAnnotations;

using MealPlanner.Domain.Meals;

namespace MealPlanner.Application.Meals;

public record PaginatedMealRequest
{
    [Range(1, int.MaxValue)] 
    public int PageNumber { get; init; } = 1;

    [AllowedValues(10, 20, 50, 100)]
    public int PageSize { get; init; } = 10;

    public string? SearchTerm { get; init; }

    public bool IsDescending { get; init; } = true;

    public List<MealType> FilterByMealTypeList { get; init; } = [];

    public List<CuisineType> FilterByCuisineTypeList { get; init; } = [];
}