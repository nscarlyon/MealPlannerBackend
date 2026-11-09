namespace MealPlanner.Application.WeeklyMealPlan;

public interface IWeeklyMealPlanService
{
    Task<WeeklyMealPlanResponse> GetWeeklyMealPlanById(Guid id);
    Task<WeeklyMealPlanResponse> CreateWeeklyMealPlan(WeeklyMealPlanRequest request);
    Task<WeeklyMealPlanResponse> UpdateWeeklyMealPlan(Guid id, WeeklyMealPlanRequest request);
    Task DeleteWeeklyMealPlan(Guid id);
    Task<WeeklyMealPlanResponse?> GetWeeklyMealPlanByStartDate(DateOnly startDate);
}