# Meal Planner

Personal Meal Planner Project

## Technologies

- **.NET:** .NET Core
- **Database:** SQL Server
- **ORM:** Entity Framework Core
- **API:** ASP.NET Core Web API

## Prerequisites

Before getting started, ensure you have the following installed:

- [.NET SDK](https://dotnet.microsoft.com/download)
- [Visual Studio](https://visualstudio.microsoft.com/), [JetBrains Rider](https://www.jetbrains.com/rider/), or [Visual Studio Code](https://code.visualstudio.com/)
- SQL Server (if applicable)
- Git

## Getting Started

### 1. Clone the Repository

```bash
git clone <repository-url>
cd <project-directory>
```

### 2. Restore Dependencies

Restore the NuGet packages required by the project.

```bash
dotnet restore
```

### 3. Configure the Application

Update the `appsettings.json` file with the appropriate application settings.

**Note:** Replace the placeholders with your actual settings.

Update the 'AllowAngular' policy in program.cs as necessary.
This project has no security or authentication involved since it's built to run locally. 

### 4. Run Docker
Configure docker-compose.yml as necessary. Currently, the project uses SQL Server as the database. If you want to use a different database, 
you will need to modify the docker-compose.yml file accordingly.

```bash
docker-compose up -d
```

### 5. Apply Database Migrations

If the project uses Entity Framework Core migrations, run:

```bash
dotnet ef database update
```

If the EF Core CLI is not installed:

```bash
dotnet tool install --global dotnet-ef
```

### 6. Run the Application

Start the application using:

```bash
dotnet run
```

Alternatively, use watch mode during development:

```bash
dotnet watch run
```

The application will be available at the URL displayed in the console.

## Building the Application

To build the project:

```bash
dotnet build
```

To build in Release configuration:

```bash
dotnet build --configuration Release
```

## Running Tests

Execute the automated test suite:

```bash
dotnet test
```

To run tests with detailed output:

```bash
dotnet test --verbosity normal
```