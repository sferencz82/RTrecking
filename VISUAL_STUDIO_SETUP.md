# Visual Studio Setup Guide

## Solution Files

This repository contains Visual Studio solution files for easy development in Visual Studio or JetBrains Rider.

### Root Solution: `RTracking.sln`

Located at the repository root, this solution contains all backend projects:

- **RTracking.Api** - ASP.NET Core Web API

### Backend Solution: `backend/RTracking.sln`

Located in the `backend` folder, this is an alternative solution focused only on backend development.

## Opening in Visual Studio

### Option 1: Root Solution (Recommended)

```bash
# Open from command line
start RTracking.sln

# Or double-click RTracking.sln in File Explorer
```

### Option 2: Backend Solution Only

```bash
# Open from command line
start backend\RTracking.sln

# Or double-click backend\RTracking.sln in File Explorer
```

## Visual Studio Configuration

### 1. Set Startup Project

Right-click on `RTracking.Api` in Solution Explorer → **Set as Startup Project**

### 2. Configure Launch Settings

The project includes `launchSettings.json` with:
- HTTP: http://localhost:5000
- HTTPS: https://localhost:5001
- Swagger: https://localhost:5001/swagger

### 3. Database Connection

Update `appsettings.json` or `appsettings.Development.json`:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=localhost;Port=3306;Database=RTrackingDb;User=root;Password=your_password;CharSet=utf8mb4;"
  }
}
```

## Building and Running

### Build Solution

- **Visual Studio**: `Ctrl+Shift+B` or Build → Build Solution
- **Command Line**: `dotnet build`

### Run Project

- **Visual Studio**: `F5` (Debug) or `Ctrl+F5` (Run without debugging)
- **Command Line**: `dotnet run --project backend/RTracking.Api`

### Run with Docker

See [LOCAL_RUN_INSTRUCTIONS.md](./LOCAL_RUN_INSTRUCTIONS.md) for Docker setup.

## Database Migrations

### Create Migration

```bash
# From Visual Studio Package Manager Console
Add-Migration InitialCreate -OutputDir Migrations

# Or from terminal
cd backend/RTracking.Api
dotnet ef migrations add InitialCreate --output-dir Migrations
```

### Apply Migration

```bash
# From Visual Studio Package Manager Console
Update-Database

# Or from terminal
cd backend/RTracking.Api
dotnet ef database update

# Or use PowerShell script
.\backend\RTracking.Api\apply-migrations.ps1
```

### Auto-Apply Migrations

Migrations are automatically applied when running in Development mode (see `Program.cs`).

## NuGet Packages

The solution uses the following key packages:

- **MySql.EntityFrameworkCore** - MySQL provider for EF Core
- **Microsoft.EntityFrameworkCore.Design** - EF Core tools
- **Tesseract** - OCR functionality
- **Swashbuckle.AspNetCore** - Swagger/OpenAPI

### Restore Packages

- **Visual Studio**: Right-click solution → Restore NuGet Packages
- **Command Line**: `dotnet restore`

## Frontend Development

The Angular frontend is in the `frontend` folder and is not part of the .NET solution.

### Open Frontend in VS Code

```bash
cd frontend
code .
```

### Run Frontend

```bash
cd frontend
npm install
npm start
```

Frontend will be available at: http://localhost:4200

## Solution Structure

```
RTracking/
├── RTracking.sln                    # Root solution
├── backend/
│   ├── RTracking.sln                # Backend-only solution
│   └── RTracking.Api/
│       ├── RTracking.Api.csproj     # Web API project
│       ├── Controllers/
│       ├── Models/
│       ├── Services/
│       ├── Data/
│       └── DTOs/
└── frontend/
    ├── package.json
    └── src/
        └── app/
```

## Debugging

### Debug Backend

1. Set breakpoints in Visual Studio
2. Press `F5` to start debugging
3. Use Swagger UI at https://localhost:5001/swagger to test endpoints

### Debug with Frontend

1. Start backend in Visual Studio (F5)
2. Start frontend in separate terminal: `cd frontend && npm start`
3. Access application at http://localhost:4200

### Debug Docker

See [DOCKER_SETUP.md](./DOCKER_SETUP.md) for Docker debugging.

## Common Tasks

### Add New Controller

1. Right-click `Controllers` folder → Add → Controller
2. Choose "API Controller - Empty"
3. Name it (e.g., `MyController.cs`)

### Add New Service

1. Create interface in `Services/IMyService.cs`
2. Create implementation in `Services/MyService.cs`
3. Register in `Program.cs`:
   ```csharp
   builder.Services.AddScoped<IMyService, MyService>();
   ```

### Add New Model

1. Create class in `Models/MyModel.cs`
2. Add `DbSet` to `ApplicationDbContext.cs`
3. Create migration: `Add-Migration AddMyModel`
4. Update database: `Update-Database`

## Troubleshooting

### Build Errors

```bash
# Clean solution
dotnet clean

# Restore packages
dotnet restore

# Rebuild
dotnet build
```

### Database Connection Issues

1. Verify MySQL is running
2. Check connection string in `appsettings.json`
3. Test connection: `mysql -h localhost -P 3306 -u root -p`

### Migration Issues

```bash
# Remove last migration
dotnet ef migrations remove

# Reset database (WARNING: deletes data)
dotnet ef database drop
dotnet ef database update
```

## Visual Studio Extensions (Recommended)

- **ReSharper** or **Rider** - Enhanced C# development
- **Web Essentials** - Web development tools
- **GitLens** - Git integration
- **Docker Tools** - Docker integration

## JetBrains Rider

The solution also works with JetBrains Rider:

1. Open `RTracking.sln`
2. Rider will automatically detect the solution structure
3. Configure run configuration for `RTracking.Api`
4. Run or Debug as usual

## Next Steps

1. Open `RTracking.sln` in Visual Studio
2. Build the solution
3. Start MySQL (Docker or local)
4. Run the project (F5)
5. Access Swagger at https://localhost:5001/swagger
