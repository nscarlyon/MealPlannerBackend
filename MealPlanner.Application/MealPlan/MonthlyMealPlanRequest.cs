using System.ComponentModel.DataAnnotations;

namespace MealPlanner.Application.MealPlan;

public class MonthlyMealPlanRequest
{
    [Required] public int Year { get; set; }

    [Required] public int Month { get; set; }
}