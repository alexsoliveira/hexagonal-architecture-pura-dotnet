# ──────────────────────────────────────────────────────────────
# Docker Management Script for HexagonalLab
# PowerShell helper for Windows users
# ──────────────────────────────────────────────────────────────

param(
    [Parameter(Position = 0)]
    [ValidateSet('start', 'stop', 'restart', 'logs', 'ps', 'clean', 'test-db', 'rebuild')]
    [string]$Command = 'ps'
)

# ──────────────────────────────────────────────────────────────
# Configuration
# ──────────────────────────────────────────────────────────────
$RootPath = Split-Path $PSScriptRoot -Parent
$EnvFile = Join-Path $RootPath ".env"
$LogFile = Join-Path $RootPath "logs\docker-helper.log"

# Create logs directory if it doesn't exist
$LogDir = Split-Path $LogFile -Parent
if (-not (Test-Path $LogDir)) {
    New-Item -ItemType Directory -Path $LogDir -Force | Out-Null
}

function Write-Header {
    param([string]$Text)
    Write-Host "`n═══════════════════════════════════════════════════" -ForegroundColor Cyan
    Write-Host "  $Text" -ForegroundColor Yellow
    Write-Host "═══════════════════════════════════════════════════`n" -ForegroundColor Cyan
}

function Write-Log {
    param(
        [string]$Message,
        [ValidateSet('INFO', 'WARN', 'ERROR', 'SUCCESS')]
        [string]$Level = 'INFO'
    )
    $Timestamp = Get-Date -Format "yyyy-MM-dd HH:mm:ss"
    $LogMessage = "[$Timestamp] [$Level] $Message"
    Add-Content -Path $LogFile -Value $LogMessage -ErrorAction SilentlyContinue
}

function Get-EnvVariable {
    param(
        [string]$VariableName,
        [string]$DefaultValue = ""
    )
    
    # First check if environment variable is set
    $EnvVar = [Environment]::GetEnvironmentVariable($VariableName)
    if ($EnvVar) {
        return $EnvVar
    }
    
    # Then check .env file
    if (Test-Path $EnvFile) {
        $EnvContent = Get-Content $EnvFile
        $Match = $EnvContent | Select-String "^$VariableName=(.+)" | Select-Object -First 1
        if ($Match) {
            return $Match.Matches.Groups[1].Value
        }
    }
    
    # Return default value
    return $DefaultValue
}

function Validate-Prerequisites {
    Write-Header "Validating Prerequisites"
    
    $IsValid = $true
    
    # Check Docker
    if (-not (Get-Command docker -ErrorAction SilentlyContinue)) {
        Write-Host "❌ Docker is not installed or not in PATH" -ForegroundColor Red
        Write-Log "Docker not found" ERROR
        $IsValid = $false
    } else {
        Write-Host "✅ Docker found" -ForegroundColor Green
    }
    
    # Check docker-compose
    if (-not (Get-Command docker-compose -ErrorAction SilentlyContinue)) {
        Write-Host "❌ docker-compose is not installed or not in PATH" -ForegroundColor Red
        Write-Log "docker-compose not found" ERROR
        $IsValid = $false
    } else {
        Write-Host "✅ docker-compose found" -ForegroundColor Green
    }
    
    # Check .env exists
    if (-not (Test-Path $EnvFile)) {
        Write-Host "❌ .env not found at $EnvFile" -ForegroundColor Red
        Write-Log ".env not found" ERROR
        $IsValid = $false
    } else {
        Write-Host "✅ .env configuration found" -ForegroundColor Green
    }
    
    # Check docker service is running
    try {
        docker ps > $null 2>&1
        Write-Host "✅ Docker daemon is running" -ForegroundColor Green
    } catch {
        Write-Host "❌ Docker daemon is not running" -ForegroundColor Red
        Write-Log "Docker daemon not accessible" ERROR
        $IsValid = $false
    }
    
    if (-not $IsValid) {
        Write-Host "`n❌ Prerequisites validation failed" -ForegroundColor Red
        Write-Log "Prerequisites validation failed" ERROR
        exit 1
    }
    
    Write-Host "`n✅ All prerequisites validated" -ForegroundColor Green
    Write-Log "Prerequisites validation successful" SUCCESS
}

