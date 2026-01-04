# Kubernetes Manifests - File Manifest

Complete list of all Kubernetes manifests and supporting files.

**Last Updated**: January 4, 2026
**Total Files**: 22
**Total Lines**: ~3,104

---

## Core Configuration (4 files)

### `namespace.yaml` (7 lines)
- Creates `eshop` namespace
- Labels for environment and name

### `configmap.yaml` (36 lines)
- Non-sensitive configuration
- Service hostnames and ports (RabbitMQ, Redis, MongoDB, SQL Server, Consul)
- Logging configuration
- Environment settings

### `secrets.yaml` (47 lines)
- **⚠️ SENSITIVE** - Do not commit with real passwords!
- Database passwords (SQL Server, MongoDB, RabbitMQ)
- Connection strings for all databases
- Redis password

### `ingress.yaml` (73 lines)
- NGINX Ingress Controller configuration
- Routes:
  - `eshop.local` → Next.js UI
  - `api.eshop.local` → API Gateway
  - `health.eshop.local` → Health Monitor
  - `rabbitmq.eshop.local` → RabbitMQ Management
  - `consul.eshop.local` → Consul UI
- SSL/TLS ready (cert-manager annotations)

---

## Infrastructure Services (5 files)

### `infrastructure/sqlserver.yaml` (87 lines)
- **StatefulSet** with 1 replica
- **Image**: `mcr.microsoft.com/mssql/server:2022-latest`
- **Storage**: 10Gi PVC
- **Resources**: 1-2 CPU, 2-4Gi memory
- **Health Checks**: TCP socket on port 1433
- **Service**: Headless (ClusterIP: None)
- **Databases**: OrderDb, InventoryDb, PaymentDb, DeliveryDb, InvoiceDb, HealthMonitorDb

### `infrastructure/mongodb.yaml` (99 lines)
- **StatefulSet** with 1 replica
- **Image**: `mongo:7.0`
- **Storage**: 10Gi (data) + 1Gi (config)
- **Resources**: 0.5-1 CPU, 512Mi-2Gi memory
- **Health Checks**: `mongosh` ping command
- **Service**: Headless (ClusterIP: None)
- **Databases**: Read models for all services

### `infrastructure/redis.yaml` (67 lines)
- **StatefulSet** with 1 replica
- **Image**: `redis:7-alpine`
- **Storage**: 2Gi PVC with AOF persistence
- **Resources**: 0.25-0.5 CPU, 256Mi-512Mi memory
- **Health Checks**: `redis-cli ping`
- **Service**: Headless (ClusterIP: None)
- **Purpose**: Distributed cache

### `infrastructure/rabbitmq.yaml` (101 lines)
- **StatefulSet** with 1 replica
- **Image**: `rabbitmq:3-management`
- **Storage**: 5Gi PVC
- **Resources**: 0.5-1 CPU, 512Mi-1Gi memory
- **Ports**: 5672 (AMQP), 15672 (Management UI)
- **Health Checks**: `rabbitmq-diagnostics` commands
- **Services**: Headless + LoadBalancer for Management UI
- **Purpose**: Event-driven messaging

### `infrastructure/consul.yaml` (99 lines)
- **StatefulSet** with 1 replica
- **Image**: `consul:1.17`
- **Storage**: 1Gi PVC
- **Resources**: 0.25-0.5 CPU, 256Mi-512Mi memory
- **Ports**: 8500 (HTTP), 8600 (DNS), 8300 (Server)
- **Health Checks**: HTTP on `/v1/status/leader`
- **Services**: Headless + LoadBalancer for UI
- **Purpose**: Service discovery and configuration

---

## Microservices (6 files)

All microservices share common configuration:
- **Deployment** with 2 replicas (except HealthMonitor: 1)
- **Resources**: 250m-500m CPU, 256Mi-512Mi memory
- **Health Checks**:
  - Liveness: `/health/live` (30s initial, 10s period)
  - Readiness: `/health/ready` (15s initial, 5s period)
- **Service**: ClusterIP
- **Environment**: ConfigMap + Secret injection

### `services/orderservice.yaml` (95 lines)
- **Purpose**: Order management and orchestration
- **Database**: OrderDb (SQL Server)
- **Read Model**: MongoDB
- **Cache**: Redis
- **Events**: Publishes OrderCreated, consumes Inventory/Payment/Shipment events

### `services/inventoryservice.yaml` (95 lines)
- **Purpose**: Product inventory management
- **Database**: InventoryDb (SQL Server)
- **Read Model**: MongoDB
- **Cache**: Redis
- **Events**: Publishes InventoryReserved/Failed, consumes OrderCreated

### `services/paymentservice.yaml` (95 lines)
- **Purpose**: Payment processing
- **Database**: PaymentDb (SQL Server)
- **Read Model**: MongoDB (full CQRS)
- **Cache**: Redis
- **Events**: Publishes PaymentAuthorized/Captured/Failed, consumes InventoryReserved

