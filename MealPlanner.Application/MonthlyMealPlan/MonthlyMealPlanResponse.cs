using MealPlanner.Application.WeeklyMealPlan;

namespace MealPlanner.Application.MonthlyMealPlan;

public record MonthlyMealPlanResponse
{
    public List<WeeklyMealPlanResponse> WeeklyMealPlans { get; set; }
}