run:
	docker compose up --build

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