# Simple test runner script
Write-Host "Starting simple Playwright test..." -ForegroundColor Green

# Check if application is running
try {
    $response = Invoke-WebRequest -Uri "http://localhost:5231" -TimeoutSec 5
    Write-Host "Application is running on http://localhost:5231" -ForegroundColor Green
} catch {
    Write-Host "Application is not running. Please start it first with:" -ForegroundColor Yellow
    Write-Host "dotnet run --project ../CustomerSupportSystem.Web" -ForegroundColor Yellow
    exit 1
}

# Try to run a simple test
Write-Host "Running a simple test..." -ForegroundColor Yellow
dotnet test --filter "TestName~Dashboard_Should_Load_Successfully" --logger "console;verbosity=detailed"

Write-Host "Test completed!" -ForegroundColor Green
