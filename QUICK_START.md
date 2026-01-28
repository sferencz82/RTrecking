# Quick Start Guide

## Prerequisites

- Docker and Docker Compose
- .NET 10 SDK (for creating migrations)
- Node.js (optional, for local frontend development)

## First Time Setup

### 1. Create Database Migrations

```bash
cd backend/RTracking.Api
dotnet ef migrations add InitialCreate --output-dir Migrations
cd ../..
```

### 2. Start All Services

```bash
# Build and start
docker-compose up -d --build

# Check logs
docker-compose logs -f
```

### 3. Verify Services

- Frontend: http://localhost:4200
- Backend: http://localhost:5000
- Swagger: http://localhost:5001/swagger

## Daily Development

### Start Services

```bash
docker-compose up -d
```

### Stop Services

```bash
docker-compose down
```

### View Logs

```bash
docker-compose logs -f [service-name]
# Examples:
# docker-compose logs -f backend
# docker-compose logs -f frontend
# docker-compose logs -f mysql
```

## Local Development (Without Docker)

### 1. Start MySQL Only

```bash
docker-compose up mysql -d
```

### 2. Run Backend

```bash
cd backend/RTracking.Api
dotnet run
```

Migrations auto-apply in Development mode.

### 3. Run Frontend

```bash
cd frontend
npm start
```

## Troubleshooting

### Migrations Not Applied

```bash
# Check backend logs
docker-compose logs backend

# Manually apply
docker-compose exec backend dotnet ef database update
```

### Port Already in Use

Update ports in `docker-compose.yml` or stop conflicting services.

### Database Connection Issues

Verify MySQL is running:
```bash
docker-compose ps mysql
docker-compose logs mysql
```

## Next Steps

1. Access frontend at http://localhost:4200
2. Upload a receipt to test the flow
3. Check Swagger at http://localhost:5001/swagger for API docs
