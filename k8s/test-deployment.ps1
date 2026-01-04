# eShop Kubernetes Deployment Test Script (PowerShell)
# Tests all services for health and basic functionality

$ErrorActionPreference = "Continue"

Write-Host "=========================================" -ForegroundColor Cyan
Write-Host "eShop Kubernetes Deployment Tests" -ForegroundColor Cyan
Write-Host "=========================================" -ForegroundColor Cyan

$Passed = 0
$Failed = 0

function Test-Result {
    param(
        [bool]$Success,
        [string]$Message
    )
    
    if ($Success) {
        Write-Host "✓ PASSED: $Message" -ForegroundColor Green
        $script:Passed++
    } else {
        Write-Host "✗ FAILED: $Message" -ForegroundColor Red
        $script:Failed++
    }
}

Write-Host ""
Write-Host "Test 1: Checking if all pods are running..."
$NotRunning = (kubectl get pods -n eshop --field-selector=status.phase!=Running --no-headers 2>$null).Count
Test-Result ($NotRunning -eq 0) "All pods should be in Running state"

Write-Host ""
Write-Host "Test 2: Checking infrastructure services..."

Write-Host "  - Testing SQL Server..."
$result = kubectl exec -n eshop sqlserver-0 -- /opt/mssql-tools/bin/sqlcmd -S localhost -U sa -P "YourStrong@Passw0rd" -Q "SELECT 1" 2>$null
Test-Result ($LASTEXITCODE -eq 0) "SQL Server is accessible"

Write-Host "  - Testing MongoDB..."
$result = kubectl exec -n eshop mongodb-0 -- mongosh --quiet --eval "db.adminCommand('ping')" 2>$null
Test-Result ($LASTEXITCODE -eq 0) "MongoDB is accessible"

Write-Host "  - Testing Redis..."
$result = kubectl exec -n eshop redis-0 -- redis-cli ping 2>$null
Test-Result ($LASTEXITCODE -eq 0) "Redis is accessible"

Write-Host "  - Testing RabbitMQ..."
$result = kubectl exec -n eshop rabbitmq-0 -- rabbitmq-diagnostics -q ping 2>$null
Test-Result ($LASTEXITCODE -eq 0) "RabbitMQ is accessible"

Write-Host "  - Testing Consul..."
$result = kubectl exec -n eshop consul-0 -- consul info 2>$null
Test-Result ($LASTEXITCODE -eq 0) "Consul is accessible"

Write-Host ""
Write-Host "Test 3: Checking microservices health endpoints..."

$services = @("orderservice", "inventoryservice", "paymentservice", "deliveryservice", "invoiceservice", "healthmonitorservice")

foreach ($service in $services) {
    Write-Host "  - Testing $service..."
    
    # Port forward in background
    $job = Start-Job -ScriptBlock {
        param($svc)
        kubectl port-forward -n eshop svc/$svc 18080:80
    } -ArgumentList $service
    
    Start-Sleep -Seconds 2
    
    # Test health endpoint
    try {
        $response = Invoke-WebRequest -Uri "http://localhost:18080/health/ready" -TimeoutSec 5 -UseBasicParsing 2>$null
        $success = $response.StatusCode -eq 200
    } catch {
        $success = $false
    }
    
    # Kill port-forward job
    Stop-Job -Job $job 2>$null
    Remove-Job -Job $job 2>$null
    
    Test-Result $success "$service health endpoint is responding"
}

Write-Host ""
Write-Host "Test 4: Checking API Gateway..."
$job = Start-Job -ScriptBlock {
    kubectl port-forward -n eshop svc/apigateway 18080:80
}

Start-Sleep -Seconds 2

try {
    $response = Invoke-WebRequest -Uri "http://localhost:18080/health" -TimeoutSec 5 -UseBasicParsing 2>$null
    $success = $response.StatusCode -eq 200
} catch {
    $success = $false
}

Stop-Job -Job $job 2>$null
Remove-Job -Job $job 2>$null

Test-Result $success "API Gateway is responding"

Write-Host ""
Write-Host "Test 5: Checking service registration in Consul..."
$job = Start-Job -ScriptBlock {
    kubectl port-forward -n eshop svc/consul 18500:8500
}

Start-Sleep -Seconds 2

try {
    $response = Invoke-WebRequest -Uri "http://localhost:18500/v1/catalog/services" -TimeoutSec 5 -UseBasicParsing 2>$null
    $services = ($response.Content | ConvertFrom-Json).PSObject.Properties.Count
    $success = $services -gt 0
} catch {
    $success = $false
    $services = 0
}

Stop-Job -Job $job 2>$null
Remove-Job -Job $job 2>$null

Test-Result $success "Services are registered in Consul ($services services found)"

Write-Host ""
Write-Host "Test 6: Checking persistent volumes..."
$boundPVCs = (kubectl get pvc -n eshop --field-selector=status.phase=Bound --no-headers 2>$null).Count
Test-Result ($boundPVCs -ge 5) "Persistent volumes are bound ($boundPVCs PVCs)"

Write-Host ""
Write-Host "=========================================" -ForegroundColor Cyan
Write-Host "Test Summary" -ForegroundColor Cyan
Write-Host "=========================================" -ForegroundColor Cyan
Write-Host "Passed: $Passed" -ForegroundColor Green
Write-Host "Failed: $Failed" -ForegroundColor Red
Write-Host ""

if ($Failed -eq 0) {
    Write-Host "All tests passed! ✓" -ForegroundColor Green
    exit 0
} else {
    Write-Host "Some tests failed. Please check the logs above." -ForegroundColor Red
    exit 1
}

