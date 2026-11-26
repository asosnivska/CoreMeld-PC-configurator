# Clean install check (#81)

Checked on 2026-11-26: a fresh clone into an empty folder, a new empty
PostgreSQL 17 database, .NET SDK 10.0.402, Windows 10.

## Steps

1. `git clone` the repository into an empty folder.
2. Create an empty database.
3. Run all migrations, then all seeds, each sorted by file name (see README, step 2).
4. Copy `src/.env.example` to `src/.env`, fill in the connection string, set `USE_FAKE_DATA=false`.
5. `dotnet build CoreMeld.PcConfigurator.sln`
6. `dotnet test CoreMeld.PcConfigurator.sln`
7. `dotnet run --project src/CoreMeld.UI.Desktop`

## Result

| Check | Result |
|-------|--------|
| All 8 SQL scripts run on an empty DB with `ON_ERROR_STOP=1` | OK |
| Scripts run a second time on the same DB | OK, no duplicates |
| Build from the fresh clone | OK, 0 errors |
| Tests | OK, 259 passed |
| `GenerateBuildAsync` on the fresh DB (HomeFullHd 35k, GamingFullHd 45k, Gaming2K 80k) | OK, compatible builds |
| `GenerateBuildAsync` Office 25k | Rejected as expected: minimum is 26 155 UAH |
| `src/.env` after setup | Ignored by git (`*.env` in `.gitignore`) |
| Secrets in tracked files and in git history | None: only `.env.example` with placeholders |

## Problems found and fixed

1. **Migrations 002_add_case_board_limits and 003_add_power_connectors failed in `psql`.**
   They were saved as UTF-8 with BOM. With the default Windows console encoding
   (WIN1251 / 866) `psql` fails with `syntax error at or near "BEGIN"`.
   pgAdmin hides the problem. Fixed: the BOM was removed, the SQL did not change.
2. **README listed only 2 of the 8 scripts** and was cut off in the middle of the
   `.env` step. Fixed: full ordered list, `.env` location (`src/`), build, test, run.
3. **Seed 002 reused id 5** of the Corsair CX650 PSU from seed 001 for a motherboard,
   so one component was both a PSU and a motherboard. Fixed by `Seeds/003_fix_seed_integrity.sql` (#61).
4. **On the real DB the engine always failed with "could not pick a case"**:
   repositories did not read the columns from migrations 002/003 and seed 002
   left them empty. Fixed by the DAL change and `Seeds/004_fill_power_and_board_limits.sql` (#67).

## Notes

* There are two migrations with number 002. They are independent, so their order
  does not matter, but new migrations should use the next free number (004).
* Do not edit scripts that are already merged: add a new migration or seed,
  so that everyone can update an existing database without dropping it.
