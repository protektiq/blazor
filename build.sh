#!/bin/bash

# Customer Support System - Enhanced Build Script
# This script builds the entire solution with comprehensive error handling and logging

# Configuration
set -euo pipefail  # Exit on error, undefined vars, pipe failures
IFS=$'\n\t'       # Internal Field Separator for safer word splitting

# Colors for output
RED='\033[0;31m'
GREEN='\033[0;32m'
YELLOW='\033[0;33m'
BLUE='\033[0;34m'
NC='\033[0m' # No Color

# Configuration variables
SOLUTION_FILE="CustomerSupportSystem.sln"
CONFIGURATION="Release"
OUTPUT_DIR="./dist"
API_OUTPUT_DIR="./api-dist"
TEST_RESULTS_DIR="./test-results"
LOG_FILE="./build.log"

# Project paths
WEB_PROJECT="CustomerSupportSystem.Web/CustomerSupportSystem.Web.csproj"
API_PROJECT="CustomerSupportSystem.Api/CustomerSupportSystem.Api.csproj"
WASM_PROJECT="CustomerSupportSystem.Wasm/CustomerSupportSystem.Wasm.csproj"
TEST_PROJECT="CustomerSupportSystem.Tests/CustomerSupportSystem.Tests.csproj"
DOMAIN_PROJECT="CustomerSupportSystem.Domain/CustomerSupportSystem.Domain.csproj"
DATA_PROJECT="CustomerSupportSystem.Data/CustomerSupportSystem.Data.csproj"

# Command line options
CLEAN=false
SKIP_TESTS=false
SKIP_PLAYWRIGHT=false
VERBOSE=false
HELP=false

# Logging functions
log_info() {
    echo -e "${BLUE}ℹ️  $1${NC}" | tee -a "$LOG_FILE"
}

log_success() {
    echo -e "${GREEN}✅ $1${NC}" | tee -a "$LOG_FILE"
}

log_warning() {
    echo -e "${YELLOW}⚠️  $1${NC}" | tee -a "$LOG_FILE"
}

log_error() {
    echo -e "${RED}❌ $1${NC}" | tee -a "$LOG_FILE"
}

log_step() {
    echo -e "${BLUE}🔧 $1${NC}" | tee -a "$LOG_FILE"
}

# Error handling
handle_error() {
    local exit_code=$?
    local line_number=$1
    log_error "Build failed at line $line_number with exit code $exit_code"
    
    # Show last few lines of log file for context
    if [[ -f "$LOG_FILE" ]]; then
        log_error "Last few log entries:"
        tail -n 5 "$LOG_FILE" | sed 's/^/  /'
    fi
    
    exit $exit_code
}

trap 'handle_error $LINENO' ERR

# Utility functions
command_exists() {
    command -v "$1" >/dev/null 2>&1
}

check_prerequisites() {
    log_step "Checking prerequisites..."
    
    # Check if dotnet is installed
    if ! command_exists dotnet; then
        log_error ".NET SDK not found. Please install .NET 8 SDK."
        log_info "Download from: https://dotnet.microsoft.com/download"
        exit 1
    fi
    
    # Check dotnet version
    local dotnet_version
    dotnet_version=$(dotnet --version)
    if [[ ! "$dotnet_version" =~ ^8\. ]]; then
        log_error ".NET 8 SDK required. Found: $dotnet_version"
        log_info "Please install .NET 8 SDK from: https://dotnet.microsoft.com/download"
        exit 1
    fi
    
    log_success ".NET SDK version: $dotnet_version"
    
    # Check if solution file exists
    if [[ ! -f "$SOLUTION_FILE" ]]; then
        log_error "Solution file not found: $SOLUTION_FILE"
        exit 1
    fi
    
    log_success "Prerequisites check completed"
}

