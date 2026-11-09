using MealPlanner.Application.Exceptions;
using MealPlanner.Domain.MealPlan;
using MealPlanner.Infrastructure.Persistence;

using Microsoft.EntityFrameworkCore;

namespace MealPlanner.Application.WeeklyMealPlan;

public class WeeklyMealPlanService(MealPlannerDbContext context) : IWeeklyMealPlanService
{
    public async Task<WeeklyMealPlanResponse> GetWeeklyMealPlanById(Guid id)
    {
        Domain.MealPlan.WeeklyMealPlan? plan = await context.WeeklyMealPlans
            .AsNoTracking()
            .Include(x => x.Items)
            .ThenInclude(x => x.Meal)
            .FirstOrDefaultAsync(x => x.Id == id);

        return plan == null
            ? throw new WeeklyMealPlanNotFoundException(id)
            : WeeklyMealPlanMapper.ToResponse(plan);
    }

    public async Task<WeeklyMealPlanResponse?> GetWeeklyMealPlanByStartDate(DateOnly startDate)
    {
        Domain.MealPlan.WeeklyMealPlan? plan = await context.WeeklyMealPlans
            .AsNoTracking()
            .Include(x => x.Items)
            .ThenInclude(x => x.Meal)
            .FirstOrDefaultAsync(x => x.WeekStartDate == startDate);

        return plan is null
            ? null
            : WeeklyMealPlanMapper.ToResponse(plan);
    }

    public async Task<WeeklyMealPlanResponse> CreateWeeklyMealPlan(WeeklyMealPlanRequest request)
    {
        if (request.WeekStartDate.DayOfWeek != DayOfWeek.Monday)
        {
            throw new InvalidWeekStartDateException(request.WeekStartDate);
        }

        if (await context.WeeklyMealPlans.AnyAsync(x => x.WeekStartDate == request.WeekStartDate))
        {
            throw new WeeklyMealPlanAlreadyExistsException(request.WeekStartDate);
        }

        Domain.MealPlan.WeeklyMealPlan plan = WeeklyMealPlanMapper.ToEntity(request);
        plan.WeekEndDate = request.WeekStartDate.AddDays(6);
        context.WeeklyMealPlans.Add(plan);

        await context.SaveChangesAsync();

        return WeeklyMealPlanMapper.ToResponse(plan);
    }

    public async Task<WeeklyMealPlanResponse> UpdateWeeklyMealPlan(Guid id, WeeklyMealPlanRequest request)
    {
        Domain.MealPlan.WeeklyMealPlan? plan = await context.WeeklyMealPlans
            .Include(x => x.Items)
            .FirstOrDefaultAsync(x => x.Id == id);

        if (plan is null)
        {
            throw new WeeklyMealPlanNotFoundException(id);
        }

        plan.WeekStartDate = request.WeekStartDate;
        plan.WeekEndDate = request.WeekStartDate.AddDays(6);

        context.MealPlanItems.RemoveRange(plan.Items);
        plan.Items.Clear();

        foreach (MealPlanItemRequest itemReq in request.Items)
        {
            plan.Items.Add(new MealPlanItem
            {
                Day = itemReq.Day,
                Date = itemReq.Date,
                SortOrder = itemReq.SortOrder,
                MealId = itemReq.MealId,
                WeeklyMealPlanId = plan.Id
            });
        }

        await context.SaveChangesAsync();

        Domain.MealPlan.WeeklyMealPlan updatedPlan = await context.WeeklyMealPlans
            .Include(x => x.Items)
            .ThenInclude(x => x.Meal)
            .SingleAsync(x => x.Id == id);

        return WeeklyMealPlanMapper.ToResponse(updatedPlan);
    }

    public async Task DeleteWeeklyMealPlan(Guid id)
    {
        Domain.MealPlan.WeeklyMealPlan? plan = await context.WeeklyMealPlans
            .FirstOrDefaultAsync(x => x.Id == id);

        if (plan == null)
        {
            throw new WeeklyMealPlanNotFoundException(id);
        }

        context.WeeklyMealPlans.Remove(plan);
        await context.SaveChangesAsync();
    }
}