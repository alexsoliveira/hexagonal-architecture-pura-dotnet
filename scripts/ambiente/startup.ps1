# HexagonalLab Docker Startup Script
# Purpose: Start all services (SQL Server, API, Worker)
# Usage: ./startup.ps1 or ./startup.ps1 -Rebuild

param([switch]$Rebuild)

Write-Host ""
Write-Host "=====================================================" -ForegroundColor Cyan
Write-Host "  HexagonalLab Docker Environment Startup" -ForegroundColor Cyan
Write-Host "=====================================================" -ForegroundColor Cyan
Write-Host ""

# Step 1: Validate Docker
Write-Host "[STEP 1] Validating Prerequisites..." -ForegroundColor Cyan

if (-not (Get-Command docker -ErrorAction SilentlyContinue)) {
    Write-Host "ERROR: Docker not found" -ForegroundColor Red
    exit 1
}

try {
    docker info > $null 2>&1
} catch {
    Write-Host "ERROR: Docker daemon not running" -ForegroundColor Red
    exit 1
}

$DockerCompose = if (Get-Command docker-compose -ErrorAction SilentlyContinue) { "docker-compose" } else { "docker compose" }
Write-Host "OK: Docker and Docker Compose available" -ForegroundColor Green

# Step 2: Navigate to root
Write-Host ""
Write-Host "[STEP 2] Finding docker-compose.yml..." -ForegroundColor Cyan

$RootPath = Split-Path -Parent (Split-Path -Parent $PSScriptRoot)
if (-not (Test-Path "$RootPath/docker-compose.yml")) {
    Write-Host "ERROR: docker-compose.yml not found at $RootPath" -ForegroundColor Red
    exit 1
}
Write-Host "OK: Found docker-compose.yml" -ForegroundColor Green

# Step 3: Start services
Write-Host ""
Write-Host "[STEP 3] Starting Docker Services..." -ForegroundColor Cyan

Push-Location $RootPath

if ($Rebuild) {
    Write-Host "Running: $DockerCompose up --build -d" -ForegroundColor Yellow
    Write-Host ""
    & $DockerCompose up --build -d
} else {
    Write-Host "Running: $DockerCompose up -d" -ForegroundColor Yellow
    Write-Host ""
    & $DockerCompose up -d
}

if ($LASTEXITCODE -ne 0) {
    Write-Host "ERROR: Docker Compose failed" -ForegroundColor Red
    Pop-Location
    exit 1
}

Write-Host ""
Write-Host "OK: Services started" -ForegroundColor Green

# Step 4: Wait for SQL Server
Write-Host ""
Write-Host "[STEP 4] Waiting for SQL Server to be healthy..." -ForegroundColor Cyan

$Attempts = 0
$MaxAttempts = 12

while ($Attempts -lt $MaxAttempts) {
    $Status = docker inspect hexagonal-sqlserver --format='{{.State.Health.Status}}' 2>$null
    if ($Status -eq "healthy") {
        Write-Host "OK: SQL Server is healthy" -ForegroundColor Green
        break
    }
    Write-Host "  Attempt $($Attempts + 1)/$MaxAttempts - Status: $Status" -ForegroundColor Gray
    Start-Sleep 5
    $Attempts++
}

if ($Status -ne "healthy") {
    Write-Host "WARNING: SQL Server status inconclusive (may still be starting)" -ForegroundColor Yellow
}

Pop-Location

# Step 5: Display Status
Write-Host ""
Write-Host "[STEP 5] Container Status" -ForegroundColor Cyan
Write-Host ""

docker ps --filter "name=hexagonal*" --format "table {{.Names}}\t{{.Status}}\t{{.Ports}}"

# Step 6: Display URLs
Write-Host ""
Write-Host "[STEP 6] Service Access Information" -ForegroundColor Cyan
Write-Host ""
Write-Host "API Service:" -ForegroundColor Yellow
Write-Host "  URL:     http://localhost:5000" -ForegroundColor Green
Write-Host "  Swagger: http://localhost:5000/swagger" -ForegroundColor Green
Write-Host ""
Write-Host "SQL Server:" -ForegroundColor Yellow
Write-Host "  Server:   localhost,1433" -ForegroundColor Green
Write-Host "  User:     sa" -ForegroundColor Green
Write-Host "  Database: HexagonalLab" -ForegroundColor Green
Write-Host ""
Write-Host "Worker Service:" -ForegroundColor Yellow
Write-Host "  Status: Running in background" -ForegroundColor Green
Write-Host ""

# Step 7: Useful Commands
Write-Host "[STEP 7] Useful Commands" -ForegroundColor Cyan
Write-Host ""
Write-Host "View logs:        docker-compose logs -f api" -ForegroundColor Gray
Write-Host "Stop services:    docker-compose down" -ForegroundColor Gray
Write-Host "Stop & cleanup:   docker-compose down -v" -ForegroundColor Gray
Write-Host ""

Write-Host "=====================================================" -ForegroundColor Cyan
Write-Host "  Environment startup completed successfully!" -ForegroundColor Green
Write-Host "=====================================================" -ForegroundColor Cyan
Write-Host ""
