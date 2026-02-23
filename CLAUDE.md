# Claude Code Instructions

## Project Overview

TTC Aalst backend - ASP.NET Core 8.0 Web API for a table tennis club management system. Manages players, teams, matches, and integrates with the Belgian table tennis federation (Frenoy) API.

## Build & Test Commands

```sh
# Build the solution
dotnet build Ttc.slnx

# Run tests
dotnet test Ttc.slnx

# Run only integration tests
dotnet test Ttc.slnx --filter "FullyQualifiedName~Integration"

# Format code
dotnet format Ttc.slnx

# Check formatting without making changes
dotnet format Ttc.slnx --verify-no-changes
```

## Project Structure

```
src/
├── Ttc.WebApi/          # REST API layer
│   ├── Controllers/     # API endpoints
│   ├── Utilities/       # Auth, pipeline, helpers
│   └── Emailing/        # Email service
├── Ttc.DataAccess/      # Business logic layer
│   ├── Services/        # Business services (PlayerService, MatchService, etc.)
│   ├── Utilities/       # AutoMapper profiles, caching
│   └── TtcDbContext.cs  # EF Core context
├── Ttc.DataEntities/    # Database entities
│   └── Core/            # ITtcDbContext interface
├── Ttc.Model/           # DTOs and view models
│   └── Core/            # Settings, enums
├── Frenoy.Api/          # External federation API client
│   └── FrenoyMatchesApi.cs, FrenoyPlayersApi.cs
└── Ttc.UnitTests/       # Tests
    └── Integration/     # Integration tests with Testcontainers
```

## Architecture

### Request Flow
```
Controller → Service → DbContext → Database
     ↓           ↓
  DTOs      Entities
```

### Key Components

- **Controllers**: Thin, handle HTTP concerns only. Delegate to services.
- **Services**: Business logic lives here. Inject `ITtcDbContext` and other services.
- **DbContext**: `TtcDbContext` implements `ITtcDbContext`. Use interface for testing.
- **Entities**: EF Core models in `Ttc.DataEntities`. Suffixed with `Entity`.
- **DTOs**: View models in `Ttc.Model`. No suffix, named after domain concept.

### Dependency Injection

Services registered in `GlobalBackendConfiguration.Configure()`:
```csharp
services.AddScoped<ClubService>();
services.AddScoped<ConfigService>();
services.AddScoped<MatchService>();
services.AddScoped<TeamService>();
services.AddScoped<PlayerService>();
```

### Authentication

- JWT-based authentication configured in `AddAuthentication.cs`
- `IUserProvider` / `UserProvider` provides current user context
- Use `[Authorize]` attribute on controllers/actions requiring auth
- Player ID available via `_userProvider.PlayerId`

### Caching

- `CacheHelper` provides in-memory caching
- Config cached for 5 hours, players for 1 hour
- Invalidate via `ConfigService.ClearCache()`

### Real-time Updates

- SignalR hub at `/hubs/ttc` (`TtcHub.cs`)
- Broadcasts match updates to connected clients

## Code Style

- **Nullable reference types** enabled (`<Nullable>enable</Nullable>`)
- **File-scoped namespaces** preferred
- **EF navigation properties**: Use `= null!;` to satisfy nullability
- **DTOs**: Use `string?` for optional string properties
- **Async**: All I/O operations should be async
- **Formatting**: Enforced by `.editorconfig` and pre-commit hook

### Naming Conventions

- `*Entity` suffix for EF Core entities
- `*Service` suffix for business services
- `*Controller` suffix for API controllers
- `I*` prefix for interfaces
- `_camelCase` for private fields
- `PascalCase` for public members

## Testing Strategy

### Integration Tests

Located in `src/Ttc.UnitTests/Integration/`. Use Testcontainers for MySQL:

```csharp
public class MyControllerTests : IntegrationTestBase
{
    public MyControllerTests(TtcWebApplicationFactory factory) : base(factory) { }

    [Fact]
    public async Task Get_ReturnsExpectedData()
    {
        var result = await GetAsync<MyDto>("/api/myendpoint");
        Assert.NotNull(result);
    }
}
```

Key points:
- Tests share a MySQL container per test collection
- Database is migrated fresh for each test class
- Use `GetDbContext()` to seed test data
- `TESTCONTAINERS_RYUK_DISABLED=true` required on Windows

### Unit Tests

