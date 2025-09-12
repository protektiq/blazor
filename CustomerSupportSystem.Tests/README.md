# Customer Support System - Playwright Tests

This project contains comprehensive functional tests for the Customer Support System using Playwright for .NET.

## Test Coverage

The test suite covers the following areas:

### 1. Dashboard Tests (`DashboardTests.cs`)
- Dashboard loading and display
- Statistics cards and data
- Recent tickets display
- Quick actions functionality
- Navigation from dashboard

### 2. Tickets Tests (`TicketsTests.cs`)
- Tickets page loading
- Tickets table display
- Sample ticket data verification
- Status and priority badges
- Navigation to ticket details
- Loading states

### 3. Ticket Detail Tests (`TicketDetailTests.cs`)
- Ticket detail page loading
- Ticket information display
- Customer information
- Comments section
- Action buttons
- Navigation back to tickets
- Error handling for non-existent tickets

### 4. Navigation Tests (`NavigationTests.cs`)
- Navigation menu visibility
- Link functionality
- Active page highlighting
- Browser back/forward support
- Direct URL access
- Invalid route handling
- State maintenance

### 5. Responsive Tests (`ResponsiveTests.cs`)
- Mobile responsiveness (375x667)
- Tablet responsiveness (768x1024)
- Desktop responsiveness (1920x1080)
- Viewport change handling
- Accessibility across screen sizes

### 6. End-to-End Tests (`EndToEndTests.cs`)
- Complete user journeys
- Multiple navigation scenarios
- Data consistency
- Refresh scenarios
- Error handling
- Browser action support

## Running Tests

### Prerequisites
1. Ensure the Customer Support System application is running on `http://localhost:5231`
2. Install Playwright browsers (run once):
   ```powershell
   dotnet build
   pwsh bin/Debug/net8.0/playwright.ps1 install
   ```

### Running All Tests
```bash
dotnet test
```

### Running Specific Test Categories
```bash
# Dashboard tests only
dotnet test --filter "TestCategory=Dashboard"

# Navigation tests only
dotnet test --filter "TestCategory=Navigation"

# Responsive tests only
dotnet test --filter "TestCategory=Responsive"
```

### Running Tests in Different Browsers
```bash
# Chrome/Chromium
dotnet test --filter "Browser=chromium"

# Firefox
dotnet test --filter "Browser=firefox"

# Safari/WebKit
dotnet test --filter "Browser=webkit"
```

### Using the PowerShell Script
```powershell
# Run all tests
.\run-tests.ps1

# Run tests in Firefox
.\run-tests.ps1 -Browser "firefox"

# Install browsers and run tests
.\run-tests.ps1 -InstallBrowsers

# Run specific test pattern
.\run-tests.ps1 -TestPattern "*Dashboard*"
```

## Test Configuration

- **Base URL**: `http://localhost:5231`
- **Timeout**: 30 seconds for general operations
- **Navigation Timeout**: 10 seconds
- **Screenshots**: Captured on failures and key test points
- **Videos**: Recorded on test failures
- **Traces**: Captured on first retry

## Test Data

The tests rely on seeded data from the application:
- 3 sample tickets with different statuses and priorities
- 3 users (admin, agent, customer)
- Sample comments on tickets

## Screenshots and Reports

- Screenshots are saved in the `screenshots/` directory
- Test reports are generated in the `test-results/` directory
- HTML reports are available for detailed test analysis

## Troubleshooting

### Application Not Running
If tests fail because the application isn't running:
1. Start the application manually: `dotnet run --project ../CustomerSupportSystem.Web`
2. Or use the PowerShell script with auto-start: `.\run-tests.ps1`

### Browser Installation Issues
If browsers aren't installed:
```bash
dotnet build
pwsh bin/Debug/net8.0/playwright.ps1 install
```

### Test Timeouts
If tests are timing out:
1. Check if the application is responding at `http://localhost:5231`
2. Increase timeout values in `TestConfiguration.cs`
3. Check for any console errors in the application

## Continuous Integration

These tests are designed to run in CI/CD pipelines:
- Tests are parallelized for faster execution
- Screenshots and videos are captured for debugging
- Test results are exported in multiple formats (HTML, JSON, JUnit)
