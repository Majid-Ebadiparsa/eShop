#!/bin/bash
# eShop Kubernetes Teardown Script
# Usage: ./teardown.sh [--force]
#   --force: Skip confirmation prompt

set -e

FORCE=${1}

echo "========================================="
echo "eShop Kubernetes Teardown"
echo "========================================="

# Color codes
RED='\033[0;31m'
YELLOW='\033[1;33m'
GREEN='\033[0;32m'
NC='\033[0m'

print_warning() {
    echo -e "${YELLOW}[WARN]${NC} $1"
}

print_info() {
    echo -e "${GREEN}[INFO]${NC} $1"
}

print_error() {
    echo -e "${RED}[ERROR]${NC} $1"
}

# Check if namespace exists
if ! kubectl get namespace eshop &> /dev/null; then
    print_info "Namespace 'eshop' does not exist. Nothing to teardown."
    exit 0
fi

# Confirmation prompt
if [ "$FORCE" != "--force" ]; then
    print_warning "This will DELETE all eShop resources in the 'eshop' namespace."
    print_warning "This includes all data in databases, caches, and message queues."
    echo ""
    read -p "Are you sure you want to continue? (yes/no): " confirm
    
    if [ "$confirm" != "yes" ]; then
        print_info "Teardown cancelled."
        exit 0
    fi
fi

print_info "Starting teardown..."

# Delete in reverse order of deployment
print_info "Step 1: Deleting Ingress..."
kubectl delete -f ingress.yaml --ignore-not-found=true

print_info "Step 2: Deleting Gateway and UI..."
kubectl delete -f gateway/apigateway.yaml --ignore-not-found=true
kubectl delete -f gateway/ui.yaml --ignore-not-found=true

print_info "Step 3: Deleting Microservices..."
kubectl delete -f services/ --ignore-not-found=true

print_info "Step 4: Deleting Infrastructure..."
kubectl delete -f infrastructure/ --ignore-not-found=true

print_info "Step 5: Deleting ConfigMaps and Secrets..."
kubectl delete -f configmap.yaml --ignore-not-found=true
kubectl delete -f secrets.yaml --ignore-not-found=true

print_info "Step 6: Deleting Namespace..."
kubectl delete -f namespace.yaml --ignore-not-found=true

print_info "Waiting for namespace to be fully deleted..."
kubectl wait --for=delete namespace/eshop --timeout=300s || print_warning "Namespace deletion timed out"

echo ""
echo "========================================="
print_info "Teardown Complete!"
echo "========================================="

