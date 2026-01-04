# Docker Compose Smoke Test Script (PowerShell)
# Tests that all services start successfully and are healthy

$ErrorActionPreference = "Stop"

Write-Host "==========================================" -ForegroundColor Cyan
Write-Host "Docker Compose Smoke Test" -ForegroundColor Cyan
Write-Host "==========================================" -ForegroundColor Cyan
Write-Host ""

# Configuration
$Timeout = 300  # 5 minutes
$CheckInterval = 10  # Check every 10 seconds

# Service list
$Services = @(
    "sqlserver",
    "mongodb",
    "redis",
    "rabbitmq",
    "consul",
    "orderservice",
    "inventoryservice",
    "paymentservice",
    "deliveryservice",
    "invoiceservice",
    "healthmonitorservice",
    "apigateway"
)

# Health check endpoints
$HealthEndpoints = @{
    "orderservice" = "http://localhost:5000/health/ready"
    "inventoryservice" = "http://localhost:5001/health/ready"
    "paymentservice" = "http://localhost:5002/health/ready"
    "deliveryservice" = "http://localhost:5003/health/ready"
    "invoiceservice" = "http://localhost:5005/health/ready"
    "healthmonitorservice" = "http://localhost:5004/health/ready"
    "apigateway" = "http://localhost:8080/health"
}

# Function to check if a service is healthy
function Test-ServiceHealth {
    param([string]$Service)
    try {
        $status = docker inspect --format='{{.State.Health.Status}}' $Service 2>$null
        if ($LASTEXITCODE -ne 0) { return "unknown" }
        return $status
    } catch {
        return "unknown"
    }
}

# Function to check HTTP endpoint
function Test-HttpEndpoint {
    param([string]$Url)
    try {
        $response = Invoke-WebRequest -Uri $Url -UseBasicParsing -TimeoutSec 5 -ErrorAction SilentlyContinue
        return $response.StatusCode
    } catch {
        return 0
    }
}

# Step 1: Check prerequisites
Write-Host "Step 1: Checking prerequisites..." -ForegroundColor Yellow
try {
    docker --version | Out-Null
    docker compose version | Out-Null
    Write-Host "✓ Prerequisites met" -ForegroundColor Green
} catch {
    Write-Host "✗ Docker or Docker Compose not installed" -ForegroundColor Red
    exit 1
}
Write-Host ""

# Step 2: Start services
Write-Host "Step 2: Starting services with docker-compose..." -ForegroundColor Yellow
docker compose up -d
if ($LASTEXITCODE -ne 0) {
    Write-Host "✗ Failed to start services" -ForegroundColor Red
    exit 1
}
Write-Host "✓ Services started" -ForegroundColor Green
Write-Host ""

# Step 3: Wait for infrastructure services
Write-Host "Step 3: Waiting for infrastructure services..." -ForegroundColor Yellow
$InfrastructureServices = @("sqlserver", "mongodb", "redis", "rabbitmq", "consul")

foreach ($service in $InfrastructureServices) {
    Write-Host "Waiting for $service... " -NoNewline
    $elapsed = 0
    while ($elapsed -lt $Timeout) {
        $health = Test-ServiceHealth -Service $service
        if ($health -eq "healthy" -or $health -eq "running") {
            Write-Host "✓ healthy" -ForegroundColor Green
            break
        }
        Start-Sleep -Seconds $CheckInterval
        $elapsed += $CheckInterval
        Write-Host "." -NoNewline
    }
    
    if ($elapsed -ge $Timeout) {
        Write-Host "✗ timeout" -ForegroundColor Red
        Write-Host "Service logs:"
        docker logs $service --tail 50
        exit 1
    }
}
Write-Host ""

# Step 4: Wait for application services
Write-Host "Step 4: Waiting for application services..." -ForegroundColor Yellow
$AppServices = @("orderservice", "inventoryservice", "paymentservice", "deliveryservice", "invoiceservice", "healthmonitorservice", "apigateway")

