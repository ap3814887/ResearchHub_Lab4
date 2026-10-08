$ErrorActionPreference = 'Stop'
$Root = Split-Path $PSScriptRoot -Parent
Set-Location $Root

Write-Host '1/3 PostgreSQL tests'
& "$PSScriptRoot/run-db-tests.ps1"

Write-Host '2/3 Backend xUnit + Testcontainers'
dotnet test ResearchHub.sln --configuration Release
if ($LASTEXITCODE -ne 0) { throw 'Backend tests failed' }

Write-Host '3/3 Frontend Vitest + build'
Push-Location frontend
try {
    npm install --no-audit --no-fund
    npm test
    if ($LASTEXITCODE -ne 0) { throw 'Frontend tests failed' }
    npm run build
    if ($LASTEXITCODE -ne 0) { throw 'Frontend build failed' }
}
finally { Pop-Location }

Write-Host 'ALL LAB4 TESTS PASSED' -ForegroundColor Green
