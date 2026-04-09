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
git config core.hooksPath .github/hooks
```

### Run tests
```bash
make test
```