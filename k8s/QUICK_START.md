# eShop Kubernetes - Quick Start Guide

Get the eShop microservices running on Kubernetes in 15 minutes.

## Prerequisites Check

```bash
# Check kubectl is installed
kubectl version --client

# Check cluster is accessible
kubectl cluster-info

# Check you have enough resources (8+ CPU, 16+ GB RAM)
kubectl top nodes
```

## Step 1: Build Images (5 minutes)

```bash
# Navigate to source directory
cd D:/Majid/Work/Projects/EShopMicroservices/src/eShop

# Build all images (run in parallel for speed)
docker build -t orderservice:latest -f OrderService/OrderService.API/Dockerfile . &
docker build -t inventoryservice:latest -f InventoryService/InventoryService.API/Dockerfile . &
docker build -t paymentservice:latest -f PaymentService/PaymentService.API/Dockerfile . &
docker build -t deliveryservice:latest -f DeliveryService/DeliveryService.API/Dockerfile . &
docker build -t invoiceservice:latest -f InvoiceProcessingService/src/InvoiceService/InvoiceService.API/Dockerfile . &
docker build -t healthmonitorservice:latest -f HealthMonitorService/HealthMonitorService.API/Dockerfile . &
docker build -t apigateway:latest -f ApiGateway/Dockerfile . &
docker build -t eshop-ui:latest -f ui/Dockerfile . &

# Wait for all builds to complete
wait
```

**For Minikube users:**

```bash
# Use Minikube's Docker daemon
eval $(minikube docker-env)

# Then build images (they'll be directly available in Minikube)
# Run the same build commands as above
```

**For Kind users:**

```bash
# Build images first, then load them
kind load docker-image orderservice:latest
kind load docker-image inventoryservice:latest
kind load docker-image paymentservice:latest
kind load docker-image deliveryservice:latest
kind load docker-image invoiceservice:latest
kind load docker-image healthmonitorservice:latest
kind load docker-image apigateway:latest
kind load docker-image eshop-ui:latest
```

## Step 2: Update Secrets (1 minute)

⚠️ **IMPORTANT for production!** For development/testing, you can skip this step.

```bash
cd k8s

# Edit secrets.yaml
nano secrets.yaml  # or code secrets.yaml

# Change these passwords:
# - SQL_SERVER_SA_PASSWORD
# - MONGODB_ROOT_PASSWORD
# - RABBITMQ_PASSWORD
```

## Step 3: Deploy to Kubernetes (5 minutes)

```bash
cd k8s

# Linux/macOS
chmod +x deploy.sh
./deploy.sh

# Windows PowerShell
.\deploy.ps1
```

**What happens:**
1. Creates `eshop` namespace
2. Applies ConfigMaps and Secrets
3. Deploys infrastructure (SQL, Mongo, Redis, RabbitMQ, Consul)
4. Waits for infrastructure to be ready
5. Deploys microservices
6. Deploys API Gateway and UI
7. Applies Ingress rules

## Step 4: Monitor Deployment (2-3 minutes)

```bash
# Watch pods starting up
kubectl get pods -n eshop -w

# Wait for all pods to be Running (press Ctrl+C when done)
```

Expected output when ready:

```
NAME                                    READY   STATUS    RESTARTS   AGE
consul-0                                1/1     Running   0          5m
mongodb-0                               1/1     Running   0          5m
rabbitmq-0                              1/1     Running   0          5m
redis-0                                 1/1     Running   0          5m
sqlserver-0                             1/1     Running   0          5m
orderservice-xxx-yyy                    1/1     Running   0          3m
inventoryservice-xxx-yyy                1/1     Running   0          3m
paymentservice-xxx-yyy                  1/1     Running   0          3m
deliveryservice-xxx-yyy                 1/1     Running   0          3m
invoiceservice-xxx-yyy                  1/1     Running   0          3m
healthmonitorservice-xxx-yyy            1/1     Running   0          3m
apigateway-xxx-yyy                      1/1     Running   0          2m
eshop-ui-xxx-yyy                        1/1     Running   0          2m
```

## Step 5: Access the Application (1 minute)

### Option A: Port Forwarding (Easiest)

```bash
# eShop UI (customer-facing)
kubectl port-forward -n eshop svc/eshop-ui 3000:80
# Open: http://localhost:3000

# API Gateway
kubectl port-forward -n eshop svc/apigateway 8080:80
# Open: http://localhost:8080

# Health Monitor
kubectl port-forward -n eshop svc/healthmonitorservice 8081:80
# Open: http://localhost:8081
```

### Option B: Ingress (Recommended)

1. Get Ingress IP:

```bash
kubectl get ingress -n eshop
```

2. Update hosts file:

**Linux/macOS:**
```bash
sudo nano /etc/hosts
```

