# ============================================================
# reset-db.ps1 — Drop, migrate, and seed the dev database.
# Usage: .\scripts\reset-db.ps1
# ============================================================

$ErrorActionPreference = "Stop"

$api  = "src/ClaudeProjectBackend.Api"
$infra = "src/ClaudeProjectBackend.Infrastructure"

Write-Host "Dropping and recreating database..." -ForegroundColor Yellow
dotnet ef database drop --force -p $infra -s $api

Write-Host "Running migrations..." -ForegroundColor Yellow
dotnet ef database update -p $infra -s $api

Write-Host "Seeding data..." -ForegroundColor Yellow
$connString = (Get-Content "$api/appsettings.json" | ConvertFrom-Json).ConnectionStrings.DefaultConnection
Invoke-Sqlcmd -ConnectionString $connString -InputFile "scripts/seed-data.sql"

Write-Host "Done! Database is ready." -ForegroundColor Green
