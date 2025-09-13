#!/bin/bash

# GitHub Actions Setup Script for Customer Support System
# This script helps validate the GitHub Actions setup

set -euo pipefail

# Colors
RED='\033[0;31m'
GREEN='\033[0;32m'
YELLOW='\033[0;33m'
BLUE='\033[0;34m'
NC='\033[0m'

echo -e "${BLUE}🚀 GitHub Actions Setup Validation${NC}"
echo "=================================="

# Check if we're in a git repository
if ! git rev-parse --git-dir > /dev/null 2>&1; then
    echo -e "${RED}❌ Not in a git repository${NC}"
    exit 1
fi

# Check if .github/workflows directory exists
if [[ ! -d ".github/workflows" ]]; then
    echo -e "${RED}❌ .github/workflows directory not found${NC}"
    exit 1
fi

echo -e "${GREEN}✅ Git repository detected${NC}"

# Check workflow files
workflows=(
    "ci.yml"
    "e2e-tests.yml"
    "deploy-vercel.yml"
    "release.yml"
    "security.yml"
    "dependency-update.yml"
)

echo -e "${BLUE}📋 Checking workflow files...${NC}"
for workflow in "${workflows[@]}"; do
    if [[ -f ".github/workflows/$workflow" ]]; then
        echo -e "${GREEN}✅ $workflow${NC}"
    else
        echo -e "${RED}❌ $workflow missing${NC}"
    fi
done

# Check if Makefile exists
if [[ -f "Makefile" ]]; then
    echo -e "${GREEN}✅ Makefile found${NC}"
else
    echo -e "${RED}❌ Makefile missing${NC}"
fi

# Check if build.sh exists and is executable
if [[ -f "build.sh" && -x "build.sh" ]]; then
    echo -e "${GREEN}✅ build.sh found and executable${NC}"
else
    echo -e "${RED}❌ build.sh missing or not executable${NC}"
    if [[ -f "build.sh" ]]; then
        echo -e "${YELLOW}   Fix: chmod +x build.sh${NC}"
    fi
fi

# Check solution file
if [[ -f "CustomerSupportSystem.sln" ]]; then
    echo -e "${GREEN}✅ Solution file found${NC}"
else
    echo -e "${RED}❌ Solution file missing${NC}"
fi

# Test build process
echo -e "${BLUE}🔨 Testing build process...${NC}"
if command -v make >/dev/null 2>&1; then
    if make check-dotnet >/dev/null 2>&1; then
        echo -e "${GREEN}✅ Makefile build check passed${NC}"
    else
        echo -e "${YELLOW}⚠️  Makefile build check failed (might need .NET SDK)${NC}"
    fi
else
    echo -e "${YELLOW}⚠️  Make not available (install make for full functionality)${NC}"
fi

# Check for required files
echo -e "${BLUE}📁 Checking required files...${NC}"
required_files=(
    "CustomerSupportSystem.Web/CustomerSupportSystem.Web.csproj"
    "CustomerSupportSystem.Api/CustomerSupportSystem.Api.csproj"
    "CustomerSupportSystem.Wasm/CustomerSupportSystem.Wasm.csproj"
    "CustomerSupportSystem.Tests/CustomerSupportSystem.Tests.csproj"
    "vercel.json"
)

for file in "${required_files[@]}"; do
    if [[ -f "$file" ]]; then
        echo -e "${GREEN}✅ $file${NC}"
    else
        echo -e "${RED}❌ $file missing${NC}"
    fi
done

# Check git remote
echo -e "${BLUE}🔗 Checking git remote...${NC}"
if git remote get-url origin >/dev/null 2>&1; then
    remote_url=$(git remote get-url origin)
    echo -e "${GREEN}✅ Remote: $remote_url${NC}"
    
    # Check if it's a GitHub repository
    if [[ "$remote_url" == *"github.com"* ]]; then
        echo -e "${GREEN}✅ GitHub repository detected${NC}"
    else
        echo -e "${YELLOW}⚠️  Not a GitHub repository (workflows won't run)${NC}"
    fi
else
    echo -e "${YELLOW}⚠️  No git remote configured${NC}"
fi

# Summary
echo ""
echo -e "${BLUE}📋 Setup Summary${NC}"
echo "==============="
echo ""
echo -e "${YELLOW}Next steps:${NC}"
echo "1. Push your code to GitHub:"
echo "   git add ."
echo "   git commit -m 'Add GitHub Actions workflows'"
echo "   git push origin main"
echo ""
echo "2. Configure secrets in GitHub repository:"
echo "   - Go to Settings → Secrets and variables → Actions"
echo "   - Add required secrets (see .github/SECRETS_SETUP.md)"
echo ""
echo "3. Test the workflows:"
echo "   - Go to Actions tab in your GitHub repository"
echo "   - Run workflows manually to test"
echo ""
echo -e "${GREEN}🎉 GitHub Actions setup validation complete!${NC}"
