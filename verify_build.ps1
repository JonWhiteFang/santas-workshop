# Verification script for Santa's Workshop Automation project setup
# This script performs a quick build test to verify the project is properly configured

$unityPath = "C:\Program Files\Unity\Hub\Editor\6000.2.10f1\Editor\Unity.exe"
$projectPath = "$PSScriptRoot\SantasWorkshopAutomation"
$buildPath = "$PSScriptRoot\Builds\Verification\SantasWorkshop.exe"
$logFile = "$PSScriptRoot\build_verification.log"

Write-Host "=== Santa's Workshop Automation - Build Verification ===" -ForegroundColor Cyan
Write-Host ""

# Check if Unity exists
if (-not (Test-Path $unityPath))
{
    Write-Host "ERROR: Unity not found at $unityPath" -ForegroundColor Red
    exit 1
}

Write-Host "✓ Unity found: $unityPath" -ForegroundColor Green

# Check if project exists
if (-not (Test-Path $projectPath))
{
    Write-Host "ERROR: Project not found at $projectPath" -ForegroundColor Red
    exit 1
}

Write-Host "✓ Project found: $projectPath" -ForegroundColor Green
Write-Host ""

# Create build directory
$buildDir = Split-Path $buildPath -Parent
if (-not (Test-Path $buildDir))
{
    New-Item -ItemType Directory -Path $buildDir -Force | Out-Null
}

Write-Host "Starting Unity build test..." -ForegroundColor Yellow
Write-Host "This may take several minutes..." -ForegroundColor Yellow
Write-Host ""

# Run Unity build
$arguments = @(
    "-quit",
    "-batchmode",
    "-nographics",
    "-projectPath", $projectPath,
    "-buildWindows64Player", $buildPath,
    "-logFile", $logFile
)

$process = Start-Process -FilePath $unityPath -ArgumentList $arguments -Wait -PassThru -NoNewWindow

# Check result
if ($process.ExitCode -eq 0)
{
    Write-Host "✓ Build completed successfully!" -ForegroundColor Green
    Write-Host ""
    
    # Check if build file exists
    if (Test-Path $buildPath)
    {
        $buildSize = (Get-Item $buildPath).Length / 1MB
        Write-Host "✓ Build file created: $buildPath" -ForegroundColor Green
        Write-Host "  Size: $([math]::Round($buildSize, 2)) MB" -ForegroundColor Gray
    }
    else
    {
        Write-Host "WARNING: Build completed but executable not found" -ForegroundColor Yellow
    }
    
    Write-Host ""
    Write-Host "=== Verification Complete ===" -ForegroundColor Cyan
    Write-Host "All checks passed! Project is ready for development." -ForegroundColor Green
    exit 0
}
else
{
    Write-Host "ERROR: Build failed with exit code $($process.ExitCode)" -ForegroundColor Red
    Write-Host ""
    Write-Host "Check the log file for details: $logFile" -ForegroundColor Yellow
    
    # Show last 20 lines of log if it exists
    if (Test-Path $logFile)
    {
        Write-Host ""
        Write-Host "=== Last 20 lines of build log ===" -ForegroundColor Yellow
        Get-Content $logFile -Tail 20
    }
    
    exit 1
}
