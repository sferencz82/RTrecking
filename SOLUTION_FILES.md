# Visual Studio Solution Files

## Overview

This repository includes Visual Studio solution files for easy development in Visual Studio, Visual Studio Code, or JetBrains Rider.

## Solution Files

### 1. Root Solution: `RTracking.sln`

**Location**: Repository root  
**Contains**: All backend projects organized in solution folders

```
RTracking.sln
└── backend/
    └── RTracking.Api (ASP.NET Core Web API)
```

**Use this for**: Full-stack development with Visual Studio

**Open**:
```bash
# Windows
start RTracking.sln

# Or double-click in File Explorer
```

### 2. Backend Solution: `backend/RTracking.sln`

**Location**: `backend/` folder  
**Contains**: Backend projects only

```
backend/RTracking.sln
└── RTracking.Api (ASP.NET Core Web API)
```

**Use this for**: Backend-only development

**Open**:
```bash
# Windows
start backend\RTracking.sln

# Or double-click in File Explorer
```

## Quick Start

### Visual Studio

1. Open `RTracking.sln`
2. Set `RTracking.Api` as startup project
3. Press F5 to run

### JetBrains Rider

1. Open `RTracking.sln`
2. Rider will auto-detect configuration
3. Run or Debug as usual

### Visual Studio Code

For backend:
```bash
cd backend/RTracking.Api
code .
```

For frontend:
```bash
cd frontend
code .
```

## Building

### Visual Studio
- Build: `Ctrl+Shift+B`
- Rebuild: Right-click solution → Rebuild

### Command Line
```bash
# Build root solution
dotnet build RTracking.sln

# Build backend solution
dotnet build backend/RTracking.sln

# Build specific project
dotnet build backend/RTracking.Api/RTracking.Api.csproj
```

## Running

### Visual Studio
- Debug: `F5`
- Run without debugging: `Ctrl+F5`

### Command Line
```bash
# Run backend
dotnet run --project backend/RTracking.Api

# Or navigate to project
cd backend/RTracking.Api
dotnet run
```

## Project Details

### RTracking.Api

- **Type**: ASP.NET Core 8 Web API
- **Framework**: .NET 8.0
- **Database**: MySQL (via Pomelo EF Core provider)
- **Features**:
  - RESTful API with controllers
  - Entity Framework Core
  - Swagger/OpenAPI documentation
  - Tesseract OCR integration
  - File storage for receipt images

**Key Dependencies**:
- Pomelo.EntityFrameworkCore.MySql
- Microsoft.EntityFrameworkCore.Design
- Tesseract
- Swashbuckle.AspNetCore

## Configuration

### Connection String

Update in `appsettings.json` or `appsettings.Development.json`:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=localhost;Port=3306;Database=RTrackingDb;User=root;Password=your_password;CharSet=utf8mb4;"
  }
}
```

### Launch Settings

Configured in `Properties/launchSettings.json`:
- HTTP: http://localhost:5000
- HTTPS: https://localhost:5001
- Swagger: https://localhost:5001/swagger

## Database Migrations

### Visual Studio Package Manager Console

```powershell
# Create migration
Add-Migration MigrationName -OutputDir Migrations

# Apply migration
Update-Database

# Remove last migration
Remove-Migration
```

### Command Line

```bash
cd backend/RTracking.Api

# Create migration
dotnet ef migrations add MigrationName --output-dir Migrations

# Apply migration
dotnet ef database update

# Remove last migration
dotnet ef migrations remove
```

### PowerShell Script

```powershell
.\backend\RTracking.Api\apply-migrations.ps1
```

### Bash Script

```bash
./backend/RTracking.Api/apply-migrations.sh
```

## Frontend (Angular)

The Angular frontend is **not** included in the .NET solution files.

### Development

```bash
cd frontend
npm install
npm start
```

Access at: http://localhost:4200

### IDE

Use Visual Studio Code or WebStorm:
```bash
cd frontend
code .  # VS Code
# or
webstorm .  # WebStorm
```

## Docker

The solution can also be run with Docker Compose:

```bash
# Start all services
docker-compose up -d

# Build and start
docker-compose up -d --build
```

See [DOCKER_SETUP.md](./DOCKER_SETUP.md) for details.

## Troubleshooting

### Build Errors

```bash
# Clean and rebuild
dotnet clean
dotnet restore
dotnet build
```

### NuGet Package Issues

```bash
# Restore packages
dotnet restore

# Clear NuGet cache
dotnet nuget locals all --clear
dotnet restore
```

### Migration Issues

```bash
# Check migrations
dotnet ef migrations list

# Reset database (WARNING: deletes data)
dotnet ef database drop
dotnet ef database update
```

## Documentation

- **[VISUAL_STUDIO_SETUP.md](./VISUAL_STUDIO_SETUP.md)** - Detailed Visual Studio setup
- **[LOCAL_RUN_INSTRUCTIONS.md](./LOCAL_RUN_INSTRUCTIONS.md)** - Local development guide
- **[DOCKER_SETUP.md](./DOCKER_SETUP.md)** - Docker configuration
- **[ARCHITECTURE.md](./ARCHITECTURE.md)** - Architecture overview

## Next Steps

1. Open `RTracking.sln` in Visual Studio
2. Build the solution (`Ctrl+Shift+B`)
3. Start MySQL (Docker or local)
4. Run the project (`F5`)
5. Access Swagger at https://localhost:5001/swagger
6. Start frontend: `cd frontend && npm start`
7. Access app at http://localhost:4200
