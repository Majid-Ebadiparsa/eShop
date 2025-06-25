#!/bin/bash

set -e

echo "Applying SQL Migrations..."

# Apply OrderDb
for file in $(ls deploy/sql/OrderDb/*.sql | sort); do
  filename=$(basename "$file")
  if ! grep -q "$filename" deploy/sql/migration_state.json; then
    echo "Applying $filename for OrderDb..."
    docker cp "$file" sql-migrator:/tmp/$filename
    docker exec sql-migrator /opt/mssql-tools/bin/sqlcmd -S sqlserver-db -U sa -P "$SQLSERVER_SA_PASSWORD" -d master -i "/tmp/$filename"
    jq --arg f "$filename" '.OrderDb += [$f]' deploy/sql/migration_state.json > temp.json && mv temp.json deploy/sql/migration_state.json
  fi
done

# Apply InventoryDb
for file in $(ls deploy/sql/InventoryDb/*.sql | sort); do
  filename=$(basename "$file")
  if ! grep -q "$filename" deploy/sql/migration_state.json; then
    echo "Applying $filename for InventoryDb..."
    docker cp "$file" sql-migrator:/tmp/$filename
    docker exec sql-migrator /opt/mssql-tools/bin/sqlcmd -S sqlserver-db -U sa -P "$SQLSERVER_SA_PASSWORD" -d master -i "/tmp/$filename"
    jq --arg f "$filename" '.InventoryDb += [$f]' deploy/sql/migration_state.json > temp.json && mv temp.json deploy/sql/migration_state.json
  fi
done

echo "Migrations completed."
