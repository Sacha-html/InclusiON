# InclusiON workspace

Start with the nearest `opencode.md`: `InclusiON.Client/opencode.md`, `InclusiON.Server/opencode.md`, or `InclusiON.Documents/opencode.md`. This file covers changes that cross those boundaries.

## Components and ownership

| Component | Owns | Does not own |
|---|---|---|
| `InclusiON.Client/` | Angular UI, role portals, browser-side accessibility, API/SignalR clients, Capacitor packaging | API rules, persistence, database migrations |
| `InclusiON.Server/` | .NET API, domain rules, authorization, CQRS handlers, PostgreSQL schema/migrations/seed data, SignalR hub | Browser UI and product documentation |
| `InclusiON.Documents/` | Product/process/acceptance documentation, architecture references, local Docker Compose infrastructure | Runtime application code |

The repository root describes the three components as the application workspace. `demo-repository/` is present but is not part of this ownership model.

## How components connect

- The client calls the server under `http://localhost:5000/api` in `InclusiON.Client/src/environments/environment.ts`; production currently contains a LAN IP and must be reviewed before deployment.
- The API exposes REST controllers and the SignalR hub at `/hubs/notifications`. It configures CORS from `Cors:AllowedOrigins`.
- The API persists through EF Core/Npgsql to PostgreSQL. `InclusiON.Documents/Infra/docker-compose.yml` runs PostgreSQL 17 with pgvector and can also build the API and frontend containers.
- Contracts are implicit rather than generated: server DTOs/controllers and client `models/` plus `services/` must change together. Use the development-only Scalar/OpenAPI endpoint as an additional API reference when the API is running.
- Product requirements, process rules, acceptance criteria, data definitions, and implementation status live in `InclusiON.Documents/`. Treat them as input to a feature, then verify behavior in both application modules.

## Cross-project workflow

1. Locate the relevant process/HU/feature document under `InclusiON.Documents/` and confirm current implementation status in `State/`.
2. For contract or behavior changes, update server DTOs, handler/controller, authorization, persistence/migration where needed, then update the matching client model/service/view.
3. Keep the client API base URL and API CORS origins compatible for the environment being used.
4. For schema changes, create the EF migration in `InclusiON.Server`; local database lifecycle instructions and Compose live in `InclusiON.Documents/Infra/`.
5. Add module-level tests. E2E/accessibility testing is documented in `InclusiON.Documents/Test/README.md` but its test repository is external to this workspace.

## Change-impact checklist

- API request, response, route, pagination, encrypted ID, or error behavior changed: inspect `InclusiON.Server/InclusiON.DTOs/`, controller/handler tests, and Client models/services/call sites.
- New protected resource or role behavior: apply server policy plus row-level authorization where applicable; align Client guards/routes and document the rule if it changes product behavior.
- New database entity or column: update Domain, Data configuration, `AppDbContext`, migration, seed/fixtures as applicable, and the data documentation.
- New UI or portal flow: preserve Angular standalone/lazy-route conventions and accessibility profiles; connect to the real server contract rather than duplicating business rules in the client.
- Accessibility, authentication, role, reporting, roadmap, or adaptive-engine behavior changed: check `Documents/References/`, `Documents/Process/`, `Documents/HU/`, and applicable `Documents/Features/` before and after implementation.
- Local infrastructure changed: verify relative paths in `Documents/Infra/docker-compose.yml`; its build contexts reference sibling modules.

## Repository commands

There is no root package manifest or `.sln`; run commands from the relevant module. Do not assume `dotnet test` at `InclusiON.Server/` discovers all test projects. See the module guides for supported commands.

## Guardrails

- Preserve domain and authorization enforcement in the server; client guards are navigation controls, not a security boundary.
- Do not put credentials or service tokens into new tracked configuration. Existing development settings contain sensitive-looking values and should not be propagated.
- Do not treat historical counts, implementation-status prose, or future-work sections in documentation as current runtime truth without checking source/configuration.
