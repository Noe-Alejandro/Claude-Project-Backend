# Claude Project Backend

REST API for **Claude Project Web** — built with ASP.NET Core 9, Clean Architecture, and Entity Framework Core.

## Tech stack

| Concern | Choice |
|---|---|
| Framework | ASP.NET Core 9 (controllers) |
| Architecture | Clean Architecture (4 layers) |
| ORM + Migrations | Entity Framework Core + SQL Server |
| Authentication | JWT Bearer |
| Password hashing | BCrypt |
| Docs | Swagger / OpenAPI |
| Logging | Serilog |
| Tests | xUnit + NSubstitute + FluentAssertions |

## Architecture

```
src/
  ClaudeProjectBackend.Domain/          # Entities, enums, repository interfaces — zero dependencies
  ClaudeProjectBackend.Application/     # Services, DTOs, business logic
  ClaudeProjectBackend.Infrastructure/  # EF Core, repositories, JWT, BCrypt
  ClaudeProjectBackend.Api/             # Controllers, middleware, Program.cs
tests/
  ClaudeProjectBackend.Tests/           # Unit tests (services only)
scripts/
  seed-data.sql                         # Dev seed data
  reset-db.ps1                          # Drop + migrate + seed
```

**Dependency rule:** `Domain` ← `Application` ← `Infrastructure` ← `Api`

## Prerequisites

- [.NET 9 SDK](https://dotnet.microsoft.com/download/dotnet/9)
- SQL Server (local or Docker)
- EF Core CLI: `dotnet tool install --global dotnet-ef`

## Getting started

### 1. Clone and restore

```bash
git clone <repo-url>
cd Claude-Project-Backend
dotnet restore
```

### 2. Configure

Copy the example settings and fill in your values:

```bash
cp src/ClaudeProjectBackend.Api/appsettings.json src/ClaudeProjectBackend.Api/appsettings.Local.json
```

Edit `appsettings.Local.json`:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=localhost;Database=ClaudeProjectBackend;Trusted_Connection=True;TrustServerCertificate=True;"
  },
  "JwtSettings": {
    "Secret": "<your-secret-min-32-chars>",
    "Issuer": "ClaudeProjectBackend",
    "Audience": "ClaudeProjectWeb",
    "ExpiryMinutes": 60
  },
  "Cors": {
    "AllowedOrigins": "http://localhost:5173"
  }
}
```

> `appsettings.Local.json` is git-ignored — never commit secrets.

### 3. Run migrations

```bash
dotnet ef migrations add InitialCreate \
  -p src/ClaudeProjectBackend.Infrastructure \
  -s src/ClaudeProjectBackend.Api

dotnet ef database update \
  -p src/ClaudeProjectBackend.Infrastructure \
  -s src/ClaudeProjectBackend.Api
```

### 4. Run the API

```bash
dotnet run --project src/ClaudeProjectBackend.Api
```

Swagger is available at: `http://localhost:5168/swagger`
Health check: `http://localhost:5168/health`

## Common commands

```bash
# Run all tests
dotnet test

# Build solution
dotnet build ClaudeProjectBackend.sln

# Add a new migration
dotnet ef migrations add <Name> -p src/ClaudeProjectBackend.Infrastructure -s src/ClaudeProjectBackend.Api

# Apply migrations
dotnet ef database update -p src/ClaudeProjectBackend.Infrastructure -s src/ClaudeProjectBackend.Api

# Reset dev database (drop + migrate + seed)
./scripts/reset-db.ps1
```

## API endpoints

### Auth — `/api/auth`

| Method | Endpoint | Auth | Description |
|---|---|---|---|
| `POST` | `/api/auth/register` | Public | Register a new account |
| `POST` | `/api/auth/login` | Public | Login and receive a JWT |

### Users — `/api/users`

| Method | Endpoint | Auth | Description |
|---|---|---|---|
| `GET` | `/api/users` | Admin | List all users (paginated) |
| `GET` | `/api/users/{id}` | Any | Get a user by id |
| `POST` | `/api/users` | Admin | Create a user |
| `DELETE` | `/api/users/{id}` | Admin | Delete a user |

## GitHub Actions

| Workflow | Trigger | Purpose |
|---|---|---|
| **CI** | push / PR | Restore, build, test, upload coverage |
| **Security** | push / PR / weekly | Scan for vulnerable NuGet packages |

## Adding a new feature

1. **Domain** — add entity + repository interface if needed
2. **Application** — create `FeatureName/` folder with action subfolders, service, and interface
3. **Infrastructure** — implement the repository, add entity configuration and `DbSet`
4. **Api** — add controller, wire service in DI
5. **Tests** — add service tests

```
Application/
  Orders/
    Create/
      CreateOrderRequest.cs
    List/
      ListOrdersQuery.cs
      OrderSummary.cs
    IOrderService.cs
    OrderService.cs
    OrderResponse.cs
```
