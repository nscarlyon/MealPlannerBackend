using System.Text.Json.Serialization;

using MealPlanner.Api.ExceptionHandling;
using MealPlanner.Application.Meals;
using MealPlanner.Application.MonthlyMealPlan;
using MealPlanner.Application.WeeklyMealPlan;
using MealPlanner.Infrastructure.DependencyInjection;

using Microsoft.AspNetCore.Mvc;

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

// Built-in HTTP JSON options (used by OpenAPI)
builder.Services.ConfigureHttpJsonOptions(options =>
{
    options.SerializerOptions.NumberHandling =
        JsonNumberHandling.Strict;

    options.SerializerOptions.Converters.Add(
        new JsonStringEnumConverter());
});

// MVC controller JSON options
builder.Services
    .AddControllers(options =>
        options.Filters.Add(new ProducesAttribute("application/json")))
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.NumberHandling =
            JsonNumberHandling.Strict;

        options.JsonSerializerOptions.Converters.Add(
            new JsonStringEnumConverter());
    });


// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

builder.Services.AddInfrastructure(builder.Configuration);

builder.Services.AddExceptionHandler<GlobalExceptionHandler>();
builder.Services.AddProblemDetails();

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAngular", policy =>
    {
        policy
            .WithOrigins("http://localhost:4200")
            .AllowAnyHeader()
            .AllowAnyMethod();
    });
});

// Services
builder.Services.AddScoped<IMealsService, MealsService>();
builder.Services.AddScoped<IWeeklyMealPlanService, WeeklyMealPlanService>();
builder.Services.AddScoped<IMonthlyMealPlanService, MonthlyMealPlanService>();

WebApplication app = builder.Build();
app.UseExceptionHandler();
app.UseCors("AllowAngular");

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}


app.MapControllers();

app.Run();