param([string]$Type = 'sample')

$DbUser = 'sa'
$DbPassword = 'Hexagon123'
$DbName = 'HexagonalLab'

Write-Host "`n========================================" -ForegroundColor Cyan
Write-Host "  Insert Test Data - HexagonalLab" -ForegroundColor Yellow
Write-Host "========================================`n" -ForegroundColor Cyan

Write-Host "Validating SQL Server connection..." -ForegroundColor Yellow

$TestCmd = docker exec hexagonal-sqlserver /opt/mssql-tools18/bin/sqlcmd -S localhost -U $DbUser -P $DbPassword -C -Q 'SELECT 1' 2>&1

if ($LASTEXITCODE -ne 0) {
    Write-Host "`nCannot connect to SQL Server" -ForegroundColor Red
    Write-Host "Make sure containers are running`n" -ForegroundColor Yellow
    exit 1
}

Write-Host "Connected to SQL Server`n" -ForegroundColor Green

if ($Type -eq 'sample') {
    Write-Host "Inserting 5 sample items..." -ForegroundColor Yellow
    
    $Sql = 'INSERT INTO Items (Id, Name, Status, CreatedAt) VALUES'
    $Sql += ' (''ITEM-001'', ''Pedido 001'', ''Pending'', GETUTCDATE()),'
    $Sql += ' (''ITEM-002'', ''Pedido 002'', ''Pending'', GETUTCDATE()),'
    $Sql += ' (''ITEM-003'', ''Pedido 003'', ''Pending'', GETUTCDATE()),'
    $Sql += ' (''ITEM-004'', ''Pedido 004'', ''Pending'', GETUTCDATE()),'
    $Sql += ' (''ITEM-005'', ''Pedido 005'', ''Pending'', GETUTCDATE())'
    
    docker exec hexagonal-sqlserver /opt/mssql-tools18/bin/sqlcmd `
        -S localhost -U $DbUser -P $DbPassword -d $DbName -C -Q $Sql
    
    if ($LASTEXITCODE -eq 0) {
        Write-Host "`nData inserted successfully!`n" -ForegroundColor Green
    } else {
        Write-Host "`nFailed to insert data`n" -ForegroundColor Red
        exit 1
    }
}

Write-Host "Current items in database:" -ForegroundColor Cyan

$SelectSql = 'SELECT Id, Name, Status, CreatedAt FROM Items ORDER BY CreatedAt DESC'

docker exec hexagonal-sqlserver /opt/mssql-tools18/bin/sqlcmd `
    -S localhost -U $DbUser -P $DbPassword -d $DbName -C -Q $SelectSql

Write-Host "`n========================================`n" -ForegroundColor Cyan
