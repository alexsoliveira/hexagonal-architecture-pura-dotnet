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

# Step 4b: Restore NuGet Dependencies
Write-Host ""
Write-Host "[STEP 4b] Restoring NuGet Dependencies..." -ForegroundColor Cyan

Pop-Location

Push-Location $RootPath

try {
    Write-Host "Running: dotnet restore" -ForegroundColor Yellow
    dotnet restore | Out-Null
    
    if ($LASTEXITCODE -eq 0) {
        Write-Host "OK: Dependencies restored successfully" -ForegroundColor Green
    } else {
        Write-Host "WARNING: dotnet restore returned code $LASTEXITCODE" -ForegroundColor Yellow
    }
} catch {
    Write-Host "WARNING: Could not restore dependencies: $_" -ForegroundColor Yellow
}

# Step 4c: Apply Database Migrations
Write-Host ""
Write-Host "[STEP 4c] Applying Database Migrations..." -ForegroundColor Cyan

$env:ConnectionStrings__DefaultConnection = "Server=localhost,1433;Database=HexagonalLab;User Id=sa;Password=HexagonalLab@2024!;TrustServerCertificate=true;"

try {
    Write-Host "Running: dotnet ef database update" -ForegroundColor Yellow
    
    dotnet ef database update `
        --project src/HexagonalLab.Infrastructure `
        --startup-project src/HexagonalLab.API `
        --verbose 2>&1 | Where-Object { $_ -match "(migration|Migration|Applied|error|Error)" } | ForEach-Object {
        Write-Host "  $_" -ForegroundColor Gray
    }
    
    if ($LASTEXITCODE -eq 0) {
        Write-Host "OK: Database migrations applied successfully" -ForegroundColor Green
    } else {
        Write-Host "WARNING: Migrations completed with code $LASTEXITCODE" -ForegroundColor Yellow
        Write-Host "INFO: If database is ready, this may be normal" -ForegroundColor Cyan
    }
} catch {
    Write-Host "ERROR: Could not apply migrations: $_" -ForegroundColor Red
    Write-Host "Run manually to diagnose:" -ForegroundColor Yellow
    Write-Host "  cd $RootPath" -ForegroundColor Gray
    Write-Host '  $env:ConnectionStrings__DefaultConnection = "Server=localhost,1433;Database=HexagonalLab;User Id=sa;Password=HexagonalLab@2024!;TrustServerCertificate=true;"' -ForegroundColor Gray
    Write-Host "  dotnet ef database update --project src/HexagonalLab.Infrastructure --startup-project src/HexagonalLab.API --verbose" -ForegroundColor Gray
}

Pop-Location

# Step 5: Display Status
Write-Host ""
Write-Host "[STEP 5] Container Status" -ForegroundColor Cyan
Write-Host ""

$Containers = docker ps --filter "name=hexagonal*" --format "table {{.Names}}\t{{.Status}}\t{{.Ports}}"
Write-Host $Containers

# Wait a bit more for services to be fully ready
Write-Host ""
Write-Host "Waiting for services to fully initialize..." -ForegroundColor Cyan
Start-Sleep -Seconds 3

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
Write-Host "View logs:" -ForegroundColor Yellow
Write-Host "  docker-compose logs -f api              # API logs" -ForegroundColor Gray
Write-Host "  docker-compose logs -f worker           # Worker logs" -ForegroundColor Gray
Write-Host "  docker-compose logs -f sqlserver        # SQL Server logs" -ForegroundColor Gray
Write-Host ""
Write-Host "Manage services:" -ForegroundColor Yellow
Write-Host "  docker-compose stop                     # Stop all services" -ForegroundColor Gray
Write-Host "  docker-compose down                     # Stop and remove containers" -ForegroundColor Gray
Write-Host "  docker-compose down -v                  # Stop and remove everything (including volumes)" -ForegroundColor Gray
Write-Host "  docker-compose up -d --build -Rebuild   # Rebuild images (rebuild flag)" -ForegroundColor Gray
Write-Host ""
Write-Host "Database:" -ForegroundColor Yellow
Write-Host "  docker exec hexagonal-sqlserver bash    # Access SQL Server container" -ForegroundColor Gray
Write-Host ""
Write-Host "Testing:" -ForegroundColor Yellow
Write-Host "  Invoke-WebRequest http://localhost:5000/api/items/ -Method Get  # List items" -ForegroundColor Gray
Write-Host ""
Write-Host ""

Write-Host "=====================================================" -ForegroundColor Cyan
Write-Host "  Environment startup completed successfully!" -ForegroundColor Green
Write-Host "=====================================================" -ForegroundColor Cyan
Write-Host ""
Write-Host "💡 TIP: Check logs with: docker-compose logs -f worker" -ForegroundColor Cyan
Write-Host ""
