# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Overview

ASP.NET Core Web API (.NET 10) backend for a personal meal planner, using EF Core with SQL Server. It is built to run locally only: there is no authentication. CORS policy `AllowAngular` in `MealPlanner.Api/Program.cs` allows the Angular frontend at `http://localhost:4200`.

## Commands

```bash
docker-compose up -d                     # Start SQL Server (localhost:1433, sa / YourStrongPassword123!)
dotnet build
dotnet run --project MealPlanner.Api     # or: dotnet watch run --project MealPlanner.Api
dotnet test

# Single test class / method (xUnit v3)
dotnet test --filter "FullyQualifiedName~MealPlanner.Tests.Application.WeeklyMealPlanServiceTests"
dotnet test --filter "FullyQualifiedName~WeeklyMealPlanServiceTests.CreateWeeklyMealPlan"

# EF Core migrations (migrations live in Infrastructure; design-time factory has a hardcoded connection string)
dotnet ef migrations add <Name> --project MealPlanner.Infrastructure
dotnet ef database update --project MealPlanner.Infrastructure
```

OpenAPI is mapped only in Development (`/openapi/v1.json`).

## Architecture

Three projects, with dependencies flowing **Api → Application → Infrastructure**:

- **MealPlanner.Infrastructure**: EF Core entities (`Entities/Meals`, `Entities/MealPlan`), `MealPlannerDbContext`, migrations, and `AddInfrastructure()` DI extension (reads the `MealPlanner` connection string). There is no separate domain layer: entities live here. The old `MealPlanner.Domain` project was removed, and its folder holds only leftover `bin/obj`.
- **MealPlanner.Application**: Services organized by feature folder (`Meals/`, `MealPlan/`). Each feature has an `I*Service` interface, a service, request/response DTOs, and a static `*Mapper` class (manual mapping, no AutoMapper). Services inject `MealPlannerDbContext` directly; there is no repository layer.
- **MealPlanner.Api**: Thin controllers that call services. Services are registered manually in `Program.cs`, so a new service must be added there.

### Error handling

Services throw custom exceptions from `MealPlanner.Application/Exceptions` (e.g. `MealNotFoundException`, `WeeklyMealPlanAlreadyExistsException`) instead of returning error results. `Api/ExceptionHandling/GlobalExceptionHandler.cs` maps each exception type to a `ProblemDetails` status code (400/404/409). Anything not mapped becomes a 500. **When you add a new exception type, add a case to this switch.**

### Domain rules worth knowing

- `Meal` is soft-deleted (`IsDeleted`) and has a global query filter in `OnModelCreating`, so deleted meals are excluded automatically.
- `WeeklyMealPlan.WeekStartDate` must be a Monday. This is checked in the service (`InvalidWeekStartDateException`) and with the `[MustBeMonday]` validation attribute. It is unique (DB index), and `WeekEndDate` = start + 6 days. Items cascade-delete with their plan.
- `MealIngredient` is a join entity with a composite key. Deleting an `Ingredient` is restricted while meals reference it.
- JSON uses string enums and strict number handling (configured for both MVC and minimal HTTP JSON options).

## Tests

`MealPlanner.Tests` uses xUnit v3 and Moq. Folders mirror the layers (`Api/`, `Application/`).
- Service tests mock `MealPlannerDbContext` (its `DbSet` properties are `virtual` for this reason) and use **MockQueryable.Moq** (`BuildMockDbSet()`) to back `DbSet`s with in-memory lists. There is no EF InMemory provider or test database.
- Controller tests mock the `I*Service` interface and assert on the `ActionResult`.
- Tests follow the Arrange / Act / Assert comment style and use fixed GUIDs and dates.

## Code style

From `.editorconfig`: file-scoped namespaces that match the folder, explicit types instead of `var`, braces always, and primary constructors for DI (as used throughout).
