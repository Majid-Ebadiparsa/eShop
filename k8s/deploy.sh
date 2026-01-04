#!/bin/bash
# eShop Kubernetes Deployment Script
# Usage: ./deploy.sh [environment] [--dry-run]
#   environment: dev, staging, prod (default: dev)
#   --dry-run: Validate manifests without applying

set -e

ENVIRONMENT=${1:-dev}
DRY_RUN=${2}

echo "========================================="
echo "eShop Kubernetes Deployment"
echo "Environment: $ENVIRONMENT"
echo "========================================="

# Color codes for output
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
RED='\033[0;31m'
NC='\033[0m' # No Color

# Function to print colored output
print_info() {
    echo -e "${GREEN}[INFO]${NC} $1"
}

print_warning() {
    echo -e "${YELLOW}[WARN]${NC} $1"
}

print_error() {
    echo -e "${RED}[ERROR]${NC} $1"
}

# Check if kubectl is installed
if ! command -v kubectl &> /dev/null; then
    print_error "kubectl not found. Please install kubectl first."
    exit 1
fi

# Check if cluster is accessible
if ! kubectl cluster-info &> /dev/null; then
    print_error "Cannot connect to Kubernetes cluster. Please check your kubeconfig."
    exit 1
fi

print_info "Connected to cluster: $(kubectl config current-context)"

# Dry-run flag for kubectl
KUBECTL_FLAGS=""
if [ "$DRY_RUN" == "--dry-run" ]; then
    KUBECTL_FLAGS="--dry-run=client"
    print_warning "DRY RUN MODE - No resources will be created"
fi

# Step 1: Create namespace
print_info "Step 1: Creating namespace..."
kubectl apply -f namespace.yaml $KUBECTL_FLAGS

# Step 2: Apply ConfigMaps and Secrets
print_info "Step 2: Applying ConfigMaps and Secrets..."
kubectl apply -f configmap.yaml $KUBECTL_FLAGS
kubectl apply -f secrets.yaml $KUBECTL_FLAGS

# Step 3: Deploy Infrastructure (databases, message broker, cache, service discovery)
print_info "Step 3: Deploying infrastructure services..."
kubectl apply -f infrastructure/sqlserver.yaml $KUBECTL_FLAGS
kubectl apply -f infrastructure/mongodb.yaml $KUBECTL_FLAGS
kubectl apply -f infrastructure/redis.yaml $KUBECTL_FLAGS
kubectl apply -f infrastructure/rabbitmq.yaml $KUBECTL_FLAGS
kubectl apply -f infrastructure/consul.yaml $KUBECTL_FLAGS

# Wait for infrastructure to be ready (skip in dry-run)
if [ "$DRY_RUN" != "--dry-run" ]; then
    print_info "Waiting for infrastructure to be ready (this may take a few minutes)..."
    
    print_info "  - Waiting for SQL Server..."
    kubectl wait --for=condition=ready pod -l app=sqlserver -n eshop --timeout=300s || print_warning "SQL Server not ready yet"
    
    print_info "  - Waiting for MongoDB..."
    kubectl wait --for=condition=ready pod -l app=mongodb -n eshop --timeout=300s || print_warning "MongoDB not ready yet"
    
    print_info "  - Waiting for Redis..."
    kubectl wait --for=condition=ready pod -l app=redis -n eshop --timeout=180s || print_warning "Redis not ready yet"
    
    print_info "  - Waiting for RabbitMQ..."
    kubectl wait --for=condition=ready pod -l app=rabbitmq -n eshop --timeout=300s || print_warning "RabbitMQ not ready yet"
    
    print_info "  - Waiting for Consul..."
    kubectl wait --for=condition=ready pod -l app=consul -n eshop --timeout=180s || print_warning "Consul not ready yet"
    
    sleep 10
fi

