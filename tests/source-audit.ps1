$ErrorActionPreference = 'Stop'
$Root = Split-Path (Split-Path $PSScriptRoot -Parent) -Parent
if (-not (Test-Path (Join-Path $Root 'docker-compose.yml'))) { $Root = Split-Path $PSScriptRoot -Parent }

$required = @(
  'docker-compose.yml', '.env.example', 'README.md', 'ResearchHub.sln',
  'backend/ResearchHub.Api/Dockerfile', 'backend/ResearchHub.Tests/ResearchHub.Tests.csproj',
  'frontend/Dockerfile', 'frontend/nginx.conf', 'frontend/vitest.config.ts',
  'database/09_lab4_tests.sql', 'docs/test-checklist.md', 'docs/requirements-matrix.md',
  '.github/workflows/ci.yml'
)
foreach ($item in $required) {
  $path = Join-Path $Root $item
  if (-not (Test-Path $path)) { throw "Missing required file: $item" }
}

$compose = Get-Content (Join-Path $Root 'docker-compose.yml') -Raw
foreach ($token in @('frontend:', 'backend:', 'postgres:', 'postgres-test:', 'healthcheck:', 'docker-entrypoint-initdb.d')) {
  if (-not $compose.Contains($token)) { throw "Compose requirement missing: $token" }
}

$tests = Get-Content (Join-Path $Root 'backend/ResearchHub.Tests/ApiIntegrationTests.cs') -Raw
$factCount = ([regex]::Matches($tests, '\[Fact\]')).Count
if ($factCount -lt 7) { throw "Only $factCount xUnit tests found" }

$checklist = Get-Content (Join-Path $Root 'docs/test-checklist.md') -Raw
$checkCount = ([regex]::Matches($checklist, '^\|\s*\d+\s*\|', 'Multiline')).Count
if ($checkCount -lt 20) { throw "Only $checkCount checklist rows found" }

Write-Host "SOURCE AUDIT PASSED: $factCount xUnit tests, $checkCount checklist checks, required Lab4 files found." -ForegroundColor Green
