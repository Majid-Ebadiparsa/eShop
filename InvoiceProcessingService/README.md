
# Invoice Processing Service (API + Subscriber)

A minimal **ASP.NET Core** + **.NET 5 Console** demo that implements the Onventis tech assignment:

- An **API** that accepts an `Invoice` payload, persists it to **SQLite (EF Core)**, and publishes the same structure to **RabbitMQ**.
- A **subscriber console** that consumes the message and prints it to the console.
- **JWT auth** for securing the invoice endpoint.
- **Postman collection** for a one‑click test flow.
- **Docker Compose** for local orchestration (API + RabbitMQ + Subscriber). 
- Optional **CloudAMQP** connectivity via a single environment variable.

> Designed to be self‑explanatory: the steps below let anyone run and validate the solution without extra guidance.

---

## 1) Tech Stack

- **API**: ASP.NET Core 9 (C#), MediatR, MassTransit
- **Auth**: JWT (demo credentials)
- **Data**: SQLite + EF Core (auto-migrations on startup)
- **Messaging**: RabbitMQ (local via Docker, or hosted via CloudAMQP)
- **Observability**: Swagger (with Bearer auth), Health Checks (`/health/live`, `/health/ready`)
- **Subscriber**: .NET 5 console app (MassTransit consumer)

---

## 2) Project Structure (high level)

```
/InvoiceProcessingService.sln
/src
  /InvoiceService
    /InvoiceService.API                  # ASP.NET Core API
    /InvoiceService.Application          # Commands, DTOs, CQRS abstractions
    /InvoiceService.Infrastructure       # EF Core, MassTransit wiring, migrations
  /Subscriber
    /InvoiceSubscriber.Console           # .NET 5 console subscriber (MassTransit)
/tests                                  # Sample unit/integration tests
/postman                                # Postman collection
docker-compose.yml
```

---

## 3) Endpoints (API)

**Swagger UI**: `http://localhost:5169/swagger` (Docker) or `https://localhost:<port>/swagger` (VS)

- **Get JWT**  
  `POST /api/v1/auth/token`  
  Body:
  ```json
  { "username": "demo", "password": "Passw0rd!" }
  ```

- **Submit Invoice (secured)**  
  `POST /api/v1/invoices` (requires `Authorization: Bearer <token>`)  
  Example body:
  ```json
  {
    "description": "Office supplies",
    "dueDate": "2030-01-01T00:00:00Z",
    "supplier": "ACME GmbH",
    "lines": [
      { "description": "Paper A4", "price": 5.99, "quantity": 10 },
      { "description": "Pens",     "price": 1.99, "quantity": 5  }
    ]
  }
  ```

- **Health**  
  - Liveness: `GET /health/live`  
  - Readiness: `GET /health/ready`  

> The API uses **API versioning**: `v1` in the route (e.g., `/api/v1/invoices`).

---

## 4) How to Run (two easy options)

### Option A — Run everything locally via Docker (default)

1. Ensure Docker is installed.
2. From the repository root, run:
   ```bash
   docker compose up --build
   ```
3. Open **Swagger** at `http://localhost:5169/swagger`.
4. **Get a token** via `POST /api/v1/auth/token` using:
   - `username`: `demo`
   - `password`: `Passw0rd!`
5. Click **Authorize** in Swagger and paste `Bearer <your-token>`.
6. Call `POST /api/v1/invoices` with the sample body above.
7. Check the **subscriber console** logs in Docker: it will print the invoice summary.

**Ports & data** (Docker):
- API: `http://localhost:5169`
- RabbitMQ management UI: `http://localhost:15672` (user/pass: `guest/guest`)
- SQLite files are persisted in Docker volumes:
  - API DB: `invoices-data:/app/data`
  - Subscriber Inbox: `subscriber-inbox:/app/data`


### Option B — Use CloudAMQP (no local RabbitMQ required)

1. Create a free **CloudAMQP** instance (e.g., "Little Lemur").  
   Copy your **AMQP URL** from the instance dashboard. It looks like:
   ```
   amqps://USER:PASSWORD@HOST/VHOST
   ```
2. Create a `.env` file next to `docker-compose.yml` and set:
   ```ini
   CLOUDAMQP_URL=amqps://USER:PASSWORD@HOST/VHOST
   ```
3. Run:
   ```bash
   docker compose up --build
   ```
4. Test via Swagger or Postman as in Option A.

> The app automatically prefers `CLOUDAMQP_URL` when provided; otherwise it falls back to the local RabbitMQ settings (`RabbitMQ__Host`, etc.). TLS is used for `amqps` connections.

---

## 5) Run via Visual Studio (Developer mode)

- Set startup project to **InvoiceService.API** (and optionally run the **InvoiceSubscriber.Console** separately).
- API default launch URL exposes Swagger.  
- If you want to test with CloudAMQP from VS, add an environment variable to the launch profile:
  ```json
  "environmentVariables": {
    "ASPNETCORE_ENVIRONMENT": "Development",
    "CLOUDAMQP_URL": "amqps://USER:PASSWORD@HOST/VHOST"
  }
  ```
- Otherwise, leave it unset to use local RabbitMQ (Docker).

---

## 6) Postman (Bonus)

Import the collection:
```
postman/Invoice_Processing_Service_v1.postman_collection.json
```
Flow:
1. **Auth** → `POST /api/v1/auth/token`
2. **Use token** in `Authorization: Bearer <token>`
3. **Submit invoice** → `POST /api/v1/invoices`
4. Observe **subscriber** console output

---

## 7) Expected Subscriber Output

A successful run prints to the console (and logs), e.g.:
```
[Subscriber] Invoice: Office supplies, Supplier: ACME GmbH, Lines: 2
```

---

## 8) Configuration Summary

You can configure via **appsettings.json** or **environment variables**.

### RabbitMQ (local fallback)
```json
"RabbitMQ": {
  "Host": "rabbitmq",
  "VirtualHost": "/",
  "Username": "guest",
  "Password": "guest"
}
```

### CloudAMQP (preferred if set)
```
CLOUDAMQP_URL=amqps://USER:PASSWORD@HOST/VHOST
```

### SQLite
- API connection string (Docker): `Data Source=/app/data/invoices.db;Cache=Shared`
- Auto‑migrations run on startup.

### JWT (demo only)
```json
"Jwt": {
  "Issuer": "InvoiceProcessingService",
  "Audience": "InvoiceProcessingService.Clients",
  "Key": "super-secret-demo-key",
  "ExpiryMinutes": 60
}
```
> **Credentials (demo)**: `demo / Passw0rd!`  
> In production, replace with a proper user store and secure key management.

---

## 9) Why it matches the assignment

- **Endpoint & contract**: `POST /api/v1/invoices` accepts `Invoice` with `Lines` (each has `Description`, `Price`, `Quantity`).  
- **Persistence**: Data saved to **SQLite** via **EF Core**.  
- **Messaging**: Entire structure is published to **RabbitMQ** and consumed by the console app.  
- **Security**: Endpoint is **JWT‑protected**; token obtained via `POST /api/v1/auth/token`.  
- **Bonus**: Postman collection and Docker orchestration provided.

---

## 10) Clean Code & SOLID at a glance

- **Separation of concerns**: API / Application / Infrastructure layers; messaging and data access are isolated.
- **Options pattern**: All messaging/db/jwt settings wired via configuration & DI.
- **MassTransit wiring**: Encapsulated in dedicated registration with a single switch between `CLOUDAMQP_URL` and local settings.
- **Migrations**: Applied by a hosted service—no manual step needed.
- **Tests**: Sample tests included (API integration & infrastructure unit tests).

---

## 11) Troubleshooting

- **401 on /invoices**: Missing/expired JWT → call `/api/v1/auth/token` and click **Authorize** in Swagger.
- **No message consumed**: Check RabbitMQ connectivity.  
  - Local: `http://localhost:15672` (guest/guest).  
  - CloudAMQP: verify `CLOUDAMQP_URL` and allowed connections. 
- **DB not created**: Ensure container can write to volume; migrations run on startup.
- **CORS issues**: Not expected when calling from Swagger/Postman. For browser apps, adjust CORS policy as needed.

---

## 12) Security Notes (demo)

- Demo credentials and JWT key are for assignment/testing only.
- For production: rotate secrets, store keys in a secret manager, replace demo user with a real identity provider.

---

**Enjoy!** If you prefer a 60‑second live demo:  
1) `docker compose up --build`  
2) Swagger → Token → Submit Invoice  
3) Watch the subscriber console print the message.
