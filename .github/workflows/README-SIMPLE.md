# Simplified GitHub Actions Workflows

This directory contains simplified GitHub Actions workflows that are more reliable and easier to debug.

## Simplified Workflows

### 🔄 CI - Simple Build (`ci-simple.yml`)
**Triggers:** Push to main/develop, Pull Requests
**Purpose:** Basic CI - Build, test, and verify the solution

**Features:**
- ✅ Simple dotnet commands (no make dependency)
- ✅ Build verification (no E2E tests)
- ✅ Security scanning
- ✅ Package vulnerability check
- ✅ Artifact upload

### 🎭 E2E Tests (Simple) (`e2e-tests-simple.yml`)
**Triggers:** Push to main/develop, Pull Requests, Manual
**Purpose:** End-to-end testing with simplified setup

**Features:**
- ✅ Simplified Playwright installation
- ✅ Auto-started application
- ✅ Basic E2E test execution
- ✅ Test result uploads

### 🚀 Deploy - Simple (`deploy-simple.yml`)
**Triggers:** Push to main, Manual
**Purpose:** Simple deployment to Vercel

**Features:**
- ✅ Basic Vercel deployment
- ✅ Artifact uploads
- ✅ Optional Vercel integration (requires secrets)

## Why Simplified?

The original workflows had these issues:
1. **Make dependency** - `make` not available in all GitHub runners
2. **PowerShell scripts** - E2E tests used PowerShell which can be unreliable
3. **Complex dependencies** - Too many moving parts

## Quick Start

### 1. Test Locally First
```bash
# Test the build process
dotnet restore CustomerSupportSystem.sln
dotnet build CustomerSupportSystem.sln -c Release

# Test publishing
dotnet publish CustomerSupportSystem.Wasm/CustomerSupportSystem.Wasm.csproj -c Release -o ./dist
```

### 2. Push and Test
```bash
git add .github/workflows/ci-simple.yml
git commit -m "Add simplified CI workflow"
git push origin main
```

### 3. Check Results
- Go to Actions tab in GitHub
- Look for "CI - Simple Build" workflow
- Check the logs if it fails

## Troubleshooting

### Common Issues

**"dotnet command not found"**
- ✅ Fixed: Uses `actions/setup-dotnet@v4`

**"make command not found"**
- ✅ Fixed: Uses direct `dotnet` commands

**"PowerShell script failed"**
- ✅ Fixed: Uses `playwright` CLI directly

**"E2E tests failed"**
- ✅ Fixed: Simplified E2E workflow with better error handling

### Debug Steps

1. **Check the workflow logs** in GitHub Actions
2. **Test locally** with the same commands
3. **Verify secrets** are configured (for deployment)
4. **Check file paths** are correct

### Local Testing

Test the same commands locally:

```bash
# CI workflow commands
dotnet restore CustomerSupportSystem.sln
dotnet build CustomerSupportSystem.sln -c Release --no-restore
dotnet build CustomerSupportSystem.Tests/CustomerSupportSystem.Tests.csproj -c Release --no-restore
dotnet publish CustomerSupportSystem.Wasm/CustomerSupportSystem.Wasm.csproj -c Release -o ./dist

# E2E workflow commands
cd CustomerSupportSystem.Web
dotnet run --configuration Release --urls http://localhost:5231 &
# Then run tests in another terminal
```

## Migration from Complex Workflows

If you want to use the simplified workflows:

1. **Disable complex workflows** (rename or delete)
2. **Use simplified workflows** (already created)
3. **Configure secrets** (same as before)
4. **Test thoroughly**

## Next Steps

Once the simplified workflows work:

1. **Add more features** gradually
2. **Configure secrets** for deployment
3. **Add more test coverage**
4. **Optimize for speed**

## Support

For issues:
1. Check GitHub Actions logs
2. Test commands locally
3. Verify all dependencies are available
4. Check this README for solutions