### `services/deliveryservice.yaml` (95 lines)
- **Purpose**: Shipment and delivery tracking
- **Database**: DeliveryDb (SQL Server)
- **Read Model**: MongoDB
- **Cache**: Redis
- **Events**: Publishes ShipmentCreated/Dispatched/Delivered, consumes PaymentCaptured

### `services/invoiceservice.yaml` (95 lines)
- **Purpose**: Invoice generation and management
- **Database**: InvoiceDb (SQL Server)
- **Read Model**: MongoDB
- **Events**: Publishes InvoiceSubmitted, consumes order events

### `services/healthmonitorservice.yaml` (92 lines)
- **Purpose**: Service health monitoring and dashboard
- **Database**: HealthMonitorDb (SQL Server)
- **Cache**: Redis
- **Replicas**: 1 (background service)
- **UI**: Embedded HTML/JS dashboard
- **Events**: Publishes ServiceHealthChanged

---

## Gateway & UI (2 files)

### `gateway/apigateway.yaml` (78 lines)
- **Deployment** with 2 replicas
- **Image**: `apigateway:latest`
- **Resources**: 250m-500m CPU, 256Mi-512Mi memory
- **Service**: LoadBalancer (external access)
- **Technology**: Ocelot with Consul service discovery
- **Routes**: All microservices via Consul

### `gateway/ui.yaml` (66 lines)
- **Deployment** with 2 replicas
- **Image**: `eshop-ui:latest`
- **Resources**: 250m-500m CPU, 256Mi-512Mi memory
- **Service**: LoadBalancer (external access)
- **Technology**: Next.js 14 with TypeScript
- **Features**: PWA-ready, responsive design

---

## Deployment Scripts (4 files)

### `deploy.sh` (198 lines) - Linux/macOS
Automated deployment script:
- Validates kubectl and cluster access
- Creates namespace
- Applies ConfigMaps and Secrets
- Deploys infrastructure (with wait for ready)
- Deploys microservices (with wait for ready)
- Deploys gateway and UI
- Applies Ingress
- Displays deployment summary and access URLs
- Supports `--dry-run` mode for validation

### `deploy.ps1` (176 lines) - Windows PowerShell
PowerShell version of deploy.sh with identical functionality:
- Parameter validation
- Color-coded output
- Error handling
- Progress tracking
- Access URL display

### `teardown.sh` (82 lines) - Linux/macOS
Cleanup script:
- Confirmation prompt (skip with `--force`)
- Deletes in reverse order
- Waits for complete deletion
- Safe and idempotent

### `teardown.ps1` (79 lines) - Windows PowerShell
PowerShell version of teardown.sh with identical functionality.

---

## Test Scripts (2 files)

### `test-deployment.sh` (152 lines) - Linux/macOS
Comprehensive smoke tests:
- Test 1: All pods running
- Test 2: Infrastructure services accessible (SQL, Mongo, Redis, RabbitMQ, Consul)
- Test 3: Microservices health endpoints
- Test 4: API Gateway responding
- Test 5: Services registered in Consul
- Test 6: Persistent volumes bound
- Color-coded output (PASS/FAIL)
- Exit code for CI/CD integration

### `test-deployment.ps1` (162 lines) - Windows PowerShell
PowerShell version of test-deployment.sh with identical tests.

---

## Documentation (3 files)

### `README.md` (637 lines)
Complete documentation:
- Architecture overview
- Directory structure
- Prerequisites and requirements
- Quick start guide
- Configuration details
- Scaling and resource management
- Health checks and monitoring
- Persistence and storage
- Troubleshooting guide (detailed)
- Production considerations:
  - Security (RBAC, NetworkPolicies, secrets)
  - High Availability (clustering, multi-zone)
  - Observability (logging, metrics, tracing)
  - CI/CD integration examples

### `QUICK_START.md` (397 lines)
15-minute getting started guide:
- Step-by-step deployment (6 steps)
- Build images instructions
- Deploy to cluster commands
- Access application methods
- Test functionality steps
- Common commands reference
- Troubleshooting tips
- What's next guidance

### `kustomization.yaml` (31 lines)
Kustomize configuration:
- All resources listed
- Common labels
- Namespace management
- Ready for overlays (dev/staging/prod)
- Image tag override support

---

## Resource Summary

### Total Resources Deployed

**StatefulSets**: 5
- sqlserver-0
- mongodb-0
- redis-0
- rabbitmq-0
- consul-0

**Deployments**: 8 (6 microservices + gateway + UI)
- orderservice (2 replicas)
- inventoryservice (2 replicas)
- paymentservice (2 replicas)
- deliveryservice (2 replicas)
- invoiceservice (2 replicas)
- healthmonitorservice (1 replica)
- apigateway (2 replicas)
- eshop-ui (2 replicas)

**Services**: 13
- 5 headless (infrastructure)
- 6 ClusterIP (microservices)
- 2 LoadBalancer (gateway, UI)
- 2 optional LoadBalancer (RabbitMQ Management, Consul UI)

