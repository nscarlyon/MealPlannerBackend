namespace MealPlanner.Application.Exceptions;

public class WeeklyMealPlanAlreadyExistsException(DateOnly requestWeekStartDate) :
    Exception($"Weekly Meal with '{requestWeekStartDate}' already exists.");