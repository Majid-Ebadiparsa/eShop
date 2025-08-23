#!/bin/bash
set -euo pipefail

echo "Running EF Core migrations for DeliveryDb..."
echo "ConnectionStrings__DeliveryDb: ${ConnectionStrings__DeliveryDb:-<not set>}"

dotnet ef database update \
  -p DeliveryService/DeliveryService.Infrastructure/DeliveryService.Infrastructure.csproj \
  -s DeliveryService/DeliveryService.API/DeliveryService.API.csproj \
  -c DeliveryService.Infrastructure.Repositories.EF.DeliveryDbContext

echo "Migrations applied successfully."