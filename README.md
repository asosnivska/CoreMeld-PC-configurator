# CoreMeld – PC Configurator

A desktop application (.NET / WPF) designed for custom PC building, component validation, and compatibility verification.

## Architecture

* **CoreMeld.Domain** – Core domain entities and database models.
* **CoreMeld.DAL** – Data Access Layer built with Pure ADO.NET and PostgreSQL via Npgsql.
* **CoreMeld.BLL** – Business logic layer, compatibility engine, and DTOs.
* **CoreMeld.UI.Desktop** – WPF presentation layer with Dependency Injection (`Microsoft.Extensions.DependencyInjection`).

---

## Setup & Getting Started

### 1. Prerequisites

* [.NET 10.0 SDK](https://dotnet.microsoft.com/download)
* [PostgreSQL](https://www.postgresql.org/download/) 15+ running locally or remotely (checked on 17)
* Windows (the UI is WPF)

### 2. Database Initialization

1. Create a database, e.g. `coremeld_db`.
2. Run **all** scripts from `src/CoreMeld.DAL/Scripts` in this order: first every migration, then every seed, each sorted by file name.

   | # | Script | What it does |
   |---|--------|--------------|
   | 1 | `Migrations/001_initial_schema.sql` | Tables |
   | 2 | `Migrations/002_add_case_board_limits.sql` | `cases.max_board_form_factor_id` |
   | 3 | `Migrations/002_add_has_wifi_to_motherboards.sql` | `motherboards.has_wifi` |
   | 4 | `Migrations/003_add_power_connectors.sql` | GPU / PSU power connector columns |
   | 5 | `Seeds/001_seed_components.sql` | Reference data and first parts |
   | 6 | `Seeds/002_seed_components.sql` | Main parts catalog |
   | 7 | `Seeds/003_fix_seed_integrity.sql` | Fixes the id 5 conflict, cooler sockets, AM4 CPU |
   | 8 | `Seeds/004_fill_power_and_board_limits.sql` | Power connectors and case board limits |

   Every script is safe to run again, so on an existing database just run the ones you do not have yet.
   Fix scripts are always added as new files: an existing database never has to be dropped.

   With `psql` (PowerShell, from the repository root):
   ```powershell
   $scripts = @(Get-ChildItem src/CoreMeld.DAL/Scripts/Migrations/*.sql | Sort-Object Name) +
              @(Get-ChildItem src/CoreMeld.DAL/Scripts/Seeds/*.sql | Sort-Object Name)
   foreach ($f in $scripts) { psql -h localhost -U postgres -d coremeld_db -v ON_ERROR_STOP=1 -f $f.FullName }
   ```
   In pgAdmin open each file in the Query Tool and run it in the same order.

### 3. Environment Configuration

1. Copy `src/.env.example` to `src/.env`:
   ```powershell
   Copy-Item src/.env.example src/.env
   ```
2. In `src/.env` set your user, password and database, and set `USE_FAKE_DATA=false` to use PostgreSQL.
   With `USE_FAKE_DATA=true` (or the variable missing) the app runs on the built-in in-memory catalog without a database.
3. `.env` is in `.gitignore`. Never commit it.

The app looks for `.env` starting from its own folder and going up, so it works with `dotnet run`, F5 in Visual Studio, or any working directory.

### 4. Build, Test, Run

```powershell
dotnet build CoreMeld.PcConfigurator.sln
dotnet test CoreMeld.PcConfigurator.sln
dotnet run --project src/CoreMeld.UI.Desktop
```

On start with `USE_FAKE_DATA=false` the app shows a message with the number of parts read from the database.

See [docs/clean-install.md](docs/clean-install.md) for the clean install checklist and known problems.
