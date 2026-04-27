setup:
	git config core.hooksPath .github/hooks
	dotnet tool install --global dotnet-ef || true

run:
	docker compose up --build

build:
	dotnet build Klosterbryggeriet.slnx --configuration Release

watch:
	docker compose up -d db
	@echo "Waiting for database..."
	@until docker compose exec -T db mysqladmin ping -uroot -prootpassword --silent 2>/dev/null; do sleep 2; done
	dotnet watch --project App/BlazorApp.csproj

test:
	dotnet test App.Tests/App.Tests.csproj

# Create a new migration. Usage: make migrate name=MyMigrationName
migrate:
	cd App && dotnet ef migrations add $(name)

# Re-seed the database with product data (truncates Products first)
db-seed:
	docker compose exec -T db mysql -uroot -prootpassword klosterbryggeriet < App/Data/product_datafill.sql