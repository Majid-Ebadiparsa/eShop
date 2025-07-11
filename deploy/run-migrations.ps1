# Resolve SQL Server IP dynamically (fallback option)
$SqlServerContainerIp = docker inspect -f '{{range .NetworkSettings.Networks}}{{.IPAddress}}{{end}}' sqlserver-db

# Priority connection targets
$ConnectionTargets = @("sqlserver-db", "sqlserver-db.docker.internal", $SqlServerContainerIp) # Fully Fallback

# Find first available SQL Server connection
Write-Host "Probing SQL Server connection targets..."
$SqlServerHost = $null

Start-Sleep -Seconds 5
foreach ($target in $ConnectionTargets) {
    Write-Host "Trying SQL Server target: $target"
    $success = $false
    for ($i = 1; $i -le 5; $i++) {
        try {
            docker exec sql-migrator /opt/mssql-tools/bin/sqlcmd -S $target -U sa -P $env:SQLSERVER_SA_PASSWORD -Q "SELECT 1"
            Write-Host "Successfully connected to SQL Server at $target"
            $SqlServerHost = $target
            $success = $true
            break
        }
        catch {
            Write-Host "Attempt $i failed for $target. Retrying..."
            Start-Sleep -Seconds 5
        }
    }
    if ($success) { break }
}

if (!$SqlServerHost) {
    Write-Error "Unable to connect to SQL Server using any connection target!"
    exit 1
}

Write-Host "Starting SQL migrations using target: $SqlServerHost"

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
        docker exec sql-migrator /opt/mssql-tools/bin/sqlcmd -S $SqlServerHost -U sa -P $env:SQLSERVER_SA_PASSWORD -d master -i "/tmp/$scriptName"
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
        docker exec sql-migrator /opt/mssql-tools/bin/sqlcmd -S $SqlServerHost -U sa -P $env:SQLSERVER_SA_PASSWORD -d master -i "/tmp/$scriptName"
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
