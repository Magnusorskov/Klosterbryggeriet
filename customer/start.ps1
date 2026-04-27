#Requires -Version 5.1
$ErrorActionPreference = 'Stop'

Set-Location -Path $PSScriptRoot

$AppUrl     = 'http://localhost:5008'
$DbName     = 'klosterbryggeriet'
$DbPassword = 'rootpassword'

if (Test-Path 'App/Data/product_datafill.sql') {
    $SeedFile = 'App/Data/product_datafill.sql'
} elseif (Test-Path 'product_datafill.sql') {
    $SeedFile = 'product_datafill.sql'
} else {
    $SeedFile = $null
}

function Write-Step($msg) { Write-Host "[start] $msg" -ForegroundColor Cyan }
function Write-Warn($msg) { Write-Host "[start] $msg" -ForegroundColor Yellow }
function Write-Fail($msg) { Write-Host "[start] $msg" -ForegroundColor Red; exit 1 }

if (-not (Get-Command docker -ErrorAction SilentlyContinue)) {
    Write-Fail 'Docker is not installed. Install Docker Desktop from https://www.docker.com/products/docker-desktop/ and try again.'
}

docker info *> $null
if ($LASTEXITCODE -ne 0) {
    Write-Fail 'Docker is installed but not running. Start Docker Desktop and try again.'
}

docker compose version *> $null
if ($LASTEXITCODE -ne 0) {
    Write-Fail "'docker compose' is unavailable. Update Docker Desktop to a recent version."
}

Write-Step 'Pulling latest images (skipped if offline)...'
docker compose pull *> $null
if ($LASTEXITCODE -ne 0) { Write-Warn 'Could not pull updates - using cached images.' }

Write-Step 'Starting containers (this may take a few minutes the first time)...'
docker compose up --build -d
if ($LASTEXITCODE -ne 0) { Write-Fail "'docker compose up' failed." }

Write-Step 'Waiting for the database to accept connections…'
$dbReady = $false
for ($i = 0; $i -lt 60; $i++) {
    docker compose exec -T db mysqladmin ping -uroot -p$DbPassword --silent *> $null
    if ($LASTEXITCODE -eq 0) { $dbReady = $true; break }
    Start-Sleep -Seconds 2
}
if (-not $dbReady) {
    Write-Fail "Database did not become ready in time. Run 'docker compose logs db' to investigate."
}

Write-Step 'Waiting for the app (migrations apply automatically on startup)…'
$appReady = $false
for ($i = 0; $i -lt 120; $i++) {
    try {
        $resp = Invoke-WebRequest -Uri $AppUrl -UseBasicParsing -TimeoutSec 5 -ErrorAction Stop
        if ($resp.StatusCode -ge 200 -and $resp.StatusCode -lt 500) { $appReady = $true; break }
    } catch {
        # connection refused / 5xx / timeout — keep polling
    }
    Start-Sleep -Seconds 2
}
if (-not $appReady) {
    Write-Fail "App did not respond on $AppUrl. Run 'docker compose logs app' to investigate."
}

Write-Step 'Checking whether the database needs to be seeded…'
$rawCount = docker compose exec -T db mysql -N -B -uroot -p$DbPassword $DbName -e 'SELECT COUNT(*) FROM Products;' 2>$null
$productCount = (($rawCount | Out-String) -replace '\s', '')

if (-not $productCount) {
    Write-Warn "Could not read the Products table — skipping seed. Check 'docker compose logs app'."
} elseif ($productCount -eq '0') {
    if (-not $SeedFile) {
        Write-Fail 'Seed file not found (looked for App/Data/product_datafill.sql and product_datafill.sql).'
    }
    Write-Step "Database is empty. Seeding from ${SeedFile}..."
    docker compose cp $SeedFile db:/tmp/seed.sql
    if ($LASTEXITCODE -ne 0) { Write-Fail 'Failed to copy seed file into the database container.' }
    docker compose exec -T db sh -c "mysql --default-character-set=utf8mb4 -uroot -p$DbPassword $DbName < /tmp/seed.sql"
    if ($LASTEXITCODE -ne 0) { Write-Fail 'Seeding failed.' }
    Write-Step 'Seed applied.'
} else {
    Write-Step "Database already has $productCount products — skipping seed."
}

Write-Step "Klosterbryggeriet is ready at $AppUrl"
Start-Process $AppUrl