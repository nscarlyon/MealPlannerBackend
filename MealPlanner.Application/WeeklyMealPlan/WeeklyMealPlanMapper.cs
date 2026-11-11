using MealPlanner.Application.Meals;
using MealPlanner.Domain.MealPlan;

namespace MealPlanner.Application.WeeklyMealPlan;

public static class WeeklyMealPlanMapper
{
    public static WeeklyMealPlanResponse ToResponse(Domain.MealPlan.WeeklyMealPlan plan)
    {
        return new WeeklyMealPlanResponse
        {
            Id = plan.Id,
            WeekStartDate = plan.WeekStartDate,
            WeekEndDate = plan.WeekEndDate,
            Items = plan.Items
                .OrderBy(x => x.Date)
                .ThenBy(x => x.SortOrder)
                .Select(ToItemResponse)
                .ToList()
        };
    }

    private static MealPlanItemResponse ToItemResponse(MealPlanItem item)
    {
        return new MealPlanItemResponse
        {
            Id = item.Id,
            Date = item.Date,
            SortOrder = item.SortOrder,
            MealId = item.MealId,
            Meal = MealsMapper.ToResponse(item.Meal)
        };
    }

    public static Domain.MealPlan.WeeklyMealPlan ToEntity(WeeklyMealPlanRequest request)
    {
        Domain.MealPlan.WeeklyMealPlan plan = new()
        {
            WeekStartDate = request.WeekStartDate, WeekEndDate = request.WeekStartDate.AddDays(6), Items = []
        };

        if (request.Items.Count != 0)
        {
            foreach (MealPlanItemRequest itemReq in request.Items)
            {
                plan.Items.Add(new MealPlanItem
                {
                    Date = itemReq.Date,
                    SortOrder = itemReq.SortOrder,
                    MealId = itemReq.MealId
                });
            }
        }

        return plan;
    }
}