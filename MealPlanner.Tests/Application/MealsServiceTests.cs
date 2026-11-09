using MealPlanner.Application.Exceptions;
using MealPlanner.Application.Meals;
using MealPlanner.Domain.Meals;
using MealPlanner.Infrastructure.Persistence;

using Microsoft.EntityFrameworkCore;

using MockQueryable.Moq;

using Moq;

using Xunit;

using MealIngredientDto = MealPlanner.Application.Meals.MealIngredient;
using MealIngredientEntity = MealPlanner.Domain.Meals.MealIngredient;

namespace MealPlanner.Tests.Application;

public class MealsServiceTests
{
    private static readonly Guid MealAId = new("dd5d9556-a040-4cd5-a31c-a4f647d34809");
    private static readonly Guid MealBId = new("762ec297-5afc-475c-898c-f2a1d1953c7b");
    private static readonly Guid MealCId = new("f7d4ac6e-0b41-4099-aea2-0b06c228f1e6");
    private static readonly Guid SaltId = new("0b8e7d1e-3f43-4b8a-9d55-1f1a2b3c4d5e");
    private readonly Mock<MealPlannerDbContext> _db;
    private readonly List<Ingredient> _ingredients;

    private readonly List<Meal> _meals;
    private readonly Mock<DbSet<Ingredient>> _mockIngredients;
    private readonly Mock<DbSet<Meal>> _mockMeals;
    private readonly MealsService _service;

    public MealsServiceTests()
    {
        Ingredient salt = new() { Id = SaltId, Name = "Salt" };
        _ingredients = [salt];

        _meals =
        [
            new Meal
            {
                Id = MealAId,
                Name = "A",
                Description = "feawf",
                NumberOfServings = 2,
                Type = MealType.Dinner,
                Cuisine = CuisineType.Italian,
                Ingredients =
                [
                    new MealIngredientEntity
                    {
                        MealId = MealAId,
                        IngredientId = SaltId,
                        Ingredient = salt,
                        Quantity = 1,
                        Unit = UnitOfMeasurement.None
                    }
                ]
            },
            new Meal { Id = MealBId, Name = "B", Description = "feawf" },
            new Meal { Id = MealCId, Name = "C", Description = "feawf", IsDeleted = true }
        ];

        (_mockMeals, _mockIngredients, _db) = BuildContext(_meals, _ingredients);
        _service = new MealsService(_db.Object);
    }

    private static (Mock<DbSet<Meal>>, Mock<DbSet<Ingredient>>, Mock<MealPlannerDbContext>) BuildContext(
        List<Meal> meals,
        List<Ingredient> ingredients)
    {
        Mock<DbSet<Meal>> mockMeals = meals.BuildMockDbSet();
        mockMeals.Setup(x => x.FindAsync(It.IsAny<object[]>()))
            .Returns<object[]>(ids => ValueTask.FromResult(meals.FirstOrDefault(m => m.Id == (Guid)ids[0])));
        mockMeals.Setup(x => x.Add(It.IsAny<Meal>()))
            .Callback<Meal>(meals.Add);

        Mock<DbSet<Ingredient>> mockIngredients = ingredients.BuildMockDbSet();
        mockIngredients.Setup(x => x.Add(It.IsAny<Ingredient>()))
            .Callback<Ingredient>(ingredients.Add);

        Mock<MealPlannerDbContext> db = new(new DbContextOptions<MealPlannerDbContext>());
        db.Setup(x => x.Meals).Returns(mockMeals.Object);
        db.Setup(x => x.Ingredients).Returns(mockIngredients.Object);

        return (mockMeals, mockIngredients, db);
    }

    private static MealsService BuildService(List<Meal> meals)
    {
        return new MealsService(BuildContext(meals, []).Item3.Object);
    }