For service testing, mock `ITtcDbContext`:
```csharp
var mockContext = new Mock<ITtcDbContext>();
mockContext.Setup(x => x.Players).Returns(mockDbSet);
var service = new PlayerService(mockContext.Object, ...);
```

### Test Coverage Goals

- All controller endpoints should have integration tests
- Service methods with complex logic should have unit tests
- Cover error scenarios (not found, validation failures)

## Error Handling

### Controller Level

Controllers should return appropriate HTTP status codes:
```csharp
[HttpGet("{id}")]
public async Task<ActionResult<PlayerDto>> Get(int id)
{
    var player = await _service.GetPlayer(id);
    if (player == null)
        return NotFound();
    return Ok(player);
}
```

### Service Level

- Use `SingleOrDefault()` instead of `Single()` to avoid exceptions
- Return `null` for not-found cases (let controller decide HTTP status)
- Throw `InvalidOperationException` for business rule violations

### Global Exception Handler

`GlobalExceptionHandler` catches unhandled exceptions and returns ProblemDetails. Avoid exposing internal details in production.

## Security Guidelines

### Secrets Management

- **Never commit secrets** to the repository
- Use `appsettings.{Environment}.json` for environment-specific config
- Production secrets should come from environment variables or secret managers
- `appsettings.Testing.json` contains test-only values

### Input Validation

- Validate all user input in controllers
- Use data annotations on DTOs: `[Required]`, `[StringLength]`, `[Range]`
- Check `ModelState.IsValid` before processing

### Authentication

- Protect endpoints with `[Authorize]` attribute
- Use `[Authorize(Roles = "Board")]` for admin-only endpoints
- Never trust client-provided user IDs; use `_userProvider.PlayerId`

### File Uploads

- Validate file types and sizes
- Store uploads outside web root or use blob storage
- Current implementation in `UploadController` stores to `PublicImageFolder`

## Database Conventions

### Entities

```csharp
public class PlayerEntity
{
    public int Id { get; set; }
    public string Name { get; set; } = null!;  // Required
    public string? Email { get; set; }          // Nullable

    // Navigation properties
    public ClubEntity Club { get; set; } = null!;
    public ICollection<MatchPlayerEntity> Matches { get; set; } = [];
}
```

### Queries

- Use `Include()` for eager loading related data
- Use `AsSplitQuery()` for complex includes to avoid cartesian explosion
- Use `AsNoTracking()` for read-only queries

### Migrations

```sh
# Add a migration
dotnet ef migrations add MigrationName -p src/Ttc.DataAccess -s src/Ttc.WebApi

# Apply migrations
dotnet ef database update -p src/Ttc.DataAccess -s src/Ttc.WebApi
```

## External APIs

### Frenoy API (Belgian Table Tennis Federation)

- `FrenoyPlayersApi`: Syncs player rankings and info
- `FrenoyMatchesApi`: Syncs match results and schedules
- `FrenoyTeamsApi`: Syncs team compositions
- `FrenoySyncJob`: Background job that syncs periodically

Configuration:
- `TtcSettings.StartSyncJob`: Enable/disable background sync
- Sync runs every 6 hours when enabled

## Common Tasks

### Adding a New Endpoint

1. Add DTO to `Ttc.Model` if needed
2. Add service method to appropriate service in `Ttc.DataAccess`
3. Add controller action in `Ttc.WebApi/Controllers`
4. Add integration test in `Ttc.UnitTests/Integration`

### Adding a New Entity

1. Create entity class in `Ttc.DataEntities`
2. Add `DbSet<T>` to `TtcDbContext` and `ITtcDbContext`
3. Create and apply migration
4. Add AutoMapper profile if mapping to DTO

### Modifying Configuration

1. Add property to `TtcSettings` in `Ttc.Model/Core`
2. Add to `appsettings.json` with default value
3. Add to `appsettings.Testing.json` with test value

## Known Issues & TODOs

Check these files for known issues marked with TODO comments:
- `MatchService.cs`: Sync timing bugs, performance issues
- `FrenoyMatchesApi.cs`: Derby match handling
- `FrenoyApiBase.cs`: Pre-season data handling

## Git Hooks

Hooks are in the `hooks/` directory and enforced via `git config core.hooksPath hooks`:

- **pre-commit**: Format check + build
- **pre-push**: Build + tests

## CI/CD

GitHub Actions workflow in `.github/workflows/ci.yml`:
- Runs on push/PR to master
- Steps: restore → format check → build → test
- Uses `ubuntu-latest` with .NET 8.0
