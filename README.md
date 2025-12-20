# eShop Microservices System

A comprehensive microservices-based e-commerce system built with .NET 10, demonstrating modern architecture patterns and best practices.

## Architecture

The system follows **Domain-Driven Design (DDD)** with **Onion Architecture** per microservice, implementing:

- **CQRS** (Command Query Responsibility Segregation)
- **Event-Driven Architecture** with MassTransit and RabbitMQ
- **API Gateway** pattern with Ocelot
- **Service Discovery** with Consul
- **Health Monitoring** with dedicated service
- **Docker Compose** for local orchestration

## Services

### Core Services

1. **OrderService** - Manages order lifecycle
   - Creates orders
   - Updates order state based on events
   - Publishes `OrderCreatedEvent`

2. **InventoryService** - Manages product inventory
   - Reserves inventory on order creation
   - Releases inventory on payment failure
   - Publishes `InventoryReserved` / `InventoryReservationFailed`

3. **PaymentService** - Handles payment processing
   - Authorizes and captures payments
   - Uses MongoDB for read models
   - Publishes payment events

4. **DeliveryService** - Manages shipments
   - Creates shipments on payment capture
   - Tracks shipment status
   - Publishes shipment events

### Infrastructure Services

5. **ApiGateway** - Ocelot-based API Gateway
   - Routes requests to backend services
   - Integrates with Consul for service discovery

6. **HealthMonitorService** - Health monitoring
   - Polls service health endpoints
   - Stores health status history
   - Exposes health status API

## Technology Stack

- **.NET 10**
- **SQL Server** - Command database (EF Core)
- **MongoDB** - Read models / projections
- **Redis** - Caching (to be implemented)
- **RabbitMQ** - Message broker (MassTransit)
- **Consul** - Service discovery
- **Ocelot** - API Gateway
- **Docker** - Containerization

## Event Flow

```
OrderService (OrderCreatedEvent)
    ↓
InventoryService (InventoryReserved / InventoryReservationFailed)
    ↓
PaymentService (PaymentAuthorized / PaymentCaptured / PaymentFailed)
    ↓
DeliveryService (ShipmentCreated / ShipmentDispatched / ShipmentDelivered)
    ↓
OrderService (updates order state)
```

## How to Run

### Prerequisites

- Docker Desktop
- .NET 10 SDK (for local development)

### Using Docker Compose

1. Navigate to the project root:
```bash
cd src/eShop
```

2. Start all services:
```bash
docker-compose up -d
```

3. Wait for all services to be healthy (check logs):
```bash
docker-compose logs -f
```

4. Access services:
   - **API Gateway**: http://localhost:8080
   - **OrderService**: http://localhost:5000
   - **InventoryService**: http://localhost:5001
   - **PaymentService**: http://localhost:5002
   - **DeliveryService**: http://localhost:5003
   - **HealthMonitorService**: http://localhost:5004
   - **RabbitMQ Management**: http://localhost:15672 (guest/guest)
   - **Consul UI**: http://localhost:8500
   - **MongoDB**: localhost:27017

### Health Checks

All services expose health endpoints:
- `/health/live` - Liveness probe
- `/health/ready` - Readiness probe (checks DB, RabbitMQ, etc.)

### Health Monitoring

Access health status:
```bash
# Get all services status
curl http://localhost:5004/api/healthstatus

# Get specific service status
curl http://localhost:5004/api/healthstatus/orderservice
```

## API Gateway Routes

- `/api/order/*` → OrderService
- `/api/inventory/*` → InventoryService
- `/api/payment/*` → PaymentService
- `/api/shipment/*` → DeliveryService

## Development

### Running Locally

1. Start infrastructure services:
```bash
docker-compose up -d sqlserver mongodb redis rabbitmq consul
```

2. Update connection strings in `appsettings.Development.json` for each service

3. Run services individually from Visual Studio or using `dotnet run`

### Database Migrations

Each service uses EF Core migrations. To apply migrations:

```bash
# For OrderService
cd OrderService/OrderService.API
dotnet ef migrations add Fix_OrderModel --project ../OrderService.Infrastructure --startup-project . 
dotnet ef database update --project ../OrderService.Infrastructure
# For InventoryService
cd InventoryService/InventoryService.API
dotnet ef migrations add Fix_InventoryModel --project ../InventoryService.Infrastructure --startup-project . 
dotnet ef database update --project ../InventoryService.Infrastructure
# For DeliveryService
cd DeliveryService/DeliveryService.API
dotnet ef migrations add Fix_DeliveryModel --project ../DeliveryService.Infrastructure --startup-project . 
dotnet ef database update --project ../DeliveryService.Infrastructure
# For InvoiceService
cd InvoiceProcessingService/src/InvoiceService/InvoiceService.API
dotnet ef migrations add Fix_InvoiceModel --project ../InvoiceService.Infrastructure --startup-project . 
dotnet ef database update --project ../InvoiceService.Infrastructure
# For PaymentService
cd PaymentService/PaymentService.API
dotnet ef migrations add Fix_PaymentModel --context PaymentDbContext --project ../PaymentService.Infrastructure --startup-project . 
dotnet ef database update --context PaymentDbContext --project ../PaymentService.Infrastructure

```

## Testing

### Place an Order

```bash
curl -X POST http://localhost:8080/api/order \
  -H "Content-Type: application/json" \
  -d '{
    "customerId": "123e4567-e89b-12d3-a456-426614174000",
    "street": "123 Main St",
    "city": "New York",
    "postalCode": "10001",
    "items": [
      {
        "productId": "223e4567-e89b-12d3-a456-426614174000",
        "quantity": 2,
        "unitPrice": 29.99
      }
    ]
  }'
```

## License

This is a demonstration project.
