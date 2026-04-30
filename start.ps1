#Requires -Version 5.1
Set-Location -Path $PSScriptRoot

$AppUrl      = 'http://localhost:5008'
$DbName      = 'klosterbryggeriet'
$DbPassword  = 'rootpassword'
$ContainerDb = 'klosterbryggeriet-db-1'

# 1. Locate the SQL seed file
$SeedFile = Join-Path $PSScriptRoot "App/Data/product_datafill.sql"
if (-not (Test-Path $SeedFile)) { $SeedFile = Join-Path $PSScriptRoot "product_datafill.sql" }

Write-Host "1. STARTING CONTAINERS..." -ForegroundColor Cyan
docker compose up -d

Write-Host "2. APP RUNNING MIGRATIONS (Waiting for 200 OK)..." -ForegroundColor Cyan
$ready = $false
while (-not $ready) {
    try {
        $response = Invoke-WebRequest -Uri $AppUrl -UseBasicParsing -TimeoutSec 2 -ErrorAction Stop
        if ($response.StatusCode -eq 200) { $ready = $true }
    } catch {
        Write-Host "." -NoNewline
        Start-Sleep -Seconds 2
    }
}
Write-Host "`nApp is ready. Migrations complete." -ForegroundColor Green

Write-Host "3. DATA SEED RUN (Internal Docker)..." -ForegroundColor Cyan

# Define the base command for reuse
# We use --protocol=tcp to bypass the 'localhost' socket issue that causes 1045 errors
$BaseMysql = "mysql -h 127.0.0.1 -u root -p$DbPassword --protocol=tcp $DbName"

# Get count
$countStr = docker exec -i $ContainerDb sh -c "$BaseMysql -N -s -e 'SELECT COUNT(*) FROM Products;'" 2>$null
$count = if ($countStr) { [int]$countStr.Trim() } else { 0 }

if ($count -eq 0) {
    Write-Host "Seeding data..." -ForegroundColor Yellow
    if (Test-Path $SeedFile) {
        # Using sh -c inside the container ensures the pipe is handled correctly
        Get-Content $SeedFile -Raw | docker exec -i $ContainerDb sh -c "$BaseMysql"
        
        # Verify
        $finalCount = docker exec -i $ContainerDb sh -c "$BaseMysql -N -s -e 'SELECT COUNT(*) FROM Products;'"
        if ($finalCount -gt 0) {
            Write-Host "SUCCESS: Data injected. Current row count: $finalCount" -ForegroundColor Green
        } else {
            Write-Host "ERROR: Command ran but row count is still 0." -ForegroundColor Red
        }
    } else {
        Write-Host "ERROR: Seed file not found at $SeedFile" -ForegroundColor Red
    }
} else {
    Write-Host "SKIP: Table already has $count rows." -ForegroundColor Green
}

Start-Process $AppUrl