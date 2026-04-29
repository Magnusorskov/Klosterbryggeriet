#!/usr/bin/env bash
set -euo pipefail

cd "$(dirname "$0")"

APP_URL="http://localhost:5008"
DB_NAME="klosterbryggeriet"
DB_PASSWORD="rootpassword"

if [ -f "App/Data/product_datafill.sql" ]; then
  SEED_FILE="App/Data/product_datafill.sql"
elif [ -f "product_datafill.sql" ]; then
  SEED_FILE="product_datafill.sql"
else
  SEED_FILE=""
fi

log()  { printf '\033[1;34m[start]\033[0m %s\n' "$*"; }
warn() { printf '\033[1;33m[start]\033[0m %s\n' "$*"; }
fail() { printf '\033[1;31m[start]\033[0m %s\n' "$*" >&2; exit 1; }

if ! command -v docker >/dev/null 2>&1; then
  fail "Docker is not installed. Install Docker Desktop from https://www.docker.com/products/docker-desktop/ and try again."
fi

if ! docker info >/dev/null 2>&1; then
  fail "Docker is installed but not running. Start Docker Desktop and try again."
fi

if ! docker compose version >/dev/null 2>&1; then
  fail "'docker compose' is unavailable. Update Docker Desktop to a recent version."
fi

log "Pulling latest images (skipped if offline)..."
docker compose pull >/dev/null 2>&1 || warn "Could not pull updates — using cached images."

log "Starting containers (this may take a few minutes the first time)..."
docker compose up --build -d

log "Waiting for the database to accept connections…"
for _ in $(seq 1 60); do
  if docker compose exec -T db mysqladmin ping -uroot -p"$DB_PASSWORD" --silent >/dev/null 2>&1; then
    break
  fi
  sleep 2
done
if ! docker compose exec -T db mysqladmin ping -uroot -p"$DB_PASSWORD" --silent >/dev/null 2>&1; then
  fail "Database did not become ready in time. Run 'docker compose logs db' to investigate."
fi

log "Waiting for the app (migrations apply automatically on startup)…"
ready=0
for _ in $(seq 1 120); do
  status=$(curl -s -o /dev/null -w '%{http_code}' "$APP_URL" 2>/dev/null) || status=""
  case "$status" in
    2*|3*|4*) ready=1; break ;;
  esac
  sleep 2
done
if [ "$ready" -ne 1 ]; then
  fail "App did not respond on $APP_URL (last status: ${status:-no-response}). Run 'docker compose logs app' to investigate."
fi

log "Checking whether the database needs to be seeded…"
product_count=$(docker compose exec -T db mysql -N -B -uroot -p"$DB_PASSWORD" "$DB_NAME" \
  -e "SELECT COUNT(*) FROM Products;" 2>/dev/null | tr -d '[:space:]' || true)

if [ -z "$product_count" ]; then
  warn "Could not read the Products table — skipping seed. Check 'docker compose logs app'."
elif [ "$product_count" = "0" ]; then
  if [ -z "$SEED_FILE" ]; then
    fail "Seed file not found (looked for App/Data/product_datafill.sql and product_datafill.sql)."
  fi
  log "Database is empty. Seeding from ${SEED_FILE}..."
  docker compose cp "$SEED_FILE" db:/tmp/seed.sql
  docker compose exec -T db sh -c "mysql --default-character-set=utf8mb4 -uroot -p$DB_PASSWORD $DB_NAME < /tmp/seed.sql"
  log "Seed applied."
else
  log "Database already has $product_count products — skipping seed."
fi

log "Klosterbryggeriet is ready at $APP_URL"

if command -v open >/dev/null 2>&1; then
  open "$APP_URL"
elif command -v xdg-open >/dev/null 2>&1; then
  xdg-open "$APP_URL" >/dev/null 2>&1 &
else
  warn "Could not auto-open a browser. Visit $APP_URL manually."
fi