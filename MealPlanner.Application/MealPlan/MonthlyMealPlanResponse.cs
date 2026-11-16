namespace MealPlanner.Application.MealPlan;

public record MonthlyMealPlanResponse
{
    public List<WeeklyMealPlanResponse> WeeklyMealPlans { get; set; } = [];
}