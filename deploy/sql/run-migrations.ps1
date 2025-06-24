# Load migration state
$migrationStatePath = "./deploy/migration_state.json"
if (!(Test-Path $migrationStatePath)) {
    $initialState = @{ OrderDb = @(); InventoryDb = @() }
    $initialState | ConvertTo-Json | Set-Content $migrationStatePath
}
$migrationState = Get-Content $migrationStatePath | ConvertFrom-Json

# Process OrderDb Scripts
$orderDbScripts = Get-ChildItem -Path "./deploy/sql/OrderDb" -Filter *.sql | Sort-Object Name
foreach ($script in $orderDbScripts) {
    if ($migrationState.OrderDb -notcontains $script.Name) {
        Write-Host "Processing OrderDb script: $($script.Name)"
        docker cp "./deploy/sql/OrderDb/$($script.Name)" sql-migrator:/tmp/$($script.Name)
        docker exec sql-migrator /opt/mssql-tools/bin/sqlcmd -S sqlserver-db -U sa -P "$env:SQLSERVER_SA_PASSWORD" -d master -i "/tmp/$($script.Name)"
        $migrationState.OrderDb += $script.Name
        $migrationState | ConvertTo-Json | Set-Content $migrationStatePath
    } else {
        Write-Host "Skipping already applied OrderDb script: $($script.Name)"
    }
}

# Process InventoryDb Scripts
$inventoryDbScripts = Get-ChildItem -Path "./deploy/sql/InventoryDb" -Filter *.sql | Sort-Object Name
foreach ($script in $inventoryDbScripts) {
    if ($migrationState.InventoryDb -notcontains $script.Name) {
        Write-Host "Processing InventoryDb script: $($script.Name)"
        docker cp "./deploy/sql/InventoryDb/$($script.Name)" sql-migrator:/tmp/$($script.Name)
        docker exec sql-migrator /opt/mssql-tools/bin/sqlcmd -S sqlserver-db -U sa -P "$env:SQLSERVER_SA_PASSWORD" -d master -i "/tmp/$($script.Name)"
        $migrationState.InventoryDb += $script.Name
        $migrationState | ConvertTo-Json | Set-Content $migrationStatePath
    } else {
        Write-Host "Skipping already applied InventoryDb script: $($script.Name)"
    }
}
