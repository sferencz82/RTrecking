# Local Run Instructions

## Prerequisites

- Docker and Docker Compose installed
- .NET 8 SDK (for local backend development, optional)
- Node.js and npm (for local frontend development, optional)

## Option 1: Docker Compose (Recommended)

### First Time Setup

```bash
# 1. Create migrations (if not already created)
cd backend/RTracking.Api
dotnet ef migrations add InitialCreate --output-dir Migrations

# 2. Build and start all services
cd ../..
docker-compose up -d --build

# View logs to verify migrations applied
docker-compose logs backend | grep -i migration
```

### Start All Services

```bash
# Start MySQL, backend, and frontend
docker-compose up -d

# View logs
docker-compose logs -f

# Stop all services
docker-compose down

# Stop and remove volumes (clears database)
docker-compose down -v
```

### Access Services

- **Frontend**: http://localhost:4200
- **Backend API**: http://localhost:5000
- **Backend Swagger**: http://localhost:5001/swagger
- **MySQL**: localhost:3306

### Database Connection (from host)

```bash
mysql -h localhost -P 3306 -u rtracking -prtrackingpassword RTrackingDb
```

## Option 2: Local Development (Without Docker)

### 1. Start MySQL

```bash
# Using Docker Compose (MySQL only)
docker-compose up mysql -d

# Or use your local MySQL installation
# Update connection string in appsettings.json
```

### 2. Run Backend

```bash
cd backend/RTracking.Api

# Restore packages
dotnet restore

# Create migration (first time)
dotnet ef migrations add InitialCreate --output-dir Migrations

# Apply migrations
dotnet ef database update

# Or use script:
# Windows: .\apply-migrations.ps1
# Linux/Mac: ./apply-migrations.sh

# Run backend
dotnet run
```

Backend will be available at:
- HTTP: http://localhost:5000
- HTTPS: https://localhost:5001
- Swagger: https://localhost:5001/swagger

**Note**: Migrations are auto-applied in Development mode on startup.

### 3. Run Frontend

```bash
cd frontend

# Install dependencies (first time)
npm install

# Start development server
npm start
```

Frontend will be available at: http://localhost:4200

## Database Setup

### First Time Setup

1. **With Docker Compose**: Database is created automatically
2. **Local MySQL**: Create database manually:
   ```sql
   CREATE DATABASE RTrackingDb CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci;
   ```

### Apply Migrations

**Automatic (Development)**:
- Migrations are auto-applied when backend starts in Development mode

**Manual**:
```bash
cd backend/RTracking.Api
dotnet ef database update

# Or use scripts:
# Windows: .\apply-migrations.ps1
# Linux/Mac: ./apply-migrations.sh
```

## Configuration

### Backend Connection String

**Docker Compose** (automatic):
```
Server=mysql;Port=3306;Database=RTrackingDb;User=rtracking;Password=rtrackingpassword;CharSet=utf8mb4;
```

**Local Development** (update `appsettings.json`):
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=localhost;Port=3306;Database=RTrackingDb;User=root;Password=your_password;CharSet=utf8mb4;"
  }
}
```

### Frontend API URL

**Docker Compose**: Uses `http://localhost:5000` (from environment variable)

**Local Development**: Update `src/environments/environment.ts`:
```typescript
export const environment = {
  production: false,
  apiBaseUrl: 'http://localhost:5000'
};
```

## Docker Compose Services

### MySQL
- **Image**: mysql:8.0
- **Port**: 3306
- **Volume**: `mysql_data` (persistent storage)
- **Credentials**:
  - Root: `rootpassword`
  - User: `rtracking` / `rtrackingpassword`
  - Database: `RTrackingDb`

### Backend
- **Ports**: 5000 (HTTP), 5001 (HTTPS)
- **Volumes**:
  - `wwwroot/uploads` - Receipt images
  - `tessdata` - OCR language data
- **Auto-migrations**: Enabled in Development

### Frontend
- **Port**: 4200
- **Hot reload**: Enabled via volume mount
- **API URL**: Configured via environment variable

## Troubleshooting

### MySQL Connection Issues

```bash
# Check if MySQL is running
docker-compose ps mysql

# View MySQL logs
docker-compose logs mysql

# Restart MySQL
docker-compose restart mysql
```

### Backend Migration Issues

```bash
# Check backend logs
docker-compose logs backend

# Manually apply migrations
docker-compose exec backend dotnet ef database update
```

### Frontend Build Issues

```bash
# Rebuild frontend container
docker-compose build frontend
docker-compose up frontend -d
```

### Port Conflicts

If ports are already in use:

1. **MySQL**: Change `3306:3306` to `3307:3306` in docker-compose.yml
2. **Backend**: Change `5000:8080` to `5002:8080`
3. **Frontend**: Change `4200:4200` to `4201:4200`

Update frontend `environment.ts` API URL accordingly.

## Development Workflow

### With Docker Compose

1. Start services: `docker-compose up -d`
2. Make code changes
3. Backend: Rebuild container or restart
4. Frontend: Changes hot-reload automatically
5. View logs: `docker-compose logs -f [service]`

### Local Development

1. Start MySQL: `docker-compose up mysql -d`
2. Run backend: `cd backend/RTracking.Api && dotnet run`
3. Run frontend: `cd frontend && npm start`
4. Make changes - both hot-reload automatically

## Production Build

### Backend

```bash
cd backend/RTracking.Api
dotnet publish -c Release -o ./publish
```

### Frontend

```bash
cd frontend
ng build --configuration production
```

## Notes

- **Migrations**: Auto-applied only in Development mode
- **Volumes**: Receipt images and tessdata are persisted via volumes
- **Hot Reload**: Frontend has hot reload in Docker via volume mount
- **Database**: MySQL data persists in Docker volume `mysql_data`
