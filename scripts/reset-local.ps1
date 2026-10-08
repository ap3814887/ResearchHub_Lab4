$ErrorActionPreference = 'Stop'
Set-Location (Split-Path $PSScriptRoot -Parent)
Write-Host 'Stopping containers and deleting the local PostgreSQL volume...'
docker compose down -v
docker compose up --build
