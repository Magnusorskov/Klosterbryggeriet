setup:
	git config core.hooksPath .github/hooks
	dotnet tool install --global dotnet-ef || true

init:
	docker compose up --build -d
	@echo "Waiting for database..."
	@until docker compose exec -T db mysqladmin ping -uroot -prootpassword --silent 2>/dev/null; do sleep 2; done
	cd App && dotnet ef database update
	docker compose exec -T db mysql --default-character-set=utf8mb4 -uroot -prootpassword klosterbryggeriet < App/Data/product_datafill.sql

run:
	docker compose up --build

build:
	dotnet build Klosterbryggeriet.slnx --configuration Release

watch:
	dotnet watch --project App/BlazorApp.csproj

test:
	dotnet test App.Tests/App.Tests.csproj

# Create a new migration. Usage: make migrate name=MyMigrationName
migrate:
	cd App && dotnet ef migrations add $(name)

# Apply all pending migrations to the database
db-update:
	cd App && dotnet ef database update

# Seed the database with product data
db-seed:
	docker compose exec -T db mysql -uroot -prootpassword klosterbryggeriet < App/Data/product_datafill.sql