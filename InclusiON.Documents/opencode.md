# InclusiON.Documents

Read `../opencode.md` for workspace boundaries. This module is the documentation and local infrastructure authority; it does not contain the Client or Server runtime source.

## Purpose and map

```text
Docs/          product documentation, data model, business rules, story map, UI patterns
Process/       process definitions and BPMN diagrams
HU/            user stories and acceptance criteria
Features/      technical feature specifications
References/    cross-cutting accessibility and authentication references
State/         implementation/status tracking
ABM/           CRUD-oriented functional documentation
Sprints/        sprint history and reports
Infra/          local Docker Compose and database bootstrap instructions
Test/           documentation for the external Playwright E2E/a11y repository
ARQUITECTURA.md system architecture overview
CLAUDE_*.md    module working guidance for frontend and backend
```

`README.md` is the documentation index. `ARQUITECTURA.md` describes the intended architecture and dependency direction; use application project files and source as the runtime authority when they differ.

## Local infrastructure

`Infra/docker-compose.yml` is the only Compose file found here. It defines:

- `postgres`: `pgvector/pgvector:pg17`, host port `5432`, persistent `inclusion_pgdata`, and init SQL mounted from `../../InclusiON.Server/InclusiON.Data/Scripts/db-dev-setup.sql`.
- `api`: builds `../../InclusiON.Server`, exposes `5000`, and configures the development API to use the Compose database.
- `frontend`: builds `../../InclusiON.Client`, exposes `4200` from Nginx.
- `inclusion-agent`: references `../../Inclusion.Agent`; that directory is outside the three primary modules and should be verified before relying on full-stack Compose.

Run from `Infra/`:

```bash
docker compose up -d postgres
docker compose up -d
docker compose stop
docker compose down
docker compose down -v
docker compose restart postgres
```

`down -v` deletes the persistent database volume. The PostgreSQL init script only runs with an empty volume. API startup applies migrations and seed data except in its integration-test environment.

## Documentation workflow

1. Begin feature analysis with the matching `HU/`, `Process/`, and, if present, `Features/` document.
2. Check `State/` for tracked implementation status, but verify it against Client/Server source before declaring behavior complete.
3. For behavior or data-model changes, update the relevant specification/reference alongside application changes when the documentation represents the accepted product contract.
4. Keep links relative and update indexes only when a new document needs discovery.

## Test documentation boundary

`Test/README.md` documents an external `InclusiON.Testing` Playwright repository, including E2E and WCAG tests. Its commands are not runnable from this module unless that separate repository is available. It states that E2E requires the API and `inclusion_test`, while static frontend tests use a copied Angular build.

## Guardrails

- Do not use historical sprint notes, planned-work sections, or entity/feature counts as proof of current implementation.
- Do not change Compose build contexts or SQL mount paths without checking the relative path from `Infra/` and the referenced file in the Server module.
- Treat database credentials and external endpoints in infrastructure/readme files as environment-specific; do not copy them into new tracked files.
- Keep product rules centralized here, but enforce them in Server code and represent them accessibly in Client code.
