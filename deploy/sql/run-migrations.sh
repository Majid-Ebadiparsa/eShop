#!/bin/bash

set -e

echo "Waiting for SQL Server to be ready..."

for i in {1..30}; do
  if docker exec sql-migrator /opt/mssql-tools/bin/sqlcmd -S sqlserver-db -U sa -P "$SQLSERVER_SA_PASSWORD" -Q "SELECT 1" > /dev/null 2>&1; then
    echo "SQL Server is ready."
    break
  else
    echo "Waiting ($i)..."
    sleep 2
  fi
done

echo "Applying SQL Migrations..."

# Ensure migration state file exists
if [ ! -f deploy/sql/migration_state.json ]; then
  echo '{ "OrderDb": [], "InventoryDb": [] }' > deploy/sql/migration_state.json
fi

# Apply OrderDb
for file in $(ls deploy/sql/OrderDb/*.sql | sort); do
  filename=$(basename "$file")
  if ! grep -q "$filename" deploy/sql/migration_state.json; then
    echo "Applying $filename to OrderDb..."
    docker cp "$file" sql-migrator:/tmp/$filename
    docker exec sql-migrator /opt/mssql-tools/bin/sqlcmd -S sqlserver-db -U sa -P "$SQLSERVER_SA_PASSWORD" -d OrderDb -i "/tmp/$filename"
    jq --arg f "$filename" '.OrderDb += [$f]' deploy/sql/migration_state.json > temp.json && mv temp.json deploy/sql/migration_state.json
  fi
done

# Apply InventoryDb
for file in $(ls deploy/sql/InventoryDb/*.sql | sort); do
  filename=$(basename "$file")
  if ! grep -q "$filename" deploy/sql/migration_state.json; then
    echo "Applying $filename to InventoryDb..."
    docker cp "$file" sql-migrator:/tmp/$filename
    docker exec sql-migrator /opt/mssql-tools/bin/sqlcmd -S sqlserver-db -U sa -P "$SQLSERVER_SA_PASSWORD" -d InventoryDb -i "/tmp/$filename"
    jq --arg f "$filename" '.InventoryDb += [$f]' deploy/sql/migration_state.json > temp.json && mv temp.json deploy/sql/migration_state.json
  fi
done

echo "Migrations completed."
