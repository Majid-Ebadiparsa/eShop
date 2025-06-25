# Resolve SQL Server IP dynamically
$SqlServerContainerIp = docker inspect -f '{{range .NetworkSettings.Networks}}{{.IPAddress}}{{end}}' sqlserver-db

# Load migration state
$migrationStatePath = "deploy/sql/migration_state.json"
if (!(Test-Path $migrationStatePath)) {
    $initialState = @{ OrderDb = @(); InventoryDb = @() }
    $initialState | ConvertTo-Json -Depth 3 | Set-Content $migrationStatePath
}
$migrationState = Get-Content $migrationStatePath | ConvertFrom-Json

# Apply OrderDb scripts
Get-ChildItem -Path "deploy/sql/OrderDb" -Filter *.sql | Sort-Object Name | ForEach-Object {
    $scriptName = $_.Name
    if ($migrationState.OrderDb -notcontains $scriptName) {
        Write-Host "Applying $scriptName for OrderDb..."
        docker cp "deploy/sql/OrderDb/$scriptName" sql-migrator:/tmp/$scriptName
        docker exec sql-migrator /opt/mssql-tools/bin/sqlcmd -S $SqlServerContainerIp -U sa -P $env:SQLSERVER_SA_PASSWORD -d master -i "/tmp/$scriptName"
        $migrationState.OrderDb += $scriptName
        $migrationState | ConvertTo-Json -Depth 3 | Set-Content $migrationStatePath
    }
}

# Apply InventoryDb scripts
Get-ChildItem -Path "deploy/sql/InventoryDb" -Filter *.sql | Sort-Object Name | ForEach-Object {
    $scriptName = $_.Name
    if ($migrationState.InventoryDb -notcontains $scriptName) {
        Write-Host "Applying $scriptName for InventoryDb..."
        docker cp "deploy/sql/InventoryDb/$scriptName" sql-migrator:/tmp/$scriptName
        docker exec sql-migrator /opt/mssql-tools/bin/sqlcmd -S $SqlServerContainerIp -U sa -P $env:SQLSERVER_SA_PASSWORD -d master -i "/tmp/$scriptName"
        $migrationState.InventoryDb += $scriptName
        $migrationState | ConvertTo-Json -Depth 3 | Set-Content $migrationStatePath
    }
}
