using MealPlanner.Application.Exceptions;
using MealPlanner.Application.MealPlan;
using MealPlanner.Infrastructure.Entities.MealPlan;
using MealPlanner.Infrastructure.Entities.Meals;
using MealPlanner.Infrastructure.Persistence;

using Microsoft.EntityFrameworkCore;

using MockQueryable.Moq;

using Moq;

using Xunit;


namespace MealPlanner.Tests.Application;

public class WeeklyMealPlanServiceTests
{
    private static readonly Guid PlanId = new("3f2b6c1a-8d4e-4a7b-9c0d-1e2f3a4b5c6d");
    private static readonly Guid OtherPlanId = new("7a8b9c0d-1e2f-4a3b-8c4d-5e6f7a8b9c0d");
    private static readonly Guid PancakesId = new("a1b2c3d4-0000-0000-0000-000000000001");
    private static readonly Guid CurryId = new("a1b2c3d4-0000-0000-0000-000000000002");
    private static readonly Guid LasagnaId = new("a1b2c3d4-0000-0000-0000-000000000003");

    // 2026-11-16 is a Monday.
    private static readonly DateOnly Monday = new(2026, 11, 16);
    private static readonly DateOnly NextMonday = Monday.AddDays(7);

    private readonly Mock<MealPlannerDbContext> _db;
    private readonly List<Meal> _meals;
    private readonly Mock<DbSet<MealPlanItem>> _mockItems;
    private readonly Mock<DbSet<WeeklyMealPlan>> _mockPlans;
    private readonly List<WeeklyMealPlan> _plans;
    private readonly WeeklyMealPlanService _service;

    public WeeklyMealPlanServiceTests()
    {
        _meals =
        [
            new Meal { Id = PancakesId, Name = "Pancakes", Type = MealType.Breakfast },
            new Meal { Id = CurryId, Name = "Curry", Type = MealType.Lunch },
            new Meal { Id = LasagnaId, Name = "Lasagna", Type = MealType.Dinner }
        ];

        _plans =
        [
            new WeeklyMealPlan
            {
                Id = PlanId,
                WeekStartDate = Monday,
                WeekEndDate = Monday.AddDays(6),
                Items =
                [
                    // Deliberately out of order to verify the mapper sorts by Date then SortOrder.
                    Item(Guid.Parse("b0000000-0000-0000-0000-000000000003"), Monday.AddDays(1), 0, LasagnaId),
                    Item(Guid.Parse("b0000000-0000-0000-0000-000000000002"), Monday, 1, CurryId),
                    Item(Guid.Parse("b0000000-0000-0000-0000-000000000001"), Monday, 0, PancakesId)
                ]
            },
            new WeeklyMealPlan
            {
                Id = OtherPlanId,
                WeekStartDate = NextMonday,
                WeekEndDate = NextMonday.AddDays(6),
                Items = []
            }
        ];

        _mockPlans = _plans.BuildMockDbSet();
        _mockPlans.Setup(x => x.Add(It.IsAny<WeeklyMealPlan>()))
            .Callback<WeeklyMealPlan>(_plans.Add);
        _mockPlans.Setup(x => x.Remove(It.IsAny<WeeklyMealPlan>()))
            .Callback<WeeklyMealPlan>(p => _plans.Remove(p));

        _mockItems = new List<MealPlanItem>().BuildMockDbSet();

        _db = new Mock<MealPlannerDbContext>(new DbContextOptions<MealPlannerDbContext>());
        _db.Setup(x => x.WeeklyMealPlans).Returns(_mockPlans.Object);
        _db.Setup(x => x.MealPlanItems).Returns(_mockItems.Object);

        _service = new WeeklyMealPlanService(_db.Object);
    }

    private MealPlanItem Item(Guid id, DateOnly date, int sortOrder, Guid mealId)
    {
        return new MealPlanItem
        {
            Id = id,
            Date = date,
            SortOrder = sortOrder,
            MealId = mealId,
            Meal = _meals.Single(m => m.Id == mealId),
            WeeklyMealPlanId = PlanId
        };
    }

    /// <summary>
    ///     UpdateWeeklyMealPlan reloads the plan with <c>.ThenInclude(x => x.Meal)</c> after saving, which in EF
    ///     populates <see cref="MealPlanItem.Meal" />. The mock DbSet can't do that, so simulate it on save.
    /// </summary>
    private void SimulateMealIncludeOnSave()
    {
        _db.Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .Callback(() =>
            {
                foreach (MealPlanItem item in _plans.SelectMany(p => p.Items))
                {
                    item.Meal = _meals.Single(m => m.Id == item.MealId);
                }
            })
            .ReturnsAsync(1);
    }

