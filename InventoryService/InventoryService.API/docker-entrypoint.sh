#!/bin/bash
set -e

echo "Applying InventoryService database migrations..."

dotnet ef database update \
  --project InventoryService.Infrastructure/InventoryService.Infrastructure.csproj \
  --startup-project InventoryService.API/InventoryService.API.csproj \
  --context InventoryDbContext
