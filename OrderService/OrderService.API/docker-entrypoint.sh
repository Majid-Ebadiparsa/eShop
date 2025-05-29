#!/bin/bash
set -e

echo "Applying OrderService database migrations..."
dotnet ef database update \
  --project OrderService/OrderService.Infrastructure/OrderService.Infrastructure.csproj \
  --startup-project OrderService/OrderService.API/OrderService.API.csproj \
  --context OrderDbContext
