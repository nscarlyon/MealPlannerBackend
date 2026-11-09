namespace MealPlanner.Application.Exceptions;

public class MealNotFoundException(Guid id)
    : Exception($"Meal with ID '{id}' was not found.");