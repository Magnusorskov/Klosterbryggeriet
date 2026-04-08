run:
	docker compose up --build

test:
	dotnet test App.Tests/App.Tests.csproj