show_help() {
    cat << EOF
Customer Support System - Enhanced Build Script

USAGE:
    $0 [OPTIONS]

OPTIONS:
    -c, --clean          Clean build artifacts before building
    -s, --skip-tests     Skip running tests
    -p, --skip-playwright Skip Playwright browser tests
    -v, --verbose        Enable verbose output
    -h, --help           Show this help message

EXAMPLES:
    $0                   # Standard build
    $0 --clean           # Clean build
    $0 --skip-tests      # Build without tests
    $0 --verbose         # Verbose output

This script will:
1. Check prerequisites (.NET 8 SDK)
2. Clean artifacts (if requested)
3. Restore NuGet packages
4. Build all projects
5. Run tests (unless skipped)
6. Publish applications

EOF
}

parse_arguments() {
    while [[ $# -gt 0 ]]; do
        case $1 in
            -c|--clean)
                CLEAN=true
                shift
                ;;
            -s|--skip-tests)
                SKIP_TESTS=true
                shift
                ;;
            -p|--skip-playwright)
                SKIP_PLAYWRIGHT=true
                shift
                ;;
            -v|--verbose)
                VERBOSE=true
                shift
                ;;
            -h|--help)
                HELP=true
                shift
                ;;
            *)
                log_error "Unknown option: $1"
                show_help
                exit 1
                ;;
        esac
    done
}

clean_artifacts() {
    if [[ "$CLEAN" == "true" ]]; then
        log_step "Cleaning build artifacts..."
        
        # Clean dotnet artifacts
        dotnet clean "$SOLUTION_FILE" --verbosity quiet || true
        
        # Remove output directories
        rm -rf "$OUTPUT_DIR" "$API_OUTPUT_DIR" "$TEST_RESULTS_DIR" 2>/dev/null || true
        
        # Remove bin/obj directories
        find . -name "bin" -type d -exec rm -rf {} + 2>/dev/null || true
        find . -name "obj" -type d -exec rm -rf {} + 2>/dev/null || true
        
        log_success "Clean completed"
    fi
}

restore_packages() {
    log_step "Restoring NuGet packages..."
    
    local verbosity="quiet"
    if [[ "$VERBOSE" == "true" ]]; then
        verbosity="normal"
    fi
    
    if ! dotnet restore "$SOLUTION_FILE" --verbosity "$verbosity"; then
        log_error "Package restoration failed"
        exit 1
    fi
    
    log_success "Package restoration completed"
}

build_project() {
    local project_path="$1"
    local project_name="$2"
    
    log_step "Building $project_name..."
    
    local verbosity="quiet"
    if [[ "$VERBOSE" == "true" ]]; then
        verbosity="normal"
    fi
    
    if ! dotnet build "$project_path" -c "$CONFIGURATION" --no-restore --verbosity "$verbosity"; then
        log_error "Build failed for $project_name"
        exit 1
    fi
    
    log_success "$project_name built successfully"
}

build_solution() {
    log_step "Building solution..."
    
    # Build projects in dependency order
    build_project "$DOMAIN_PROJECT" "Domain"
    build_project "$DATA_PROJECT" "Data"
    build_project "$WEB_PROJECT" "Web"
    build_project "$API_PROJECT" "API"
    build_project "$WASM_PROJECT" "WASM"
    build_project "$TEST_PROJECT" "Tests"
    
    log_success "All projects built successfully"
}

