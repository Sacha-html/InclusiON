# InclusiON.Client

Read `../opencode.md` for cross-project work. This module owns the accessible Angular client; it does not own server authorization, business rules, DTO definitions, or database schema.

## Stack and entry points

- Angular standalone application: Angular packages `^20.2.2`, Angular CLI/build `^20.2.1`, TypeScript `~5.8.3`, RxJS `~7.8.2`, and CoreUI Angular `^5.5.11` (`package.json`). Node is constrained to `^20.19.0 || ^22.12.0 || ^24.0.0`; npm must be `>=9`.
- `src/main.ts` bootstraps `appConfig`; `src/app/app.config.ts` provides hash routing, lazy routes, HTTP interceptors, CoreUI providers, animations, and development-safe view transitions.
- `src/app/app.routes.ts` is the role portal boundary: AAC/person (`/app`, `/person`), professional (`/pro`), family (`/family`), admin (`/admin`), plus public login/invitation flows.
- `src/environments/environment.ts` uses `http://localhost:5000/api`. The production environment is a specific LAN address, not a deploy-neutral default.
- `src/scss/styles.scss` is the global style entrypoint; `_accessibility-themes.scss` supplies the `--a11y-*` profile variables.

## Folder map

```text
src/app/
  components/       global UI (accessibility panel, toaster, help)
  guards/           authentication, guest, role, permission checks
  interceptors/     HTTP authentication and spinner behavior
  layout/           role-specific shells and navigation
  models/           client request/response/domain types
  services/         HTTP, SignalR, UI-state, and browser services
  shared/           reusable components, constants, utilities
  views/            lazy-loaded admin, professional, family, AAC, and public pages
src/environments/   API and runtime values
src/scss/           CoreUI customization and accessibility themes
```

TypeScript aliases in `tsconfig.json` include `@app`, `@services`, `@models`, `@components`, `@guards`, `@interceptors`, `@shared`, and `@env`. Strict compiler and template checks are enabled.

## Supported commands

Run from `InclusiON.Client/`:

```bash
npm install
npm start
npm run start:prod
npm run build
npm run build:dev
npx tsc --noEmit
ng test
npx karma start --single-run --browsers=ChromeHeadless
npm run cap:sync
npm run cap:open:android
```

`package.json` defines no lint script. Karma/Jasmine is configured in `karma.conf.js`; its normal launcher is Chrome and coverage is written under `coverage/coreui-free-angular-admin-template/`. The repository has some component/guard specs, so add nearby `*.spec.ts` tests for new behavior. External Playwright E2E/accessibility coverage is documented in `../InclusiON.Documents/Test/README.md`.

## Server contract

- Use services and typed models to call the API. The API base URL is configured in environments; do not hardcode endpoint hosts in components.
- `authInterceptor` and `spinnerInterceptor` are registered globally. The client stores access token, refresh token, and basic current-user data under the keys declared in `environment.ts`.
- Server notification delivery uses SignalR; keep changes to notification behavior aligned with `InclusiON.Server/InclusiON.Api/Hubs/NotificationHub.cs` and the client SignalR service.
- Routes and client guards improve UX only. The server remains the authority for JWT, permissions, and resource-level access.

## UI and accessibility guardrails

- Use standalone components and lazy `loadComponent`/`loadChildren` routes. Do not introduce NgModules for new UI.
- Use `--a11y-*` variables rather than hardcoded colors. Profiles are applied through `data-color-mode` and `data-profile` on the document root.
- Maintain semantic controls, visible focus, labels/`aria-label` for icon-only controls, and information that is not color-only. The documented baseline is WCAG 2.1 AA/AAA.
- Preserve role boundaries in `app.routes.ts` and child route files; add the matching server authorization before exposing a new capability.
- `capacitor.config.ts` expects web output at `dist/inclusion-client/browser`; its Android configuration permits cleartext HTTP, so do not assume it is production-safe.
- Consult `../InclusiON.Documents/CLAUDE_FRONTEND.md`, `../InclusiON.Documents/References/REF-accesibilidad.md`, and the relevant HU/process document for feature-specific rules, but validate their claims against current source.
