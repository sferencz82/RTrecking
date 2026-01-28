# Docker Setup Guide

## Overview

Complete Docker Compose setup for running the entire application stack locally.

## Services

### MySQL
- **Image**: mysql:8.0
- **Port**: 3306
- **Volume**: `mysql_data` (persistent)
- **Credentials**:
  - Root: `rootpassword`
  - User: `rtracking` / `rtrackingpassword`
  - Database: `RTrackingDb`

### Backend
- **Ports**: 5000 (HTTP), 5001 (HTTPS)
- **Volumes**:
  - `wwwroot/uploads` - Receipt images
  - `tessdata` - OCR language data
  - `Migrations` - Database migrations
- **Auto-migrations**: Enabled in Development mode

### Frontend
- **Port**: 4200
- **Hot reload**: Enabled via volume mount
- **API URL**: `http://localhost:5000`

## Quick Start

```bash
# Start all services
docker-compose up -d

# View logs
docker-compose logs -f

# Stop services
docker-compose down

# Stop and remove volumes (clears database)
docker-compose down -v
```

## Commands

### Start Services

```bash
# Start all services in background
docker-compose up -d

# Start specific service
docker-compose up mysql -d
docker-compose up backend -d
docker-compose up frontend -d
```

### View Logs

```bash
# All services
docker-compose logs -f

# Specific service
docker-compose logs -f backend
docker-compose logs -f frontend
docker-compose logs -f mysql
```

### Rebuild Services

```bash
# Rebuild all
docker-compose build

# Rebuild specific service
docker-compose build backend
docker-compose build frontend

# Rebuild and restart
docker-compose up -d --build
```

### Stop Services

```bash
# Stop services (keeps volumes)
docker-compose stop

# Stop and remove containers
docker-compose down

# Stop, remove containers and volumes
docker-compose down -v
```

## Database Migrations

### Automatic (Development)

Migrations are automatically applied when the backend starts in Development mode.

Check backend logs:
```bash
docker-compose logs backend | grep -i migration
```

### Manual

```bash
# Execute migration command in container
docker-compose exec backend dotnet ef database update
```

## Access Services

- **Frontend**: http://localhost:4200
- **Backend API**: http://localhost:5000
- **Backend Swagger**: http://localhost:5001/swagger
- **MySQL**: localhost:3306

## Database Connection

### From Host Machine

```bash
mysql -h localhost -P 3306 -u rtracking -prtrackingpassword RTrackingDb
```

### From Another Container

```bash
docker-compose exec mysql mysql -u rtracking -prtrackingpassword RTrackingDb
```

## Volumes

### Persistent Data

- **MySQL Data**: `mysql_data` volume (persists database)
- **Uploads**: `./backend/RTracking.Api/wwwroot/uploads` (receipt images)
- **Tessdata**: `./backend/RTracking.Api/tessdata` (OCR language files)

### Development Volumes

- **Frontend Source**: `./frontend` (hot reload)
- **Migrations**: `./backend/RTracking.Api/Migrations` (for creating migrations)

## Environment Variables

### Backend

- `ASPNETCORE_ENVIRONMENT=Development`
- `ConnectionStrings__DefaultConnection` - MySQL connection string

### Frontend

- `API_BASE_URL=http://localhost:5000`

## Troubleshooting

### MySQL Not Starting

```bash
# Check logs
docker-compose logs mysql

# Check if port is in use
netstat -an | grep 3306

# Remove volume and restart
docker-compose down -v
docker-compose up mysql -d
```

### Backend Migration Errors

```bash
# Check backend logs
docker-compose logs backend

# Manually apply migrations
docker-compose exec backend dotnet ef database update

# Rebuild backend
docker-compose build backend
docker-compose up backend -d
```

### Frontend Build Issues

```bash
# Rebuild frontend
docker-compose build frontend

# Check logs
docker-compose logs frontend

# Restart frontend
docker-compose restart frontend
```

### Port Conflicts

If ports are already in use, update `docker-compose.yml`:

```yaml
ports:
  - "3307:3306"  # MySQL
  - "5002:8080"  # Backend HTTP
  - "4201:4200"  # Frontend
```

## Development Workflow

### Making Code Changes

1. **Backend**: Rebuild container
   ```bash
   docker-compose build backend
   docker-compose up backend -d
   ```

2. **Frontend**: Changes hot-reload automatically (volume mount)

3. **Database**: Migrations auto-apply in Development

### Creating New Migrations

```bash
# From host (requires .NET SDK)
cd backend/RTracking.Api
dotnet ef migrations add MigrationName --output-dir Migrations

# Or from container
docker-compose exec backend dotnet ef migrations add MigrationName --output-dir Migrations
```

## Production Considerations

For production:
1. Change `ASPNETCORE_ENVIRONMENT=Production`
2. Remove auto-migration code or use init containers
3. Use production build for frontend
4. Configure proper secrets management
5. Use external MySQL or managed database service
