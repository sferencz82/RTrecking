# RTracking Backend API

ASP.NET Core 10 Web API for Receipt Tracking Application.

## Project Setup

The project has been created with the following structure:

```
RTracking.Api/
├── Controllers/
│   └── HealthController.cs
├── Data/
│   └── ApplicationDbContext.cs
├── Models/
│   ├── BaseEntity.cs
│   ├── Receipt.cs
│   ├── LineItem.cs
│   └── Category.cs
├── Migrations/
│   └── .gitkeep
├── Program.cs
├── appsettings.json
├── appsettings.Development.json
└── RTracking.Api.csproj
```

## Commands Used to Create the Project

```bash
# Create the backend directory (if not exists)
mkdir backend

# Create the Web API project
cd backend
dotnet new webapi -n RTracking.Api -f net10.0

# Add required NuGet packages (already added to .csproj)
# - MySql.EntityFrameworkCore (10.0.0)
# - Swashbuckle.AspNetCore (6.6.2)
# - Microsoft.EntityFrameworkCore.Design (10.0.0)
```

## Configuration

### Connection String

Update the MySQL connection string in `appsettings.json` and `appsettings.Development.json`:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=localhost;Port=3306;Database=RTrackingDb;User=root;Password=your_password;CharSet=utf8mb4;"
  }
}
```

### Features Configured

- ✅ Controllers (not minimal APIs)
- ✅ Swagger enabled in Development environment
- ✅ CORS configured for `http://localhost:4200`
- ✅ Health endpoint at `GET /api/health`
- ✅ Entity Framework Core with MySql.EntityFrameworkCore
- ✅ ApplicationDbContext configured
- ✅ Migrations folder created

## Running the Application

```bash
cd backend/RTracking.Api
dotnet restore
dotnet run
```

The API will be available at:
- HTTP: `http://localhost:5000`
- HTTPS: `https://localhost:5001`
- Swagger UI: `https://localhost:5001` (in Development)

## Creating Migrations

To create your first migration (after updating the connection string):

```bash
cd backend/RTracking.Api
dotnet tool restore
dotnet ef migrations add InitialCreate --output-dir Migrations
```

To apply migrations to the database:

```bash
dotnet tool restore
dotnet ef database update
```

If you run migrations from the repo root (or a different directory), specify the project explicitly:

```bash
dotnet tool restore --tool-manifest backend/RTracking.Api/.config/dotnet-tools.json
dotnet ef database update --project backend/RTracking.Api/RTracking.Api.csproj --startup-project backend/RTracking.Api/RTracking.Api.csproj
```

> Note: There is no `RTracking.Data` project in this repository. Use `RTracking.Api.csproj` for both `--project` and `--startup-project`.

### Seed Data

Category seed data is configured in `ApplicationDbContext` and will be applied as part of the EF Core migrations.

## Testing the Health Endpoint

```bash
curl http://localhost:5000/api/health
```

Expected response:
```json
{
  "status": "healthy",
  "timestamp": "2026-01-22T18:00:00Z"
}
```
