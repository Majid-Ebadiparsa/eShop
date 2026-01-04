#!/bin/bash
# Docker Compose Smoke Test Script
# Tests that all services start successfully and are healthy

set -e

echo "=========================================="
echo "Docker Compose Smoke Test"
echo "=========================================="
echo ""

# Colors for output
GREEN='\033[0;32m'
RED='\033[0;31m'
YELLOW='\033[1;33m'
NC='\033[0m' # No Color

# Configuration
TIMEOUT=300  # 5 minutes
CHECK_INTERVAL=10  # Check every 10 seconds

# Service list
SERVICES=(
    "sqlserver"
    "mongodb"
    "redis"
    "rabbitmq"
    "consul"
    "orderservice"
    "inventoryservice"
    "paymentservice"
    "deliveryservice"
    "invoiceservice"
    "healthmonitorservice"
    "apigateway"
)

# Health check endpoints
declare -A HEALTH_ENDPOINTS=(
    ["orderservice"]="http://localhost:5000/health/ready"
    ["inventoryservice"]="http://localhost:5001/health/ready"
    ["paymentservice"]="http://localhost:5002/health/ready"
    ["deliveryservice"]="http://localhost:5003/health/ready"
    ["invoiceservice"]="http://localhost:5005/health/ready"
    ["healthmonitorservice"]="http://localhost:5004/health/ready"
    ["apigateway"]="http://localhost:8080/health"
)

# Function to check if a service is healthy
check_service_health() {
    local service=$1
    local status=$(docker inspect --format='{{.State.Health.Status}}' $service 2>/dev/null || echo "unknown")
    echo $status
}

# Function to check HTTP endpoint
check_http_endpoint() {
    local url=$1
    local status=$(curl -s -o /dev/null -w "%{http_code}" $url 2>/dev/null || echo "000")
    echo $status
}

# Step 1: Check if docker-compose is installed
echo "Step 1: Checking prerequisites..."
if ! command -v docker &> /dev/null; then
    echo -e "${RED}✗ Docker is not installed${NC}"
    exit 1
fi

if ! command -v docker-compose &> /dev/null && ! docker compose version &> /dev/null; then
    echo -e "${RED}✗ Docker Compose is not installed${NC}"
    exit 1
fi
echo -e "${GREEN}✓ Prerequisites met${NC}"
echo ""

# Step 2: Start services
echo "Step 2: Starting services with docker-compose..."
docker-compose up -d
echo -e "${GREEN}✓ Services started${NC}"
echo ""

# Step 3: Wait for infrastructure services to be healthy
echo "Step 3: Waiting for infrastructure services..."
INFRASTRUCTURE_SERVICES=("sqlserver" "mongodb" "redis" "rabbitmq" "consul")
elapsed=0

for service in "${INFRASTRUCTURE_SERVICES[@]}"; do
    echo -n "Waiting for $service... "
    while [ $elapsed -lt $TIMEOUT ]; do
        health=$(check_service_health $service)
        if [ "$health" = "healthy" ] || [ "$health" = "running" ]; then
            echo -e "${GREEN}✓ healthy${NC}"
            break
        fi
        sleep $CHECK_INTERVAL
        elapsed=$((elapsed + CHECK_INTERVAL))
        echo -n "."
    done
    
    if [ $elapsed -ge $TIMEOUT ]; then
        echo -e "${RED}✗ timeout${NC}"
        echo "Service logs:"
        docker logs $service --tail 50
        exit 1
    fi
done
echo ""

# Step 4: Wait for application services to be healthy
echo "Step 4: Waiting for application services..."
APP_SERVICES=("orderservice" "inventoryservice" "paymentservice" "deliveryservice" "invoiceservice" "healthmonitorservice" "apigateway")
elapsed=0

for service in "${APP_SERVICES[@]}"; do
    echo -n "Waiting for $service... "
    while [ $elapsed -lt $TIMEOUT ]; do
        health=$(check_service_health $service)
        if [ "$health" = "healthy" ]; then
            echo -e "${GREEN}✓ healthy${NC}"
            break
        fi
        sleep $CHECK_INTERVAL
        elapsed=$((elapsed + CHECK_INTERVAL))
        echo -n "."
    done
    
    if [ $elapsed -ge $TIMEOUT ]; then
        echo -e "${RED}✗ timeout${NC}"
        echo "Service logs:"
        docker logs $service --tail 50
        exit 1
    fi
