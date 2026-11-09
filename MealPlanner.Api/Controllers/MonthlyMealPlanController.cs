using MealPlanner.Application.MonthlyMealPlan;

using Microsoft.AspNetCore.Mvc;

namespace MealPlanner.Api.Controllers;

[Route("api/[controller]")]
[ApiController]
public class MonthlyMealPlanController(IMonthlyMealPlanService monthlyMealPlanService) : ControllerBase
{
    [HttpGet("{request}")]
    public async Task<ActionResult<MonthlyMealPlanResponse>> Get(DateOnly request)
    {
        MonthlyMealPlanResponse result = await monthlyMealPlanService.GetMonthlyMealPlan(request.Year, request.Month);
        return Ok(result);
    }
}