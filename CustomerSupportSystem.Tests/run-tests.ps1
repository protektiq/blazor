# PowerShell script to run Playwright tests
param(
    [string]$Browser = "chromium",
    [string]$TestPattern = "*",
    [switch]$Headless = $true,
    [switch]$InstallBrowsers = $false
)

Write-Host "Starting Playwright Tests for Customer Support System" -ForegroundColor Green

# Install browsers if requested
if ($InstallBrowsers) {
    Write-Host "Installing Playwright browsers..." -ForegroundColor Yellow
    dotnet build
    pwsh bin/Debug/net8.0/playwright.ps1 install
}

# Ensure the application is running
Write-Host "Checking if application is running..." -ForegroundColor Yellow
$response = try { Invoke-WebRequest -Uri "http://localhost:5231" -TimeoutSec 5 } catch { $null }
if (-not $response) {
    Write-Host "Starting application..." -ForegroundColor Yellow
    Start-Process -FilePath "dotnet" -ArgumentList "run", "--project", "../CustomerSupportSystem.Web" -WindowStyle Hidden
    Start-Sleep -Seconds 10
}

# Run tests
Write-Host "Running tests..." -ForegroundColor Yellow
dotnet test --logger "console;verbosity=detailed" --filter "Browser=$Browser" --filter "TestPattern=$TestPattern"

Write-Host "Tests completed!" -ForegroundColor Green
