# Klosterbryggeriet
Fjerde semester projekt.

## Tech stack
- .NET 10 (Blazor Server)
- MySQL 8
- Docker

---

## User guide

For end users running the app on their own machine. The only thing you need installed is **Docker Desktop** — no .NET SDK, no manual database setup.

### 1. Install Docker Desktop
Download and install from <https://www.docker.com/products/docker-desktop/>, then start it. Wait until the Docker icon in your taskbar/menu bar shows that it's running.

### 2. Run the start script

**macOS / Linux**
```bash
./start.sh
```
If your shell complains that the file isn't executable (e.g. after downloading the project as a zip), use:
```bash
bash start.sh
```

**Windows**

Right-click `start.ps1` → **Run with PowerShell**.

Or, from a PowerShell terminal in the project folder:
```powershell
powershell -ExecutionPolicy Bypass -File start.ps1
```
If Windows says the script is blocked because it was downloaded from the internet, run `Unblock-File .\start.ps1` once and try again.

### What the script does
1. Checks that Docker is installed and running.
2. Builds and starts the application + MySQL containers.
3. Waits for the database, then waits for the app to come online.
4. Applies any pending database migrations automatically (handled inside the app on startup).
5. If the product catalog is empty, seeds it from `App/Data/product_datafill.sql`.
6. Opens <http://localhost:5008> in your default browser.

The first run takes a few minutes (Docker has to download images and build the app). Subsequent runs are much faster.

### Stopping the app
From the project folder:
```bash
docker compose down
```

### Resetting everything
To wipe the database and start fresh:
```bash
docker compose down -v
```
The next `start.sh` / `start.ps1` will rebuild and re-seed.

---

## Development guide

For working on the codebase.

### Prerequisites
- [.NET 10 SDK](https://dotnet.microsoft.com/download)
- [Docker Desktop](https://www.docker.com/products/docker-desktop/)

### First-time setup
```bash
make setup    # configure git hooks (conventional commits) and install dotnet-ef
./start.sh    # build containers, run migrations, seed the DB
```

After this, you can stop the containers (`docker compose down`) and switch to the hot-reload dev loop below.

### Day-to-day development
```bash
make watch
```
Starts MySQL in Docker and runs the Blazor app locally with `dotnet watch`, so code changes reload automatically. The app connects to MySQL on `localhost:3308` using the connection string in `App/appsettings.json`.

For a production-like local run (everything in Docker, no hot reload):
```bash
make run
```

### Project structure
```
Klosterbryggeriet/
├── App/                  # Blazor Server application
│   ├── Components/       # Razor components (Pages, Layout)
│   ├── Data/             # AppDbContext + seed SQL
│   ├── Migrations/       # EF Core migrations
│   ├── Models/           # Domain models / entities
│   ├── Services/         # Business logic layer
│   └── wwwroot/          # Static files (CSS, images)
├── App.Tests/            # xUnit test project (Testcontainers-based)
├── compose.yaml
├── Makefile
├── start.sh              # User-facing bootstrap (macOS/Linux)
└── start.ps1             # User-facing bootstrap (Windows)
```

### Migrations
Migrations are applied automatically when the app starts (`db.Database.Migrate()` in `App/Program.cs`). To create a new migration after changing an entity or `DbContext`:
```bash
make migrate name=DescribeYourChange
```
The next `dotnet watch` / container start will apply it. To apply migrations against the DB without starting the app:
```bash
cd App && dotnet ef database update
```

### Seeding
Seed data lives at `App/Data/product_datafill.sql`. The start scripts seed only when the `Products` table is empty. To force a re-seed (truncates + reloads):
```bash
make db-seed
```

### Tests
```bash
make test
```
The suite uses **Testcontainers.MySql** to spin up a real MySQL container per test fixture, so Docker must be running. Run a single test with:
```bash
dotnet test App.Tests/App.Tests.csproj --filter "FullyQualifiedName~ProductServiceIntegrationTest.MapProductsByCategory_GroupsProductsByCategory"
```

### Make commands

| Command | Description |
|---|---|
| `make setup` | Configure git hooks and install `dotnet-ef` |
| `make watch` | DB in Docker + `dotnet watch` locally (recommended dev loop) |
| `make run` | Full Docker stack, no hot reload |
| `make build` | Release build of the solution (mirrors CI) |
| `make test` | Run the xUnit suite |
| `make migrate name=MyMigrationName` | Create a new EF Core migration |
| `make db-seed` | Truncate + re-seed the `Products` table |

### Commit convention
`make setup` wires `.github/hooks/commit-msg` as the git hooks path. Commit headers must follow Conventional Commits:
```
type(scope)!: subject
```
where `type` ∈ `build, chore, ci, docs, feat, fix, perf, refactor, revert, style, test`. Merge and revert auto-messages are allowed through.