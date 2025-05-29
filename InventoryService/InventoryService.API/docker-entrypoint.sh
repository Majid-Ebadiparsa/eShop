#!/bin/bash
set -e

echo "Applying InventoryService database migrations..."
dotnet ef database update \
  --project InventoryService/InventoryService.Infrastructure/InventoryService.Infrastructure.csproj \
  --startup-project InventoryService/InventoryService.API/InventoryService.API.csproj \
  --context InventoryDbContext
