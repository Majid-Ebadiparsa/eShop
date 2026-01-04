# eShop Kubernetes Deployment

Complete Kubernetes manifests for deploying the eShop microservices system.

## Architecture Overview

### Services
- **OrderService** - Order management and orchestration
- **InventoryService** - Product inventory management
- **PaymentService** - Payment processing
- **DeliveryService** - Shipment and delivery tracking
- **InvoiceService** - Invoice generation and management
- **HealthMonitorService** - Service health monitoring and dashboard

### Infrastructure
- **SQL Server** - Command database (write models)
- **MongoDB** - Query database (read models / CQRS)
- **RabbitMQ** - Message broker (event-driven communication)
- **Redis** - Distributed cache
- **Consul** - Service discovery and configuration

### Gateway & UI
- **API Gateway** - Ocelot-based API gateway with service discovery
- **Next.js UI** - Customer-facing Progressive Web App (PWA)

## Directory Structure

```
k8s/
├── namespace.yaml              # Namespace definition
├── configmap.yaml              # Configuration values
├── secrets.yaml                # Sensitive data (DO NOT COMMIT WITH REAL VALUES)
├── ingress.yaml                # Ingress routing rules
├── infrastructure/             # Infrastructure services
│   ├── sqlserver.yaml
│   ├── mongodb.yaml
│   ├── redis.yaml
│   ├── rabbitmq.yaml
│   └── consul.yaml
├── services/                   # Microservices
│   ├── orderservice.yaml
│   ├── inventoryservice.yaml
│   ├── paymentservice.yaml
│   ├── deliveryservice.yaml
│   ├── invoiceservice.yaml
│   └── healthmonitorservice.yaml
├── gateway/                    # API Gateway & UI
│   ├── apigateway.yaml
│   └── ui.yaml
├── deploy.sh / deploy.ps1      # Deployment scripts
├── teardown.sh / teardown.ps1  # Cleanup scripts
├── test-deployment.sh / .ps1   # Smoke test scripts
└── README.md                   # This file
```

## Prerequisites

### Required Tools
- **kubectl** - Kubernetes command-line tool
  - Install: https://kubernetes.io/docs/tasks/tools/
  - Version: 1.25+

- **Kubernetes Cluster** - One of:
  - **Minikube** (local development)
  - **Docker Desktop** (local development)
  - **Kind** (local development)
  - **AKS** (Azure Kubernetes Service)
  - **EKS** (Amazon Elastic Kubernetes Service)
  - **GKE** (Google Kubernetes Engine)

### Optional Tools
- **Helm** - Package manager for Kubernetes
- **k9s** - Terminal UI for Kubernetes
- **Lens** - Kubernetes IDE

### Cluster Requirements

**Minimum Resources:**
- CPU: 8 cores
- Memory: 16 GB RAM
- Storage: 50 GB

**Recommended Resources:**
- CPU: 16 cores
- Memory: 32 GB RAM
- Storage: 100 GB

### Ingress Controller (Optional)

If using Ingress, install NGINX Ingress Controller:

```bash
# For Minikube
minikube addons enable ingress

# For Docker Desktop
kubectl apply -f https://raw.githubusercontent.com/kubernetes/ingress-nginx/main/deploy/static/provider/cloud/deploy.yaml

# For cloud providers (AKS, EKS, GKE)
helm repo add ingress-nginx https://kubernetes.github.io/ingress-nginx
helm install ingress-nginx ingress-nginx/ingress-nginx
```

## Quick Start

### 1. Build Docker Images

Before deploying, build all Docker images:

```bash
# From repository root
cd src/eShop

# Build all services
docker build -t orderservice:latest -f OrderService/OrderService.API/Dockerfile .
docker build -t inventoryservice:latest -f InventoryService/InventoryService.API/Dockerfile .
docker build -t paymentservice:latest -f PaymentService/PaymentService.API/Dockerfile .
docker build -t deliveryservice:latest -f DeliveryService/DeliveryService.API/Dockerfile .
docker build -t invoiceservice:latest -f InvoiceProcessingService/src/InvoiceService/InvoiceService.API/Dockerfile .
docker build -t healthmonitorservice:latest -f HealthMonitorService/HealthMonitorService.API/Dockerfile .
docker build -t apigateway:latest -f ApiGateway/Dockerfile .
docker build -t eshop-ui:latest -f ui/Dockerfile .
```

**For Minikube/Kind:** Load images into the cluster:

```bash
# Minikube
minikube image load orderservice:latest
minikube image load inventoryservice:latest
# ... load all images

# Kind
kind load docker-image orderservice:latest
kind load docker-image inventoryservice:latest
# ... load all images
```