function Start-Containers {
    Write-Header "Starting HexagonalLab Docker Containers"
    
    Validate-Prerequisites
    
    try {
        if (Test-Path $EnvFile) {
            Write-Host "Using configuration from: $EnvFile" -ForegroundColor Cyan
            docker-compose --env-file $EnvFile up -d
        }
        else {
            Write-Host "⚠️  .env not found, using docker-compose.yml defaults" -ForegroundColor Yellow
            docker-compose up -d
        }
        
        if ($LASTEXITCODE -ne 0) {
            Write-Host "❌ Failed to start containers" -ForegroundColor Red
            Write-Log "Failed to start containers" ERROR
            exit 1
        }
        
        Write-Host "✅ Containers starting... waiting for services to be ready" -ForegroundColor Green
        Write-Log "Containers started successfully" SUCCESS
        
        # Wait for containers to be ready with polling
        Wait-ForContainers
        
        Show-ContainerStatus
    }
    catch {
        Write-Host "❌ Error starting containers: $_" -ForegroundColor Red
        Write-Log "Error starting containers: $_" ERROR
        exit 1
    }
}

function Wait-ForContainers {
    Write-Host "`nWaiting for containers to be ready..." -ForegroundColor Cyan
    
    $MaxWaitTime = 120  # seconds
    $ElapsedTime = 0
    $CheckInterval = 5  # seconds
    
    while ($ElapsedTime -lt $MaxWaitTime) {
        try {
            $Status = docker-compose ps --services --filter "status=running"
            $AllRunning = @(docker-compose ps --services) | Where-Object { $_ }
            $RunningCount = @($Status) | Measure-Object -Line | Select-Object -ExpandProperty Lines
            
            Write-Host "  [$(Get-Date -Format 'HH:mm:ss')] Containers running: $RunningCount/3" -ForegroundColor Yellow
            
            if ($RunningCount -ge 3) {
                Write-Host "✅ All containers are running" -ForegroundColor Green
                Start-Sleep -Seconds 2  # Extra buffer for services to stabilize
                return
            }
        }
        catch {
            # Silently continue checking
        }
        
        Start-Sleep -Seconds $CheckInterval
        $ElapsedTime += $CheckInterval
    }
    
    Write-Host "⚠️  Timeout waiting for containers (waited for $MaxWaitTime seconds)" -ForegroundColor Yellow
    Write-Log "Timeout waiting for containers" WARN
}

function Stop-Containers {
    Write-Header "Stopping HexagonalLab Docker Containers"
    
    try {
        docker-compose down
        
        if ($LASTEXITCODE -eq 0) {
            Write-Host "✅ Containers stopped" -ForegroundColor Green
            Write-Log "Containers stopped successfully" SUCCESS
        }
        else {
            Write-Host "❌ Failed to stop containers" -ForegroundColor Red
            Write-Log "Failed to stop containers" ERROR
        }
    }
    catch {
        Write-Host "❌ Error stopping containers: $_" -ForegroundColor Red
        Write-Log "Error stopping containers: $_" ERROR
    }
}

function Restart-Containers {
    Write-Header "Restarting HexagonalLab Docker Containers"
    
    Stop-Containers
    Start-Sleep -Seconds 2
    Start-Containers
}

function Show-Logs {
    Write-Header "Docker Compose Logs"
    Write-Host "Showing live logs. Press Ctrl+C to exit.`n" -ForegroundColor Yellow
    Write-Log "Logs display started" INFO
    
    docker-compose logs -f
}

function Show-ContainerStatus {
    Write-Header "Container Status"
    
    try {
        docker-compose ps
        Write-Log "Container status displayed" INFO
    }
    catch {
        Write-Host "❌ Error getting container status: $_" -ForegroundColor Red
        Write-Log "Error getting container status: $_" ERROR
    }
}

