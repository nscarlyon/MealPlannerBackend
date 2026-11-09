namespace MealPlanner.Application.Exceptions;

public class InvalidWeekStartDateException(DateOnly requestWeekStartDate) :
    Exception($"'{requestWeekStartDate}' is not a Monday.");