### 2. Update Secrets

**⚠️ IMPORTANT:** Update `secrets.yaml` with secure passwords before deploying to production!

```bash
cd k8s
nano secrets.yaml  # or use your favorite editor
```

Change at minimum:
- `SQL_SERVER_SA_PASSWORD`
- `MONGODB_ROOT_PASSWORD`
- `MONGODB_PASSWORD`
- `RABBITMQ_PASSWORD`

### 3. Deploy to Kubernetes

**Linux/macOS:**
```bash
cd k8s
chmod +x deploy.sh
./deploy.sh
```

**Windows PowerShell:**
```powershell
cd k8s
.\deploy.ps1
```

### 4. Verify Deployment

Wait for all pods to be ready (this may take 5-10 minutes):

```bash
kubectl get pods -n eshop -w
```

Run smoke tests:

```bash
# Linux/macOS
./test-deployment.sh

# Windows
.\test-deployment.ps1
```

### 5. Access the Application

#### Option A: Using LoadBalancer (Cloud Providers)

Get external IPs:

```bash
kubectl get svc -n eshop
```

Access:
- **eShop UI**: `http://<ESHOP-UI-EXTERNAL-IP>`
- **API Gateway**: `http://<API-GATEWAY-EXTERNAL-IP>`

#### Option B: Using Ingress

Update your `/etc/hosts` (Linux/macOS) or `C:\Windows\System32\drivers\etc\hosts` (Windows):

```
<INGRESS-IP>  eshop.local
<INGRESS-IP>  api.eshop.local
<INGRESS-IP>  health.eshop.local
<INGRESS-IP>  rabbitmq.eshop.local
<INGRESS-IP>  consul.eshop.local
```

Get Ingress IP:

```bash
kubectl get ingress -n eshop
```

Access:
- **eShop UI**: http://eshop.local
- **API Gateway**: http://api.eshop.local
- **Health Monitor**: http://health.eshop.local
- **RabbitMQ Management**: http://rabbitmq.eshop.local
- **Consul UI**: http://consul.eshop.local

#### Option C: Using Port Forwarding (Development)

```bash
# eShop UI
kubectl port-forward -n eshop svc/eshop-ui 3000:80
# Access: http://localhost:3000

# API Gateway
kubectl port-forward -n eshop svc/apigateway 8080:80
# Access: http://localhost:8080

# Health Monitor
kubectl port-forward -n eshop svc/healthmonitorservice 8081:80
# Access: http://localhost:8081
```

## Configuration

### Environment Variables

All configuration is managed through ConfigMaps and Secrets:

- **configmap.yaml** - Non-sensitive configuration (hosts, ports, feature flags)
- **secrets.yaml** - Sensitive data (passwords, connection strings)

### Resource Limits

Each service has resource requests and limits defined:

**Microservices:**
- Requests: 250m CPU, 256Mi memory
- Limits: 500m CPU, 512Mi memory

**Infrastructure:**
- **SQL Server**: 1-2 CPU, 2-4Gi memory
- **MongoDB**: 0.5-1 CPU, 512Mi-2Gi memory
- **RabbitMQ**: 0.5-1 CPU, 512Mi-1Gi memory
- **Redis**: 0.25-0.5 CPU, 256Mi-512Mi memory
- **Consul**: 0.25-0.5 CPU, 256Mi-512Mi memory

Adjust these based on your workload in each service's YAML file.

### Scaling

Scale services horizontally:

```bash
# Scale OrderService to 5 replicas
kubectl scale deployment orderservice -n eshop --replicas=5

# Auto-scaling (requires Metrics Server)
kubectl autoscale deployment orderservice -n eshop --min=2 --max=10 --cpu-percent=70
```

## Health Checks

All services have configured liveness and readiness probes:

- **Liveness Probe**: Determines if container should be restarted
  - Path: `/health/live`
  - Initial Delay: 30s
  - Period: 10s

- **Readiness Probe**: Determines if container can receive traffic
  - Path: `/health/ready`
  - Initial Delay: 15s
  - Period: 5s

## Persistence

Persistent data is stored in StatefulSets with PersistentVolumeClaims:

- **SQL Server**: 10Gi (databases)
- **MongoDB**: 10Gi (data) + 1Gi (config)
- **RabbitMQ**: 5Gi (message queues)
- **Redis**: 2Gi (cache)
- **Consul**: 1Gi (service registry)

**⚠️ Important:** Data persists across pod restarts but may be lost if PVCs are deleted.

For production, use:
- StorageClass with proper backup/snapshot capabilities
- Regular backups to external storage
- Database replication and clustering

## Monitoring & Logging

### View Logs

