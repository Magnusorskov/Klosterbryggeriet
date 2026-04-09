run:
	docker compose up --build

watch:
	dotnet watch --project App/BlazorApp.csproj

test:
	dotnet test App.Tests/App.Tests.csproj