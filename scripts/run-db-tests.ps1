$ErrorActionPreference = 'Stop'
$Root = Split-Path $PSScriptRoot -Parent
Set-Location $Root
if (-not (Test-Path '.env')) { Copy-Item '.env.example' '.env' }

Write-Host 'Starting separate test database...'
docker compose --profile test up -d postgres-test

Write-Host 'Waiting for postgres-test health...'
$containerId = docker compose --profile test ps -q postgres-test
if (-not $containerId) { throw 'postgres-test container was not created' }
for ($i = 0; $i -lt 30; $i++) {
    $status = docker inspect --format='{{.State.Health.Status}}' $containerId 2>$null
    if ($status -eq 'healthy') { break }
    Start-Sleep -Seconds 2
}
if ($status -ne 'healthy') { throw 'postgres-test did not become healthy' }

$envValues = @{}
Get-Content '.env' | ForEach-Object {
    if ($_ -and -not $_.StartsWith('#') -and $_.Contains('=')) {
        $k, $v = $_.Split('=', 2)
        $envValues[$k] = $v
    }
}
$sql = Get-Content 'database/09_lab4_tests.sql' -Raw
$sql | docker compose --profile test exec -T postgres-test psql -v ON_ERROR_STOP=1 -U $envValues['POSTGRES_TEST_USER'] -d $envValues['POSTGRES_TEST_DB']
if ($LASTEXITCODE -ne 0) { throw 'Database tests failed' }
Write-Host 'DATABASE TESTS PASSED' -ForegroundColor Green