```bash
# View logs for a specific service
kubectl logs -f deployment/orderservice -n eshop

# View logs for all pods with a label
kubectl logs -f -l app=orderservice -n eshop

# View logs from previous crashed container
kubectl logs orderservice-xxx-yyy -n eshop --previous
```

### Service Metrics

```bash
# View resource usage
kubectl top pods -n eshop
kubectl top nodes

# Describe pod for events
kubectl describe pod orderservice-xxx-yyy -n eshop
```

### Health Monitor Dashboard

Access the Health Monitor UI to view service health status:
- http://health.eshop.local (via Ingress)
- Or port-forward: `kubectl port-forward -n eshop svc/healthmonitorservice 8081:80`

## Troubleshooting

### Pods Not Starting

```bash
# Check pod status
kubectl get pods -n eshop

# Describe pod for events
kubectl describe pod <pod-name> -n eshop

# Check logs
kubectl logs <pod-name> -n eshop
```

### Database Connection Issues

```bash
# Test SQL Server connection
kubectl exec -n eshop sqlserver-0 -- /opt/mssql-tools/bin/sqlcmd -S localhost -U sa -P "YourStrong@Passw0rd" -Q "SELECT 1"

# Test MongoDB connection
kubectl exec -n eshop mongodb-0 -- mongosh --eval "db.adminCommand('ping')"

# Check connection strings in secrets
kubectl get secret eshop-secrets -n eshop -o yaml
```

### Service Discovery Issues

```bash
# Check Consul
kubectl port-forward -n eshop svc/consul 8500:8500
# Open: http://localhost:8500

# Check if services are registered
kubectl exec -n eshop consul-0 -- consul catalog services
```

### Image Pull Errors

If using local images with Minikube/Kind:

```bash
# Minikube: Load images
eval $(minikube docker-env)
# Rebuild images
docker build -t orderservice:latest ...

# Kind: Load images
kind load docker-image orderservice:latest
```

### Persistent Volume Issues

```bash
# Check PVCs
kubectl get pvc -n eshop

# Check PVs
kubectl get pv

# If using local storage, ensure paths exist
```

## Cleanup

### Delete All Resources

**Linux/macOS:**
```bash
./teardown.sh
```

**Windows:**
```powershell
.\teardown.ps1
```

### Delete Specific Components

```bash
# Delete services only (keep infrastructure)
kubectl delete -f services/ -n eshop

# Delete specific service
kubectl delete -f services/orderservice.yaml -n eshop

# Delete namespace (removes everything)
kubectl delete namespace eshop
```

## Production Considerations

### Security

1. **Secrets Management**
   - Use External Secrets Operator or cloud provider secret managers
   - Enable encryption at rest for Secrets
   - Rotate credentials regularly

2. **Network Policies**
   - Implement NetworkPolicies to restrict pod-to-pod communication
   - Use service mesh (Istio, Linkerd) for mTLS

3. **RBAC**
   - Define ServiceAccounts per service
   - Apply least-privilege access policies

4. **Image Security**
   - Scan images for vulnerabilities
   - Use private container registry
   - Implement image signing

### High Availability

1. **Database Clustering**
   - SQL Server: Always On Availability Groups
   - MongoDB: Replica Sets
   - RabbitMQ: Cluster mode

2. **Multi-Zone Deployment**
   - Spread pods across availability zones
   - Use pod anti-affinity rules

3. **Backups**
   - Implement automated backup for databases
   - Test restore procedures regularly

### Observability

1. **Logging**
   - Deploy ELK/EFK stack or use cloud logging
   - Centralize logs from all services

2. **Metrics**
   - Install Prometheus + Grafana
   - Configure service monitors

3. **Tracing**
   - Deploy Jaeger or Zipkin
   - Enable distributed tracing

### CI/CD

Example GitHub Actions workflow:

```yaml
name: Deploy to Kubernetes

on:
  push:
    branches: [main]

jobs:
  deploy:
    runs-on: ubuntu-latest
    steps:
      - uses: actions/checkout@v3
      
      - name: Build images
        run: |
          docker build -t ${{ secrets.REGISTRY }}/orderservice:${{ github.sha }} ...
          
      - name: Push images
        run: |
          docker push ${{ secrets.REGISTRY }}/orderservice:${{ github.sha }}
          
      - name: Deploy to K8s
        run: |
          kubectl set image deployment/orderservice orderservice=${{ secrets.REGISTRY }}/orderservice:${{ github.sha }} -n eshop
```

## Support

For issues or questions:
1. Check the troubleshooting section above
2. Review pod logs and events
3. Consult the main project documentation

## License

This deployment configuration is part of the eShop Microservices project.

