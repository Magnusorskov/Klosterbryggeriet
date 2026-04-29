# Klosterbryggeriet — Local install

This folder contains everything you need to run the Klosterbryggeriet app on your own computer.

## Requirements

- **Docker Desktop** — install from <https://www.docker.com/products/docker-desktop/>.
  Open it after installing and wait until the icon shows that Docker is running.

That's it. You don't need to install .NET, MySQL, or anything else.

## Run the app

### macOS / Linux
Open a terminal in this folder and run:
```bash
./start.sh
```
If you get a "permission denied" error (e.g. after extracting from a zip), use:
```bash
bash start.sh
```

### Windows
Right-click `start.ps1` → **Run with PowerShell**.

If Windows blocks the script with a security warning, open PowerShell in this folder and run:
```powershell
Unblock-File .\start.ps1
.\start.ps1
```

## What the script does

1. Verifies Docker is installed and running.
2. Pulls the latest application image (skipped if you're offline — uses what you already have).
3. Starts the application and database in containers.
4. Waits for the app to be ready.
5. Seeds the product catalog the first time you run it.
6. Opens <http://localhost:5008> in your default browser.

The first run downloads about 250 MB of images and takes ~30 seconds.
Later runs are nearly instant.

## Stopping the app

In a terminal in this folder:
```bash
docker compose down
```

## Resetting the database

To wipe all data and start fresh:
```bash
docker compose down -v
```
The next `start.sh` / `start.ps1` will re-seed the product catalog.