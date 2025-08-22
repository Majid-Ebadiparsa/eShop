#!/bin/bash
set -e

SQLSERVER_HOST="sqlserver-db"

for db in OrderDb InventoryDb DeliveryDb; do
  for file in deploy/sql/$db/*.sql; do
    [ -f "$file" ] || continue
    filename=$(basename "$file")
    echo "Applying $filename for $db..."
    docker cp "$file" sql-migrator:/sql/$filename
    docker exec sql-migrator /opt/mssql-tools/bin/sqlcmd -S $SQLSERVER_HOST -U sa -P "$SQLSERVER_SA_PASSWORD" -d master -i "/sql/$filename"
  done
done

if [ -f artifacts/sql/DeliveryService_All.sql ]; then
  echo "Applying EF idempotent script for DeliveryDb..."
  docker cp artifacts/sql/DeliveryService_All.sql sql-migrator:/sql/DeliveryService_All.sql
  docker exec sql-migrator /opt/mssql-tools/bin/sqlcmd -S $SQLSERVER_HOST -U sa -P "$SQLSERVER_SA_PASSWORD" -d master -i "/sql/DeliveryService_All.sql"
fi

echo "All migrations completed successfully."
