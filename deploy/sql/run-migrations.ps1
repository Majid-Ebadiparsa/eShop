# Use host connection via Docker published port
$SqlServerHost = "localhost"
$SqlServerPort = "1433"

# Load migration state file
$migrationStatePath = "./deploy/migration_state.json"
if (!(Test-Path $migrationStatePath)) {
    $initialState = @{ OrderDb = @(); InventoryDb = @() }
    $initialState | ConvertTo-Json | Set-Content $migrationStatePath
}
$migrationState = Get-Content $migrationStatePath | ConvertFrom-Json

# Process OrderDb Migrations
$orderDbScripts = Get-ChildItem -Path "./deploy/sql/OrderDb" -Filter *.sql | Sort-Object Name
foreach ($script in $orderDbScripts) {
    if ($migrationState.OrderDb -notcontains $script.Name) {
        Write-Host "Executing OrderDb script: $($script.Name)"
        sqlcmd -S "$SqlServerHost,$SqlServerPort" -U sa -P "$env:SQLSERVER_SA_PASSWORD" -d master -i "./deploy/sql/OrderDb/$($script.Name)"
        $migrationState.OrderDb += $script.Name
        $migrationState | ConvertTo-Json | Set-Content $migrationStatePath
    } else {
        Write-Host "Skipping OrderDb script (already applied): $($script.Name)"
    }
}

# Process InventoryDb Migrations
$inventoryDbScripts = Get-ChildItem -Path "./deploy/sql/InventoryDb" -Filter *.sql | Sort-Object Name
foreach ($script in $inventoryDbScripts) {
    if ($migrationState.InventoryDb -notcontains $script.Name) {
        Write-Host "Executing InventoryDb script: $($script.Name)"
        sqlcmd -S "$SqlServerHost,$SqlServerPort" -U sa -P "$env:SQLSERVER_SA_PASSWORD" -d master -i "./deploy/sql/InventoryDb/$($script.Name)"
        $migrationState.InventoryDb += $script.Name
        $migrationState | ConvertTo-Json | Set-Content $migrationStatePath
    } else {
        Write-Host "Skipping InventoryDb script (already applied): $($script.Name)"
    }
}
