# PowerShell script to manually apply database migrations
$env:ConnectionStrings__Default="Server=localhost;Port=3307;Database=RTrackingDb;User=rtracking;Password=rtrackingpassword;CharSet=utf8mb4;"
Write-Host "Applying database migrations..." -ForegroundColor Green
$projectPath = Join-Path $PSScriptRoot "RTracking.Api.csproj"
dotnet tool restore
dotnet ef database update --project $projectPath --startup-project $projectPath

if ($LASTEXITCODE -eq 0) {
    Write-Host "Migrations applied successfully!" -ForegroundColor Green
} else {
    Write-Host "Error applying migrations." -ForegroundColor Red
    exit 1
}
