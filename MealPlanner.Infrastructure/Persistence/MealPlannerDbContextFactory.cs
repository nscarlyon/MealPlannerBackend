using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace MealPlanner.Infrastructure.Persistence;

public class MealPlannerDbContextFactory : IDesignTimeDbContextFactory<MealPlannerDbContext>
{
    public MealPlannerDbContext CreateDbContext(string[] args)
    {
        DbContextOptionsBuilder<MealPlannerDbContext> optionsBuilder = new();

        optionsBuilder.UseSqlServer(
            "Server=localhost,1433;Database=MealPlanner;User Id=sa;Password=YourStrongPassword123!;TrustServerCertificate=True");

        return new MealPlannerDbContext(optionsBuilder.Options);
    }
}