**Windows:**
```powershell
notepad C:\Windows\System32\drivers\etc\hosts
```

Add these lines (replace `<INGRESS-IP>` with actual IP):

```
<INGRESS-IP>  eshop.local
<INGRESS-IP>  api.eshop.local
<INGRESS-IP>  health.eshop.local
<INGRESS-IP>  rabbitmq.eshop.local
<INGRESS-IP>  consul.eshop.local
```

3. Access:
   - **eShop UI**: http://eshop.local
   - **API Gateway**: http://api.eshop.local
   - **Health Monitor**: http://health.eshop.local

## Step 6: Test Everything (2 minutes)

```bash
cd k8s

# Linux/macOS
chmod +x test-deployment.sh
./test-deployment.sh

# Windows PowerShell
.\test-deployment.ps1
```

Expected output:

```
✓ PASSED: All pods should be in Running state
✓ PASSED: SQL Server is accessible
✓ PASSED: MongoDB is accessible
✓ PASSED: Redis is accessible
✓ PASSED: RabbitMQ is accessible
✓ PASSED: Consul is accessible
✓ PASSED: orderservice health endpoint is responding
✓ PASSED: inventoryservice health endpoint is responding
...
All tests passed! ✓
```

## Step 7: Use the Application

### Seed Sample Data

```bash
# Port forward to API Gateway
kubectl port-forward -n eshop svc/apigateway 8080:80

# In another terminal, seed inventory
curl -X POST http://localhost:8080/api/inventory/seed
```

### Test the Flow

1. **Browse Products**: http://localhost:3000/products
2. **Add to Cart**: Click "Add to Cart" on any product
3. **Checkout**: Go to cart → Checkout
4. **Place Order**: Fill out form and submit
5. **Track Order**: View order status

### Monitor Services

**Health Monitor Dashboard:**
```bash
kubectl port-forward -n eshop svc/healthmonitorservice 8081:80
# Open: http://localhost:8081
```

**RabbitMQ Management:**
```bash
kubectl port-forward -n eshop svc/rabbitmq-management 15672:15672
# Open: http://localhost:15672
# Login: guest / guest
```

**Consul UI:**
```bash
kubectl port-forward -n eshop svc/consul-ui 8500:8500
# Open: http://localhost:8500
```

## Common Commands

### View Logs

```bash
# Specific service
kubectl logs -f deployment/orderservice -n eshop

# All instances of a service
kubectl logs -f -l app=orderservice -n eshop

# All services (using stern if installed)
stern -n eshop .
```

### Scale Services

```bash
# Scale up
kubectl scale deployment orderservice -n eshop --replicas=5

# Scale down
kubectl scale deployment orderservice -n eshop --replicas=1
```

### Restart Service

```bash
kubectl rollout restart deployment/orderservice -n eshop
```

### Check Resource Usage

```bash
kubectl top pods -n eshop
kubectl top nodes
```

## Troubleshooting

### Pods Stuck in Pending

```bash
# Check events
kubectl get events -n eshop --sort-by='.lastTimestamp'

# Describe pod
kubectl describe pod <pod-name> -n eshop

# Common causes:
# - Not enough resources (CPU/memory)
# - Image pull errors
# - PVC binding issues
```

### Service Not Responding

```bash
# Check if pod is ready
kubectl get pods -n eshop

# Check logs
kubectl logs <pod-name> -n eshop

# Check health endpoints
kubectl exec -n eshop <pod-name> -- wget -qO- http://localhost:8080/health/ready
```

### Database Connection Errors

```bash
# Check SQL Server
kubectl exec -n eshop sqlserver-0 -- /opt/mssql-tools/bin/sqlcmd -S localhost -U sa -P "YourStrong@Passw0rd" -Q "SELECT 1"

# Check MongoDB
kubectl exec -n eshop mongodb-0 -- mongosh --eval "db.adminCommand('ping')"

# Check secrets
kubectl get secret eshop-secrets -n eshop -o yaml | grep CONNECTION
```

## Cleanup

```bash
cd k8s

# Linux/macOS
./teardown.sh

# Windows PowerShell
.\teardown.ps1
```

## What's Next?

1. **Explore the UI** - Browse products, place orders, track deliveries
2. **Monitor Services** - Check Health Monitor dashboard
3. **View Logs** - Follow event flows through services
4. **Test Resilience** - Kill pods and watch them recover
5. **Scale Services** - Increase replicas and see load balancing
6. **Production Setup** - Review README.md for production considerations

## Need Help?

- **Full Documentation**: See `README.md` in the k8s directory
- **Architecture Details**: See `AUDIT_AND_GAP_LIST.md` in the project root
- **Service Details**: See individual service documentation

---

**Congratulations!** 🎉 You now have a complete microservices system running on Kubernetes!

