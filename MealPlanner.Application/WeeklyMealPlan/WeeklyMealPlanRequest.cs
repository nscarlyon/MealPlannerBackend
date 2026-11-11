namespace MealPlanner.Application.WeeklyMealPlan;

public record WeeklyMealPlanRequest
{
    public Guid Id { get; init; }

    [MustBeMonday] public required DateOnly WeekStartDate { get; init; }

    public List<MealPlanItemRequest> Items { get; init; } = [];
}

public record MealPlanItemRequest
{
    public Guid Id { get; init; }

    public required DateOnly Date { get; init; }

    public required int SortOrder { get; init; }

    public required Guid MealId { get; init; }
}