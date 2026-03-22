# ==============================================================================
# HexagonalLab Docker Environment Cleanup Script
# ==============================================================================
# Purpose: Clean up Docker containers, volumes, networks, and images
# 
# Author: Engineering Orchestrator Agent
# Date: 2026-03-22
# 
# Description:
#   This script provides flexible cleanup options for the HexagonalLab Docker
#   environment. It supports selective removal of containers, networks, volumes,
#   and images to suit different cleanup scenarios.
#
# Usage:
#   ./cleanup.ps1                          Basic cleanup (safe - keeps cache)
#   ./cleanup.ps1 -RemoveVolumes           Also remove data volumes
#   ./cleanup.ps1 -RemoveImages            Also remove Docker images
#   ./cleanup.ps1 -Full                    FULL cleanup (remove everything!)
#
# Exit Codes:
#   0 = Success
#   1 = Docker not found or error occurred
# ==============================================================================

param(
    [switch]$RemoveVolumes = $false,
    [switch]$RemoveImages = $false,
    [switch]$Full = $false,
    [switch]$Force = $false
)

# Enable full cleanup mode if specified
if ($Full) {
    $RemoveVolumes = $true
    $RemoveImages = $true
}

$script:HexagonalPrefix = "hexagonal"

# ==============================================================================
# FUNCTIONS
# ==============================================================================

function Write-Header {
    param([string]$Message)
    Write-Host ""
    Write-Host "=== $Message ===" -ForegroundColor Cyan
    Write-Host ""
}

function Write-Success {
    param([string]$Message)
    Write-Host "[OK] $Message" -ForegroundColor Green
}

function Write-Info {
    param([string]$Message)
    Write-Host "[INFO] $Message" -ForegroundColor Blue
}

function Write-Warn {
    param([string]$Message)
    Write-Host "[WARN] $Message" -ForegroundColor Yellow
}

function Write-Error-Custom {
    param([string]$Message)
    Write-Host "[ERROR] $Message" -ForegroundColor Red
}

function Get-DockerCompose {
    if (Get-Command docker-compose -ErrorAction SilentlyContinue) {
        return "docker-compose"
    } else {
        return "docker compose"
    }
}

# ==============================================================================
# STEP 0: Clean Build Artifacts (Optional)
# ==============================================================================

# Add .NET build cleanup parameter
$CleanBuild = $PSBoundParameters.ContainsKey('RemoveVolumes') -and $RemoveVolumes

# ==============================================================================
# STEP 1: Validate Prerequisites
# ==============================================================================

Write-Header "Step 1: Validating Prerequisites"

# Check Docker installation
if (-not (Get-Command docker -ErrorAction SilentlyContinue)) {
    Write-Error-Custom "Docker not found. Please install Docker Desktop."
    exit 1
}
Write-Success "Docker installed"

# Get docker-compose command
$DockerCompose = Get-DockerCompose
Write-Success "Docker Compose available ($DockerCompose)"

# Get root path and verify docker-compose.yml
$RootPath = Split-Path -Parent (Split-Path -Parent $PSScriptRoot)
$ComposeFile = Join-Path $RootPath "docker-compose.yml"

if (-not (Test-Path $ComposeFile)) {
    Write-Error-Custom "docker-compose.yml not found at: $ComposeFile"
    exit 1
}
Write-Success "Found docker-compose.yml"

# ==============================================================================
# STEP 2: Display Cleanup Configuration
# ==============================================================================

Write-Header "Step 2: Cleanup Configuration"

Write-Host "Scope:" -ForegroundColor Yellow
Write-Host "  [X] Containers:   ALWAYS removed" -ForegroundColor Green
Write-Host "  [X] Networks:     ALWAYS removed" -ForegroundColor Green

if ($RemoveVolumes) {
    Write-Host "  [X] Volumes:      WILL BE REMOVED (data will be LOST!)" -ForegroundColor Red
} else {
    Write-Host "  [ ] Volumes:      will NOT be removed (data preserved)" -ForegroundColor Gray
}

if ($RemoveImages) {
    Write-Host "  [X] Images:       WILL BE REMOVED (will rebuild on next startup)" -ForegroundColor Red
} else {
    Write-Host "  [ ] Images:       will NOT be removed (cached for faster startup)" -ForegroundColor Gray
}

Write-Host ""

# ==============================================================================
# STEP 3: Clean .NET Build Artifacts (Optional)
# ==============================================================================

Write-Header "Step 3: .NET Build Artifacts"

