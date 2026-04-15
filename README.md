# Klosterbryggeriet
Fjerde semester projekt

## Tech stack
- .NET 10 (Blazor Server)
- Docker

## Project structure
```
Klosterbryggeriet/
├── App/                  # Blazor Server application
│   ├── Components/       # Razor components (Pages, Layout)
│   ├── Core/             # Core logic
│   ├── Models/           # Domain models / entities
│   ├── Repository/       # Data access layer
│   ├── Services/         # Business logic layer
│   └── wwwroot/          # Static files (CSS, images)
├── App.Tests/            # xUnit test project
├── compose.yaml
└── Makefile
```

## Getting started

### Prerequisites
- [.NET 10 SDK](https://dotnet.microsoft.com/download)
- [Docker](https://www.docker.com/products/docker-desktop)

### Run the application
```bash
make run
```
The app will be available at `http://localhost:5008`.

### Git hooks
After cloning, run this once to enable shared git hooks (e.g. conventional commit message validation):
```bash
make setup
```

## Make commands

| Command | Description |
|---|---|
| `make setup` | Configure git hooks for conventional commit validation |
| `make init` | First-time setup: start containers, wait for DB, apply migrations, and seed data |
| `make run` | Build and start the app with Docker Compose |
| `make watch` | Run the app locally with hot reload (requires .NET SDK) |
| `make test` | Run the xUnit test suite |
| `make migrate name=MyMigrationName` | Create a new EF Core migration |
| `make db-update` | Apply all pending migrations to the database |
| `make db-seed` | Seed the database with product data |