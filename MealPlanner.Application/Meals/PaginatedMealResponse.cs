using MealPlanner.Domain.Meals;

namespace MealPlanner.Application.Meals;

public class PaginatedMealResponse
{
    public required List<PaginatedMeal> Meals { get; set; }

    public int PageNumber { get; set; } = 1;

    public int PageSize { get; set; } = 10;

    public int TotalPages { get; set; } = 0;

    public int TotalCount { get; set; } = 0;

    public string? SearchTerm { get; set; }

    public bool IsDescending { get; set; } = true;

    public string? SortBy { get; set; }

    public List<MealType> FilterByMealTypeList { get; set; } = [];
    public List<CuisineType> FilterByCuisineTypeList { get; set; } = [];
}