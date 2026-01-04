# eShop Microservices System

A production-grade, event-driven e-commerce platform demonstrating modern microservices architecture patterns with .NET, implementing Domain-Driven Design (DDD), CQRS, Onion Architecture, and comprehensive observability.

## Table of Contents

- [Project Overview](#project-overview)
- [Architecture Overview](#architecture-overview)
- [Technology Stack](#technology-stack)
- [Repository Structure](#repository-structure)
- [Prerequisites](#prerequisites)
- [Configuration](#configuration)
- [Running the System](#running-the-system)
- [Accessing the System](#accessing-the-system)
- [Health Monitoring](#health-monitoring)
- [Event-Driven Flow](#event-driven-flow)
- [Testing](#testing)
- [Troubleshooting](#troubleshooting)
- [Known Limitations](#known-limitations)
- [Development Notes](#development-notes)
- [Kubernetes Deployment](#kubernetes-deployment)
- [Future Enhancements](#future-enhancements)

---

## Project Overview

The eShop Microservices System is a complete, production-ready e-commerce platform built to demonstrate enterprise-level architecture patterns and best practices. The system handles the full order lifecycle from product browsing through payment processing to delivery tracking.

### Core Goals

- **Domain-Driven Design (DDD)**: Each microservice represents a bounded context with rich domain models and aggregates
- **CQRS (Command Query Responsibility Segregation)**: Separate read and write models for optimal performance and scalability
- **Event-Driven Architecture**: Asynchronous communication between services using domain events
- **Scalability**: Horizontal scaling support for stateless services
- **Observability**: Comprehensive health monitoring, logging, and tracing capabilities
- **Resilience**: Idempotent message processing, health checks, and automatic recovery

---

## Architecture Overview

### Microservices

The system consists of **7 microservices** working together through event-driven communication:

#### 1. **OrderService** (Port 5000)
**Responsibility**: Order lifecycle management and orchestration

- Creates and manages customer orders
- Orchestrates the order fulfillment process through events
- Updates order status based on inventory, payment, and delivery events
- **Database**: `OrderDb` (SQL Server) for write model
- **Read Model**: MongoDB projections
- **Events Published**: `OrderCreatedEvent`
- **Events Consumed**: `InventoryReserved`, `InventoryReservationFailed`, `PaymentAuthorized`, `PaymentCaptured`, `PaymentFailed`, `ShipmentCreated`, `ShipmentDispatched`, `ShipmentDelivered`

#### 2. **InventoryService** (Port 5001)
**Responsibility**: Product inventory management

- Manages product stock levels
- Reserves inventory when orders are placed
- Releases inventory when orders fail or are cancelled
- **Database**: `InventoryDb` (SQL Server) for write model
- **Read Model**: MongoDB projections
- **Events Published**: `InventoryReserved`, `InventoryReservationFailed`, `InventoryReleased`
- **Events Consumed**: `OrderCreatedEvent`
- **Special Endpoint**: `POST /api/inventory/seed` - Seeds 30 sample products (3,615 total units across 5 categories)

#### 3. **PaymentService** (Port 5002)
**Responsibility**: Payment processing

- Authorizes and captures payments
- Handles payment failures and refunds
- **Full CQRS implementation** - uses MongoDB for all read operations
- **Database**: `PaymentDb` (SQL Server) for write model
- **Read Model**: MongoDB (queries read directly from Mongo)
- **Events Published**: `PaymentAuthorized`, `PaymentCaptured`, `PaymentFailed`, `PaymentRefunded`, `PaymentCancelled`
- **Events Consumed**: `InventoryReserved`

#### 4. **DeliveryService** (Port 5003)
**Responsibility**: Shipment and delivery tracking

- Creates shipments when payment is captured
- Tracks shipment status (Created → Dispatched → Delivered)
- Manages carrier information and tracking details
- **Database**: `DeliveryDb` (SQL Server) for write model
- **Read Model**: MongoDB projections
- **Events Published**: `ShipmentCreated`, `ShipmentDispatched`, `ShipmentDelivered`, `ShipmentFailed`, `ShipmentBooked`
- **Events Consumed**: `PaymentCaptured`, `OrderReadyToShip`

#### 5. **InvoiceProcessingService** (Port 5005)
**Responsibility**: Invoice generation and management

- Generates invoices for completed orders
- Manages invoice lifecycle (Draft → Submitted → Paid)
- **Status**: Fully integrated into docker-compose and API Gateway
- **Database**: `InvoiceDb` (SQL Server) for write model
- **Read Model**: MongoDB
- **Events Published**: `InvoiceSubmitted`
- **Events Consumed**: Order-related events

#### 6. **ApiGateway** (Port 8080)
**Responsibility**: API Gateway and routing

- Single entry point for all client requests
- Routes requests to appropriate microservices
- **Technology**: Ocelot with Consul service discovery
- **Service Discovery**: Dynamic routing based on Consul registration
- **Routes Configured**:
  - `/api/order/*` → OrderService
  - `/api/inventory/*` → InventoryService
  - `/api/payment/*` → PaymentService
  - `/api/shipment/*` → DeliveryService
  - `/api/v1/invoices/*` → InvoiceService
  - `/api/health/*` → HealthMonitorService
  - Health check routes for all services

#### 7. **HealthMonitorService** (Port 5004)
**Responsibility**: Service health monitoring and observability

- Polls all service health endpoints every 30 seconds
- Stores current health status and historical data
- Publishes `ServiceHealthChanged` events
- Provides web UI for monitoring (HTML/JS dashboard with charts)
- **Architecture**: Full Onion Architecture (API/Application/Domain/Infrastructure layers)
- **Database**: `HealthMonitorDb` (SQL Server)
- **Features**:
  - Service health status tracking
  - Health history (last 1000 records per service)
  - Execution logs with detailed error information
  - Real-time dashboard with latency charts and status timeline
  - CSV export functionality
  - Search and filtering capabilities
  - Structured error details (ErrorCode, ExceptionType, StackTrace)

### Architecture Patterns

#### Domain-Driven Design (DDD)
- **Aggregates**: Each service has rich domain models (Order, InventoryItem, Payment, Shipment, Invoice, ServiceHealthStatus)
- **Bounded Contexts**: Clear service boundaries aligned with business domains
- **Domain Events**: Business events published from aggregates
- **Value Objects**: Used extensively (Address, Money, OrderItem, ShipmentDetails, etc.)

#### Onion Architecture (Clean Architecture)
Each microservice follows a layered approach:

```
Service/
├── API/                    # Controllers, Middleware, Startup
├── Application/            # Use Cases, Commands, Queries (MediatR)
├── Domain/                 # Entities, Aggregates, Domain Events, Interfaces
├── Infrastructure/         # EF Core, MongoDB, MassTransit, External APIs
└── Tests/                  # Unit and Integration Tests
```

**Dependency Rule**: Inner layers have no knowledge of outer layers. Dependencies point inward.

#### CQRS (Command Query Responsibility Segregation)

- **Command Side**: SQL Server with EF Core
  - Write operations update domain aggregates
  - Transactional consistency
  - All commands go through MediatR handlers

- **Query Side**: MongoDB read models
  - Optimized for fast queries
  - Eventually consistent
  - Updated via projection consumers listening to domain events
  
- **Implementation Status**:
  - ✅ **PaymentService**: Full CQRS - queries read from MongoDB
  - ✅ **OrderService**: Projections exist, queries use MongoDB
  - ✅ **InventoryService**: Projections exist, queries use MongoDB
  - ✅ **DeliveryService**: Projections exist, queries use MongoDB
  - ✅ **InvoiceService**: MongoDB read models implemented

#### Event-Driven Communication

- **Message Broker**: RabbitMQ with MassTransit
- **Event Contracts**: Shared event definitions in `SharedService.Contracts`
- **Event Metadata**: All events include CorrelationId, MessageId, CausationId, OccurredAt for distributed tracing
- **Idempotency**: All consumers use `ProcessedMessage` table (MessageId + ConsumerName) to prevent duplicate processing
- **Publishing**: Direct publishing (Outbox pattern implemented in InvoiceService as pilot)

---

## Technology Stack

### Backend
- **.NET 8.0** - Primary framework
- **ASP.NET Core 8.0** - Web API framework
- **Entity Framework Core 8.0** - ORM for SQL Server
- **MediatR 12.x** - CQRS pattern implementation
- **FluentValidation** - Input validation

### Messaging
- **MassTransit 8.x** - Message bus abstraction
- **RabbitMQ 3.x (with Management Plugin)** - Message broker
- **Event Contracts**: Shared across services via `SharedService.Contracts`

### Databases
- **SQL Server 2022** - Command database (write models)
  - OrderDb, InventoryDb, PaymentDb, DeliveryDb, InvoiceDb, HealthMonitorDb
- **MongoDB 7.0** - Query database (read models/projections)
  - Separate collections per service for read models
- **Redis 7.x** - Distributed cache
  - Used for query result caching
  - Cache invalidation on command execution

### API Gateway & Service Discovery
- **Ocelot 21.x** - API Gateway with routing and load balancing
- **Consul** - Service discovery and configuration
  - Auto-registration: All services register on startup
  - Health checks: Services deregister when unhealthy

### Containerization & Orchestration
- **Docker 24.x** - Container runtime
- **Docker Compose 3.8** - Local orchestration
- **Kubernetes** - Production orchestration (manifests available in `/k8s`)

### Frontend
- **Next.js 14** - React framework with App Router
- **TypeScript** - Type-safe frontend code
- **Tailwind CSS** - Utility-first styling
- **PWA Support** - Installable, offline-capable application
- **Features**:
  - Product catalog with search and filtering
  - Shopping cart with localStorage persistence
  - Complete checkout flow
  - Order tracking with real-time updates
  - Responsive design (mobile/tablet/desktop)

### Testing
- **xUnit** - Unit testing framework
- **Moq** - Mocking library
- **FluentAssertions** - Assertion library
- **Test Coverage**: 145+ unit tests across 6 test projects

### Observability
- **Health Checks** - ASP.NET Core Health Checks on all services
- **HealthMonitorService** - Custom monitoring with UI dashboard
- **Structured Logging** - JSON logs with correlation IDs
- **Docker Logs** - Centralized logging (10MB rotation)

---

## Repository Structure

```
src/eShop/
├── OrderService/                    # Order management microservice
│   ├── OrderService.API/            # Controllers, health checks
│   ├── OrderService.Application/    # Commands, queries, handlers
│   ├── OrderService.Domain/         # Order aggregate, domain events
│   ├── OrderService.Infrastructure/ # EF Core, MassTransit, MongoDB projections
│   └── OrderService.Tests/          # Unit tests
│
├── InventoryService/                # Inventory management microservice
│   ├── InventoryService.API/
│   ├── InventoryService.Application/
│   ├── InventoryService.Domain/
│   ├── InventoryService.Infrastructure/
│   └── InventoryService.Tests/
│
├── PaymentService/                  # Payment processing microservice
│   ├── PaymentService.API/
│   ├── PaymentService.Application/
│   ├── PaymentService.Domain/
│   ├── PaymentService.Infrastructure/
│   └── PaymentService.Tests/
│
├── DeliveryService/                 # Shipment & delivery microservice
│   ├── DeliveryService.API/
│   ├── DeliveryService.Application/
│   ├── DeliveryService.Domain/
│   ├── DeliveryService.Infrastructure/
│   └── DeliveryService.Tests/
│
├── InvoiceProcessingService/        # Invoice management microservice
│   ├── src/InvoiceService/
│   │   ├── InvoiceService.API/
│   │   ├── InvoiceService.Application/
│   │   ├── InvoiceService.Domain/
│   │   ├── InvoiceService.Infrastructure/
│   │   └── InvoiceService.Infrastructure.Mongo/
│   └── tests/
│       ├── InvoiceService.Domain.Tests/
│       ├── InvoiceService.Application.Tests/
│       └── InvoiceService.Infrastructure.Tests/
│
├── ApiGateway/                      # Ocelot API Gateway
│   ├── Program.cs
│   ├── ocelot.json                  # Route configuration
│   └── Dockerfile
│
├── HealthMonitorService/            # Health monitoring service
│   ├── HealthMonitorService.API/
│   ├── HealthMonitorService.Application/
│   ├── HealthMonitorService.Domain/
│   ├── HealthMonitorService.Infrastructure/
│   ├── HealthMonitorService.Tests/
│   └── wwwroot/                     # Web dashboard (HTML/JS/CSS with Chart.js)
│
├── SharedService/                   # Shared libraries
│   ├── SharedService.Contracts/     # Event contracts (all events)
│   ├── SharedService.Caching/       # Redis client abstraction
│   └── SharedService.Consul/        # Consul registration utilities
│
├── ui/                              # Next.js frontend (PWA)
│   ├── app/                         # App Router pages
│   │   ├── products/                # Product catalog
│   │   ├── checkout/                # Checkout flow
│   │   ├── orders/                  # Order history & tracking
│   │   └── ...
│   ├── components/                  # React components
│   ├── contexts/                    # React contexts (Basket)
│   ├── lib/                         # API client
│   ├── public/                      # Static assets, PWA files
│   └── Dockerfile
│
├── k8s/                             # Kubernetes manifests
│   ├── infrastructure/              # StatefulSets (SQL, Mongo, Redis, RabbitMQ, Consul)
│   ├── services/                    # Service Deployments
│   ├── gateway/                     # API Gateway & UI
│   ├── deploy.sh / deploy.ps1       # Deployment scripts
│   ├── test-deployment.sh / .ps1    # Smoke tests
│   └── README.md                    # Kubernetes documentation
│
├── docker-compose.yml               # Main orchestration file
├── docker-compose-smoke-test.sh/ps1 # Smoke test scripts
└── README.md                        # This file
```

### Typical Service Layout

Each microservice follows Onion Architecture:

```
{Service}Service/
├── {Service}Service.API/
│   ├── Controllers/                 # API endpoints
│   ├── Middlewares/                 # Custom middleware
│   ├── Program.cs                   # App configuration, DI
│   ├── appsettings.json             # Configuration
│   ├── appsettings.Docker.json      # Docker-specific config
│   └── Dockerfile
│
├── {Service}Service.Application/
│   ├── Commands/                    # Write operations (CQRS)
│   ├── Queries/                     # Read operations (CQRS)
│   ├── DTOs/                        # Data transfer objects
│   ├── Interfaces/                  # Application contracts
│   └── DependencyInjection.cs
│
├── {Service}Service.Domain/
│   ├── Entities/                    # Aggregates, entities
│   ├── Events/                      # Domain events
│   ├── ValueObjects/                # Value objects
│   ├── Enums/                       # Domain enums
│   └── Interfaces/                  # Repository contracts
│
├── {Service}Service.Infrastructure/
│   ├── Persistence/                 # EF Core DbContext, repositories
│   ├── Messaging/                   # MassTransit consumers
│   ├── Projections/                 # MongoDB projection writers
│   ├── Migrations/                  # EF Core migrations
│   ├── Services/                    # External service clients
│   └── DependencyInjection.cs
│
└── {Service}Service.Tests/
    ├── Domain/                      # Domain logic tests
    ├── Application/                 # Use case tests
    └── Infrastructure/              # Integration tests
```

### Shared Libraries

- **SharedService.Contracts**: Event definitions used across all services
  - All events include metadata: CorrelationId, MessageId, CausationId, OccurredAt
  - Located in `SharedService/SharedService.Contracts/Events/`

- **SharedService.Caching**: Redis client abstraction
  - Interface: `IRedisCacheClient`
  - Used by OrderService, HealthMonitorService

- **SharedService.Consul**: Consul service registration utilities
  - Auto-registration on startup
  - Health check registration

---

## Prerequisites

### Required Tools

1. **Docker Desktop** (or Docker Engine + Docker Compose)
   - Version: 24.x or higher
   - Ensure Docker daemon is running
   - Minimum resources: 8GB RAM, 4 CPUs

2. **.NET SDK** (for local development)
   - Version: 8.0 or higher
   - Download: https://dotnet.microsoft.com/download

3. **Node.js and npm** (for UI development)
   - Version: 20.x or higher (for Next.js 14)
   - Download: https://nodejs.org/

### Optional Tools

- **Visual Studio 2022** or **JetBrains Rider** - IDE for .NET development
- **Visual Studio Code** - Lightweight editor
- **Azure Data Studio** or **SQL Server Management Studio** - Database management
- **MongoDB Compass** - MongoDB GUI
- **Postman** or **curl** - API testing

### Operating System

- **Windows**: Windows 10/11 with WSL2 (for Docker Desktop)
- **macOS**: macOS 11 or higher
- **Linux**: Any recent distribution with Docker support

---

## Configuration

### Environment Variables (docker-compose.yml)

The system uses environment variables for configuration. Key variables:

#### SQL Server
```bash
SA_PASSWORD=eSh@pDem@1                 # SQL Server SA password
```

#### MongoDB
```bash
MONGO_INITDB_ROOT_USERNAME=root
MONGO_INITDB_ROOT_PASSWORD=example
```

#### RabbitMQ
```bash
RABBITMQ_DEFAULT_USER=guest
RABBITMQ_DEFAULT_PASS=guest
```

#### Service Configuration (Example: OrderService)
```bash
ASPNETCORE_ENVIRONMENT=Docker
ASPNETCORE_URLS=http://+:8080
ConnectionStrings__OrderDb=Server=sqlserver;Database=OrderDb;User Id=sa;Password=eSh@pDem@1;TrustServerCertificate=True
RabbitMq__Host=rabbitmq
RabbitMq__Username=guest
RabbitMq__Password=guest
Consul__Host=consul
Consul__Port=8500
ServiceName=orderservice
ServiceAddress=orderservice
ServicePort=8080
```

### Connection Strings

All services use these connection patterns:

- **SQL Server**: `Server=sqlserver;Database={ServiceDb};User Id=sa;Password=eSh@pDem@1;TrustServerCertificate=True`
- **MongoDB**: `mongodb://root:example@mongodb:27017`
- **Redis**: `redis:6379`
- **RabbitMQ**: `amqp://guest:guest@rabbitmq:5672`

⚠️ **Security Note**: These are development credentials. Use secrets management (Azure Key Vault, Kubernetes Secrets, etc.) in production.

### Consul Configuration

All services auto-register with Consul on startup:
- **Health Check Endpoint**: `/health/ready`
- **Health Check Interval**: 10 seconds
- **Deregister After**: 1 minute of failures

### Ocelot Gateway Configuration

Routes defined in `ApiGateway/ocelot.json`:
- Service discovery via Consul
- Load balancing: LeastConnection algorithm
- Polling interval: 1000ms

---

## Running the System

### Step-by-Step Startup Guide

#### 1. Clone the Repository

```bash
git clone <repository-url>
cd EShopMicroservices/src/eShop
```

#### 2. Start All Services with Docker Compose

```bash
# From src/eShop directory
docker-compose up -d
```

This command will:
- Pull required images (SQL Server, MongoDB, Redis, RabbitMQ, Consul)
- Build all microservice images
- Start infrastructure services first (with health checks)
- Start microservices when dependencies are healthy
- Start API Gateway and HealthMonitor last

#### 3. Wait for Services to be Healthy

Watch the startup process:

```bash
docker-compose logs -f
```

Or check service health:

```bash
docker-compose ps
```

**Expected startup time**: 2-3 minutes for all services to be healthy.

#### 4. Verify Infrastructure Services

```bash
# Check SQL Server
docker exec sqlserver-db /opt/mssql-tools18/bin/sqlcmd -S localhost -U sa -P "eSh@pDem@1" -C -Q "SELECT name FROM sys.databases"

# Check MongoDB
docker exec mongodb mongosh --eval "db.adminCommand('ping')"

# Check Redis
docker exec redis redis-cli ping

# Check RabbitMQ
docker exec rabbitmq rabbitmq-diagnostics check_running
```

#### 5. Verify Database Creation

On first run, all databases are created automatically via EF Core migrations:
- OrderDb
- InventoryDb
- PaymentDb
- DeliveryDb
- InvoiceDb
- HealthMonitorDb

#### 6. Seed Sample Data (Optional)

Seed 30 products (3,615 total units) across 5 categories:

```bash
curl -X POST http://localhost:8080/api/inventory/seed
```

This creates products in categories: Electronics, Mobile Devices, Audio Equipment, Accessories, Home & Office.

#### 7. Run Smoke Tests

Verify all services are operational:

**Linux/macOS:**
```bash
./docker-compose-smoke-test.sh
```

**Windows PowerShell:**
```powershell
.\docker-compose-smoke-test.ps1
```

Expected output: 15 tests passed.

### Stopping the System

```bash
# Stop all containers
docker-compose down

# Stop and remove volumes (WARNING: Deletes all data)
docker-compose down -v
```

### Rebuilding Services

After code changes:

```bash
# Rebuild all services
docker-compose build

# Rebuild specific service
docker-compose build orderservice

# Rebuild and restart
docker-compose up -d --build
```

---

## Accessing the System

### Service Endpoints

| Service | Direct URL | Via Gateway |
|---------|-----------|-------------|
| **Next.js UI** | http://localhost:3000 | N/A |
| **API Gateway** | http://localhost:8080 | N/A |
| **OrderService** | http://localhost:5000 | http://localhost:8080/api/order |
| **InventoryService** | http://localhost:5001 | http://localhost:8080/api/inventory |
| **PaymentService** | http://localhost:5002 | http://localhost:8080/api/payment |
| **DeliveryService** | http://localhost:5003 | http://localhost:8080/api/shipment |
| **InvoiceService** | http://localhost:5005 | http://localhost:8080/api/v1/invoices |
| **HealthMonitor** | http://localhost:5004 | http://localhost:8080/api/health |

### Infrastructure UIs

- **RabbitMQ Management**: http://localhost:15672 (guest / guest)
- **Consul UI**: http://localhost:8500
- **HealthMonitor Dashboard**: http://localhost:5004

### Health Check Endpoints

All services expose standardized health endpoints:

```bash
# Liveness probe (is the service running?)
curl http://localhost:5000/health/live

# Readiness probe (can the service handle requests?)
curl http://localhost:5000/health/ready
```

Readiness probes check:
- Database connectivity (SQL Server, MongoDB)
- RabbitMQ connectivity
- Redis connectivity (if used)

### Next.js UI Features

Access the customer-facing UI at http://localhost:3000:

1. **Home Page** - API connectivity status
2. **Products** (`/products`) - Browse catalog, search, filter by category
3. **Product Details** (`/products/{id}`) - View details, adjust quantity, add to cart
4. **Shopping Cart** - Slide-in drawer, persistent across sessions (localStorage)
5. **Checkout** (`/checkout`) - Customer info, shipping address, place order
6. **Order Confirmation** (`/order-confirmation`) - Success page with order details
7. **Order Tracking** (`/orders/{id}`) - Real-time status updates (auto-refresh every 5s)
8. **Order History** (`/orders`) - All orders with search and filtering

**PWA Features:**
- Installable on desktop and mobile
- Offline support with service worker
- Add to Home Screen prompt

### API Examples

#### Place an Order (via Gateway)

```bash
curl -X POST http://localhost:8080/api/order \
  -H "Content-Type: application/json" \
  -d '{
    "customerId": "11111111-1111-1111-1111-111111111111",
    "street": "123 Main St",
    "city": "New York",
    "postalCode": "10001",
    "items": [
      {
        "productId": "10000000-0000-0000-0000-000000000001",
        "quantity": 2,
        "unitPrice": 1299.99
      }
    ]
  }'
```

#### Get Order Status

```bash
curl http://localhost:8080/api/order/{orderId}
```

#### Check Inventory

```bash
curl http://localhost:8080/api/inventory/{productId}
```

#### Get Payment Status

```bash
curl http://localhost:8080/api/payment/{paymentId}
```

---

## Health Monitoring

### HealthMonitorService

The HealthMonitorService provides comprehensive monitoring of all services:

#### How It Works

1. **Background Service** polls all registered services every 30 seconds
2. Checks health endpoints (`/health/ready`)
3. Stores current status in `ServiceHealthStatus` table
4. Stores history in `ServiceHealthHistory` (last 1000 per service)
5. Stores execution logs in `ServiceExecutionLog` with detailed error info
6. Publishes `ServiceHealthChanged` events when status changes
7. Invalidates Redis cache on status changes

#### What It Monitors

- **All microservices**: Order, Inventory, Payment, Delivery, Invoice
- **Infrastructure**: API Gateway
- **Metrics tracked**:
  - Health status (Healthy/Unhealthy)
  - Response time (latency in ms)
  - Error messages and stack traces
  - Last checked timestamp
  - Execution timing and success/failure

#### Accessing the Dashboard

Open http://localhost:5004 in a browser:

- **Main View**: All services with current status, latency, last checked
- **Statistics**: Total services, healthy count, unhealthy count, average latency
- **Charts**:
  - Latency trend line chart (last 50 data points)
  - Status timeline bar chart (color-coded)
- **History View**: Click any service to see historical data
- **Execution Logs**: Detailed execution history with error information
- **Search & Filter**: Real-time search by service name/error, filter by status
- **Export**: Download service list or history as CSV
- **Auto-refresh**: Dashboard refreshes every 20 seconds

#### API Endpoints

```bash
# Get all services status
curl http://localhost:5004/api/healthstatus

# Get specific service status
curl http://localhost:5004/api/healthstatus/orderservice

# Get execution logs (all services)
curl http://localhost:5004/api/health/execution-logs

# Get execution logs (specific service)
curl http://localhost:5004/api/health/execution-logs/orderservice
```

#### Database Schema

**ServiceHealthStatus** (Current state):
- ServiceName, IsHealthy, StatusMessage, LastChecked, LatencyMs
- ErrorCode, ExceptionType, StackTrace (structured error details)

**ServiceHealthHistory** (Historical data):
- Same fields as status + CheckedAt timestamp
- Retention: Last 1000 records per service

**ServiceExecutionLog** (Execution details):
- ExecutedAt, ServiceName, ExecutionTimeMs, IsSuccess, IsHealthy
- HttpStatusCode, ServiceAddress, ServicePort
- ErrorMessage, ErrorType, ErrorStackTrace
- Retention: Last 1000 records per service (cleaned every 6 hours)

---

## Event-Driven Flow

### Complete Order Fulfillment Flow

```
1. Customer places order via UI → POST /api/order
   ↓
2. OrderService creates Order aggregate
   └→ Publishes: OrderCreatedEvent
      ↓
3. InventoryService receives OrderCreatedEvent
   ├─ Success: Reserves inventory
   │  └→ Publishes: InventoryReserved
   │     ↓
   │  4. PaymentService receives InventoryReserved
   │     ├─ Authorizes payment
   │     ├─ Captures payment
   │     └→ Publishes: PaymentAuthorized, PaymentCaptured
   │        ↓
   │     5. DeliveryService receives PaymentCaptured
   │        ├─ Creates shipment
   │        ├─ Updates status (Dispatched → Delivered)
   │        └→ Publishes: ShipmentCreated, ShipmentDispatched, ShipmentDelivered
   │           ↓
   │        6. OrderService receives ShipmentCreated/Dispatched/Delivered
   │           └─ Updates order status (Processing → Shipped → Completed)
   │
   └─ Failure: Insufficient inventory
      └→ Publishes: InventoryReservationFailed
         ↓
      7. OrderService receives InventoryReservationFailed
         └─ Updates order status to Failed
```

### Event Flow Details

#### OrderCreatedEvent
```json
{
  "orderId": "guid",
  "customerId": "guid",
  "items": [...],
  "totalAmount": 0.0,
  "correlationId": "guid",
  "messageId": "guid",
  "causationId": "guid",
  "occurredAt": "2024-01-01T00:00:00Z"
}
```

**Published by**: OrderService  
**Consumed by**: InventoryService

#### InventoryReserved
```json
{
  "orderId": "guid",
  "reservationId": "guid",
  "items": [...],
  "correlationId": "guid",
  "messageId": "guid",
  "causationId": "guid",
  "occurredAt": "2024-01-01T00:00:00Z"
}
```

**Published by**: InventoryService  
**Consumed by**: PaymentService, OrderService

#### PaymentCaptured
```json
{
  "paymentId": "guid",
  "orderId": "guid",
  "amount": 0.0,
  "currency": "USD",
  "correlationId": "guid",
  "messageId": "guid",
  "causationId": "guid",
  "occurredAt": "2024-01-01T00:00:00Z"
}
```

**Published by**: PaymentService  
**Consumed by**: DeliveryService, OrderService

#### ShipmentCreated
```json
{
  "shipmentId": "guid",
  "orderId": "guid",
  "carrier": "string",
  "trackingNumber": "string",
  "correlationId": "guid",
  "messageId": "guid",
  "causationId": "guid",
  "occurredAt": "2024-01-01T00:00:00Z"
}
```

**Published by**: DeliveryService  
**Consumed by**: OrderService

### Idempotency Handling

All event consumers implement idempotency using the `ProcessedMessage` table:

```sql
CREATE TABLE ProcessedMessage (
    MessageId NVARCHAR(100) NOT NULL,
    ConsumerName NVARCHAR(100) NOT NULL,
    ProcessedAt DATETIME2 NOT NULL,
    CONSTRAINT PK_ProcessedMessage PRIMARY KEY (MessageId, ConsumerName)
);
```

**Process:**
1. Consumer receives message
2. Check if `(MessageId, ConsumerName)` exists in ProcessedMessage
3. If exists: Skip processing (already handled)
4. If not exists: Process message + Insert record (in same transaction)

This ensures exactly-once processing even if messages are re-delivered.

### Event Metadata

All events include standardized metadata for distributed tracing:

- **CorrelationId**: Unique ID for entire request flow (set by first service)
- **MessageId**: Unique ID for this specific message
- **CausationId**: MessageId of the message that caused this event
- **OccurredAt**: UTC timestamp of when event occurred

This enables:
- Full request tracing across services
- Debugging of event chains
- Audit trails
- Event sourcing capabilities

---

## Testing

### Unit Tests

The system includes **145+ unit tests** across **6 test projects**:

| Service | Test Project | Test Count | Coverage |
|---------|-------------|-----------|----------|
| OrderService | OrderService.Tests | 20+ | Domain layer |
| InventoryService | InventoryService.Tests | 15+ | Domain layer |
| PaymentService | PaymentService.Tests | 25+ | Domain + Value Objects |
| DeliveryService | DeliveryService.Tests | 20+ | Domain layer |
| HealthMonitorService | HealthMonitorService.Tests | 15+ | Domain entities |
| InvoiceService | InvoiceService.Domain.Tests, etc. | 50+ | Comprehensive |

### Running Tests

```bash
# Run all tests
dotnet test

# Run tests for specific service
cd OrderService/OrderService.Tests
dotnet test

# Run with code coverage
dotnet test /p:CollectCoverage=true /p:CoverageReporterOutput=html
```

### Test Organization

Tests follow **AAA pattern** (Arrange, Act, Assert):

```csharp
[Fact]
public void Order_CanBeCreated_WithValidData()
{
    // Arrange
    var customerId = Guid.NewGuid();
    var address = new Address("123 Main St", "New York", "10001");
    var items = new List<OrderItem> { new OrderItem(Guid.NewGuid(), 2, 29.99m) };

    // Act
    var order = Order.Create(customerId, address, items);

    // Assert
    order.Should().NotBeNull();
    order.Status.Should().Be(OrderStatus.Pending);
    order.TotalAmount.Should().Be(59.98m);
}
```

### Integration Tests

Integration test templates are available in `STEP25_TESTING_GUIDE.md`.

To run integration tests:
1. Start infrastructure: `docker-compose up -d sqlserver mongodb rabbitmq redis consul`
2. Update connection strings in test projects
3. Run integration tests: `dotnet test --filter Category=Integration`

### Smoke Tests

Comprehensive smoke tests for docker-compose deployment:

```bash
# Linux/macOS
./docker-compose-smoke-test.sh

# Windows PowerShell
.\docker-compose-smoke-test.ps1
```

**Tests performed:**
1. All containers running
2. Infrastructure services accessible (SQL, Mongo, Redis, RabbitMQ, Consul)
3. All microservices health endpoints responding
4. API Gateway responding
5. Services registered in Consul
6. Databases created and accessible

**Expected result**: 15/15 tests passed

### End-to-End Testing

Test the complete order flow:

1. **Seed inventory**: `curl -X POST http://localhost:8080/api/inventory/seed`

2. **Place order** (use product ID from seed data):
```bash
curl -X POST http://localhost:8080/api/order \
  -H "Content-Type: application/json" \
  -d '{
    "customerId": "11111111-1111-1111-1111-111111111111",
    "street": "123 Main St",
    "city": "New York",
    "postalCode": "10001",
    "items": [
      {
        "productId": "10000000-0000-0000-0000-000000000001",
        "quantity": 1,
        "unitPrice": 1299.99
      }
    ]
  }'
```

3. **Monitor events** in RabbitMQ Management UI: http://localhost:15672

4. **Check order status**: `curl http://localhost:8080/api/order/{orderId}`

5. **Verify in UI**: http://localhost:3000/orders/{orderId}

Expected flow: Pending → Processing → Shipped → Delivered (takes ~30-60 seconds)

---

## Troubleshooting

### Common Startup Issues

#### Problem: Containers fail to start

**Symptom**: `docker-compose up` shows errors

**Solution**:
```bash
# Check Docker daemon is running
docker ps

# Check logs for specific service
docker-compose logs orderservice

# Rebuild and restart
docker-compose down
docker-compose build --no-cache
docker-compose up -d
```

#### Problem: Port conflicts

**Symptom**: "Port already in use" error

**Solution**:
```bash
# Find process using port (example: port 5000)
# Windows
netstat -ano | findstr :5000
# Linux/macOS
lsof -i :5000

# Kill process or change port in docker-compose.yml
```

#### Problem: Services not healthy

**Symptom**: `docker-compose ps` shows "unhealthy" status

**Solution**:
```bash
# Check specific service health
docker exec orderservice curl http://localhost:8080/health/ready

# Check logs for errors
docker-compose logs orderservice

# Common causes:
# - Database not ready (wait longer)
# - RabbitMQ not ready (wait longer)
# - Configuration error (check environment variables)
```

### Database Issues

#### Problem: SQL Server migrations not applied

**Symptom**: "Invalid object name" errors in logs

**Solution**:
```bash
# Migrations are applied automatically on startup
# If not, manually apply:
cd OrderService/OrderService.API
dotnet ef database update --project ../OrderService.Infrastructure

# Or restart the service (migrations run on startup)
docker-compose restart orderservice
```

#### Problem: MongoDB connection issues

**Symptom**: "Unable to connect to MongoDB" errors

**Solution**:
```bash
# Check MongoDB is running
docker exec mongodb mongosh --eval "db.adminCommand('ping')"

# Check connection string in docker-compose.yml
# Verify: mongodb://root:example@mongodb:27017

# Restart MongoDB
docker-compose restart mongodb
```

#### Problem: SQL Server authentication failed

**Symptom**: "Login failed for user 'sa'" errors

**Solution**:
```bash
# Verify password matches in all places:
# 1. sqlserver service (SA_PASSWORD)
# 2. Service connection strings (Password=...)

# Reset by removing volume and restarting
docker-compose down -v
docker-compose up -d
```

### Messaging Issues

#### Problem: RabbitMQ not reachable

**Symptom**: "RabbitMQ connection failed" in service logs

**Solution**:
```bash
# Check RabbitMQ is healthy
docker exec rabbitmq rabbitmq-diagnostics check_running

# Check Management UI
curl http://localhost:15672

# Restart RabbitMQ
docker-compose restart rabbitmq

# Check credentials (guest/guest)
```

#### Problem: Events not being consumed

**Symptom**: Order stuck in "Pending" status

**Solution**:
1. Check RabbitMQ Management UI (http://localhost:15672)
2. Go to Queues tab
3. Check if messages are accumulating
4. Check service logs for consumer errors
5. Verify service is registered as consumer

```bash
# Check consumer is running
docker-compose logs inventoryservice | grep "Consumer"

# Restart consuming service
docker-compose restart inventoryservice
```

### Service Discovery Issues

#### Problem: API Gateway returns 503

**Symptom**: Gateway cannot route to services

**Solution**:
```bash
# Check Consul UI
curl http://localhost:8500/v1/catalog/services

# Verify services are registered
# Should see: orderservice, inventoryservice, etc.

# Check service health in Consul
curl http://localhost:8500/v1/health/service/orderservice

# Restart service to re-register
docker-compose restart orderservice
```

#### Problem: Services not registering with Consul

**Symptom**: Empty service list in Consul UI

**Solution**:
```bash
# Check Consul is healthy
docker exec consul consul members

# Check service logs for registration errors
docker-compose logs orderservice | grep "Consul"

# Verify environment variables:
# - Consul__Host=consul
# - ServiceName=orderservice
# - ServiceAddress=orderservice
```

### HealthMonitor Issues

#### Problem: HealthMonitor shows all services as DOWN

**Symptom**: Dashboard shows all red

**Solution**:
```bash
# Check HealthMonitor logs
docker-compose logs healthmonitorservice

# Verify HealthMonitor can reach Consul
docker exec healthmonitorservice curl http://consul:8500/v1/catalog/services

# Check health endpoints directly
curl http://localhost:5000/health/ready

# Restart HealthMonitor
docker-compose restart healthmonitorservice
```

### UI Issues

#### Problem: UI cannot connect to API Gateway

**Symptom**: "Network Error" or "Failed to fetch" in browser

**Solution**:
```bash
# Verify API Gateway is running
curl http://localhost:8080/health

# Check browser console for errors
# Verify NEXT_PUBLIC_API_BASE_URL in ui/.env or docker-compose.yml

# Restart UI
docker-compose restart eshop-ui

# Access UI directly (not via gateway)
curl http://localhost:3000
```

#### Problem: Cart not persisting

**Symptom**: Cart empties on page refresh

**Solution**:
- Check browser localStorage is enabled
- Check browser console for errors
- Clear browser cache and reload

### Memory Issues

#### Problem: Docker running out of memory

**Symptom**: Services crashing or not starting

**Solution**:
```bash
# Check Docker resource limits
docker stats

# Increase Docker Desktop memory allocation:
# Settings → Resources → Memory → Increase to 8GB+

# Or reduce service resource limits in docker-compose.yml
```

### Network Issues

#### Problem: Services cannot communicate

**Symptom**: "Connection refused" or "Host not found" errors

**Solution**:
```bash
# Check network exists
docker network ls | grep eshop-network

# Verify all services are on same network
docker network inspect eshop-network

# Recreate network
docker-compose down
docker-compose up -d
```

---

## Known Limitations

### Partially Implemented Features

1. **Outbox Pattern**
   - **Status**: Implemented as pilot in InvoiceService
   - **Limitation**: Not yet rolled out to all services
   - **Impact**: Events published directly (not transactionally)
   - **Mitigation**: Idempotency handling prevents duplicate processing

2. **Saga Pattern**
   - **Status**: Not implemented
   - **Limitation**: No explicit compensation logic for failed orders
   - **Impact**: Failed orders require manual intervention
   - **Workaround**: InventoryReservationFailed triggers order failure

3. **Distributed Tracing**
   - **Status**: Correlation IDs present but no visualization
   - **Limitation**: No Jaeger/Zipkin integration
   - **Impact**: Manual correlation of logs across services
   - **Workaround**: Use CorrelationId in logs to trace requests

4. **Metrics Collection**
   - **Status**: Not implemented
   - **Limitation**: No Prometheus/Grafana integration
   - **Impact**: Limited performance monitoring
   - **Workaround**: Use HealthMonitor for basic status tracking

### Service-Specific Limitations

1. **InventoryService**
   - No automatic restock functionality
   - No backorder support
   - Fixed quantity after seeding

2. **PaymentService**
   - Mock payment processing (always succeeds)
   - No actual payment gateway integration
   - No payment method validation

3. **DeliveryService**
   - Mock carrier integration
   - No real tracking number validation
   - No delivery time estimates

4. **HealthMonitorService**
   - Polls every 30 seconds (not real-time)
   - Limited to HTTP health checks
   - No alerting/notifications

### UI Limitations

1. **Order History**
   - Currently uses sessionStorage
   - Production should query MongoDB read model via API
   - No pagination (loads all orders)

2. **Authentication**
   - Not implemented (no user login/registration)
   - CustomerId is generated client-side
   - No authorization on API endpoints

3. **Product Images**
   - Not implemented
   - Would require CDN integration

### Infrastructure Limitations

1. **High Availability**
   - Single instance of SQL Server, MongoDB, RabbitMQ
   - No clustering or replication
   - Downtime during infrastructure failures

2. **Scalability**
   - Microservices can scale horizontally
   - Infrastructure services are single-instance bottlenecks

3. **Security**
   - No TLS/SSL on service-to-service communication
   - No API authentication/authorization
   - Development credentials in docker-compose.yml

---

## Development Notes

### Architectural Principles

1. **Domain-Driven Design**
   - Each service owns its domain model
   - No shared database between services
   - Communication only through events or APIs

2. **Onion Architecture**
   - Dependencies point inward (Domain → Application → Infrastructure)
   - Domain layer has no dependencies
   - Infrastructure implements domain interfaces

3. **CQRS**
   - Commands update aggregates (SQL Server)
   - Queries read from projections (MongoDB)
   - Eventually consistent read models

4. **Event-Driven**
   - Services publish domain events
   - Events are facts (past tense)
   - Consumers are idempotent

### Important Conventions

1. **Event Naming**: `{Entity}{PastTenseAction}Event`
   - Example: `OrderCreatedEvent`, `PaymentCapturedEvent`

2. **Command Naming**: `{Action}{Entity}Command`
   - Example: `PlaceOrderCommand`, `ReserveInventoryCommand`

3. **Query Naming**: `Get{Entity}By{Property}Query`
   - Example: `GetOrderByIdQuery`, `GetInventoryByProductIdQuery`

4. **Consumer Naming**: `{Event}Consumer`
   - Example: `OrderCreatedEventConsumer`

### Adding a New Service

1. **Create Solution Structure**:
```bash
mkdir NewService
cd NewService
dotnet new webapi -n NewService.API
dotnet new classlib -n NewService.Application
dotnet new classlib -n NewService.Domain
dotnet new classlib -n NewService.Infrastructure
dotnet new xunit -n NewService.Tests
```

2. **Implement Onion Architecture**:
   - Domain: Aggregates, entities, value objects, domain events
   - Application: Commands, queries, DTOs, interfaces
   - Infrastructure: DbContext, repositories, MassTransit consumers, projections
   - API: Controllers, health checks, DI configuration

3. **Add to docker-compose.yml**:
   - Build configuration
   - Environment variables
   - Depends on infrastructure services
   - Health check configuration
   - Network membership

4. **Register with Consul**:
   - Add SharedService.Consul reference
   - Configure in Program.cs

5. **Configure Ocelot Routes**:
   - Add routes in ApiGateway/ocelot.json

### Adding a New Event

1. **Define Event Contract** in `SharedService.Contracts/Events/`:
```csharp
public record NewEntityCreatedEvent(
    Guid EntityId,
    // ... other properties
    Guid CorrelationId,
    Guid MessageId,
    Guid CausationId,
    DateTime OccurredAt
) : IEvent;
```

2. **Publish from Domain** (in aggregate):
```csharp
public void DoSomething()
{
    // ... domain logic
    AddDomainEvent(new NewEntityCreatedEvent(/* ... */));
}
```

3. **Publish in Handler** (Infrastructure):
```csharp
await _publishEndpoint.Publish(new NewEntityCreatedEvent(/* ... */));
```

4. **Create Consumer** in consuming service:
```csharp
public class NewEntityCreatedEventConsumer : IConsumer<NewEntityCreatedEvent>
{
    public async Task Consume(ConsumeContext<NewEntityCreatedEvent> context)
    {
        var @event = context.Message;
        // Check idempotency
        // Process event
        // Save to database
    }
}
```

5. **Register Consumer** in DependencyInjection.cs

### Adding Projections

1. **Create Projection Writer** in Infrastructure/Projections/:
```csharp
public class MongoNewEntityProjectionWriter
{
    public async Task ProjectAsync(NewEntity entity)
    {
        var collection = _database.GetCollection<NewEntityReadModel>("newentities");
        var readModel = MapToReadModel(entity);
        await collection.ReplaceOneAsync(
            x => x.Id == entity.Id,
            readModel,
            new ReplaceOptions { IsUpsert = true }
        );
    }
}
```

2. **Call from Consumer** or Command Handler:
```csharp
await _projectionWriter.ProjectAsync(entity);
```

3. **Create Read Repository**:
```csharp
public interface INewEntityReadRepository
{
    Task<NewEntityReadModel> GetByIdAsync(Guid id);
}

public class MongoNewEntityReadRepository : INewEntityReadRepository
{
    public async Task<NewEntityReadModel> GetByIdAsync(Guid id)
    {
        var collection = _database.GetCollection<NewEntityReadModel>("newentities");
        return await collection.Find(x => x.Id == id).FirstOrDefaultAsync();
    }
}
```

4. **Use in Query Handler**:
```csharp
public async Task<NewEntityDto> Handle(GetNewEntityByIdQuery request)
{
    var readModel = await _readRepository.GetByIdAsync(request.Id);
    return MapToDto(readModel);
}
```

### Adding Health Checks

In Program.cs:

```csharp
builder.Services.AddHealthChecks()
    .AddSqlServer(connectionString, name: "database")
    .AddMongoDb(mongoConnection, name: "mongodb")
    .AddRabbitMQ(rabbitMqConnection, name: "rabbitmq")
    .AddRedis(redisConnection, name: "redis");
```

Endpoints:
```csharp
app.MapHealthChecks("/health/live", new HealthCheckOptions
{
    Predicate = _ => false // No checks, just alive
});

app.MapHealthChecks("/health/ready"); // All checks
```

---

## Kubernetes Deployment

Complete Kubernetes manifests are available in the `/k8s` directory.

### Quick Start (Kubernetes)

```bash
cd k8s

# Deploy to cluster
./deploy.sh  # Linux/macOS
.\deploy.ps1  # Windows

# Run smoke tests
./test-deployment.sh  # Linux/macOS
.\test-deployment.ps1  # Windows

# Teardown
./teardown.sh  # Linux/macOS
.\teardown.ps1  # Windows
```

### What's Included

- **Infrastructure**: StatefulSets for SQL Server, MongoDB, Redis, RabbitMQ, Consul
- **Microservices**: Deployments with 2 replicas (HA)
- **Gateway & UI**: API Gateway and Next.js UI
- **Ingress**: NGINX Ingress configuration
- **ConfigMaps & Secrets**: Configuration management
- **Health Checks**: Liveness and readiness probes
- **Resource Limits**: CPU and memory constraints
- **Automated Scripts**: Deploy, test, teardown

### Documentation

- **k8s/README.md**: Complete Kubernetes documentation
- **k8s/QUICK_START.md**: 15-minute deployment guide
- **k8s/MANIFEST.md**: File inventory and reference

### Supported Environments

- Minikube
- Docker Desktop Kubernetes
- Kind
- Azure Kubernetes Service (AKS)
- Amazon Elastic Kubernetes Service (EKS)
- Google Kubernetes Engine (GKE)

### Requirements

- Kubernetes 1.25+
- kubectl 1.25+
- Minimum: 8 CPU, 16GB RAM, 50GB storage
- Recommended: 16 CPU, 32GB RAM, 100GB storage

---

## Future Enhancements

**⚠️ The following features are NOT YET IMPLEMENTED:**

### Short-Term

1. **Authentication & Authorization**
   - User registration and login
   - JWT-based authentication
   - Role-based access control (RBAC)
   - OAuth2/OIDC integration

2. **Saga Pattern**
   - Explicit compensation logic for distributed transactions
   - MassTransit saga implementation
   - Order cancellation with rollback

3. **Outbox Pattern Rollout**
   - Implement in all services (currently only in InvoiceService)
   - Ensure transactional event publishing

4. **Advanced Search**
   - Elasticsearch integration
   - Full-text search on products
   - Search suggestions and autocomplete

### Medium-Term

1. **Observability Stack**
   - **Distributed Tracing**: Jaeger or Zipkin
   - **Metrics**: Prometheus + Grafana
   - **Centralized Logging**: ELK Stack (Elasticsearch, Logstash, Kibana)
   - **APM**: Application Performance Monitoring

2. **Advanced Delivery**
   - Real carrier integration (FedEx, UPS, USPS APIs)
   - Real-time tracking updates
   - Delivery time estimates
   - Address validation

3. **Payment Gateway Integration**
   - Stripe/PayPal integration
   - PCI compliance
   - Payment method management
   - Refund processing

4. **Notification Service**
   - Email notifications (order confirmation, shipping updates)
   - SMS notifications
   - Push notifications (PWA)
   - Notification preferences

### Long-Term

1. **Service Mesh**
   - Istio or Linkerd for service-to-service communication
   - mTLS between services
   - Advanced traffic management
   - Observability integration

2. **Event Sourcing**
   - Store events as source of truth
   - Rebuild state from events
   - Temporal queries
   - Event versioning

3. **GraphQL API**
   - GraphQL gateway for flexible queries
   - Real-time subscriptions
   - Federated schema

4. **Admin Portal**
   - Inventory management UI
   - Order management UI
   - Customer management UI
   - Analytics dashboard

5. **Multi-Tenancy**
   - Support for multiple stores/tenants
   - Tenant isolation
   - Tenant-specific configuration

6. **Advanced Testing**
   - Contract testing (Pact)
   - Chaos engineering (Chaos Monkey)
   - Performance testing (k6)
   - End-to-end testing (Playwright)

---

## Contributing

This is a demonstration project showcasing enterprise architecture patterns. It serves as a reference for building production-ready microservices systems with .NET.

### Key Learning Areas

- Microservices architecture and decomposition
- Domain-Driven Design (DDD) patterns
- CQRS and Event Sourcing concepts
- Event-driven communication
- Onion/Clean Architecture
- API Gateway pattern
- Service discovery
- Health monitoring
- Containerization and orchestration
- Testing strategies

---

## License

This project is for educational and demonstration purposes.

---

## Support

For questions or issues:
1. Check the [Troubleshooting](#troubleshooting) section
2. Review logs: `docker-compose logs <service-name>`
3. Check service health: Health Monitor Dashboard (http://localhost:5004)
4. Verify Consul registration: http://localhost:8500
5. Check RabbitMQ queues: http://localhost:15672

---

**Built with ❤️ using .NET, Domain-Driven Design, and modern architecture patterns.**
