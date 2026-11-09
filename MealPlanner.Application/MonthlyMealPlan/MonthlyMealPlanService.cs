using MealPlanner.Application.WeeklyMealPlan;
using MealPlanner.Infrastructure.Persistence;

using Microsoft.EntityFrameworkCore;

namespace MealPlanner.Application.MonthlyMealPlan;

public class MonthlyMealPlanService(MealPlannerDbContext context) : IMonthlyMealPlanService
{
    public async Task<MonthlyMealPlanResponse> GetMonthlyMealPlan(int year, int month)
    {
        if (month is < 1 or > 12)
        {
            throw new ArgumentOutOfRangeException(nameof(month));
        }

        DateOnly monthStart = new(year, month, 1);
        DateOnly nextMonthStart = monthStart.AddMonths(1);

        List<Domain.MealPlan.WeeklyMealPlan> weeklyMealPlans = await context.WeeklyMealPlans
            .AsNoTracking()
            .Where(w => w.WeekStartDate < nextMonthStart
                        && w.WeekEndDate >= monthStart)
            .Include(w => w.Items)
            .ThenInclude(i => i.Meal)
            .OrderBy(w => w.WeekStartDate)
            .ToListAsync();

        return new MonthlyMealPlanResponse
        {
            WeeklyMealPlans = [.. weeklyMealPlans.Select(WeeklyMealPlanMapper.ToResponse)]
        };
    }
}