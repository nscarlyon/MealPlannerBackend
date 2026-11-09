using MealPlanner.Application.WeeklyMealPlan;

using Microsoft.AspNetCore.Mvc;

namespace MealPlanner.Api.Controllers;

[Route("api/[controller]")]
[ApiController]
public class WeeklyMealPlanController(IWeeklyMealPlanService weeklyMealPlanService) : ControllerBase
{
    [HttpGet("{startDate}")]
    public async Task<ActionResult<WeeklyMealPlanResponse>> Get(DateOnly startDate)
    {
        WeeklyMealPlanResponse? result = await weeklyMealPlanService.GetWeeklyMealPlanByStartDate(startDate);
        return Ok(result);
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<WeeklyMealPlanResponse>> Get(Guid id)
    {
        WeeklyMealPlanResponse result = await weeklyMealPlanService.GetWeeklyMealPlanById(id);
        return Ok(result);
    }

    [HttpPost]
    public async Task<ActionResult<WeeklyMealPlanResponse>> Post([FromBody] WeeklyMealPlanRequest request)
    {
        WeeklyMealPlanResponse result = await weeklyMealPlanService.CreateWeeklyMealPlan(request);
        return CreatedAtAction(
            nameof(Get),
            new { id = result.Id },
            result);
    }

    [HttpPut("{id:guid}")]
    public async Task<ActionResult<WeeklyMealPlanResponse>> Put(Guid id, [FromBody] WeeklyMealPlanRequest request)
    {
        WeeklyMealPlanResponse result = await weeklyMealPlanService.UpdateWeeklyMealPlan(id, request);
        return Ok(result);
    }

    [HttpDelete("{id:guid}")]
    public async Task<ActionResult<WeeklyMealPlanResponse>> Delete(Guid id)
    {
        await weeklyMealPlanService.DeleteWeeklyMealPlan(id);
        return NoContent();
    }
}