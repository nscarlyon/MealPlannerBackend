using System.ComponentModel.DataAnnotations;

namespace MealPlanner.Application.WeeklyMealPlan;

public class MustBeMonday : ValidationAttribute
{
    public override bool IsValid(object? value)
    {
        if (value is not DateOnly date)
        {
            return false;
        }

        return date.DayOfWeek == DayOfWeek.Monday;
    }

    public override string FormatErrorMessage(string name)
    {
        return $"The {name} must be a Monday.";
    }
}