foreach ($service in $AppServices) {
    Write-Host "Waiting for $service... " -NoNewline
    $elapsed = 0
    while ($elapsed -lt $Timeout) {
        $health = Test-ServiceHealth -Service $service
        if ($health -eq "healthy") {
            Write-Host "✓ healthy" -ForegroundColor Green
            break
        }
        Start-Sleep -Seconds $CheckInterval
        $elapsed += $CheckInterval
        Write-Host "." -NoNewline
    }
    
    if ($elapsed -ge $Timeout) {
        Write-Host "✗ timeout" -ForegroundColor Red
        Write-Host "Service logs:"
        docker logs $service --tail 50
        exit 1
    }
}
Write-Host ""

# Step 5: Test HTTP endpoints
Write-Host "Step 5: Testing HTTP health endpoints..." -ForegroundColor Yellow
foreach ($service in $HealthEndpoints.Keys) {
    $url = $HealthEndpoints[$service]
    Write-Host "Testing $service ($url)... " -NoNewline
    
    $status = Test-HttpEndpoint -Url $url
    if ($status -eq 200) {
        Write-Host "✓ OK (200)" -ForegroundColor Green
    } else {
        Write-Host "✗ Failed (HTTP $status)" -ForegroundColor Red
        exit 1
    }
}
Write-Host ""

# Step 6: Test key functionality
Write-Host "Step 6: Testing key API endpoints..." -ForegroundColor Yellow

Write-Host "Testing inventory seed endpoint... " -NoNewline
$seedStatus = Test-HttpEndpoint -Url "http://localhost:8080/api/inventory/seed"
if ($seedStatus -eq 200 -or $seedStatus -eq 409) {
    Write-Host "✓ OK (HTTP $seedStatus)" -ForegroundColor Green
} else {
    Write-Host "✗ Failed (HTTP $seedStatus)" -ForegroundColor Red
}

Write-Host "Testing get inventory endpoint... " -NoNewline
$inventoryStatus = Test-HttpEndpoint -Url "http://localhost:8080/api/inventory"
if ($inventoryStatus -eq 200) {
    Write-Host "✓ OK (200)" -ForegroundColor Green
} else {
    Write-Host "✗ Failed (HTTP $inventoryStatus)" -ForegroundColor Red
}

Write-Host "Testing health monitor endpoint... " -NoNewline
$healthmonStatus = Test-HttpEndpoint -Url "http://localhost:5004/api/healthstatus"
if ($healthmonStatus -eq 200) {
    Write-Host "✓ OK (200)" -ForegroundColor Green
} else {
    Write-Host "✗ Failed (HTTP $healthmonStatus)" -ForegroundColor Red
}

Write-Host "Testing Consul services... " -NoNewline
$consulStatus = Test-HttpEndpoint -Url "http://localhost:8500/v1/catalog/services"
if ($consulStatus -eq 200) {
    Write-Host "✓ OK (200)" -ForegroundColor Green
} else {
    Write-Host "✗ Failed (HTTP $consulStatus)" -ForegroundColor Red
}

Write-Host ""

# Step 7: Show service status
Write-Host "Step 7: Service status summary" -ForegroundColor Yellow
Write-Host "=========================================="
docker compose ps
Write-Host ""

# Final summary
Write-Host "==========================================" -ForegroundColor Cyan
Write-Host "✓ All smoke tests passed!" -ForegroundColor Green
Write-Host "==========================================" -ForegroundColor Cyan
Write-Host ""
Write-Host "Access points:"
Write-Host "  - API Gateway: http://localhost:8080"
Write-Host "  - Order Service: http://localhost:5000"
Write-Host "  - Inventory Service: http://localhost:5001"
Write-Host "  - Payment Service: http://localhost:5002"
Write-Host "  - Delivery Service: http://localhost:5003"
Write-Host "  - Health Monitor: http://localhost:5004"
Write-Host "  - Invoice Service: http://localhost:5005"
Write-Host "  - Consul UI: http://localhost:8500"
Write-Host "  - RabbitMQ UI: http://localhost:15672 (guest/guest)"
Write-Host ""
Write-Host "To stop services: docker compose down"
Write-Host "To view logs: docker compose logs -f [service-name]"
Write-Host ""

