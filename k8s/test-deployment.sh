#!/bin/bash
# eShop Kubernetes Deployment Test Script
# Tests all services for health and basic functionality

set -e

echo "========================================="
echo "eShop Kubernetes Deployment Tests"
echo "========================================="

GREEN='\033[0;32m'
RED='\033[0;31m'
YELLOW='\033[1;33m'
NC='\033[0m'

PASSED=0
FAILED=0

test_result() {
    if [ $1 -eq 0 ]; then
        echo -e "${GREEN}✓ PASSED${NC}: $2"
        ((PASSED++))
    else
        echo -e "${RED}✗ FAILED${NC}: $2"
        ((FAILED++))
    fi
}

echo ""
echo "Test 1: Checking if all pods are running..."
ALL_RUNNING=$(kubectl get pods -n eshop --field-selector=status.phase!=Running --no-headers 2>/dev/null | wc -l)
test_result $ALL_RUNNING "All pods should be in Running state"

echo ""
echo "Test 2: Checking infrastructure services..."

echo "  - Testing SQL Server..."
kubectl exec -n eshop sqlserver-0 -- /opt/mssql-tools/bin/sqlcmd -S localhost -U sa -P "YourStrong@Passw0rd" -Q "SELECT 1" > /dev/null 2>&1
test_result $? "SQL Server is accessible"

echo "  - Testing MongoDB..."
kubectl exec -n eshop mongodb-0 -- mongosh --quiet --eval "db.adminCommand('ping')" > /dev/null 2>&1
test_result $? "MongoDB is accessible"

echo "  - Testing Redis..."
kubectl exec -n eshop redis-0 -- redis-cli ping > /dev/null 2>&1
test_result $? "Redis is accessible"

echo "  - Testing RabbitMQ..."
kubectl exec -n eshop rabbitmq-0 -- rabbitmq-diagnostics -q ping > /dev/null 2>&1
test_result $? "RabbitMQ is accessible"

echo "  - Testing Consul..."
kubectl exec -n eshop consul-0 -- consul info > /dev/null 2>&1
test_result $? "Consul is accessible"

echo ""
echo "Test 3: Checking microservices health endpoints..."

SERVICES=("orderservice" "inventoryservice" "paymentservice" "deliveryservice" "invoiceservice" "healthmonitorservice")

for service in "${SERVICES[@]}"; do
    echo "  - Testing $service..."
    
    # Port forward in background
    kubectl port-forward -n eshop svc/$service 18080:80 > /dev/null 2>&1 &
    PF_PID=$!
    sleep 2
    
    # Test health endpoint
    curl -sf http://localhost:18080/health/ready > /dev/null 2>&1
    RESULT=$?
    
    # Kill port-forward
    kill $PF_PID 2>/dev/null || true
    wait $PF_PID 2>/dev/null || true
    
    test_result $RESULT "$service health endpoint is responding"
done

echo ""
echo "Test 4: Checking API Gateway..."
kubectl port-forward -n eshop svc/apigateway 18080:80 > /dev/null 2>&1 &
PF_PID=$!
sleep 2

curl -sf http://localhost:18080/health > /dev/null 2>&1
RESULT=$?
kill $PF_PID 2>/dev/null || true
wait $PF_PID 2>/dev/null || true

test_result $RESULT "API Gateway is responding"

echo ""
echo "Test 5: Checking service registration in Consul..."
kubectl port-forward -n eshop svc/consul 18500:8500 > /dev/null 2>&1 &
PF_PID=$!
sleep 2

REGISTERED_SERVICES=$(curl -sf http://localhost:18500/v1/catalog/services 2>/dev/null | grep -o "service" | wc -l)
kill $PF_PID 2>/dev/null || true
wait $PF_PID 2>/dev/null || true

if [ $REGISTERED_SERVICES -gt 0 ]; then
    test_result 0 "Services are registered in Consul ($REGISTERED_SERVICES services found)"
else
    test_result 1 "No services found in Consul"
fi

echo ""
echo "Test 6: Checking persistent volumes..."
PVC_COUNT=$(kubectl get pvc -n eshop --field-selector=status.phase=Bound --no-headers 2>/dev/null | wc -l)
if [ $PVC_COUNT -ge 5 ]; then
    test_result 0 "Persistent volumes are bound ($PVC_COUNT PVCs)"
else
    test_result 1 "Some persistent volumes are not bound (found $PVC_COUNT)"
fi

echo ""
echo "========================================="
echo "Test Summary"
echo "========================================="
echo -e "${GREEN}Passed: $PASSED${NC}"
echo -e "${RED}Failed: $FAILED${NC}"
echo ""

if [ $FAILED -eq 0 ]; then
    echo -e "${GREEN}All tests passed! ✓${NC}"
    exit 0
else
    echo -e "${RED}Some tests failed. Please check the logs above.${NC}"
    exit 1
fi

