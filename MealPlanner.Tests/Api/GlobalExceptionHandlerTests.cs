using System.Text.Json;

using MealPlanner.Api.ExceptionHandling;
using MealPlanner.Application.Exceptions;

using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

using Moq;

using Xunit;

namespace MealPlanner.Tests.Api;

public class GlobalExceptionHandlerTests
{
    private readonly GlobalExceptionHandler _handler;

    public GlobalExceptionHandlerTests()
    {
        Mock<ILogger<GlobalExceptionHandler>> loggerMock = new();
        _handler = new GlobalExceptionHandler(loggerMock.Object);
    }

    [Theory]
    [MemberData(nameof(ExceptionTestData))]
    public async Task TryHandleAsync_ReturnsExpectedProblemDetails(
        Exception exception,
        int expectedStatusCode,
        string expectedTitle,
        string expectedDetail)
    {
        // Arrange
        DefaultHttpContext httpContext = new();
        httpContext.Response.Body = new MemoryStream();

        // Act
        bool result = await _handler.TryHandleAsync(httpContext, exception, TestContext.Current.CancellationToken);

        // Assert
        Assert.True(result);
        Assert.Equal(expectedStatusCode, httpContext.Response.StatusCode);

        httpContext.Response.Body.Seek(0, SeekOrigin.Begin);
        ProblemDetails? problemDetails = await JsonSerializer.DeserializeAsync<ProblemDetails>(
            httpContext.Response.Body,
            new JsonSerializerOptions { PropertyNameCaseInsensitive = true },
            TestContext.Current.CancellationToken);

        Assert.NotNull(problemDetails);
        Assert.Equal(expectedStatusCode, problemDetails.Status);
        Assert.Equal(expectedTitle, problemDetails.Title);
        Assert.Equal(expectedDetail, problemDetails.Detail);
    }

    public static TheoryData<Exception, int, string, string> ExceptionTestData()
    {
        Guid mealId = Guid.NewGuid();
        Guid mealPlanId = Guid.NewGuid();
        DateOnly invalidDate = new(2026, 11, 8); // Sunday
        DateOnly existingDate = new(2026, 11, 9); // Monday

        return new TheoryData<Exception, int, string, string>
        {
            {
                new InvalidWeekStartDateException(invalidDate), StatusCodes.Status400BadRequest,
                "Invalid Week Start Date", $"'{invalidDate}' is not a Monday."
            },
            {
                new MealNotFoundException(mealId), StatusCodes.Status404NotFound, "Meal Not Found",
                $"Meal with ID '{mealId}' was not found."
            },
            {
                new WeeklyMealPlanNotFoundException(mealPlanId), StatusCodes.Status404NotFound,
                "Weekly Meal Plan Not Found", $"Weekly meal plan with ID '{mealPlanId}' was not found."
            },
            {
                new WeeklyMealPlanAlreadyExistsException(existingDate), StatusCodes.Status409Conflict,
                "Weekly Meal Plan Already Exists", $"Weekly Meal with '{existingDate}' already exists."
            },
            {
                new InvalidOperationException("Something went wrong internally."),
                StatusCodes.Status500InternalServerError, "Internal Server Error", "An unexpected error occurred."
            }
        };
    }
}