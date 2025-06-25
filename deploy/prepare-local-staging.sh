#!/bin/bash
set -e

echo "Preparing local staging environment..."

cp deploy/docker-compose.staging-template.yml deploy/docker-compose.staging-local.yml

sed -i "s|@@SQLSERVER_SA_PASSWORD@@|MyStrongPassword123|g" deploy/docker-compose.staging-local.yml
sed -i "s|@@ORDERDB_CONNECTIONSTRING@@|Server=sqlserver-db;Database=OrderDb;User Id=sa;Password=MyStrongPassword123;TrustServerCertificate=True;|g" deploy/docker-compose.staging-local.yml
sed -i "s|@@INVENTORYDB_CONNECTIONSTRING@@|Server=sqlserver-db;Database=InventoryDb;User Id=sa;Password=MyStrongPassword123;TrustServerCertificate=True;|g" deploy/docker-compose.staging-local.yml
sed -i "s|@@RABBITMQ_USERNAME@@|guest|g" deploy/docker-compose.staging-local.yml
sed -i "s|@@RABBITMQ_PASSWORD@@|guest|g" deploy/docker-compose.staging-local.yml
sed -i "s|@@RABBITMQ_VIRTUALHOST@@|/|g" deploy/docker-compose.staging-local.yml

echo "Local staging file generated: deploy/docker-compose.staging-local.yml"
