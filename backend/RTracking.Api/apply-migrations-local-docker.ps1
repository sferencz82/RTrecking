# PowerShell script to manually apply database migrations
$env:ConnectionStrings__Default="Server=localhost;Port=3307;Database=YOUR_DB;User=YOUR_USER;Password=YOUR_PASSWORD;"
Write-Host "Applying database migrations..." -ForegroundColor Green

dotnet ef database update

if ($LASTEXITCODE -eq 0) {
    Write-Host "Migrations applied successfully!" -ForegroundColor Green
} else {
    Write-Host "Error applying migrations." -ForegroundColor Red
    exit 1
}
