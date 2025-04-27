#!/bin/bash
set -e

echo "Waiting for SQL Server to be ready..."
for i in {1..30}; do
    if /opt/mssql-tools/bin/sqlcmd -S sqlserver -U sa -P 'eSh@pDem@1' -Q "SELECT 1" > /dev/null 2>&1; then
        echo "SQL Server is ready!"
        break
    else
        echo "SQL Server is starting up... (attempt $i)"
        sleep 3
    fi
done

echo "Applying database migrations..."
dotnet ef database update --project OrderService.Infrastructure/OrderService.Infrastructure.csproj --startup-project OrderService.API/OrderService.API.csproj

echo "Starting the application..."
exec dotnet OrderService.API.dll
