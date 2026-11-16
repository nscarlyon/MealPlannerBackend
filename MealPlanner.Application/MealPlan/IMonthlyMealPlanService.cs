namespace MealPlanner.Application.MealPlan;

public interface IMonthlyMealPlanService
{
    Task<MonthlyMealPlanResponse> GetMonthlyMealPlan(int year, int month);
}