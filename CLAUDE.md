# TTC Aalst Backend

## Project Overview

TTC Aalst backend - ASP.NET Core 8.0 Web API for table tennis club management. Integrates with Belgian table tennis federation (Frenoy) API.

## Commands

```sh
dotnet build Ttc.slnx           # Build
dotnet test Ttc.slnx            # Test
dotnet format Ttc.slnx          # Format
```

## Project Structure

- `Ttc.DataEntities`: EF Core entities (suffixed with `Entity`)
- `Ttc.Model`: DTOs and view models
- `Ttc.DataAccess`: Services and DbContext
- `Ttc.WebApi`: Controllers and API configuration

## Testing

Integration tests use Testcontainers for MySQL. Extend `IntegrationTestBase`.

On Windows: set `TESTCONTAINERS_RYUK_DISABLED=true`

## Migrations

```sh
dotnet ef migrations add Name -p src/Ttc.DataAccess -s src/Ttc.WebApi
dotnet ef database update -p src/Ttc.DataAccess -s src/Ttc.WebApi
```

## Frenoy API

External API for Belgian Table Tennis Federation data sync:
- `FrenoyPlayersApi`, `FrenoyMatchesApi`, `FrenoyTeamsApi`
- `FrenoySyncJob`: Background sync (controlled by `TtcSettings.StartSyncJob`)

## Git Hooks

Located in `hooks/` directory. Pre-commit runs format check + build.
