# PowerShell script to manually apply database migrations

Write-Host "Applying database migrations..." -ForegroundColor Green

dotnet tool restore
dotnet ef database update --project .\RTracking.Api.csproj --startup-project .\RTracking.Api.csproj

if ($LASTEXITCODE -eq 0) {
    Write-Host "Migrations applied successfully!" -ForegroundColor Green
} else {
    Write-Host "Error applying migrations." -ForegroundColor Red
    exit 1
}
