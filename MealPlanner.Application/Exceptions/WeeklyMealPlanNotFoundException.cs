namespace MealPlanner.Application.Exceptions;

public class WeeklyMealPlanNotFoundException(Guid id) : Exception(CreateMessage("ID", id))
{
    private static string CreateMessage<T>(string fieldName, T value)
    {
        return $"Weekly meal plan with {fieldName} '{value}' was not found.";
    }
}