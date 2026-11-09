namespace MealPlanner.Application.Meals;

public interface IMealsService
{
    Task<MealResponse> GetMealById(Guid id);
    Task<MealResponse> CreateMeal(MealRequest request);
    Task<MealResponse> UpdateMeal(Guid id, MealRequest request);
    Task<PaginatedMealResponse> GetPaginatedMeals(PaginatedMealRequest query);
    Task<MealResponse> DeleteMeal(Guid id);
    Task CreateTestMeals();
}