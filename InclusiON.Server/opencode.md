# InclusiON.Server

Read `../opencode.md` for cross-project workflow. This module owns the .NET API, domain rules, authorization, persistence, migrations, and seeded data. Client-side guards and documentation do not replace server enforcement.

## Stack and startup

- The inspected API, Application, Data, Infrastructure, and test projects target `net10.0` with nullable and implicit usings enabled.
- The entry project is `InclusiON.Api/InclusiON.Api.csproj` (`Microsoft.NET.Sdk.Web`). It uses ASP.NET Core, EF Core 10/Npgsql/PostgreSQL, JWT bearer auth, SignalR, Scalar/OpenAPI, Serilog, health checks, response compression/cache/rate limiting, and OpenTelemetry.
- `Program.cs` configures the application pipeline, CORS, encrypted ID JSON conversion/model binding, `/hubs/notifications`, `/metrics`, `/health`, `/health/ready`, and `/health/live`.
- Outside the `IntegrationTests` environment, API startup migrates a relational database and runs encryption/seed steps. Do not treat `dotnet run` as side-effect-free.
- Development launch settings serve HTTP `5000` and HTTPS `5001`; Scalar/OpenAPI is mapped only in Development.

## Architecture and folder map

```text
InclusiON.Api/                    entrypoint, controllers, middleware, filters, hub
InclusiON.Application/            CQRS commands/queries/handlers and interfaces
InclusiON.Domain/                 entities and domain types
InclusiON.DTOs/                   API request/response and common transport types
InclusiON.Data/                   AppDbContext, Fluent configurations, migrations, seeders, SQL scripts
InclusiON.Infrastructure/         repository implementations and external/application services
InclusiON.Infrastructure.Telemetry/ telemetry and health-check support
InclusiON.Shared/                 constants and localized resources
InclusiON.Workers/                hosted/background worker code referenced by the API
InclusiON.Tests/Unit/             xUnit unit and architecture tests
InclusiON.Tests/Integration/      xUnit API integration tests
InclusiON.Tests/Controllers/      xUnit controller-focused tests
```

The project dependency direction is enforced by references: Application depends on Domain/DTOs/Shared; Data depends on Domain/DTOs; Infrastructure depends on Application/Data; Api references Application, Data, Infrastructure, Telemetry, and Shared. There is no `.sln` file in this directory.

## Supported commands

Run from `InclusiON.Server/`:

```bash
dotnet run --project InclusiON.Api/InclusiON.Api.csproj
dotnet run --project InclusiON.Api/InclusiON.Api.csproj -- --RateLimiter:Disabled=true
dotnet test InclusiON.Tests/Unit/InclusiON.Tests.Unit.csproj
dotnet test InclusiON.Tests/Integration/InclusiON.Tests.Integration.csproj
dotnet test InclusiON.Tests/Controllers/InclusiON.Tests.Controllers.csproj
dotnet ef migrations add <MigrationName> --project InclusiON.Data --startup-project InclusiON.Api
dotnet ef database update --project InclusiON.Data --startup-project InclusiON.Api
```

Unit and controller tests use xUnit, FluentAssertions, NSubstitute, EF Core InMemory, and architecture tests where configured. Integration tests reference `Microsoft.AspNetCore.Mvc.Testing` and `Testcontainers.PostgreSql`; do not assume they use the database setup described in older README text without checking test fixtures. No analyzers or standalone lint command were found in inspected project files.

## Implementation conventions

- Controllers translate HTTP concerns and delegate to command/query handlers. Handlers implement the custom CQRS interfaces and are registered by reflection in `AddApplicationServices()`; do not register a new handler manually.
- Put repository interfaces in Application and implementations in Infrastructure. Keep persistence configuration, migrations, and seeding in Data.
- For a new entity, add the domain model, Fluent configuration, `AppDbContext` set, and an EF migration. Review seed and test impact.
- Server authorization is layered: JWT/policy authorization plus resource-level filters/services. Apply the applicable resource check instead of relying on client role checks.
- The API normalizes incoming `DateTime` to UTC and encrypts/decrypts GUIDs through global converters/binders. Preserve these contracts in controller, DTO, and Client changes.
- PostgreSQL case-insensitive search uses `EF.Functions.ILike`; avoid client-side filtering and ad hoc `ToLower()` translations for this purpose.

## Dependencies and configuration

- Client contract consumers are under `../InclusiON.Client/src/app/models/` and `../InclusiON.Client/src/app/services/`; coordinate route, DTO, pagination, error, encrypted-ID, and SignalR changes with them.
- Database Compose and bootstrap instructions live at `../InclusiON.Documents/Infra/`. The Compose API uses a development PostgreSQL connection and exposes `5000`.
- `appsettings.*.json` and environment variables configure connection strings, JWT, SMTP, telemetry, rate limiting, and CORS. Keep secrets out of new tracked config; inspected development settings already include sensitive-looking values.
- Semantic-search model assets are intentionally absent from the repository. The README states that the API uses a null embedding implementation when they are unavailable.

## Guardrails

- Do not put business logic in controllers or bypass handlers/repositories to satisfy a UI request.
- Do not create a migration without first verifying the model and configuration; applying it changes persistent state.
- Do not weaken CORS, rate limiting, resource authorization, or encryption behavior to make the Client work. Fix the contract or environment configuration instead.
- Consult `../InclusiON.Documents/CLAUDE_BACKEND.md` and the relevant process/HU/feature document for established patterns and product rules, then verify against current source before relying on status claims.
