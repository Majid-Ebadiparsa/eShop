# Load migration state
$migrationStatePath = "${env:GITHUB_WORKSPACE}\deploy\migration_state.json"
if (!(Test-Path $migrationStatePath)) {
    $initialState = @{ OrderDb = @(); InventoryDb = @() }
    $initialState | ConvertTo-Json | Set-Content $migrationStatePath
}
$migrationState = Get-Content $migrationStatePath | ConvertFrom-Json

# Set network name and sql server container name
$dockerNetwork = "eshop_eshopnetwork"
$sqlServerContainer = "sqlserver-db"

# Process OrderDb
$orderDbScripts = Get-ChildItem -Path "${env:GITHUB_WORKSPACE}\deploy\sql\OrderDb" -Filter *.sql | Sort-Object Name
foreach ($script in $orderDbScripts) {
    if ($migrationState.OrderDb -notcontains $script.Name) {
        Write-Host "Executing OrderDb script: $($script.Name)"
        docker run --rm --network $dockerNetwork -v ${env:GITHUB_WORKSPACE}\deploy\sql\OrderDb:/sql mcr.microsoft.com/mssql-tools `
            /opt/mssql-tools/bin/sqlcmd -S $sqlServerContainer -U sa -P "$env:SQLSERVER_SA_PASSWORD" -d master -i "/sql/$($script.Name)"
        $migrationState.OrderDb += $script.Name
        $migrationState | ConvertTo-Json | Set-Content $migrationStatePath
    }
}

# Process InventoryDb
$inventoryDbScripts = Get-ChildItem -Path "${env:GITHUB_WORKSPACE}\deploy\sql\InventoryDb" -Filter *.sql | Sort-Object Name
foreach ($script in $inventoryDbScripts) {
    if ($migrationState.InventoryDb -notcontains $script.Name
