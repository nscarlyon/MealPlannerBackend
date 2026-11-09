namespace MealPlanner.Application.MonthlyMealPlan;

public interface IMonthlyMealPlanService
{
    Task<MonthlyMealPlanResponse> GetMonthlyMealPlan(int year, int month);
}