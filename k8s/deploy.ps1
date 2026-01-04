# eShop Kubernetes Deployment Script (PowerShell)
# Usage: .\deploy.ps1 [-Environment dev|staging|prod] [-DryRun]

param(
    [Parameter(Mandatory=$false)]
    [ValidateSet("dev", "staging", "prod")]
    [string]$Environment = "dev",
    
    [Parameter(Mandatory=$false)]
    [switch]$DryRun
)

$ErrorActionPreference = "Stop"

Write-Host "=========================================" -ForegroundColor Cyan
Write-Host "eShop Kubernetes Deployment" -ForegroundColor Cyan
Write-Host "Environment: $Environment" -ForegroundColor Cyan
Write-Host "=========================================" -ForegroundColor Cyan

function Write-Info {
    param([string]$Message)
    Write-Host "[INFO] $Message" -ForegroundColor Green
}

function Write-Warning {
    param([string]$Message)
    Write-Host "[WARN] $Message" -ForegroundColor Yellow
}

function Write-Error {
    param([string]$Message)
    Write-Host "[ERROR] $Message" -ForegroundColor Red
}

# Check if kubectl is installed
try {
    $null = kubectl version --client 2>&1
} catch {
    Write-Error "kubectl not found. Please install kubectl first."
    exit 1
}

# Check if cluster is accessible
try {
    $null = kubectl cluster-info 2>&1
} catch {
    Write-Error "Cannot connect to Kubernetes cluster. Please check your kubeconfig."
    exit 1
}

$currentContext = kubectl config current-context
Write-Info "Connected to cluster: $currentContext"

# Dry-run flag for kubectl
$kubectlFlags = @()
if ($DryRun) {
    $kubectlFlags += "--dry-run=client"
    Write-Warning "DRY RUN MODE - No resources will be created"
}

# Step 1: Create namespace
Write-Info "Step 1: Creating namespace..."
kubectl apply -f namespace.yaml @kubectlFlags

# Step 2: Apply ConfigMaps and Secrets
Write-Info "Step 2: Applying ConfigMaps and Secrets..."
kubectl apply -f configmap.yaml @kubectlFlags
kubectl apply -f secrets.yaml @kubectlFlags

# Step 3: Deploy Infrastructure
Write-Info "Step 3: Deploying infrastructure services..."
kubectl apply -f infrastructure/sqlserver.yaml @kubectlFlags
kubectl apply -f infrastructure/mongodb.yaml @kubectlFlags
kubectl apply -f infrastructure/redis.yaml @kubectlFlags
kubectl apply -f infrastructure/rabbitmq.yaml @kubectlFlags
kubectl apply -f infrastructure/consul.yaml @kubectlFlags

# Wait for infrastructure (skip in dry-run)
if (-not $DryRun) {
    Write-Info "Waiting for infrastructure to be ready (this may take a few minutes)..."
    
    Write-Info "  - Waiting for SQL Server..."
    kubectl wait --for=condition=ready pod -l app=sqlserver -n eshop --timeout=300s
    
    Write-Info "  - Waiting for MongoDB..."
    kubectl wait --for=condition=ready pod -l app=mongodb -n eshop --timeout=300s
    
    Write-Info "  - Waiting for Redis..."
    kubectl wait --for=condition=ready pod -l app=redis -n eshop --timeout=180s
    
    Write-Info "  - Waiting for RabbitMQ..."
    kubectl wait --for=condition=ready pod -l app=rabbitmq -n eshop --timeout=300s
    
    Write-Info "  - Waiting for Consul..."
    kubectl wait --for=condition=ready pod -l app=consul -n eshop --timeout=180s
    
    Start-Sleep -Seconds 10
}

# Step 4: Deploy Microservices
Write-Info "Step 4: Deploying microservices..."
kubectl apply -f services/orderservice.yaml @kubectlFlags
kubectl apply -f services/inventoryservice.yaml @kubectlFlags
kubectl apply -f services/paymentservice.yaml @kubectlFlags
kubectl apply -f services/deliveryservice.yaml @kubectlFlags
kubectl apply -f services/invoiceservice.yaml @kubectlFlags
kubectl apply -f services/healthmonitorservice.yaml @kubectlFlags

# Wait for services (skip in dry-run)
if (-not $DryRun) {
    Write-Info "Waiting for microservices to be ready..."
    Start-Sleep -Seconds 20
    kubectl wait --for=condition=ready pod -l app=orderservice -n eshop --timeout=180s
    kubectl wait --for=condition=ready pod -l app=inventoryservice -n eshop --timeout=180s
    kubectl wait --for=condition=ready pod -l app=paymentservice -n eshop --timeout=180s
    kubectl wait --for=condition=ready pod -l app=deliveryservice -n eshop --timeout=180s
    kubectl wait --for=condition=ready pod -l app=invoiceservice -n eshop --timeout=180s
    kubectl wait --for=condition=ready pod -l app=healthmonitorservice -n eshop --timeout=180s
}

# Step 5: Deploy Gateway and UI
Write-Info "Step 5: Deploying API Gateway and UI..."
kubectl apply -f gateway/apigateway.yaml @kubectlFlags
kubectl apply -f gateway/ui.yaml @kubectlFlags

# Step 6: Apply Ingress
Write-Info "Step 6: Applying Ingress..."
try {
    kubectl apply -f ingress.yaml @kubectlFlags
} catch {
    Write-Warning "Ingress controller may not be installed"
}

# Display deployment status
if (-not $DryRun) {
    Write-Host ""
    Write-Host "=========================================" -ForegroundColor Cyan
    Write-Host "Deployment Summary" -ForegroundColor Cyan
    Write-Host "=========================================" -ForegroundColor Cyan
    
    Write-Info "Pods Status:"
    kubectl get pods -n eshop
    
    Write-Host ""
    Write-Info "Services:"
    kubectl get svc -n eshop
    
    Write-Host ""
    Write-Info "Ingress:"
    kubectl get ingress -n eshop
    
    Write-Host ""
    Write-Host "=========================================" -ForegroundColor Cyan
    Write-Info "Deployment Complete!"
    Write-Host "=========================================" -ForegroundColor Cyan
    
    Write-Host ""
    Write-Info "Access URLs:"
    Write-Host "  API Gateway: http://api.eshop.local (via Ingress)"
    Write-Host "  eShop UI: http://eshop.local (via Ingress)"
    Write-Host "  Health Monitor: http://health.eshop.local (via Ingress)"
    Write-Host "  RabbitMQ Management: http://rabbitmq.eshop.local (via Ingress)"
    Write-Host "  Consul UI: http://consul.eshop.local (via Ingress)"
    
    Write-Host ""
    Write-Info "Next Steps:"
    Write-Host "  1. Update C:\Windows\System32\drivers\etc\hosts with Ingress hostnames"
    Write-Host "  2. Run smoke tests: .\test-deployment.ps1"
    Write-Host "  3. Monitor logs: kubectl logs -f -l app=orderservice -n eshop"
} else {
    Write-Info "Dry-run validation complete! No resources were created."
}

