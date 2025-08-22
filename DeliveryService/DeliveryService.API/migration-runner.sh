#!/bin/bash
set -e
echo "Running DeliveryDb SQL..."
/opt/mssql-tools/bin/sqlcmd \
  -S $SQLSERVER_HOST -U sa -P $SQLSERVER_SA_PASSWORD \
  -d master -i /migration/migration.sql
echo "DeliveryDb migration completed."
