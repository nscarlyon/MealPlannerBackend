using MealPlanner.Application.Meals;

using Microsoft.AspNetCore.Mvc;

namespace MealPlanner.Api.Controllers;

[Route("api/[controller]")]
[ApiController]
public class MealsController(IMealsService mealsService) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<PaginatedMealResponse>> Get([FromQuery] PaginatedMealRequest query)
    {
        PaginatedMealResponse result = await mealsService.GetPaginatedMeals(query);
        return Ok(result);
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<MealResponse>> Get(Guid id)
    {
        MealResponse result = await mealsService.GetMealById(id);
        return Ok(result);
    }

    [HttpPost]
    public async Task<ActionResult<MealResponse>> Post([FromBody] MealRequest request)
    {
        MealResponse result = await mealsService.CreateMeal(request);
        return CreatedAtAction(
            nameof(Get),
            new { id = result.Id },
            result);
    }

    [HttpPut("{id:guid}")]
    public async Task<ActionResult<MealResponse>> Put(Guid id, [FromBody] MealRequest request)
    {
        MealResponse result = await mealsService.UpdateMeal(id, request);
        return Ok(result);
    }

    [HttpDelete("{id:guid}")]
    public async Task<ActionResult<MealResponse>> Delete(Guid id)
    {
        await mealsService.DeleteMeal(id);
        return NoContent();
    }

    // For Testing
    [HttpPost("test")]
    public async Task<ActionResult> PostTest()
    {
        await mealsService.CreateTestMeals();
        return NoContent();
    }
}