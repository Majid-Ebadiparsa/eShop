# Resolve SQL Server IP dynamically
$SqlServerContainerIp = docker inspect -f '{{range .NetworkSettings.Networks}}{{.IPAddress}}{{end}}' sqlserver-db

# Load migration state
$migrationStatePath = "${env:GITHUB_WORKSPACE}\deploy\migration_state.json"
if (!(Test-Path $migrationStatePath)) {
    $initialState = @{ OrderDb = @(); InventoryDb = @() }
    $initialState | ConvertTo-Json | Set-Content $migrationStatePath
}
$migrationState = Get-Content $migrationStatePath | ConvertFrom-Json

# Process OrderDb
$orderDbScripts = Get-ChildItem -Path "${env:GITHUB_WORKSPACE}\deploy\sql\OrderDb" -Filter *.sql | Sort-Object Name
foreach ($script in $orderDbScripts) {
    if ($migrationState.OrderDb -notcontains $script.Name) {
        Write-Host "Executing OrderDb script: $($script.Name)"
        docker cp "${env:GITHUB_WORKSPACE}\deploy\sql\OrderDb\$($script.Name)" sql-migrator:/sql/$($script.Name)
        docker exec sql-migrator /opt/mssql-tools/bin/sqlcmd -S sqlserver-db -U sa -P "$env:SQLSERVER_SA_PASSWORD" -d master -i "/sql/$($script.Name)"
        $migrationState.OrderDb += $script.Name
        $migrationState | ConvertTo-Json | Set-Content $migrationStatePath
    }
}

# Process InventoryDb
$inventoryDbScripts = Get-ChildItem -Path "${env:GITHUB_WORKSPACE}\deploy\sql\InventoryDb" -Filter *.sql | Sort-Object Name
foreach ($script in $inventoryDbScripts) {
    if ($migrationState.InventoryDb -notcontains $script.Name) {
        Write-Host "Executing InventoryDb script: $($script.Name)"
        docker cp "${env:GITHUB_WORKSPACE}\deploy\sql\InventoryDb\$($script.Name)" sql-migrator:/sql/$($script.Name)
        docker exec sql-migrator /opt/mssql-tools/bin/sqlcmd -S sqlserver-db -U sa -P "$env:SQLSERVER_SA_PASSWORD" -d master -i "/sql/$($script.Name)"
        $migrationState.InventoryDb += $script.Name
        $migrationState | ConvertTo-Json | Set-Content $migrationStatePath
    }
}