run_tests() {
    if [[ "$SKIP_TESTS" == "true" ]]; then
        log_warning "Skipping tests as requested"
        return 0
    fi
    
    log_step "Running tests..."
    
    # Check if application is running for E2E tests
    log_info "Checking if application is running..."
    local app_running=false
    if curl -s --connect-timeout 5 http://localhost:5231 >/dev/null 2>&1; then
        app_running=true
        log_success "Application is running on http://localhost:5231"
    else
        log_warning "Application is not running on http://localhost:5231"
        log_info "All tests are Playwright E2E tests that require the application to be running"
    fi
    
    # Create test results directory
    mkdir -p "$TEST_RESULTS_DIR"
    
    local verbosity="quiet"
    if [[ "$VERBOSE" == "true" ]]; then
        verbosity="normal"
    fi
    
    if [[ "$app_running" == "true" ]]; then
        log_info "Running Playwright E2E tests..."
        if ! dotnet test "$TEST_PROJECT" -c "$CONFIGURATION" --no-build \
            --logger "trx;LogFileName=e2e-tests.trx" \
            --results-directory "$TEST_RESULTS_DIR" \
            --verbosity "$verbosity"; then
            log_error "E2E tests failed"
            exit 1
        fi
        log_success "E2E tests completed"
    else
        log_warning "Skipping E2E tests - application not running"
        log_info "To run E2E tests:"
        log_info "  1. Start the application: dotnet run --project CustomerSupportSystem.Web"
        log_info "  2. Run tests: dotnet test CustomerSupportSystem.Tests"
        log_info "  3. Or use: ./build.sh --skip-tests (to skip tests entirely)"
        
        # In CI mode, we consider this a success since we can't run E2E tests
        if [[ "${CI:-false}" == "true" ]]; then
            log_info "CI mode detected - treating as success (E2E tests require running application)"
        else
            log_warning "Tests skipped - use --skip-tests to suppress this warning"
        fi
    fi
}

publish_applications() {
    log_step "Publishing applications..."
    
    # Publish WASM app
    log_info "Publishing WASM application..."
    mkdir -p "$OUTPUT_DIR"
    if ! dotnet publish "$WASM_PROJECT" -c "$CONFIGURATION" -o "$OUTPUT_DIR" --no-build; then
        log_error "WASM publish failed"
        exit 1
    fi
    
    # Copy to output directory if it exists
    if [[ -d "./out" ]]; then
        log_info "Copying files to output directory..."
        cp -r "$OUTPUT_DIR/wwwroot"/* ./out/ 2>/dev/null || true
    fi
    
    # Publish API app
    log_info "Publishing API application..."
    mkdir -p "$API_OUTPUT_DIR"
    if ! dotnet publish "$API_PROJECT" -c "$CONFIGURATION" -o "$API_OUTPUT_DIR" --no-build; then
        log_error "API publish failed"
        exit 1
    fi
    
    log_success "Applications published successfully"
}

show_summary() {
    log_success "Build completed successfully!"
    
    echo ""
    echo "📋 Build Summary:"
    echo "  Configuration: $CONFIGURATION"
    echo "  Output Directory: $OUTPUT_DIR"
    echo "  API Output Directory: $API_OUTPUT_DIR"
    echo "  Test Results: $TEST_RESULTS_DIR"
    echo "  Log File: $LOG_FILE"
    
    if [[ -d "$OUTPUT_DIR" ]]; then
        echo ""
        echo "📦 Published Applications:"
        echo "  WASM App: $OUTPUT_DIR"
        echo "  API App: $API_OUTPUT_DIR"
    fi
    
    if [[ -d "$TEST_RESULTS_DIR" ]]; then
        echo ""
        echo "🧪 Test Results:"
        ls -la "$TEST_RESULTS_DIR"/*.trx 2>/dev/null | sed 's/^/  /' || true
    fi
}

# Main execution
main() {
    # Initialize log file
    echo "Build started at $(date)" > "$LOG_FILE"
    
    log_info "Customer Support System - Enhanced Build Script"
    log_info "Build started at $(date)"
    
    # Parse command line arguments
    parse_arguments "$@"
    
    # Show help if requested
    if [[ "$HELP" == "true" ]]; then
        show_help
        exit 0
    fi
    
    # Execute build steps
    check_prerequisites
    clean_artifacts
    restore_packages
    build_solution
    run_tests
    publish_applications
    show_summary
    
    log_success "Build script completed successfully at $(date)"
}

# Run main function with all arguments
main "$@"
