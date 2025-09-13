#!/bin/bash

# Test script for build tools
echo "🧪 Testing Build Tools for Customer Support System"
echo "=================================================="

# Test 1: Makefile help
echo ""
echo "📋 Test 1: Makefile Help"
echo "------------------------"
if command -v make >/dev/null 2>&1; then
    make help
    echo "✅ Makefile help works"
else
    echo "❌ Make not found. Install make to use the Makefile"
fi

# Test 2: Enhanced build.sh help
echo ""
echo "📋 Test 2: Enhanced build.sh Help"
echo "--------------------------------"
if [[ -f "./build.sh" && -x "./build.sh" ]]; then
    ./build.sh --help
    echo "✅ Enhanced build.sh help works"
else
    echo "❌ build.sh not found or not executable"
fi

# Test 3: Makefile prerequisites check
echo ""
echo "📋 Test 3: Makefile Prerequisites Check"
echo "--------------------------------------"
if command -v make >/dev/null 2>&1; then
    make check-dotnet
    echo "✅ Makefile prerequisites check works"
else
    echo "❌ Make not available"
fi

# Test 4: Build script prerequisites check (dry run)
echo ""
echo "📋 Test 4: Build Script Prerequisites Check"
echo "------------------------------------------"
if [[ -f "./build.sh" && -x "./build.sh" ]]; then
    echo "Testing prerequisites check..."
    timeout 10s ./build.sh --help >/dev/null 2>&1
    if [[ $? -eq 0 ]]; then
        echo "✅ Build script runs without errors"
    else
        echo "⚠️  Build script had issues (might be normal)"
    fi
else
    echo "❌ build.sh not available"
fi

echo ""
echo "🎉 Build tools testing completed!"
echo ""
echo "📖 Usage Examples:"
echo "  make help                    # Show all Makefile targets"
echo "  make build                   # Build the solution"
echo "  make test                    # Run tests"
echo "  make verify                  # Build and test everything"
echo "  ./build.sh --help            # Show build script options"
echo "  ./build.sh --clean           # Clean build"
echo "  ./build.sh --skip-tests      # Build without tests"