function Clean-Resources {
    Write-Header "Cleaning Docker Resources"
    
    # Confirm before destructive operation
    $Confirmation = Read-Host "⚠️  This will remove containers, networks, and volumes. Continue? (yes/no)"
    
    if ($Confirmation -ne "yes") {
        Write-Host "❌ Operation cancelled" -ForegroundColor Yellow
        Write-Log "Clean operation cancelled by user" WARN
        return
    }
    
    try {
        Write-Host "Stopping containers..." -ForegroundColor Yellow
        docker-compose down -v
        
        if ($LASTEXITCODE -ne 0) {
            Write-Host "❌ Failed to stop containers" -ForegroundColor Red
            Write-Log "Failed to stop containers" ERROR
            return
        }
        
        Write-Host "Removing unused Docker resources..." -ForegroundColor Yellow
        docker system prune -f --volumes
        
        Write-Host "✅ Cleanup complete" -ForegroundColor Green
        Write-Log "Cleanup completed successfully" SUCCESS
    }
    catch {
        Write-Host "❌ Error during cleanup: $_" -ForegroundColor Red
        Write-Log "Error during cleanup: $_" ERROR
    }
}

function Test-DatabaseConnection {
    Write-Header "Testing SQL Server Connection"
    
    # Load credentials from .env file
    $DbPassword = Get-EnvVariable "DB_PASSWORD" "HexagonalLab@2024!"
    $DbUser = Get-EnvVariable "DB_USER" "sa"
    
    Write-Host "Testing connection as: $DbUser@hexagonal-sqlserver:1433" -ForegroundColor Cyan
    Write-Log "Testing DB connection as $DbUser" INFO
    
    try {
        $result = docker exec hexagonal-sqlserver /opt/mssql-tools18/bin/sqlcmd `
            -S localhost `
            -U $DbUser `
            -P $DbPassword `
            -C `
            -Q "SELECT @@VERSION" 2>&1
        
        if ($LASTEXITCODE -eq 0) {
            Write-Host "✅ SQL Server is running and accessible" -ForegroundColor Green
            Write-Host "`nVersion:`n$result" -ForegroundColor White
            Write-Log "SQL Server connection test successful" SUCCESS
        }
        else {
            Write-Host "❌ Failed to connect to SQL Server" -ForegroundColor Red
            Write-Host "Output: $result" -ForegroundColor Yellow
            Write-Log "SQL Server connection failed: $result" ERROR
        }
    }
    catch {
        Write-Host "❌ Error testing SQL Server connection: $_" -ForegroundColor Red
        Write-Log "Error testing connection: $_" ERROR
    }
}

function Rebuild-Images {
    Write-Header "Rebuilding Docker Images"
    
    Validate-Prerequisites
    
    try {
        Write-Host "Building images without cache..." -ForegroundColor Yellow
        docker-compose build --no-cache
        
        if ($LASTEXITCODE -ne 0) {
            Write-Host "❌ Failed to build images" -ForegroundColor Red
            Write-Log "Failed to build images" ERROR
            return
        }
        
        Write-Host "✅ Images rebuilt successfully" -ForegroundColor Green
        Write-Log "Images rebuilt successfully" SUCCESS
        
        Write-Host "`nStarting containers..." -ForegroundColor Yellow
        Start-Containers
    }
    catch {
        Write-Host "❌ Error rebuilding images: $_" -ForegroundColor Red
        Write-Log "Error rebuilding images: $_" ERROR
    }
}

# ──────────────────────────────────────────────────────────────
# Execute requested command
# ──────────────────────────────────────────────────────────────

switch ($Command) {
    'start'     { Start-Containers }
    'stop'      { Stop-Containers }
    'restart'   { Restart-Containers }
    'logs'      { Show-Logs }
    'ps'        { Show-ContainerStatus }
    'clean'     { Clean-Resources }
    'test-db'   { Test-DatabaseConnection }
    'rebuild'   { Rebuild-Images }
    default     { Show-ContainerStatus }
}

Write-Log "Command '$Command' executed" INFO
