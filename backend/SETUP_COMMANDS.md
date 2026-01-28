# Backend Setup - Exact Commands

## Commands to Create the Project (if starting from scratch)

```bash
# Navigate to project root
cd C:\cursor\antygravity\git\RTracking

# Create backend directory
mkdir backend

# Create ASP.NET Core 8 Web API project
cd backend
dotnet new webapi -n RTracking.Api -f net8.0

# Navigate to project directory
cd RTracking.Api

# Add required NuGet packages
dotnet add package Pomelo.EntityFrameworkCore.MySql --version 8.0.0
dotnet add package Swashbuckle.AspNetCore
dotnet add package Microsoft.EntityFrameworkCore.Design

# Restore packages
dotnet restore

# Create Migrations folder (if not exists)
mkdir Migrations
```

## Project Structure Created

```
backend/RTracking.Api/
├── Controllers/
│   └── HealthController.cs          # Health check endpoint
├── Data/
│   └── ApplicationDbContext.cs     # EF Core DbContext
├── Models/
│   ├── BaseEntity.cs               # Base entity with common fields
│   ├── Receipt.cs                  # Receipt entity
│   ├── LineItem.cs                 # LineItem entity
│   └── Category.cs                 # Category entity
├── Migrations/                      # EF Core migrations folder
│   └── .gitkeep
├── Program.cs                       # Application entry point
├── appsettings.json                 # Configuration (with MySQL connection string)
├── appsettings.Development.json     # Development configuration
└── RTracking.Api.csproj            # Project file
```

## Key Features Implemented

✅ **Controllers** - Using controller-based API (not minimal APIs)  
✅ **Swagger** - Enabled in Development environment at `/swagger`  
✅ **CORS** - Configured for `http://localhost:4200`  
✅ **Health Endpoint** - `GET /api/health`  
✅ **EF Core** - Configured with Pomelo.EntityFrameworkCore.MySql  
✅ **DbContext** - ApplicationDbContext with all entities  
✅ **Migrations Folder** - Ready for EF Core migrations  

## Next Steps

1. **Update Connection String** in `appsettings.json` and `appsettings.Development.json`
2. **Create Initial Migration**:
   ```bash
   cd backend/RTracking.Api
   dotnet ef migrations add InitialCreate --output-dir Migrations
   ```
3. **Apply Migration** (when ready):
   ```bash
   dotnet ef database update
   ```
4. **Run the Application**:
   ```bash
   dotnet run
   ```

## Testing

- **Health Endpoint**: `GET http://localhost:5000/api/health`
- **Swagger UI**: `https://localhost:5001/swagger` (Development only)