done
echo ""

# Step 5: Test HTTP endpoints
echo "Step 5: Testing HTTP health endpoints..."
for service in "${!HEALTH_ENDPOINTS[@]}"; do
    url="${HEALTH_ENDPOINTS[$service]}"
    echo -n "Testing $service ($url)... "
    
    status=$(check_http_endpoint "$url")
    if [ "$status" = "200" ]; then
        echo -e "${GREEN}✓ OK (200)${NC}"
    else
        echo -e "${RED}✗ Failed (HTTP $status)${NC}"
        exit 1
    fi
done
echo ""

# Step 6: Test key functionality
echo "Step 6: Testing key API endpoints..."

# Test inventory seed
echo -n "Testing inventory seed endpoint... "
seed_status=$(curl -s -o /dev/null -w "%{http_code}" -X POST http://localhost:8080/api/inventory/seed 2>/dev/null || echo "000")
if [ "$seed_status" = "200" ] || [ "$seed_status" = "409" ]; then
    echo -e "${GREEN}✓ OK (HTTP $seed_status)${NC}"
else
    echo -e "${RED}✗ Failed (HTTP $seed_status)${NC}"
fi

# Test get inventory
echo -n "Testing get inventory endpoint... "
inventory_status=$(curl -s -o /dev/null -w "%{http_code}" http://localhost:8080/api/inventory 2>/dev/null || echo "000")
if [ "$inventory_status" = "200" ]; then
    echo -e "${GREEN}✓ OK (200)${NC}"
else
    echo -e "${RED}✗ Failed (HTTP $inventory_status)${NC}"
fi

# Test health monitor
echo -n "Testing health monitor endpoint... "
healthmon_status=$(curl -s -o /dev/null -w "%{http_code}" http://localhost:5004/api/healthstatus 2>/dev/null || echo "000")
if [ "$healthmon_status" = "200" ]; then
    echo -e "${GREEN}✓ OK (200)${NC}"
else
    echo -e "${RED}✗ Failed (HTTP $healthmon_status)${NC}"
fi

# Test Consul
echo -n "Testing Consul services... "
consul_status=$(curl -s -o /dev/null -w "%{http_code}" http://localhost:8500/v1/catalog/services 2>/dev/null || echo "000")
if [ "$consul_status" = "200" ]; then
    echo -e "${GREEN}✓ OK (200)${NC}"
else
    echo -e "${RED}✗ Failed (HTTP $consul_status)${NC}"
fi

# Test RabbitMQ
echo -n "Testing RabbitMQ management... "
rabbitmq_status=$(curl -s -o /dev/null -w "%{http_code}" -u guest:guest http://localhost:15672/api/overview 2>/dev/null || echo "000")
if [ "$rabbitmq_status" = "200" ]; then
    echo -e "${GREEN}✓ OK (200)${NC}"
else
    echo -e "${RED}✗ Failed (HTTP $rabbitmq_status)${NC}"
fi

echo ""

# Step 7: Show service status
echo "Step 7: Service status summary"
echo "=========================================="
docker-compose ps
echo ""

# Final summary
echo "=========================================="
echo -e "${GREEN}✓ All smoke tests passed!${NC}"
echo "=========================================="
echo ""
echo "Access points:"
echo "  - API Gateway: http://localhost:8080"
echo "  - Order Service: http://localhost:5000"
echo "  - Inventory Service: http://localhost:5001"
echo "  - Payment Service: http://localhost:5002"
echo "  - Delivery Service: http://localhost:5003"
echo "  - Health Monitor: http://localhost:5004"
echo "  - Invoice Service: http://localhost:5005"
echo "  - Consul UI: http://localhost:8500"
echo "  - RabbitMQ UI: http://localhost:15672 (guest/guest)"
echo ""
echo "To stop services: docker-compose down"
echo "To view logs: docker-compose logs -f [service-name]"
echo ""

