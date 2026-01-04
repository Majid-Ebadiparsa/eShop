# eShop Kubernetes Teardown Script (PowerShell)
# Usage: .\teardown.ps1 [-Force]

param(
    [Parameter(Mandatory=$false)]
    [switch]$Force
)

$ErrorActionPreference = "Stop"

Write-Host "=========================================" -ForegroundColor Cyan
Write-Host "eShop Kubernetes Teardown" -ForegroundColor Cyan
Write-Host "=========================================" -ForegroundColor Cyan

function Write-Info {
    param([string]$Message)
    Write-Host "[INFO] $Message" -ForegroundColor Green
}

function Write-Warning {
    param([string]$Message)
    Write-Host "[WARN] $Message" -ForegroundColor Yellow
}

# Check if namespace exists
try {
    $null = kubectl get namespace eshop 2>&1
} catch {
    Write-Info "Namespace 'eshop' does not exist. Nothing to teardown."
    exit 0
}

# Confirmation prompt
if (-not $Force) {
    Write-Warning "This will DELETE all eShop resources in the 'eshop' namespace."
    Write-Warning "This includes all data in databases, caches, and message queues."
    Write-Host ""
    $confirm = Read-Host "Are you sure you want to continue? (yes/no)"
    
    if ($confirm -ne "yes") {
        Write-Info "Teardown cancelled."
        exit 0
    }
}

Write-Info "Starting teardown..."

# Delete in reverse order
Write-Info "Step 1: Deleting Ingress..."
kubectl delete -f ingress.yaml --ignore-not-found=true

Write-Info "Step 2: Deleting Gateway and UI..."
kubectl delete -f gateway/apigateway.yaml --ignore-not-found=true
kubectl delete -f gateway/ui.yaml --ignore-not-found=true

Write-Info "Step 3: Deleting Microservices..."
Get-ChildItem -Path services/*.yaml | ForEach-Object {
    kubectl delete -f $_.FullName --ignore-not-found=true
}

Write-Info "Step 4: Deleting Infrastructure..."
Get-ChildItem -Path infrastructure/*.yaml | ForEach-Object {
    kubectl delete -f $_.FullName --ignore-not-found=true
}

Write-Info "Step 5: Deleting ConfigMaps and Secrets..."
kubectl delete -f configmap.yaml --ignore-not-found=true
kubectl delete -f secrets.yaml --ignore-not-found=true

Write-Info "Step 6: Deleting Namespace..."
kubectl delete -f namespace.yaml --ignore-not-found=true

Write-Info "Waiting for namespace to be fully deleted..."
kubectl wait --for=delete namespace/eshop --timeout=300s

Write-Host ""
Write-Host "=========================================" -ForegroundColor Cyan
Write-Info "Teardown Complete!"
Write-Host "=========================================" -ForegroundColor Cyan

