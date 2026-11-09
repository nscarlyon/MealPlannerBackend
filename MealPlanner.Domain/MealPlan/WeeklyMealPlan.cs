namespace MealPlanner.Domain.MealPlan;

public class WeeklyMealPlan
{
    public Guid Id { get; set; }

    public DateOnly WeekStartDate { get; set; }

    public DateOnly WeekEndDate { get; set; }

    public ICollection<MealPlanItem> Items { get; set; } = [];
}