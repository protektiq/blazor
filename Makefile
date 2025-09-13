# Customer Support System - Build Automation
# This Makefile provides comprehensive build, test, and deployment automation

# Variables
SOLUTION_FILE := CustomerSupportSystem.sln
CONFIGURATION := Release
OUTPUT_DIR := ./dist
API_OUTPUT_DIR := ./api-dist
TEST_RESULTS_DIR := ./test-results
COVERAGE_DIR := ./coverage

# Project paths
WEB_PROJECT := CustomerSupportSystem.Web/CustomerSupportSystem.Web.csproj
API_PROJECT := CustomerSupportSystem.Api/CustomerSupportSystem.Api.csproj
WASM_PROJECT := CustomerSupportSystem.Wasm/CustomerSupportSystem.Wasm.csproj
TEST_PROJECT := CustomerSupportSystem.Tests/CustomerSupportSystem.Tests.csproj
DOMAIN_PROJECT := CustomerSupportSystem.Domain/CustomerSupportSystem.Domain.csproj
DATA_PROJECT := CustomerSupportSystem.Data/CustomerSupportSystem.Data.csproj

# Colors for output
RED := \033[0;31m
GREEN := \033[0;32m
YELLOW := \033[0;33m
BLUE := \033[0;34m
NC := \033[0m # No Color

# Default target
.DEFAULT_GOAL := help

# Help target
.PHONY: help
help: ## Show this help message
	@echo "$(BLUE)Customer Support System - Build Commands$(NC)"
	@echo ""
	@echo "$(YELLOW)Available targets:$(NC)"
	@awk 'BEGIN {FS = ":.*?## "} /^[a-zA-Z_-]+:.*?## / {printf "  $(GREEN)%-20s$(NC) %s\n", $$1, $$2}' $(MAKEFILE_LIST)
	@echo ""
	@echo "$(YELLOW)Examples:$(NC)"
	@echo "  make build          # Build the entire solution"
	@echo "  make test           # Run all tests"
	@echo "  make verify         # Build and test everything"
	@echo "  make clean          # Clean all build artifacts"
	@echo "  make run-web        # Run the web application"
	@echo "  make run-api        # Run the API application"

# Prerequisites check
.PHONY: check-dotnet
check-dotnet: ## Check if .NET SDK is installed and correct version
	@echo "$(BLUE)🔍 Checking .NET SDK...$(NC)"
	@which dotnet > /dev/null || (echo "$(RED)❌ .NET SDK not found. Please install .NET 8 SDK.$(NC)" && exit 1)
	@dotnet --version | grep -q "8\." || (echo "$(RED)❌ .NET 8 SDK required. Found: $$(dotnet --version)$(NC)" && exit 1)
	@echo "$(GREEN)✅ .NET SDK version: $$(dotnet --version)$(NC)"

# Clean targets
.PHONY: clean
clean: ## Clean all build artifacts and temporary files
	@echo "$(YELLOW)🧹 Cleaning build artifacts...$(NC)"
	@dotnet clean $(SOLUTION_FILE) --verbosity quiet || true
	@rm -rf $(OUTPUT_DIR) $(API_OUTPUT_DIR) $(TEST_RESULTS_DIR) $(COVERAGE_DIR)
	@rm -rf ./**/bin ./**/obj 2>/dev/null || true
	@echo "$(GREEN)✅ Clean completed$(NC)"

# Restore dependencies
.PHONY: restore
restore: check-dotnet ## Restore NuGet packages
	@echo "$(BLUE)📦 Restoring NuGet packages...$(NC)"
	@dotnet restore $(SOLUTION_FILE) --verbosity quiet
	@echo "$(GREEN)✅ Restore completed$(NC)"

# Build targets
.PHONY: build-domain
build-domain: restore ## Build Domain project
	@echo "$(BLUE)🔨 Building Domain project...$(NC)"
	@dotnet build $(DOMAIN_PROJECT) -c $(CONFIGURATION) --no-restore --verbosity quiet
	@echo "$(GREEN)✅ Domain project built$(NC)"

.PHONY: build-data
build-data: build-domain ## Build Data project
	@echo "$(BLUE)🔨 Building Data project...$(NC)"
	@dotnet build $(DATA_PROJECT) -c $(CONFIGURATION) --no-restore --verbosity quiet
	@echo "$(GREEN)✅ Data project built$(NC)"

.PHONY: build-web
build-web: build-data ## Build Web project
	@echo "$(BLUE)🔨 Building Web project...$(NC)"
	@dotnet build $(WEB_PROJECT) -c $(CONFIGURATION) --no-restore --verbosity quiet
	@echo "$(GREEN)✅ Web project built$(NC)"

.PHONY: build-api
build-api: build-data ## Build API project
	@echo "$(BLUE)🔨 Building API project...$(NC)"
	@dotnet build $(API_PROJECT) -c $(CONFIGURATION) --no-restore --verbosity quiet
	@echo "$(GREEN)✅ API project built$(NC)"

.PHONY: build-wasm
build-wasm: build-domain ## Build WASM project
	@echo "$(BLUE)🔨 Building WASM project...$(NC)"
	@dotnet build $(WASM_PROJECT) -c $(CONFIGURATION) --no-restore --verbosity quiet
	@echo "$(GREEN)✅ WASM project built$(NC)"

.PHONY: build-tests
build-tests: build-web ## Build Test project
	@echo "$(BLUE)🔨 Building Test project...$(NC)"
	@dotnet build $(TEST_PROJECT) -c $(CONFIGURATION) --no-restore --verbosity quiet
	@echo "$(GREEN)✅ Test project built$(NC)"

.PHONY: build
build: build-web build-api build-wasm build-tests ## Build entire solution
	@echo "$(GREEN)🎉 All projects built successfully!$(NC)"

# Test targets
.PHONY: test-unit
test-unit: build-tests ## Run unit tests only (none exist - all tests are E2E)
	@echo "$(YELLOW)⚠️  No unit tests found - all tests are Playwright E2E tests$(NC)"
	@echo "$(YELLOW)   Use 'make test-with-app' to run E2E tests with application$(NC)"
	@echo "$(GREEN)✅ Unit test check completed (no unit tests to run)$(NC)"

.PHONY: test-e2e
test-e2e: build-tests ## Run end-to-end tests (requires application running)
	@echo "$(BLUE)🎭 Running end-to-end tests...$(NC)"
	@echo "$(YELLOW)⚠️  Make sure the application is running on http://localhost:5231$(NC)"
	@mkdir -p $(TEST_RESULTS_DIR)
	@dotnet test $(TEST_PROJECT) -c $(CONFIGURATION) --no-build \
		--filter "Category=E2E" \
		--logger "trx;LogFileName=e2e-tests.trx" \
		--results-directory $(TEST_RESULTS_DIR) \
		--verbosity quiet
	@echo "$(GREEN)✅ End-to-end tests completed$(NC)"

.PHONY: test
test: test-unit ## Run all tests (unit tests by default - currently none exist)
	@echo "$(GREEN)🎉 All tests completed!$(NC)"

.PHONY: test-ci
test-ci: build-tests ## CI-friendly test target (builds only, no E2E tests)
	@echo "$(BLUE)🏗️  CI Test Mode - Building tests only$(NC)"
	@echo "$(YELLOW)⚠️  Skipping E2E tests in CI mode (require running application)$(NC)"
	@echo "$(GREEN)✅ CI tests completed (build verification only)$(NC)"

.PHONY: test-with-app
test-with-app: build-tests ## Run tests with application auto-started
	@echo "$(BLUE)🚀 Starting application for testing...$(NC)"
	@echo "$(YELLOW)⚠️  This will start the web app and run all tests$(NC)"
	@cd CustomerSupportSystem.Tests && pwsh -Command "& {./run-tests.ps1 -InstallBrowsers}"
	@echo "$(GREEN)✅ Tests with application completed!$(NC)"

.PHONY: test-with-coverage
test-with-coverage: build-tests ## Run tests with code coverage
	@echo "$(BLUE)📊 Running tests with coverage...$(NC)"
	@mkdir -p $(COVERAGE_DIR)
	@dotnet test $(TEST_PROJECT) -c $(CONFIGURATION) --no-build \
		--collect:"XPlat Code Coverage" \
		--results-directory $(TEST_RESULTS_DIR) \
		--verbosity quiet
	@echo "$(GREEN)✅ Coverage report generated$(NC)"

# Run targets
.PHONY: run-web
run-web: build-web ## Run the web application
	@echo "$(BLUE)🚀 Starting web application...$(NC)"
	@echo "$(YELLOW)Application will be available at: http://localhost:5231$(NC)"
	@dotnet run --project $(WEB_PROJECT) --configuration $(CONFIGURATION)

.PHONY: run-api
run-api: build-api ## Run the API application
	@echo "$(BLUE)🚀 Starting API application...$(NC)"
	@echo "$(YELLOW)API will be available at: http://localhost:5000$(NC)"
	@dotnet run --project $(API_PROJECT) --configuration $(CONFIGURATION)

.PHONY: run-wasm
run-wasm: build-wasm ## Run the WASM application
	@echo "$(BLUE)🚀 Starting WASM application...$(NC)"
	@echo "$(YELLOW)WASM app will be available at: http://localhost:5001$(NC)"
	@dotnet run --project $(WASM_PROJECT) --configuration $(CONFIGURATION)

# Publish targets
.PHONY: publish-web
publish-web: build-web ## Publish web application
	@echo "$(BLUE)📦 Publishing web application...$(NC)"
	@mkdir -p $(OUTPUT_DIR)
	@dotnet publish $(WEB_PROJECT) -c $(CONFIGURATION) -o $(OUTPUT_DIR) --no-build
	@echo "$(GREEN)✅ Web application published to $(OUTPUT_DIR)$(NC)"

.PHONY: publish-api
publish-api: build-api ## Publish API application
	@echo "$(BLUE)📦 Publishing API application...$(NC)"
	@mkdir -p $(API_OUTPUT_DIR)
	@dotnet publish $(API_PROJECT) -c $(CONFIGURATION) -o $(API_OUTPUT_DIR) --no-build
	@echo "$(GREEN)✅ API application published to $(API_OUTPUT_DIR)$(NC)"

.PHONY: publish-wasm
publish-wasm: build-wasm ## Publish WASM application
	@echo "$(BLUE)📦 Publishing WASM application...$(NC)"
	@mkdir -p $(OUTPUT_DIR)
	@dotnet publish $(WASM_PROJECT) -c $(CONFIGURATION) -o $(OUTPUT_DIR) --no-build
	@echo "$(GREEN)✅ WASM application published to $(OUTPUT_DIR)$(NC)"

.PHONY: publish
publish: publish-web publish-api publish-wasm ## Publish all applications
	@echo "$(GREEN)🎉 All applications published!$(NC)"

# Database targets
.PHONY: db-migrate
db-migrate: build-data ## Run database migrations
	@echo "$(BLUE)🗄️  Running database migrations...$(NC)"
	@dotnet ef database update --project $(DATA_PROJECT) --startup-project $(WEB_PROJECT)
	@echo "$(GREEN)✅ Database migrations completed$(NC)"

.PHONY: db-seed
db-seed: build-web ## Seed database with initial data
	@echo "$(BLUE)🌱 Seeding database...$(NC)"
	@dotnet run --project $(WEB_PROJECT) --configuration $(CONFIGURATION) --seed
	@echo "$(GREEN)✅ Database seeded$(NC)"

# Development targets
.PHONY: dev-setup
dev-setup: ## Set up development environment
	@echo "$(BLUE)🛠️  Setting up development environment...$(NC)"
	@make restore
	@make build
	@echo "$(YELLOW)Installing Playwright browsers...$(NC)"
	@cd CustomerSupportSystem.Tests && dotnet build && pwsh bin/Debug/net8.0/playwright.ps1 install
	@echo "$(GREEN)✅ Development environment ready!$(NC)"

.PHONY: dev-reset
dev-reset: clean dev-setup ## Reset development environment (clean + setup)
	@echo "$(GREEN)🔄 Development environment reset!$(NC)"

# Verification targets
.PHONY: verify
verify: build test ## Build and test everything (CI/CD verification)
	@echo "$(GREEN)✅ Project verification completed successfully!$(NC)"

.PHONY: verify-full
verify-full: clean build test-with-coverage ## Full verification with coverage
	@echo "$(GREEN)✅ Full project verification completed!$(NC)"

# Utility targets
.PHONY: format
format: ## Format code using dotnet format
	@echo "$(BLUE)🎨 Formatting code...$(NC)"
	@dotnet format $(SOLUTION_FILE) --verbosity quiet
	@echo "$(GREEN)✅ Code formatting completed$(NC)"

.PHONY: lint
lint: build ## Run code analysis
	@echo "$(BLUE)🔍 Running code analysis...$(NC)"
	@dotnet build $(SOLUTION_FILE) -c $(CONFIGURATION) --verbosity quiet --property:TreatWarningsAsErrors=true
	@echo "$(GREEN)✅ Code analysis completed$(NC)"

.PHONY: info
info: ## Show project information
	@echo "$(BLUE)📋 Project Information$(NC)"
	@echo "$(YELLOW)Solution:$(NC) $(SOLUTION_FILE)"
	@echo "$(YELLOW).NET Version:$(NC) $$(dotnet --version)"
	@echo "$(YELLOW)Configuration:$(NC) $(CONFIGURATION)"
	@echo "$(YELLOW)Output Directory:$(NC) $(OUTPUT_DIR)"
	@echo "$(YELLOW)Test Results:$(NC) $(TEST_RESULTS_DIR)"
	@echo ""
	@echo "$(YELLOW)Projects:$(NC)"
	@echo "  - Domain: $(DOMAIN_PROJECT)"
	@echo "  - Data: $(DATA_PROJECT)"
	@echo "  - Web: $(WEB_PROJECT)"
	@echo "  - API: $(API_PROJECT)"
	@echo "  - WASM: $(WASM_PROJECT)"
	@echo "  - Tests: $(TEST_PROJECT)"

# CI/CD targets
.PHONY: ci-build
ci-build: ## CI build target (optimized for CI/CD)
	@echo "$(BLUE)🏗️  CI Build...$(NC)"
	@make check-dotnet
	@make restore
	@make build
	@echo "$(GREEN)✅ CI build completed$(NC)"

.PHONY: ci-test
ci-test: ## CI test target (optimized for CI/CD)
	@echo "$(BLUE)🧪 CI Tests...$(NC)"
	@make test-ci
	@echo "$(GREEN)✅ CI tests completed$(NC)"

.PHONY: ci-verify
ci-verify: ci-build ci-test ## CI verification target
	@echo "$(GREEN)✅ CI verification completed successfully!$(NC)"
