#!/bin/bash
set -e

echo "Running Database Migration..."

# Run SQL migration script
/opt/mssql-tools/bin/sqlcmd \
  -S $SQLSERVER_HOST -U sa -P $SQLSERVER_SA_PASSWORD \
  -d master -i /app/migration/migration.sql

echo "Migration Completed Successfully."
