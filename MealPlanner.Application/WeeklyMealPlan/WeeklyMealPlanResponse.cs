using MealPlanner.Application.Meals;

namespace MealPlanner.Application.WeeklyMealPlan;

public record WeeklyMealPlanResponse
{
    public required Guid Id { get; init; }

    public required DateOnly WeekStartDate { get; init; }

    public required DateOnly WeekEndDate { get; init; }

    public List<MealPlanItemResponse> Items { get; init; } = [];
}

public record MealPlanItemResponse
{
    public required Guid Id { get; init; }

    public required DayOfWeek Day { get; init; }

    public required DateOnly Date { get; init; }

    public required int SortOrder { get; init; }

    public required Guid MealId { get; init; }

    public required MealResponse Meal { get; init; }
}