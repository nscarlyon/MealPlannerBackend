using MealPlanner.Domain.Meals;

namespace MealPlanner.Application.Meals;

public static class MealsMapper
{
    public static MealResponse ToResponse(Meal meal)
    {
        MealResponse mealResponse = new()
        {
            Id = meal.Id,
            Name = meal.Name,
            Description = meal.Description,
            NumberOfServings = meal.NumberOfServings,
            Type = meal.Type,
            Cuisine = meal.Cuisine,
            Ingredients =
            [
                .. meal.Ingredients.Select(x => new MealIngredient
                {
                    IngredientId = x.Ingredient.Id, Name = x.Ingredient.Name, Quantity = x.Quantity, Unit = x.Unit
                })
            ]
        };

        return mealResponse;
    }

    public static Meal ToEntity(MealRequest mealRequest)
    {
        Meal meal = new()
        {
            Id = mealRequest.Id,
            Name = mealRequest.Name,
            Description = mealRequest.Description,
            NumberOfServings = mealRequest.NumberOfServings,
            Type = mealRequest.Type,
            Cuisine = mealRequest.Cuisine
        };

        return meal;
    }

    private static List<PaginatedMeal> ToList(List<Meal> meals)
    {
        return
        [
            .. meals.Select(x => new PaginatedMeal { Id = x.Id, Name = x.Name, Type = x.Type, Cuisine = x.Cuisine })
        ];
    }

    public static PaginatedMealResponse ToPaginatedResponse(
        List<Meal> meals,
        PaginatedMealRequest query,
        int totalCount)
    {
        return new PaginatedMealResponse
        {
            Meals = ToList(meals),
            PageNumber = query.PageNumber,
            PageSize = query.PageSize,
            TotalCount = totalCount,
            TotalPages = (int)Math.Ceiling(totalCount / (double)query.PageSize),
            SearchTerm = query.SearchTerm,
            IsDescending = query.IsDescending,
            FilterByMealTypeList = query.FilterByMealTypeList,
            FilterByCuisineTypeList = query.FilterByCuisineTypeList
        };
    }
}