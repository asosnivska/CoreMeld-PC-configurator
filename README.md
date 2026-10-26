# CoreMeld — PC Configurator

A desktop application (.NET / WPF) designed for custom PC building, component validation, and compatibility verification.

## Architecture

* **CoreMeld.Domain** — Core domain entities and database models.
* **CoreMeld.DAL** — Data Access Layer built with Pure ADO.NET and PostgreSQL via Npgsql.
* **CoreMeld.BLL** — Business logic layer, compatibility engine, and DTOs.
* **CoreMeld.UI.Desktop** — WPF presentation layer with Dependency Injection (`Microsoft.Extensions.DependencyInjection`).

---

## Setup & Getting Started

### 1. Prerequisites

* [.NET 10.0 SDK](https://dotnet.microsoft.com/download)
* [PostgreSQL](https://www.postgresql.org/download/) database instance running locally or remotely

### 2. Database Initialization

1. Create a PostgreSQL database (e.g., named `coremeld_db`).
2. Run the SQL scripts in your PostgreSQL client (such as **pgAdmin** or `psql`) in sequential order:
   * `src/CoreMeld.DAL/Scripts/Migrations/001_initial_schema.sql` (Schema DDL)
   * `src/CoreMeld.DAL/Scripts/Seeds/001_seed_components.sql` (Seed data)

### 3. Environment Configuration

1. Copy the example environment file in the project root:
   ```bash
   cp .env.example .env
