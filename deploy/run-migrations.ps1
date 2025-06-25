# Resolve SQL Server IP dynamically
$SqlServerContainerIp = docker inspect -f '{{range .NetworkSettings.Networks}}{{.IPAddress}}{{end}}' sqlserver-db

# Apply migrations for OrderDb
Get-ChildItem -Path "deploy/sql/OrderDb" -Filter *.sql | Sort-Object Name | ForEach-Object {
    $scriptName = $_.Name
    Write-Host "Applying $scriptName for OrderDb..."
    docker exec sql-migrator /opt/mssql-tools/bin/sqlcmd -S $SqlServerContainerIp -U sa -P $env:SQLSERVER_SA_PASSWORD -d master -i "/sql/OrderDb/$scriptName"
}

# Apply migrations for InventoryDb
Get-ChildItem -Path "deploy/sql/InventoryDb" -Filter *.sql | Sort-Object Name | ForEach-Object {
    $scriptName = $_.Name
    Write-Host "Applying $scriptName for InventoryDb..."
    docker exec sql-migrator /opt/mssql-tools/bin/sqlcmd -S $SqlServerContainerIp -U sa -P $env:SQLSERVER_SA_PASSWORD -d master -i "/sql/InventoryDb/$scriptName"
}