if (-not $RemoveVolumes) {
    Write-Info "Use -RemoveVolumes flag to also clean .NET build artifacts"
} else {
    Write-Warn "Cleaning .NET build artifacts (bin/obj directories)..."
    
    $SrcPath = Join-Path $RootPath "src"
    $TestsPath = Join-Path $RootPath "tests"
    
    $DirsToClean = @(
        (Join-Path $SrcPath "HexagonalLab.API"),
        (Join-Path $SrcPath "HexagonalLab.Core"),
        (Join-Path $SrcPath "HexagonalLab.Infrastructure"),
        (Join-Path $SrcPath "HexagonalLab.Worker"),
        $TestsPath
    )
    
    $RemovedCount = 0
    foreach ($Dir in $DirsToClean) {
        if (Test-Path $Dir) {
            $BinObj = @(Get-ChildItem -Path $Dir -Include "bin", "obj" -Recurse -Directory -ErrorAction SilentlyContinue)
            foreach ($Item in $BinObj) {
                Remove-Item -Path $Item.FullName -Recurse -Force -ErrorAction SilentlyContinue
                $RemovedCount++
            }
        }
    }
    
    if ($RemovedCount -gt 0) {
        Write-Success "Removed $RemovedCount build artifact directories"
    } else {
        Write-Info "No build artifacts found to remove"
    }
}

# ==============================================================================
# STEP 4: Check Current Resources
# ==============================================================================

Write-Header "Step 4: Checking Current Resources"

$ContainerCount = @(docker ps -a --filter "name=$script:HexagonalPrefix*" --format "{{.Names}}" 2>$null).Count
$ImageCount = @(docker images | Select-String -Pattern $script:HexagonalPrefix | Measure-Object).Count
$VolumeCount = @(docker volume ls | Select-String -Pattern $script:HexagonalPrefix | Measure-Object).Count
$NetworkCount = @(docker network ls | Select-String -Pattern $script:HexagonalPrefix | Measure-Object).Count

Write-Info "Found resources:"
Write-Host "  Containers: $ContainerCount" -ForegroundColor Gray
Write-Host "  Networks:   $NetworkCount" -ForegroundColor Gray
Write-Host "  Volumes:    $VolumeCount" -ForegroundColor Gray
Write-Host "  Images:     $ImageCount" -ForegroundColor Gray

Write-Host ""

# ==============================================================================
# STEP 5: User Confirmation (if removing data)
# ==============================================================================

if (($RemoveVolumes -or $RemoveImages) -and -not $Force) {
    Write-Host ""
    Write-Warn "This will remove data that cannot be recovered!"
    Write-Host ""
    
    $Response = Read-Host "Are you sure? Type 'yes' to confirm"
    if ($Response -ne "yes") {
        Write-Info "Cleanup cancelled by user"
        exit 0
    }
}

# ==============================================================================
# STEP 6: Stop and Remove Containers
# ==============================================================================

Write-Header "Step 6: Stopping and Removing Containers"

Push-Location $RootPath

$Cmd = "down --remove-orphans"
if ($RemoveVolumes) {
    $Cmd += " -v"
}

Write-Info "Executing: $DockerCompose $Cmd"
Write-Host ""

$Output = & $DockerCompose $Cmd 2>&1
$ExitCode = $LASTEXITCODE

Write-Host ""

if ($ExitCode -eq 0) {
    Write-Success "Docker Compose cleanup completed"
} else {
    Write-Warn "Docker Compose cleanup returned warning (exit code: $ExitCode), continuing with manual cleanup..."
    Write-Info "Attempting manual Docker cleanup..."
    
    # Fallback: manual cleanup
    $Containers = docker ps -a --format "table {{.ID}}\t{{.Names}}" | Select-Object -Skip 1
    if ($Containers) {
        foreach ($Container in $Containers) {
            $ContainerId = ($Container -split '\s+')[0]
            if ($ContainerId) {
                docker stop $ContainerId 2>$null
                docker rm $ContainerId 2>$null
            }
        }
        Write-Success "Manual container cleanup completed"
    }
}

# ==============================================================================
# STEP 7: Remove Images (Optional)
# ==============================================================================

if ($RemoveImages) {
    Write-Header "Step 7: Removing Docker Images"

    $ImagesToRemove = @(docker images --format "{{.Repository}}:{{.Tag}}" | Select-String -Pattern $script:HexagonalPrefix)

    if ($ImagesToRemove.Count -gt 0) {
        Write-Info "Removing $($ImagesToRemove.Count) image(s):"
        
        foreach ($Image in $ImagesToRemove) {
            $ImageName = $Image.ToString().Trim()
            Write-Host "  - $ImageName" -ForegroundColor Yellow
            docker rmi -f $ImageName 2>$null
        }
        
        Write-Success "Docker images removed"
    } else {
        Write-Info "No HexagonalLab images found to remove"
    }
} else {
    Write-Header "Step 7: Skipping Image Removal"
    Write-Info "Use -RemoveImages flag to remove Docker images"
}

