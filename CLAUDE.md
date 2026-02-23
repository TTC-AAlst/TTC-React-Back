# Claude Code Instructions

## Project Overview

TTC Aalst backend - ASP.NET Core 8.0 Web API for a table tennis club management system.

## Build & Test Commands

```sh
# Build the solution
dotnet build Ttc.slnx

# Run tests
dotnet test Ttc.slnx

# Format code
dotnet format Ttc.slnx

# Check formatting without making changes
dotnet format Ttc.slnx --verify-no-changes
```

## Project Structure

- `src/Ttc.WebApi` - REST API controllers, authentication, SignalR hub
- `src/Ttc.DataAccess` - Services, EF Core DbContext, AutoMapper profiles
- `src/Ttc.DataEntities` - EF Core entity models
- `src/Ttc.Model` - DTOs and business models
- `src/Frenoy.Api` - External table tennis federation API integration
- `src/Ttc.UnitTests` - Unit tests

## Code Style

- Nullable reference types are enabled (`<Nullable>enable</Nullable>`)
- Use `= null!;` for EF Core navigation properties
- Use `string?` for nullable string properties in DTOs
- Pre-commit hook enforces formatting via `dotnet format`

## Git Hooks

Hooks are in the `hooks/` directory and enforced via `git config core.hooksPath hooks`:

- **pre-commit**: Format check + build
- **pre-push**: Build + tests
