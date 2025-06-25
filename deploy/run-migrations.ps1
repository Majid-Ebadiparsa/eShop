# Resolve SQL Server IP dynamically
$SqlServerContainerIp = docker inspect -f '{{range .NetworkSettings.Networks}}{{.IPAddress}}{{end}}' sqlserver-db

# Wait for SQL Server readiness before running any migrations
Write-Host "Waiting for SQL Server readiness..."
for ($i = 1; $i -le 30; $i++) {
    try {
        docker exec sql-migrator /opt/mssql-tools/bin/sqlcmd -S $SqlServerContainerIp -U sa -P $env:SQLSERVER_SA_PASSWORD -Q "SELECT 1"
        Write-Host "SQL Server is ready."
        break
    }
    catch {
        Write-Host "SQL Server not ready yet. Retrying in 2 seconds..."
        Start-Sleep -Seconds 2
    }
}
Write-Host "Starting SQL migrations..."

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
        if ($LASTEXITCODE -eq 0) {
            Write-Host "Successfully applied $scriptName"
            $migrationState.OrderDb += $scriptName
            $migrationState | ConvertTo-Json -Depth 3 | Set-Content $migrationStatePath
        }
        else {
            Write-Error "Failed to apply $scriptName for OrderDb"
            exit 1
        }
    }
}

# Apply InventoryDb scripts
Get-ChildItem -Path "deploy/sql/InventoryDb" -Filter *.sql | Sort-Object Name | ForEach-Object {
    $scriptName = $_.Name
    if ($migrationState.InventoryDb -notcontains $scriptName) {
        Write-Host "Applying $scriptName for InventoryDb..."
        docker cp "deploy/sql/InventoryDb/$scriptName" sql-migrator:/tmp/$scriptName
        docker exec sql-migrator /opt/mssql-tools/bin/sqlcmd -S $SqlServerContainerIp -U sa -P $env:SQLSERVER_SA_PASSWORD -d master -i "/tmp/$scriptName"
        if ($LASTEXITCODE -eq 0) {
            Write-Host "Successfully applied $scriptName"
            $migrationState.InventoryDb += $scriptName
            $migrationState | ConvertTo-Json -Depth 3 | Set-Content $migrationStatePath
        }
        else {
            Write-Error "Failed to apply $scriptName for InventoryDb"
            exit 1
        }
    }
}
