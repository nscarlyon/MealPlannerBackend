using MealPlanner.Application.Exceptions;

using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;

namespace MealPlanner.Api.ExceptionHandling;

public class GlobalExceptionHandler(
    ILogger<GlobalExceptionHandler> logger) : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(
        HttpContext httpContext,
        Exception exception,
        CancellationToken cancellationToken)
    {
        ProblemDetails problemDetails = exception switch
        {
            InvalidWeekStartDateException => new ProblemDetails
            {
                Status = StatusCodes.Status400BadRequest,
                Title = "Invalid Week Start Date",
                Detail = exception.Message
            },

            MealNotFoundException => new ProblemDetails
            {
                Status = StatusCodes.Status404NotFound, Title = "Meal Not Found", Detail = exception.Message
            },

            WeeklyMealPlanNotFoundException => new ProblemDetails
            {
                Status = StatusCodes.Status404NotFound,
                Title = "Weekly Meal Plan Not Found",
                Detail = exception.Message
            },

            WeeklyMealPlanAlreadyExistsException => new ProblemDetails
            {
                Status = StatusCodes.Status409Conflict,
                Title = "Weekly Meal Plan Already Exists",
                Detail = exception.Message
            },

            _ => new ProblemDetails
            {
                Status = StatusCodes.Status500InternalServerError,
                Title = "Internal Server Error",
                Detail = "An unexpected error occurred."
            }
        };

        problemDetails.Instance = httpContext.Request.Path;

        if (problemDetails.Status >= 500)
        {
            logger.LogError(exception, "An unhandled exception occurred.");
        }
        else
        {
            logger.LogWarning(
                "Request failed with status {StatusCode}: {Message}",
                problemDetails.Status,
                exception.Message);
        }

        httpContext.Response.StatusCode = problemDetails.Status!.Value;

        await httpContext.Response.WriteAsJsonAsync(
            problemDetails,
            cancellationToken);

        return true;
    }
}