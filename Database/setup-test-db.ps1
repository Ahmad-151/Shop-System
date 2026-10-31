$ErrorActionPreference = "Stop"

$password = $env:MSSQL_SA_PASSWORD
if (-not $password) { throw "Set MSSQL_SA_PASSWORD first (same value as in .env)." }

$testDb = "ShopDB_Test"
$container = (docker compose ps -q db)
if (-not $container) { throw "The db container is not running. Run: docker compose up -d db" }

$sqlcmd = "/opt/mssql-tools18/bin/sqlcmd"

$script = (Get-Content "$PSScriptRoot\ShopDB.Docker.sql" -Raw -Encoding UTF8) -replace "ShopDB", $testDb
$tmp = Join-Path $env:TEMP "ShopDB_Test.sql"
Set-Content -Path $tmp -Value $script -Encoding UTF8

$exists = docker exec $container $sqlcmd -S localhost -U sa -P $password -No -h -1 -W -Q "SET NOCOUNT ON; SELECT DB_ID(N'$testDb')"
if ($exists -match "\d+") {
    Write-Host "$testDb already exists. Skipping schema creation."
} else {
    docker cp $tmp "${container}:/tmp/ShopDB_Test.sql"
    docker exec $container $sqlcmd -S localhost -U sa -P $password -No -b -i /tmp/ShopDB_Test.sql
}

docker cp "$PSScriptRoot\test-seed.sql" "${container}:/tmp/test-seed.sql"
docker exec $container $sqlcmd -S localhost -U sa -P $password -No -b -d $testDb -i /tmp/test-seed.sql

$env:SHOP_SQL_CONNECTION = "Server=127.0.0.1,1433;Database=$testDb;User Id=sa;Password=$password;TrustServerCertificate=True;Encrypt=False"
Write-Host "Ready. Now run: dotnet test"