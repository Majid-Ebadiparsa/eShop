#!/bin/bash
set -e

SQLSERVER_HOST="sqlserver-db"

for db in OrderDb InventoryDb; do
  for file in deploy/sql/$db/*.sql; do
    [ -f "$file" ] || continue
    filename=$(basename "$file")
    echo "Applying $filename for $db..."
    docker cp "$file" sql-migrator:/sql/$filename
    docker exec sql-migrator /opt/mssql-tools/bin/sqlcmd -S $SQLSERVER_HOST -U sa -P "$SQLSERVER_SA_PASSWORD" -d master -i "/sql/$filename"
  done
done

echo "All migrations completed successfully."
