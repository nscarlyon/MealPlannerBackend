using System.ComponentModel.DataAnnotations;

using MealPlanner.Domain.Meals;

namespace MealPlanner.Application.Meals;

public class PaginatedMealRequest
{
    [Required] [Range(1, int.MaxValue)] public int PageNumber { get; set; } = 1;

    [Required]
    [AllowedValues(10, 20, 50, 100)]
    public int PageSize { get; set; } = 10;

    public string? SearchTerm { get; set; }

    public bool IsDescending { get; set; } = true;

    public List<MealType> FilterByMealTypeList { get; set; } = [];

    public List<CuisineType> FilterByCuisineTypeList { get; set; } = [];
}