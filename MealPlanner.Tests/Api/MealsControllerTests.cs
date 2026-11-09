using MealPlanner.Api.Controllers;
using MealPlanner.Application.Meals;
using MealPlanner.Domain.Meals;

using Microsoft.AspNetCore.Mvc;

using Moq;

using Xunit;

namespace MealPlanner.Tests.Api;

public class MealsControllerTests
{
    [Fact]
    public async Task Post_PassesRequestToServiceAndReturnsCreatedMeal()
    {
        // Arrange
        MealRequest request = new()
        {
            Name = "Vegetable Curry",
            Description = "A mildly spicy curry",
            NumberOfServings = 4,
            Type = MealType.Dinner
        };
        MealResponse expectedResponse = new()
        {
            Id = Guid.NewGuid(),
            Name = request.Name,
            Description = request.Description,
            NumberOfServings = request.NumberOfServings,
            Type = request.Type
        };
        Mock<IMealsService> mealsService = new();
        mealsService
            .Setup(x => x.CreateMeal(request))
            .ReturnsAsync(expectedResponse);
        MealsController controller = new(mealsService.Object);

        // Act
        ActionResult<MealResponse> actionResult = await controller.Post(request);

        // Assert
        CreatedAtActionResult createdResult = Assert.IsType<CreatedAtActionResult>(actionResult.Result);
        Assert.Equal(nameof(MealsController.Get), createdResult.ActionName);
        Assert.Equal(expectedResponse.Id, createdResult.RouteValues!["id"]);
        Assert.Same(expectedResponse, createdResult.Value);
        mealsService.Verify(
            x => x.CreateMeal(request),
            Times.Once);
    }
}