    private void VerifySaved(Times times)
    {
        _db.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), times);
    }

    #region GetWeeklyMealPlanById

    [Fact]
    public async Task GetWeeklyMealPlanById_ReturnsPlanWithItemsSortedByDateThenSortOrder()
    {
        WeeklyMealPlanResponse result = await _service.GetWeeklyMealPlanById(PlanId);

        Assert.Equal(PlanId, result.Id);
        Assert.Equal(Monday, result.WeekStartDate);
        Assert.Equal(Monday.AddDays(6), result.WeekEndDate);
        Assert.Equal(["Pancakes", "Curry", "Lasagna"], result.Items.Select(i => i.Meal.Name));
        Assert.Equal([PancakesId, CurryId, LasagnaId], result.Items.Select(i => i.MealId));
    }

    [Fact]
    public async Task GetWeeklyMealPlanById_MapsItemFields()
    {
        WeeklyMealPlanResponse result = await _service.GetWeeklyMealPlanById(PlanId);

        MealPlanItemResponse first = result.Items[0];
        Assert.Equal(Guid.Parse("b0000000-0000-0000-0000-000000000001"), first.Id);
        Assert.Equal(Monday, first.Date);
        Assert.Equal(0, first.SortOrder);
        Assert.Equal(PancakesId, first.MealId);
        Assert.Equal(PancakesId, first.Meal.Id);
        Assert.Equal(MealType.Breakfast, first.Meal.Type);
    }

    [Fact]
    public async Task GetWeeklyMealPlanById_PlanWithoutItems_ReturnsEmptyItems()
    {
        WeeklyMealPlanResponse result = await _service.GetWeeklyMealPlanById(OtherPlanId);

        Assert.Equal(OtherPlanId, result.Id);
        Assert.Empty(result.Items);
    }

    [Fact]
    public async Task GetWeeklyMealPlanById_NotFound_ThrowsWeeklyMealPlanNotFoundException()
    {
        Guid id = Guid.NewGuid();

        WeeklyMealPlanNotFoundException ex =
            await Assert.ThrowsAsync<WeeklyMealPlanNotFoundException>(() => _service.GetWeeklyMealPlanById(id));

        Assert.Contains(id.ToString(), ex.Message);
    }

    #endregion

    #region GetWeeklyMealPlanByStartDate

    [Fact]
    public async Task GetWeeklyMealPlanByStartDate_ReturnsMatchingPlan()
    {
        WeeklyMealPlanResponse? result = await _service.GetWeeklyMealPlanByStartDate(NextMonday);

        Assert.NotNull(result);
        Assert.Equal(OtherPlanId, result.Id);
        Assert.Equal(NextMonday, result.WeekStartDate);
    }

    [Fact]
    public async Task GetWeeklyMealPlanByStartDate_IncludesSortedItems()
    {
        WeeklyMealPlanResponse? result = await _service.GetWeeklyMealPlanByStartDate(Monday);

        Assert.NotNull(result);
        Assert.Equal(["Pancakes", "Curry", "Lasagna"], result.Items.Select(i => i.Meal.Name));
    }

    [Theory]
    [InlineData(2026, 11, 30)] // a Monday with no plan
    [InlineData(2026, 11, 17)] // a date inside an existing week, but not its start
    public async Task GetWeeklyMealPlanByStartDate_NoPlanStartingOnDate_ReturnsNull(int year, int month, int day)
    {
        WeeklyMealPlanResponse? result = await _service.GetWeeklyMealPlanByStartDate(new DateOnly(year, month, day));

        Assert.Null(result);
    }

    #endregion

    #region CreateWeeklyMealPlan

    [Fact]
    public async Task CreateWeeklyMealPlan_WithoutItems_AddsPlanSavesAndReturnsResponse()
    {
        DateOnly start = Monday.AddDays(14);
        WeeklyMealPlanRequest request = new() { WeekStartDate = start };

        WeeklyMealPlanResponse result = await _service.CreateWeeklyMealPlan(request);

        Assert.Equal(start, result.WeekStartDate);
        Assert.Equal(start.AddDays(6), result.WeekEndDate);
        Assert.Empty(result.Items);

        WeeklyMealPlan added = Assert.Single(_plans, p => p.WeekStartDate == start);
        Assert.Equal(start.AddDays(6), added.WeekEndDate);
        _mockPlans.Verify(x => x.Add(It.IsAny<WeeklyMealPlan>()), Times.Once);
        VerifySaved(Times.Once());
    }

    [Fact]
    public async Task CreateWeeklyMealPlan_WithItems_AddsItemsToPlan()
    {
        SimulateMealIncludeOnSave();
        DateOnly start = Monday.AddDays(14);
        WeeklyMealPlanRequest request = new()
        {
            WeekStartDate = start,
            Items =
            [
                new MealPlanItemRequest { Date = start, SortOrder = 0, MealId = PancakesId },
                new MealPlanItemRequest { Date = start.AddDays(2), SortOrder = 1, MealId = CurryId }
            ]
        };

            await _service.CreateWeeklyMealPlan(request);

        WeeklyMealPlan added = Assert.Single(_plans, p => p.WeekStartDate == start);
        Assert.Collection(
            added.Items.OrderBy(i => i.SortOrder),
            i =>
            {
                Assert.Equal(start, i.Date);
                Assert.Equal(0, i.SortOrder);
                Assert.Equal(PancakesId, i.MealId);
            },
            i =>
            {
                Assert.Equal(start.AddDays(2), i.Date);
                Assert.Equal(1, i.SortOrder);
                Assert.Equal(CurryId, i.MealId);
            });
        VerifySaved(Times.Once());
    }
    
    [Theory]
    [InlineData(2026, 11, 17)] // Tuesday
    [InlineData(2026, 11, 21)] // Saturday
    [InlineData(2026, 11, 22)] // Sunday
    public async Task CreateWeeklyMealPlan_NotMonday_ThrowsAndDoesNotAdd(int year, int month, int day)
    {
        DateOnly start = new(year, month, day);
        WeeklyMealPlanRequest request = new() { WeekStartDate = start };

        InvalidWeekStartDateException ex =
            await Assert.ThrowsAsync<InvalidWeekStartDateException>(() => _service.CreateWeeklyMealPlan(request));

        Assert.Contains(start.ToString(), ex.Message);
        _mockPlans.Verify(x => x.Add(It.IsAny<WeeklyMealPlan>()), Times.Never);
        VerifySaved(Times.Never());
    }

    [Fact]
    public async Task CreateWeeklyMealPlan_PlanAlreadyExistsForWeek_ThrowsAndDoesNotAdd()
    {
        WeeklyMealPlanRequest request = new() { WeekStartDate = Monday };

        WeeklyMealPlanAlreadyExistsException ex =
            await Assert.ThrowsAsync<WeeklyMealPlanAlreadyExistsException>(() =>
                _service.CreateWeeklyMealPlan(request));

        Assert.Contains(Monday.ToString(), ex.Message);
        Assert.Equal(2, _plans.Count);
        _mockPlans.Verify(x => x.Add(It.IsAny<WeeklyMealPlan>()), Times.Never);
        VerifySaved(Times.Never());
    }

    #endregion

    #region UpdateWeeklyMealPlan

    [Fact]
    public async Task UpdateWeeklyMealPlan_ReplacesItemsSavesAndReturnsReloadedPlan()
    {
        SimulateMealIncludeOnSave();
        List<MealPlanItem> originalItems = _plans.Single(p => p.Id == PlanId).Items.ToList();
        List<MealPlanItem>? removed = null;
        _mockItems.Setup(x => x.RemoveRange(It.IsAny<IEnumerable<MealPlanItem>>()))
            .Callback<IEnumerable<MealPlanItem>>(items => removed = items.ToList());

        WeeklyMealPlanRequest request = new()
        {
            WeekStartDate = Monday,
            Items =
            [
                new MealPlanItemRequest { Date = Monday.AddDays(3), SortOrder = 0, MealId = LasagnaId },
                new MealPlanItemRequest { Date = Monday.AddDays(2), SortOrder = 0, MealId = CurryId }
            ]
        };

        WeeklyMealPlanResponse result = await _service.UpdateWeeklyMealPlan(PlanId, request);

        Assert.NotNull(removed);
        Assert.Equal(originalItems, removed);

        WeeklyMealPlan plan = _plans.Single(p => p.Id == PlanId);
        Assert.Equal(2, plan.Items.Count);
        Assert.All(plan.Items, i => Assert.Equal(PlanId, i.WeeklyMealPlanId));
        Assert.DoesNotContain(plan.Items, originalItems.Contains);

        Assert.Equal(PlanId, result.Id);
        Assert.Equal(["Curry", "Lasagna"], result.Items.Select(i => i.Meal.Name));
        Assert.Equal([Monday.AddDays(2), Monday.AddDays(3)], result.Items.Select(i => i.Date));
        VerifySaved(Times.Once());
    }

    [Fact]
    public async Task UpdateWeeklyMealPlan_ChangesWeekStartAndRecalculatesWeekEnd()
    {
        SimulateMealIncludeOnSave();
        DateOnly newStart = Monday.AddDays(28);
        WeeklyMealPlanRequest request = new() { WeekStartDate = newStart };

        WeeklyMealPlanResponse result = await _service.UpdateWeeklyMealPlan(PlanId, request);

        WeeklyMealPlan plan = _plans.Single(p => p.Id == PlanId);
        Assert.Equal(newStart, plan.WeekStartDate);
        Assert.Equal(newStart.AddDays(6), plan.WeekEndDate);
        Assert.Equal(newStart, result.WeekStartDate);
        Assert.Equal(newStart.AddDays(6), result.WeekEndDate);
    }

    [Fact]
    public async Task UpdateWeeklyMealPlan_WithNoItems_ClearsAllItems()
    {
        SimulateMealIncludeOnSave();
        WeeklyMealPlanRequest request = new() { WeekStartDate = Monday };

        WeeklyMealPlanResponse result = await _service.UpdateWeeklyMealPlan(PlanId, request);

        Assert.Empty(_plans.Single(p => p.Id == PlanId).Items);
        Assert.Empty(result.Items);
        _mockItems.Verify(x => x.RemoveRange(It.IsAny<IEnumerable<MealPlanItem>>()), Times.Once);
    }

    [Fact]
    public async Task UpdateWeeklyMealPlan_DoesNotAffectOtherPlans()
    {
        SimulateMealIncludeOnSave();
        WeeklyMealPlanRequest request = new()
        {
            WeekStartDate = Monday,
            Items = [new MealPlanItemRequest { Date = Monday, SortOrder = 0, MealId = CurryId }]
        };

        await _service.UpdateWeeklyMealPlan(PlanId, request);

        WeeklyMealPlan other = _plans.Single(p => p.Id == OtherPlanId);
        Assert.Equal(NextMonday, other.WeekStartDate);
        Assert.Empty(other.Items);
    }

    [Fact]
    public async Task UpdateWeeklyMealPlan_NotFound_ThrowsAndDoesNotSave()
    {
        Guid id = Guid.NewGuid();
        WeeklyMealPlanRequest request = new() { WeekStartDate = Monday };

        WeeklyMealPlanNotFoundException ex =
            await Assert.ThrowsAsync<WeeklyMealPlanNotFoundException>(() =>
                _service.UpdateWeeklyMealPlan(id, request));

        Assert.Contains(id.ToString(), ex.Message);
        _mockItems.Verify(x => x.RemoveRange(It.IsAny<IEnumerable<MealPlanItem>>()), Times.Never);
        VerifySaved(Times.Never());
    }

    #endregion

    #region DeleteWeeklyMealPlan

    [Fact]
    public async Task DeleteWeeklyMealPlan_RemovesPlanAndSaves()
    {
        WeeklyMealPlan plan = _plans.Single(p => p.Id == PlanId);

        await _service.DeleteWeeklyMealPlan(PlanId);

        _mockPlans.Verify(x => x.Remove(plan), Times.Once);
        Assert.DoesNotContain(plan, _plans);
        Assert.Single(_plans);
        VerifySaved(Times.Once());
    }

    [Fact]
    public async Task DeleteWeeklyMealPlan_NotFound_ThrowsAndDoesNotRemove()
    {
        Guid id = Guid.NewGuid();

        WeeklyMealPlanNotFoundException ex =
            await Assert.ThrowsAsync<WeeklyMealPlanNotFoundException>(() => _service.DeleteWeeklyMealPlan(id));

        Assert.Contains(id.ToString(), ex.Message);
        _mockPlans.Verify(x => x.Remove(It.IsAny<WeeklyMealPlan>()), Times.Never);
        Assert.Equal(2, _plans.Count);
        VerifySaved(Times.Never());
    }

    #endregion
}
