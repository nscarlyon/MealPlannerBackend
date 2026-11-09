using MealPlanner.Domain.Meals;

namespace MealPlanner.Domain.MealPlan;

public class MealPlanItem
{
    public Guid Id { get; set; }

    public DayOfWeek Day { get; set; }

    public DateOnly Date { get; set; }

    public int SortOrder { get; set; }

    public Guid MealId { get; set; }
    public Meal Meal { get; set; } = null!;

    public Guid WeeklyMealPlanId { get; set; }
    public WeeklyMealPlan WeeklyMealPlan { get; set; } = null!;
}