    private void VerifySaved(Times times)
    {
        _db.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), times);
    }

    #region CreateTestMeals

    [Fact]
    public async Task CreateTestMeals_Adds100MealsAndSaves()
    {
        List<Meal>? added = null;
        _mockMeals.Setup(x => x.AddRange(It.IsAny<IEnumerable<Meal>>()))
            .Callback<IEnumerable<Meal>>(m => added = m.ToList());

        await _service.CreateTestMeals();

        Assert.NotNull(added);
        Assert.Equal(100, added.Count);
        Assert.All(added, m =>
        {
            Assert.False(m.IsDeleted);
            Assert.Equal(1, m.NumberOfServings);
            Assert.StartsWith("Test ", m.Name);
            Assert.StartsWith("Test Description ", m.Description);
        });

        Dictionary<MealType, int> byType = added.GroupBy(m => m.Type).ToDictionary(g => g.Key, g => g.Count());
        Assert.Equal(
            new Dictionary<MealType, int>
            {
                [MealType.Breakfast] = 25, [MealType.Lunch] = 25, [MealType.Dinner] = 25, [MealType.Holiday] = 25
            },
            byType);

        Assert.Contains(added,
            m => m is { Name: "Test Curry 0", Description: "Test Description 0", Type: MealType.Lunch });
        Assert.Contains(added, m => m is { Name: "Test Roast Beef 24", Type: MealType.Holiday });
        VerifySaved(Times.Once());
    }

    #endregion

    #region GetMealById

    [Fact]
    public async Task GetMealById()
    {
        MealResponse expected = new() { Id = MealBId, Name = "B", Description = "feawf" };

        MealResponse result = await _service.GetMealById(MealBId);

        Assert.Equivalent(expected, result);
    }

    [Fact]
    public async Task GetMealById_MapsAllFieldsAndIngredients()
    {
        MealResponse expected = new()
        {
            Id = MealAId,
            Name = "A",
            Description = "feawf",
            NumberOfServings = 2,
            Type = MealType.Dinner,
            Cuisine = CuisineType.Italian,
            Ingredients =
            [
                new MealIngredientDto
                {
                    IngredientId = SaltId, Name = "Salt", Quantity = 1, Unit = UnitOfMeasurement.None
                }
            ]
        };

        MealResponse result = await _service.GetMealById(MealAId);

        Assert.Equivalent(expected, result, true);
    }

    [Fact]
    public async Task GetMealById_NotFound_ThrowsMealNotFoundException()
    {
        Guid id = Guid.NewGuid();

        MealNotFoundException ex = await Assert.ThrowsAsync<MealNotFoundException>(() => _service.GetMealById(id));

        Assert.Contains(id.ToString(), ex.Message);
    }

    #endregion

    #region CreateMeal

    [Fact]
    public async Task CreateMeal_WithoutIngredients_AddsMealSavesAndReturnsResponse()
    {
        Guid id = Guid.NewGuid();
        MealRequest request = new()
        {
            Id = id,
            Name = "Lasagna",
            Description = "Cheesy",
            NumberOfServings = 4,
            Type = MealType.Dinner,
            Cuisine = CuisineType.Italian
        };

        MealResponse result = await _service.CreateMeal(request);

        MealResponse expected = new()
        {
            Id = id,
            Name = "Lasagna",
            Description = "Cheesy",
            NumberOfServings = 4,
            Type = MealType.Dinner,
            Cuisine = CuisineType.Italian
        };
        Assert.Equivalent(expected, result, true);

        Meal added = Assert.Single(_meals, m => m.Id == id);
        Assert.Equal("Lasagna", added.Name);
        Assert.Empty(added.Ingredients);
        _mockMeals.Verify(x => x.Add(It.IsAny<Meal>()), Times.Once);
        _mockIngredients.Verify(x => x.Add(It.IsAny<Ingredient>()), Times.Never);
        VerifySaved(Times.Once());
    }

    [Fact]
    public async Task CreateMeal_WithExistingIngredient_ReusesIngredient()
    {
        MealRequest request = new()
        {
            Id = Guid.NewGuid(),
            Name = "Fries",
            Ingredients =
            [
                new MealIngredientDto { Name = "  Salt  ", Quantity = 2, Unit = UnitOfMeasurement.None }
            ]
        };

        MealResponse result = await _service.CreateMeal(request);

        MealIngredientDto ingredient = Assert.Single(result.Ingredients);
        Assert.Equal(SaltId, ingredient.IngredientId);
        Assert.Equal("Salt", ingredient.Name);
        Assert.Equal(2, ingredient.Quantity);
        Assert.Single(_ingredients);
        _mockIngredients.Verify(x => x.Add(It.IsAny<Ingredient>()), Times.Never);
        VerifySaved(Times.Once());
    }

    [Fact]
    public async Task CreateMeal_WithNewIngredient_CreatesTrimmedIngredient()
    {
        Guid mealId = Guid.NewGuid();
        MealRequest request = new()
        {
            Id = mealId,
            Name = "Pancakes",
            Ingredients =
            [
                new MealIngredientDto { Name = " Flour ", Quantity = 200, Unit = UnitOfMeasurement.None }
            ]
        };

        MealResponse result = await _service.CreateMeal(request);

        Ingredient created = Assert.Single(_ingredients, i => i.Name == "Flour");
        _mockIngredients.Verify(x => x.Add(It.Is<Ingredient>(i => i.Name == "Flour")), Times.Once);

        Meal meal = Assert.Single(_meals, m => m.Id == mealId);
        MealIngredientEntity mealIngredient = Assert.Single(meal.Ingredients);
        Assert.Same(created, mealIngredient.Ingredient);
        Assert.Equal(mealId, mealIngredient.MealId);
        Assert.Equal(200, mealIngredient.Quantity);

        Assert.Equal("Flour", Assert.Single(result.Ingredients).Name);
    }

    [Fact]
    public async Task CreateMeal_WithDuplicateNewIngredientNames_CreatesIngredientOnce()
    {
        Guid mealId = Guid.NewGuid();
        MealRequest request = new()
        {
            Id = mealId,
            Name = "Omelette",
            Ingredients =
            [
                new MealIngredientDto { Name = "Egg", Quantity = 1, Unit = UnitOfMeasurement.None },
                new MealIngredientDto { Name = " Egg ", Quantity = 2, Unit = UnitOfMeasurement.None }
            ]
        };

        await _service.CreateMeal(request);

        _mockIngredients.Verify(x => x.Add(It.IsAny<Ingredient>()), Times.Once);
        Meal meal = Assert.Single(_meals, m => m.Id == mealId);
        Assert.Equal(2, meal.Ingredients.Count);
        Assert.Same(meal.Ingredients[0].Ingredient, meal.Ingredients[1].Ingredient);
    }

    [Fact]
    public async Task CreateMeal_WithMixedIngredients_OnlyCreatesMissingOnes()
    {
        MealRequest request = new()
        {
            Id = Guid.NewGuid(),
            Name = "Soup",
            Ingredients =
            [
                new MealIngredientDto { Name = "Salt", Quantity = 1, Unit = UnitOfMeasurement.None },
                new MealIngredientDto { Name = "Carrot", Quantity = 3, Unit = UnitOfMeasurement.None }
            ]
        };

        MealResponse result = await _service.CreateMeal(request);

        _mockIngredients.Verify(x => x.Add(It.Is<Ingredient>(i => i.Name == "Carrot")), Times.Once);
        _mockIngredients.Verify(x => x.Add(It.Is<Ingredient>(i => i.Name == "Salt")), Times.Never);
        Assert.Equal(["Salt", "Carrot"], result.Ingredients.Select(i => i.Name));
    }

    #endregion

    #region UpdateMeal

    [Fact]
    public async Task UpdateMeal_UpdatesFieldsSavesAndReturnsResponse()
    {
        MealRequest request = new()
        {
            Name = "B updated",
            Description = "new description",
            NumberOfServings = 6,
            Type = MealType.Breakfast,
            Cuisine = CuisineType.Mexican
        };

        MealResponse result = await _service.UpdateMeal(MealBId, request);

        Meal meal = _meals.Single(m => m.Id == MealBId);
        Assert.Equal("B updated", meal.Name);
        Assert.Equal("new description", meal.Description);
        Assert.Equal(6, meal.NumberOfServings);
        Assert.Equal(MealType.Breakfast, meal.Type);
        Assert.Equal(CuisineType.Mexican, meal.Cuisine);

        Assert.Equal(MealBId, result.Id);
        Assert.Equal("B updated", result.Name);
        Assert.Equal("new description", result.Description);
        Assert.Equal(6, result.NumberOfServings);
        Assert.Equal(MealType.Breakfast, result.Type);
        Assert.Equal(CuisineType.Mexican, result.Cuisine);
        VerifySaved(Times.Once());
    }

    [Fact]
    public async Task UpdateMeal_WithIngredients_AddsIngredientsToMeal()
    {
        MealRequest request = new()
        {
            Name = "B",
            Ingredients =
            [
                new MealIngredientDto { Name = "Pepper", Quantity = 1, Unit = UnitOfMeasurement.None },
                new MealIngredientDto { Name = "Salt", Quantity = 2, Unit = UnitOfMeasurement.None }
            ]
        };

        MealResponse result = await _service.UpdateMeal(MealBId, request);

        _mockIngredients.Verify(x => x.Add(It.Is<Ingredient>(i => i.Name == "Pepper")), Times.Once);
        _mockIngredients.Verify(x => x.Add(It.Is<Ingredient>(i => i.Name == "Salt")), Times.Never);
        Meal meal = _meals.Single(m => m.Id == MealBId);
        Assert.Equal(2, meal.Ingredients.Count);
        Assert.All(meal.Ingredients, mi => Assert.Equal(MealBId, mi.MealId));
        Assert.Equal(["Pepper", "Salt"], result.Ingredients.Select(i => i.Name));
    }

    [Fact]
    public async Task UpdateMeal_NotFound_ThrowsAndDoesNotSave()
    {
        Guid id = Guid.NewGuid();
        MealRequest request = new() { Name = "Nope" };

        MealNotFoundException ex =
            await Assert.ThrowsAsync<MealNotFoundException>(() => _service.UpdateMeal(id, request));

        Assert.Contains(id.ToString(), ex.Message);
        VerifySaved(Times.Never());
    }

    #endregion

    #region DeleteMeal

    [Fact]
    public async Task DeleteMeal_SoftDeletesSavesAndReturnsResponse()
    {
        MealResponse result = await _service.DeleteMeal(MealBId);

        Assert.True(_meals.Single(m => m.Id == MealBId).IsDeleted);
        Assert.Equal(MealBId, result.Id);
        Assert.Equal("B", result.Name);
        Assert.Equal(3, _meals.Count);
        VerifySaved(Times.Once());
    }

    [Fact]
    public async Task DeleteMeal_NotFound_ThrowsAndDoesNotSave()
    {
        Guid id = Guid.NewGuid();

        MealNotFoundException ex = await Assert.ThrowsAsync<MealNotFoundException>(() => _service.DeleteMeal(id));

        Assert.Contains(id.ToString(), ex.Message);
        VerifySaved(Times.Never());
    }

    #endregion

    #region GetPaginatedMeals

    private static List<Meal> PaginationMeals()
    {
        return
        [
            new Meal
            {
                Id = Guid.Parse("00000000-0000-0000-0000-000000000001"),
                Name = "Pasta",
                Description = "c",
                Type = MealType.Dinner,
                Cuisine = CuisineType.Italian
            },
            new Meal
            {
                Id = Guid.Parse("00000000-0000-0000-0000-000000000002"),
                Name = "Tacos",
                Description = "a",
                Type = MealType.Lunch,
                Cuisine = CuisineType.Mexican
            },
            new Meal
            {
                Id = Guid.Parse("00000000-0000-0000-0000-000000000003"),
                Name = "Pancakes",
                Description = "b",
                Type = MealType.Breakfast,
                Cuisine = CuisineType.American
            },
            new Meal
            {
                Id = Guid.Parse("00000000-0000-0000-0000-000000000004"),
                Name = "Pizza",
                Description = "e",
                Type = MealType.Dinner,
                Cuisine = CuisineType.Italian
            },
            new Meal
            {
                Id = Guid.Parse("00000000-0000-0000-0000-000000000005"),
                Name = "Ramen",
                Description = "d",
                Type = MealType.Lunch,
                Cuisine = CuisineType.Japanese
            }
        ];
    }

    [Fact]
    public async Task GetPaginatedMeals_NoFilters_ReturnsAllMealsAndEchoesQuery()
    {
        MealsService service = BuildService(PaginationMeals());
        PaginatedMealRequest query = new() { PageNumber = 1, PageSize = 10 };

        PaginatedMealResponse result = await service.GetPaginatedMeals(query);

        Assert.Equal(5, result.Meals.Count);
        Assert.Equal(5, result.TotalCount);
        Assert.Equal(1, result.TotalPages);
        Assert.Equal(1, result.PageNumber);
        Assert.Equal(10, result.PageSize);
        Assert.Null(result.SearchTerm);
        Assert.Null(result.SortBy);
        Assert.True(result.IsDescending);
        Assert.Empty(result.FilterByMealTypeList);
        Assert.Empty(result.FilterByCuisineTypeList);
    }

    [Fact]
    public async Task GetPaginatedMeals_MapsPaginatedMealFields()
    {
        MealsService service = BuildService(PaginationMeals());
        PaginatedMealRequest query = new() { SearchTerm = "Tacos" };

        PaginatedMealResponse result = await service.GetPaginatedMeals(query);

        PaginatedMeal meal = Assert.Single(result.Meals);
        Assert.Equivalent(
            new PaginatedMeal
            {
                Id = Guid.Parse("00000000-0000-0000-0000-000000000002"),
                Name = "Tacos",
                Type = MealType.Lunch,
                Cuisine = CuisineType.Mexican
            },
            meal,
            true);
    }

    [Fact]
    public async Task GetPaginatedMeals_EmptyDatabase_ReturnsEmptyPage()
    {
        MealsService service = BuildService([]);

        PaginatedMealResponse result = await service.GetPaginatedMeals(new PaginatedMealRequest());

        Assert.Empty(result.Meals);
        Assert.Equal(0, result.TotalCount);
        Assert.Equal(0, result.TotalPages);
    }

    [Fact]
    public async Task GetPaginatedMeals_SearchTerm_IsTrimmedAndFiltersByName()
    {
        MealsService service = BuildService(PaginationMeals());
        PaginatedMealRequest query = new() { SearchTerm = "  P  " };

        PaginatedMealResponse result = await service.GetPaginatedMeals(query);

        Assert.Equal(["Pancakes", "Pasta", "Pizza"], result.Meals.Select(m => m.Name).Order());
        Assert.Equal(3, result.TotalCount);
        Assert.Equal("  P  ", result.SearchTerm);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public async Task GetPaginatedMeals_BlankSearchTerm_DoesNotFilter(string? searchTerm)
    {
        MealsService service = BuildService(PaginationMeals());

        PaginatedMealResponse result =
            await service.GetPaginatedMeals(new PaginatedMealRequest { SearchTerm = searchTerm });

        Assert.Equal(5, result.TotalCount);
    }

    [Fact]
    public async Task GetPaginatedMeals_SearchTerm_NoMatches_ReturnsEmpty()
    {
        MealsService service = BuildService(PaginationMeals());

        PaginatedMealResponse result =
            await service.GetPaginatedMeals(new PaginatedMealRequest { SearchTerm = "Sushi" });

        Assert.Empty(result.Meals);
        Assert.Equal(0, result.TotalCount);
    }

    [Fact]
    public async Task GetPaginatedMeals_FilterByMealType()
    {
        MealsService service = BuildService(PaginationMeals());
        PaginatedMealRequest query = new() { FilterByMealTypeList = [MealType.Lunch, MealType.Breakfast] };

        PaginatedMealResponse result = await service.GetPaginatedMeals(query);

        Assert.Equal(["Pancakes", "Ramen", "Tacos"], result.Meals.Select(m => m.Name).Order());
        Assert.Equal([MealType.Lunch, MealType.Breakfast], result.FilterByMealTypeList);
    }

    [Fact]
    public async Task GetPaginatedMeals_FilterByCuisineType()
    {
        MealsService service = BuildService(PaginationMeals());
        PaginatedMealRequest query = new() { FilterByCuisineTypeList = [CuisineType.Italian] };

        PaginatedMealResponse result = await service.GetPaginatedMeals(query);

        Assert.Equal(["Pasta", "Pizza"], result.Meals.Select(m => m.Name).Order());
        Assert.Equal([CuisineType.Italian], result.FilterByCuisineTypeList);
    }

    [Fact]
    public async Task GetPaginatedMeals_CombinedFilters_AreAppliedTogether()
    {
        MealsService service = BuildService(PaginationMeals());
        PaginatedMealRequest query = new()
        {
            SearchTerm = "a",
            FilterByMealTypeList = [MealType.Dinner, MealType.Lunch],
            FilterByCuisineTypeList = [CuisineType.Italian, CuisineType.Japanese]
        };

        PaginatedMealResponse result = await service.GetPaginatedMeals(query);

        // "a" matches Pasta, Tacos, Pancakes, Pizza, Ramen; Dinner/Lunch drops Pancakes; Italian/Japanese drops Tacos.
        Assert.Equal(["Pasta", "Pizza", "Ramen"], result.Meals.Select(m => m.Name).Order());
        Assert.Equal(3, result.TotalCount);
    }

    [Fact]
    public async Task GetPaginatedMeals_Paging_SkipsAndTakesAndComputesTotals()
    {
        List<Meal> meals = Enumerable.Range(0, 25)
            .Select(i => new Meal { Id = Guid.NewGuid(), Name = $"Meal {i:D2}" })
            .ToList();
        MealsService service = BuildService(meals);

        PaginatedMealResponse page2 =
            await service.GetPaginatedMeals(new PaginatedMealRequest { PageNumber = 2, PageSize = 10 });
        PaginatedMealResponse page3 =
            await service.GetPaginatedMeals(new PaginatedMealRequest { PageNumber = 3, PageSize = 10 });
        PaginatedMealResponse page4 =
            await service.GetPaginatedMeals(new PaginatedMealRequest { PageNumber = 4, PageSize = 10 });

        Assert.Equal(10, page2.Meals.Count);
        Assert.Equal(5, page3.Meals.Count);
        Assert.Empty(page4.Meals);
        Assert.All([page2, page3, page4], p =>
        {
            Assert.Equal(25, p.TotalCount);
            Assert.Equal(3, p.TotalPages);
        });
        Assert.Equal(2, page2.PageNumber);
        Assert.Equal(3, page3.PageNumber);

        PaginatedMealResponse page1 =
            await service.GetPaginatedMeals(new PaginatedMealRequest { PageNumber = 1, PageSize = 10 });
        List<Guid> allIds = page1.Meals.Concat(page2.Meals).Concat(page3.Meals).Select(m => m.Id).ToList();
        Assert.Equal(25, allIds.Distinct().Count());
    }

    [Fact]
    public async Task GetPaginatedMeals_DefaultSort_Descending_SortsByNameDescending()
    {
        MealsService service = BuildService(PaginationMeals());

        PaginatedMealResponse result =
            await service.GetPaginatedMeals(new PaginatedMealRequest { IsDescending = true });

        Assert.Equal(["Tacos", "Ramen", "Pizza", "Pasta", "Pancakes"], result.Meals.Select(m => m.Name));
    }

    [Fact]
    public async Task GetPaginatedMeals_DefaultSort_Ascending_SortsByNameAscending()
    {
        MealsService service = BuildService(PaginationMeals());

        PaginatedMealResponse result =
            await service.GetPaginatedMeals(new PaginatedMealRequest { IsDescending = false });

        Assert.Equal(["Pancakes", "Pasta", "Pizza", "Ramen", "Tacos"], result.Meals.Select(m => m.Name));
    }

    #endregion
}