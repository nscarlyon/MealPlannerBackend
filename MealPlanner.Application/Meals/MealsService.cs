using MealPlanner.Application.Exceptions;
using MealPlanner.Domain.Meals;
using MealPlanner.Infrastructure.Persistence;

using Microsoft.EntityFrameworkCore;

namespace MealPlanner.Application.Meals;

public class MealsService(MealPlannerDbContext context) : IMealsService
{
    public async Task<MealResponse> GetMealById(Guid id)
    {
        Meal? meal = await context.Meals
            .Include(m => m.Ingredients)
            .ThenInclude(mi => mi.Ingredient)
            .FirstOrDefaultAsync(m => m.Id == id);

        return meal == null
            ? throw new MealNotFoundException(id)
            : MealsMapper.ToResponse(meal);
    }

    public async Task<MealResponse> CreateMeal(MealRequest request)
    {
        Meal meal = MealsMapper.ToEntity(request);

        await AddIngredientsToMeal(request, meal);

        context.Meals.Add(meal);
        await context.SaveChangesAsync();

        return MealsMapper.ToResponse(meal);
    }

    public async Task<MealResponse> UpdateMeal(Guid id, MealRequest request)
    {
        Meal? meal = await context.Meals.FindAsync(id);
        if (meal == null)
        {
            throw new MealNotFoundException(id);
        }

        meal.Name = request.Name;
        meal.Description = request.Description;
        meal.NumberOfServings = request.NumberOfServings;
        meal.Cuisine = request.Cuisine;
        meal.Type = request.Type;
        meal.Ingredients.Clear();

        await AddIngredientsToMeal(request, meal);
        await context.SaveChangesAsync();

        return MealsMapper.ToResponse(meal);
    }

    public async Task<PaginatedMealResponse> GetPaginatedMeals(PaginatedMealRequest query)
    {
        IQueryable<Meal> meals = context.Meals.AsNoTracking();

        if (!string.IsNullOrWhiteSpace(query.SearchTerm))
        {
            string searchTerm = query.SearchTerm.Trim();
            meals = meals.Where(x => x.Name.Contains(searchTerm));
        }

        if (query.FilterByMealTypeList.Count != 0)
        {
            meals = meals.Where(x => query.FilterByMealTypeList.Contains(x.Type));
        }

        if (query.FilterByCuisineTypeList.Count != 0)
        {
            meals = meals.Where(x => query.FilterByCuisineTypeList.Contains(x.Cuisine));
        }

        meals = query.IsDescending
            ? meals.OrderByDescending(x => x.Name)
            : meals.OrderBy(x => x.Name);

        int totalCount = await meals.CountAsync();
        List<Meal> page = await meals
            .Skip((query.PageNumber - 1) * query.PageSize)
            .Take(query.PageSize)
            .ToListAsync();

        return MealsMapper.ToPaginatedResponse(page, query, totalCount);
    }

    public async Task<MealResponse> DeleteMeal(Guid id)
    {
        Meal? meal = await context.Meals.FindAsync(id);
        if (meal == null)
        {
            throw new MealNotFoundException(id);
        }

        meal.IsDeleted = true;
        await context.SaveChangesAsync();

        return MealsMapper.ToResponse(meal);
    }

    public async Task CreateTestMeals()
    {
        (string prefix, MealType Type)[] categories = new (string prefix, MealType Type)[]
        {
            ("Test Strawberry short cake", MealType.Breakfast), ("Test Curry", MealType.Lunch),
            ("Test Meatloaf", MealType.Dinner), ("Test Roast Beef", MealType.Holiday)
        };

        List<Meal> meals = new(100);

        foreach ((string prefix, MealType type) in categories)
        {
            for (int i = 0; i < 25; i++)
            {
                meals.Add(new Meal
                {
                    Name = $"{prefix} {i}",
                    Description = $"Test Description {i}",
                    Type = type,
                    NumberOfServings = 1,
                    IsDeleted = false
                });
            }
        }


        context.Meals.AddRange(meals);
        await context.SaveChangesAsync();
    }

    private async Task AddIngredientsToMeal(MealRequest request, Meal meal)
    {
        if (request.Ingredients.Count > 0)
        {
            List<string> requestedIngredientNames = request.Ingredients
                .Select(x => x.Name.Trim())
                .Distinct()
                .ToList();

            Dictionary<string, Ingredient> existingIngredients = await context.Ingredients
                .Where(x => requestedIngredientNames.Contains(x.Name))
                .ToDictionaryAsync(x => x.Name);

            foreach (MealIngredient requestedIngredient in request.Ingredients)
            {
                string ingredientName = requestedIngredient.Name.Trim();

                if (!existingIngredients.TryGetValue(ingredientName, out Ingredient? ingredient))
                {
                    ingredient = new Ingredient { Name = ingredientName };

                    context.Ingredients.Add(ingredient);
                    existingIngredients.Add(ingredientName, ingredient);
                }

                meal.Ingredients.Add(new Domain.Meals.MealIngredient
                {
                    MealId = meal.Id,
                    Ingredient = ingredient,
                    Quantity = requestedIngredient.Quantity,
                    Unit = requestedIngredient.Unit
                });
            }
        }
    }
}