# Step 4: Deploy Microservices
print_info "Step 4: Deploying microservices..."
kubectl apply -f services/orderservice.yaml $KUBECTL_FLAGS
kubectl apply -f services/inventoryservice.yaml $KUBECTL_FLAGS
kubectl apply -f services/paymentservice.yaml $KUBECTL_FLAGS
kubectl apply -f services/deliveryservice.yaml $KUBECTL_FLAGS
kubectl apply -f services/invoiceservice.yaml $KUBECTL_FLAGS
kubectl apply -f services/healthmonitorservice.yaml $KUBECTL_FLAGS

# Wait for services to be ready (skip in dry-run)
if [ "$DRY_RUN" != "--dry-run" ]; then
    print_info "Waiting for microservices to be ready..."
    sleep 20
    kubectl wait --for=condition=ready pod -l app=orderservice -n eshop --timeout=180s || print_warning "OrderService not ready yet"
    kubectl wait --for=condition=ready pod -l app=inventoryservice -n eshop --timeout=180s || print_warning "InventoryService not ready yet"
    kubectl wait --for=condition=ready pod -l app=paymentservice -n eshop --timeout=180s || print_warning "PaymentService not ready yet"
    kubectl wait --for=condition=ready pod -l app=deliveryservice -n eshop --timeout=180s || print_warning "DeliveryService not ready yet"
    kubectl wait --for=condition=ready pod -l app=invoiceservice -n eshop --timeout=180s || print_warning "InvoiceService not ready yet"
    kubectl wait --for=condition=ready pod -l app=healthmonitorservice -n eshop --timeout=180s || print_warning "HealthMonitorService not ready yet"
fi

# Step 5: Deploy Gateway and UI
print_info "Step 5: Deploying API Gateway and UI..."
kubectl apply -f gateway/apigateway.yaml $KUBECTL_FLAGS
kubectl apply -f gateway/ui.yaml $KUBECTL_FLAGS

# Step 6: Apply Ingress (optional)
print_info "Step 6: Applying Ingress..."
kubectl apply -f ingress.yaml $KUBECTL_FLAGS || print_warning "Ingress controller may not be installed"

# Display deployment status
if [ "$DRY_RUN" != "--dry-run" ]; then
    echo ""
    echo "========================================="
    echo "Deployment Summary"
    echo "========================================="
    
    print_info "Pods Status:"
    kubectl get pods -n eshop
    
    echo ""
    print_info "Services:"
    kubectl get svc -n eshop
    
    echo ""
    print_info "Ingress:"
    kubectl get ingress -n eshop
    
    echo ""
    echo "========================================="
    print_info "Deployment Complete!"
    echo "========================================="
    
    # Get external IPs
    API_GATEWAY_IP=$(kubectl get svc apigateway -n eshop -o jsonpath='{.status.loadBalancer.ingress[0].ip}' 2>/dev/null || echo "pending")
    UI_IP=$(kubectl get svc eshop-ui -n eshop -o jsonpath='{.status.loadBalancer.ingress[0].ip}' 2>/dev/null || echo "pending")
    
    echo ""
    print_info "Access URLs:"
    echo "  API Gateway: http://$API_GATEWAY_IP (or http://api.eshop.local via Ingress)"
    echo "  eShop UI: http://$UI_IP (or http://eshop.local via Ingress)"
    echo "  Health Monitor: http://health.eshop.local (via Ingress)"
    echo "  RabbitMQ Management: http://rabbitmq.eshop.local (via Ingress)"
    echo "  Consul UI: http://consul.eshop.local (via Ingress)"
    
    echo ""
    print_info "Next Steps:"
    echo "  1. Wait for LoadBalancer IPs to be assigned (if using LoadBalancer)"
    echo "  2. Update /etc/hosts with Ingress hostnames if using local cluster"
    echo "  3. Run smoke tests: ./test-deployment.sh"
    echo "  4. Monitor logs: kubectl logs -f -l app=orderservice -n eshop"
else
    print_info "Dry-run validation complete! No resources were created."
fi