**PersistentVolumeClaims**: 9
- sqlserver-data (10Gi)
- mongodb-data (10Gi)
- mongodb-config (1Gi)
- redis-data (2Gi)
- rabbitmq-data (5Gi)
- consul-data (1Gi)

**ConfigMaps**: 1
**Secrets**: 1
**Ingress**: 1 (with 5 hosts)

### Total Resource Requirements

**Minimum Cluster**:
- CPU: 8 cores
- Memory: 16 GB RAM
- Storage: 50 GB

**Recommended Cluster**:
- CPU: 16 cores
- Memory: 32 GB RAM
- Storage: 100 GB

### Total Pod Count
- Minimum: 18 pods (5 infrastructure + 11 microservices + 2 gateway)
- With replicas: ~20 pods

---

## Deployment Order

The deploy scripts follow this order:

1. **Namespace** (eshop)
2. **ConfigMap** (eshop-config)
3. **Secrets** (eshop-secrets)
4. **Infrastructure** (wait for ready):
   - SQL Server
   - MongoDB
   - Redis
   - RabbitMQ
   - Consul
5. **Microservices** (wait for ready):
   - OrderService
   - InventoryService
   - PaymentService
   - DeliveryService
   - InvoiceService
   - HealthMonitorService
6. **Gateway & UI**:
   - API Gateway
   - Next.js UI
7. **Ingress** (optional)

**Total Deployment Time**: ~10-15 minutes (including wait times)

---

## Teardown Order

The teardown scripts delete in reverse order:

1. Ingress
2. Gateway & UI
3. Microservices
4. Infrastructure
5. ConfigMaps & Secrets
6. Namespace

**Total Teardown Time**: ~2-3 minutes

---

## Usage Examples

### Deploy
```bash
# Linux/macOS
cd k8s
./deploy.sh

# Windows
cd k8s
.\deploy.ps1

# Dry-run validation
./deploy.sh --dry-run
.\deploy.ps1 -DryRun
```

### Test
```bash
# Linux/macOS
./test-deployment.sh

# Windows
.\test-deployment.ps1
```

### Teardown
```bash
# Linux/macOS
./teardown.sh

# Windows
.\teardown.ps1

# Skip confirmation
./teardown.sh --force
.\teardown.ps1 -Force
```

### Using Kustomize
```bash
# Build manifests
kubectl kustomize .

# Apply with kustomize
kubectl apply -k .

# Delete with kustomize
kubectl delete -k .
```

---

## Supported Environments

- ✅ Docker Desktop Kubernetes
- ✅ Minikube
- ✅ Kind (Kubernetes in Docker)
- ✅ Azure Kubernetes Service (AKS)
- ✅ Amazon Elastic Kubernetes Service (EKS)
- ✅ Google Kubernetes Engine (GKE)
- ✅ Red Hat OpenShift
- ✅ Rancher Kubernetes Engine (RKE)
- ✅ K3s

**Requirements**:
- Kubernetes 1.25+
- kubectl 1.25+
- NGINX Ingress Controller (optional, for Ingress)
- Metrics Server (optional, for auto-scaling)

---

## Security Notes

⚠️ **IMPORTANT**:

1. **secrets.yaml** contains placeholder passwords. Update before production deployment!
2. Do NOT commit secrets.yaml with real passwords to source control.
3. Use External Secrets Operator or cloud secret managers for production.
4. Enable RBAC and NetworkPolicies for production environments.
5. Scan container images for vulnerabilities before deployment.

---

## Maintenance

### Adding a New Service

1. Create manifest: `services/newservice.yaml`
2. Add to kustomization.yaml
3. Update deploy scripts to include new service
4. Update test scripts to verify new service
5. Update README.md and this MANIFEST.md

### Updating Resource Limits

Edit the respective YAML file and update `resources.requests` and `resources.limits`.

### Scaling

```bash
# Horizontal scaling
kubectl scale deployment orderservice -n eshop --replicas=5

# Auto-scaling
kubectl autoscale deployment orderservice -n eshop --min=2 --max=10 --cpu-percent=70
```

---

## Troubleshooting Quick Reference

| Issue | Command | Solution |
|-------|---------|----------|
| Pods not starting | `kubectl get pods -n eshop` | Check events with `kubectl describe pod` |
| Image pull errors | `kubectl describe pod <name> -n eshop` | Load images to cluster (Minikube/Kind) |
| Health check failures | `kubectl logs <pod> -n eshop` | Check application logs |
| Service not accessible | `kubectl get svc -n eshop` | Verify service type and endpoints |
| PVC not binding | `kubectl get pvc -n eshop` | Check storage class and available PVs |
| Database connection errors | `kubectl exec -n eshop sqlserver-0 -- /opt/mssql-tools/bin/sqlcmd ...` | Verify connection strings in secrets |

---

**For detailed information, see**:
- `README.md` - Complete documentation
- `QUICK_START.md` - Getting started guide
- `STEP26_KUBERNETES_MANIFESTS_SUMMARY.md` - Implementation summary

