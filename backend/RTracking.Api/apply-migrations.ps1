# PowerShell script to manually apply database migrations

Write-Host "Applying database migrations..." -ForegroundColor Green

dotnet ef database update

if ($LASTEXITCODE -eq 0) {
    Write-Host "Migrations applied successfully!" -ForegroundColor Green
} else {
    Write-Host "Error applying migrations." -ForegroundColor Red
    exit 1
}
