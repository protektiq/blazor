# GitHub Actions Workflows

This directory contains GitHub Actions workflows for the Customer Support System project.

## Workflows Overview

### 🔄 CI - Build and Test (`ci.yml`)
**Triggers:** Push to main/develop, Pull Requests
**Purpose:** Continuous Integration - Build, test, and verify the solution

**Jobs:**
- `build-and-test`: Builds solution, runs CI tests, publishes applications
- `security-scan`: Runs security scans and code analysis
- `dependency-check`: Checks for outdated packages and verifies integrity

**Key Features:**
- Uses our custom Makefile and build.sh scripts
- Caches .NET packages and build artifacts
- Uploads build artifacts and logs
- CI-friendly (no E2E tests that require running application)

### 🎭 E2E Tests (`e2e-tests.yml`)
**Triggers:** Push to main/develop, Pull Requests, Manual
**Purpose:** End-to-end testing with Playwright

**Jobs:**
- `e2e-tests`: Runs E2E tests with auto-started application
- `e2e-tests-matrix`: Runs E2E tests across multiple browsers (Chromium, Firefox, WebKit)

**Key Features:**
- Automatically starts the web application
- Installs Playwright browsers
- Runs comprehensive E2E test suite
- Tests across multiple browsers
- Uploads test results and screenshots

### 🚀 Deploy to Vercel (`deploy-vercel.yml`)
**Triggers:** Push to main, Manual
**Purpose:** Deploy Blazor WASM app to Vercel and API to Azure

**Jobs:**
- `deploy-wasm`: Builds and deploys WASM app to Vercel
- `deploy-api`: Builds and deploys API to Azure App Service (if configured)

**Key Features:**
- Deploys WASM app to Vercel using Vercel CLI
- Deploys API to Azure App Service
- Comments deployment URLs on PRs
- Creates deployment status updates

### 📦 Release (`release.yml`)
**Triggers:** Git tags (v*), Manual
**Purpose:** Create releases and build release artifacts

**Jobs:**
- `create-release`: Creates GitHub release
- `build-release`: Builds release artifacts for multiple platforms
- `deploy-production`: Deploys to production (stable releases only)

**Key Features:**
- Creates releases from git tags
- Builds artifacts for Windows, Linux, and macOS
- Supports pre-releases
- Production deployment for stable releases

### 🔒 Security Scan (`security.yml`)
**Triggers:** Push to main/develop, Pull Requests, Weekly schedule
**Purpose:** Security scanning and vulnerability detection

**Jobs:**
- `dependency-scan`: Scans for vulnerable dependencies
- `code-analysis`: Runs SonarCloud analysis
- `security-headers`: Tests security headers
- `secrets-scan`: Scans for secrets in code

**Key Features:**
- Dependency vulnerability scanning
- SonarCloud integration
- Security header validation
- Secret scanning with TruffleHog

### 📋 Dependency Update (`dependency-update.yml`)
**Triggers:** Weekly schedule, Manual
**Purpose:** Automated dependency management

**Jobs:**
- `check-updates`: Checks for outdated packages and creates issues
- `auto-update-patch`: Automatically updates patch versions (manual trigger)

**Key Features:**
- Weekly dependency checks
- Automatic issue creation for outdated packages
- Automated patch version updates
- Pull request creation for updates

## Required Secrets

To use these workflows, you need to configure the following secrets in your GitHub repository:

### Required for All Workflows
- `GITHUB_TOKEN` (automatically provided)

### Required for Vercel Deployment
- `VERCEL_TOKEN`: Your Vercel API token
- `VERCEL_ORG_ID`: Your Vercel organization ID
- `VERCEL_PROJECT_ID`: Your Vercel project ID

### Required for Azure Deployment
- `AZURE_CREDENTIALS`: Azure service principal credentials
- `AZURE_APP_NAME`: Azure App Service name
- `AZURE_PUBLISH_PROFILE`: Azure publish profile

### Required for SonarCloud
- `SONAR_TOKEN`: SonarCloud authentication token

### Optional
- `SLACK_WEBHOOK`: Slack webhook URL for notifications

## Usage Examples

### Manual Workflow Triggering

```bash
# Trigger E2E tests manually
gh workflow run e2e-tests.yml

# Trigger dependency update check
gh workflow run dependency-update.yml

# Create a release manually
gh workflow run release.yml -f tag=v1.2.3 -f prerelease=false
```

### Local Testing

Before pushing changes, test your workflows locally:

```bash
# Test build process
make ci-build

# Test with our enhanced build script
./build.sh --clean --verbose

# Test E2E tests (requires running application)
make test-with-app
```

### Environment-Specific Configuration

The workflows are designed to work across different environments:

- **Development**: E2E tests run on every PR
- **Staging**: Automatic deployment from develop branch
- **Production**: Manual deployment from main branch with releases

## Workflow Status Badges

Add these badges to your README.md:

```markdown
![CI](https://github.com/yourusername/CustomerSupportSystem/workflows/CI%20-%20Build%20and%20Test/badge.svg)
![E2E Tests](https://github.com/yourusername/CustomerSupportSystem/workflows/E2E%20Tests/badge.svg)
![Security Scan](https://github.com/yourusername/CustomerSupportSystem/workflows/Security%20Scan/badge.svg)
```

## Troubleshooting

### Common Issues

1. **Build Failures**: Check the build logs in the Actions tab
2. **Test Failures**: Ensure the application can start properly
3. **Deployment Failures**: Verify secrets are configured correctly
4. **Permission Issues**: Check repository settings and token permissions

### Debug Mode

Enable debug logging by adding this to any workflow:

```yaml
env:
  ACTIONS_STEP_DEBUG: true
  ACTIONS_RUNNER_DEBUG: true
```

### Local Development

Use our build tools for consistent local development:

```bash
# Standard development workflow
make dev-setup    # Initial setup
make build        # Build everything
make run-web      # Start the application
make test-e2e     # Run E2E tests (in another terminal)

# CI simulation
make ci-verify    # Simulate CI pipeline locally
```

## Contributing

When adding new workflows:

1. Follow the existing naming conventions
2. Include proper error handling
3. Add appropriate triggers
4. Update this README
5. Test thoroughly before merging

## Support

For issues with workflows:
1. Check the Actions tab in GitHub
2. Review workflow logs
3. Verify secrets configuration
4. Test locally using our build tools
