using MealPlanner.Domain.MealPlan;
using MealPlanner.Domain.Meals;

using Microsoft.EntityFrameworkCore;

namespace MealPlanner.Infrastructure.Persistence;

public class MealPlannerDbContext(DbContextOptions<MealPlannerDbContext> options) : DbContext(options)
{
    public virtual DbSet<Meal> Meals => Set<Meal>();
    public virtual DbSet<WeeklyMealPlan> WeeklyMealPlans => Set<WeeklyMealPlan>();
    public virtual DbSet<MealPlanItem> MealPlanItems => Set<MealPlanItem>();
    public virtual DbSet<Ingredient> Ingredients => Set<Ingredient>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Meal>().HasQueryFilter(m => !m.IsDeleted);
        modelBuilder.Entity<WeeklyMealPlan>(entity =>
        {
            entity.HasMany(x => x.Items)
                .WithOne(x => x.WeeklyMealPlan)
                .HasForeignKey(x => x.WeeklyMealPlanId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasIndex(x => x.WeekStartDate)
                .IsUnique();
        });
        modelBuilder.Entity<MealIngredient>(entity =>
        {
            entity.HasKey(x => new { x.MealId, x.IngredientId });

            entity.HasOne(x => x.Meal)
                .WithMany(x => x.Ingredients)
                .HasForeignKey(x => x.MealId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(x => x.Ingredient)
                .WithMany(x => x.MealIngredients)
                .HasForeignKey(x => x.IngredientId)
                .OnDelete(DeleteBehavior.Restrict);
        });
        modelBuilder.Entity<Ingredient>()
            .HasIndex(i => i.Name)
            .IsUnique();

        base.OnModelCreating(modelBuilder);
    }
}