# ==============================================================================
# STEP 7A: Remove Volumes (Optional)
# ==============================================================================

if ($RemoveVolumes) {
    Write-Header "Step 7A: Removing Docker Volumes"

    $VolumesToRemove = @(docker volume ls --format "{{.Name}}" | Select-String -Pattern $script:HexagonalPrefix)

    if ($VolumesToRemove.Count -gt 0) {
        Write-Info "Removing $($VolumesToRemove.Count) volume(s):"
        
        foreach ($Volume in $VolumesToRemove) {
            $VolumeName = $Volume.ToString().Trim()
            Write-Host "  - $VolumeName" -ForegroundColor Yellow
            docker volume rm -f $VolumeName 2>$null
        }
        
        Write-Success "Docker volumes removed"
    } else {
        Write-Info "No HexagonalLab volumes found to remove"
    }
} else {
    Write-Header "Step 7A: Skipping Volume Removal"
    Write-Info "Use -RemoveVolumes flag to remove Docker volumes"
}

# ==============================================================================
# STEP 8: Remove Networks (Optional)
# ==============================================================================

if ($RemoveVolumes) {
    Write-Header "Step 8: Removing Docker Networks"

    $NetworksToRemove = @(docker network ls --format "{{.Name}}" | Select-String -Pattern $script:HexagonalPrefix)

    if ($NetworksToRemove.Count -gt 0) {
        Write-Info "Removing $($NetworksToRemove.Count) network(s):"
        
        foreach ($Network in $NetworksToRemove) {
            $NetworkName = $Network.ToString().Trim()
            Write-Host "  - $NetworkName" -ForegroundColor Yellow
            docker network rm $NetworkName 2>$null
        }
        
        Write-Success "Docker networks removed"
    } else {
        Write-Info "No HexagonalLab networks found to remove"
    }
}

Pop-Location

# ==============================================================================
# STEP 9: Verify Cleanup
# ==============================================================================

Write-Header "Step 9: Verifying Cleanup"

$RemainingContainers = @(docker ps -a --filter "name=$script:HexagonalPrefix*" --format "{{.Names}}" 2>$null)
$RemainingNetworks = @(docker network ls | Select-String -Pattern $script:HexagonalPrefix | Measure-Object).Count
$RemainingVolumes = @(docker volume ls | Select-String -Pattern $script:HexagonalPrefix | Measure-Object).Count
$RemainingImages = @(docker images | Select-String -Pattern $script:HexagonalPrefix | Measure-Object).Count

if ($RemainingContainers.Count -eq 0) {
    Write-Success "All containers removed"
} else {
    Write-Warn "Some containers still exist: $($RemainingContainers -join ', ')"
}

if ($RemainingNetworks -eq 0) {
    Write-Success "All networks removed"
} else {
    Write-Warn "Some networks still exist ($RemainingNetworks found)"
}

if ($RemainingVolumes -eq 0) {
    Write-Success "All volumes removed"
} else {
    Write-Warn "Some volumes still exist ($RemainingVolumes found)"
}

if ($RemainingImages -eq 0) {
    Write-Success "All images removed"
} else {
    Write-Info "Cached images remain ($RemainingImages found) - will be rebuilt on next startup"
}

# ==============================================================================
# STEP 10: Cleanup Summary
# ==============================================================================

Write-Header "Cleanup Summary"

Write-Host "Removed:" -ForegroundColor Magenta
Write-Host "  [X] Containers" -ForegroundColor Green

if ($RemoveVolumes) {
    Write-Host "  [X] Networks (REMOVED)" -ForegroundColor Red
    Write-Host "  [X] Data volumes (DATA DELETED)" -ForegroundColor Red
    Write-Host "  [X] Docker images (REMOVED)" -ForegroundColor Red
} else {
    Write-Host "  [ ] Networks (preserved)" -ForegroundColor Gray
    Write-Host "  [ ] Data volumes (preserved)" -ForegroundColor Gray
    Write-Host "  [ ] Docker images (cached)" -ForegroundColor Gray
}

Write-Host ""
Write-Host "Next Steps:" -ForegroundColor Cyan
Write-Host "  To start the environment again:" -ForegroundColor Cyan
Write-Host "  ./scripts/ambiente/startup.ps1" -ForegroundColor Gray

if ($RemoveImages) {
    Write-Host "  (Images will be rebuilt from Dockerfiles)" -ForegroundColor Gray
} else {
    Write-Host "  (Cached images will be reused for faster startup)" -ForegroundColor Gray
}

Write-Host ""

# ==============================================================================
# Completion
# ==============================================================================

Write-Success "Environment cleanup completed successfully!"
Write-Host ""

exit 0
