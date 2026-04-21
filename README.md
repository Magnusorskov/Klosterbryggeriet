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

### First-time setup
Run these commands once after cloning the repo:

```bash
make setup   # configure git hooks (conventional commits)
make init    # start containers, apply migrations, seed the database
```

`make init` will:
1. Build and start the `app` and `db` services via Docker Compose
2. Wait for MySQL to be reachable
3. Apply all EF Core migrations
4. Seed the database with product data from `App/Data/product_datafill.sql`

Once it finishes, the app is available at `http://localhost:5008` and the database is exposed on `localhost:3308`.

### Day-to-day development

You have two options depending on what you need:

**Option A — full Docker stack (`make run`)**
Runs both the app and the database in Docker. Best when you want a clean, production-like environment. No hot reload.
```bash
make run
```

**Option B — hot reload (`make watch`)** *(recommended for development)*
Starts only the database in Docker and runs the Blazor app locally with `dotnet watch`, so code changes reload automatically.
```bash
make watch
```
The app connects to the DB on `localhost:3308` using the connection string in `App/appsettings.json`.

### After pulling new changes
If a teammate added a migration, apply it before running the app:
```bash
make db-update
```

### Creating a migration
After changing an entity or `DbContext`:
```bash
make migrate name=DescribeYourChange
make db-update
```

### Running tests
```bash
make test
```

## Make commands

| Command | Description |
|---|---|
| `make setup` | Configure git hooks for conventional commit validation |
| `make init` | First-time setup: start containers, wait for DB, apply migrations, and seed data |
| `make build` | Build the full solution in Release mode (mirrors CI) |
| `make run` | Build and start the app with Docker Compose |
| `make watch` | Run the app locally with hot reload (requires .NET SDK) |
| `make test` | Run the xUnit test suite |
| `make migrate name=MyMigrationName` | Create a new EF Core migration |
| `make db-update` | Apply all pending migrations to the database |
| `make db-seed` | Seed the database with product data |