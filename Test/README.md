# InclusiON.Testing — Tests E2E

## Qué es

`InclusiON.Testing` es el repositorio de pruebas end-to-end (E2E) y de accesibilidad de la plataforma InclusiON. Usa [Playwright](https://playwright.dev/) para automatizar pruebas sobre el frontend Angular y el backend .NET.

---

## Estructura del repositorio

```
InclusiON.Testing/
├── tests/
│   ├── e2e/                           # Tests E2E funcionales (requieren backend + DB)
│   │   ├── 00-auth.spec.ts
│   │   ├── 01-instituciones.spec.ts
│   │   └── ... (28 suites)
│   ├── frontend/                      # Tests de frontend estático (no requieren backend)
│   │   ├── accessibility.spec.ts      # WCAG AAA (axe-core)
│   │   ├── coreui-validation.spec.ts  # Clases CoreUI
│   │   ├── input-validation.spec.ts   # IDs y labels
│   │   ├── responsive.spec.ts         # Diseño responsive
│   │   └── light-dark.spec.ts         # Modos claro/oscuro
│   └── helpers/                       # Utilidades compartidas
│       ├── api.ts                     # Cliente HTTP para setup/teardown vía API
│       ├── auth.ts                    # Login helpers
│       ├── constants.ts               # URLs, credenciales de test
│       ├── fixtures.ts                # Fixtures de Playwright
│       └── test-data.ts               # Datos de prueba
├── docs/
│   └── FUNDAMENTACION_TESTS.md        # Justificación normativa de cada test (WCAG/CIF)
├── app-dist/                          # Build de Angular (commiteado para CI)
├── scripts/
│   └── copy-dist.js                   # Copia dist desde InclusiON.Client
├── .github/
│   └── workflows/
│       └── playwright.yml
├── global-setup.ts                    # Setup global: seed y tokens de auth
├── global-teardown.ts                 # Teardown: limpieza de DB post-tests
├── playwright.config.ts
├── tsconfig.json
└── package.json
```

---

## Infraestructura requerida

### Tests E2E (`tests/e2e/`)

Requieren backend corriendo + base `inclusion_test` con datos de seed.

**1. Setup de base de datos** (desde `InclusiON.Server/`):

```powershell
docker cp InclusiON.Data/Scripts/db-users-setup.sql postgres:/tmp/setup.sql
docker exec postgres psql -U postgres -f /tmp/setup.sql
docker exec postgres psql -U postgres -d inclusion_test -c "CREATE EXTENSION IF NOT EXISTS vector;"
```

**2. Levantar backend en modo Testing:**

```bash
# Desde InclusiON.Server/InclusiON.Api
ASPNETCORE_ENVIRONMENT=Testing dotnet run
# o desde Rider: seleccionar launch profile "Testing"
```

> SMTP está **deshabilitado** en Testing — los emails no se envían.

### Tests de frontend (`tests/frontend/`)

Solo requieren el build de Angular en `app-dist/`. No necesitan backend.

---

## Setup del repo

```bash
npm ci
npx playwright install
```

---

## Correr tests

```bash
# Todos los tests
npm test

# Solo E2E
npx playwright test tests/e2e/

# Solo frontend/accesibilidad
npx playwright test tests/frontend/

# Con browser visible
npm run test:headed

# UI interactiva
npm run test:ui

# Debug
npm run test:debug

# Solo Chromium
npm run test:chromium
```

---

## Scripts disponibles

### General

| Script | Descripción |
|--------|-------------|
| `npm test` | Todos los tests |
| `npm run test:headed` | Browser visible |
| `npm run test:ui` | UI interactiva de Playwright |
| `npm run test:debug` | Modo debug |
| `npm run test:chromium` | Solo Chromium |
| `npm run report` | Abrir último reporte HTML |
| `npm run copy-dist` | Copiar build Angular a `app-dist/` |
| `npm run update` | Build Angular + copiar dist |
| `npm run typecheck` | Chequeo de tipos TypeScript |

### Accesibilidad

| Script | Descripción |
|--------|-------------|
| `npm run test:a11y` | Auditoría WCAG 2.1 AAA (axe-core) |
| `npm run test:a11y:headed` | Accesibilidad con browser visible |
| `npm run test:coreui` | Validación de clases CoreUI |
| `npm run test:inputs` | IDs y asociación de labels |
| `npm run test:responsive` | Diseño responsive (mobile/tablet/desktop) |
| `npm run test:theme` | Accesibilidad en modo claro/oscuro |
| `npm run test:wcag-aaa` | Solo tests WCAG AAA |
| `npm run test:contrast` | Solo tests de contraste |
| `npm run test:all` | Todos los tests de accesibilidad |

---

## Suites E2E

| Archivo | Funcionalidad |
|---------|---------------|
| `00-auth` | Login, logout, refresh token |
| `01-instituciones` | ABM instituciones |
| `02-roles` | Gestión de roles y permisos |
| `03-profesionales` | ABM profesionales, validación |
| `04-personas` | ABM personas con discapacidad |
| `05-familiares` | ABM representantes familiares |
| `06-invitaciones` | Invitaciones por email |
| `07-usuarios` | Gestión centralizada de usuarios |
| `08-reportes` | Generación y flujo de aprobación |
| `09-mensajes` | Mensajería interna |
| `10-actividades` | Creación de actividades |
| `10b-actividades-por-plantilla` | Actividades con plantillas |
| `11-diagnosticos` | Diagnósticos funcionales |
| `12-asignacion-actividades` | Asignar actividades a personas |
| `13-objetivos` | Gestión de objetivos |
| `14-family-portal` | Portal familiar |
| `15-catalogos` | ABM catálogos |
| `16-asignacion-profesional-persona` | Vincular profesional↔persona |
| `17-vinculacion-familiar-persona` | Vincular familiar↔persona |
| `18-perfil-habilidades` | Perfil de habilidades |
| `19-dashboard-profesional` | Dashboard del profesional |
| `20-login-personas` | Login adaptativo (PIN, asistido) |
| `21-integracion-completa` | Flujo integral |
| `22-reportes-profesional` | Reportes desde el profesional |
| `23-detalle-persona-profesional` | Vista detalle |
| `24-portal-aac-persona` | Portal AAC |
| `25-nueva-actividad` | Creación de nueva actividad |
| `26-family-progreso-reportes` | Progreso desde el portal familiar |
| `27-admin-dashboard` | Dashboard de administrador |
| `28-aac-roadmap-comunicacion` | Roadmap y comunicación AAC |

---

## Configuración

| Parámetro | Valor |
|-----------|-------|
| `baseURL` | `http://localhost:4200` |
| `trace` | `on-first-retry` |
| `screenshot` | `only-on-failure` |
| `video` | `retain-on-failure` |
| `retries` (CI) | 2 |
| `workers` (CI) | 1 |
| Browsers | Chromium, Microsoft Edge |

---

## CI/CD

El workflow `.github/workflows/playwright.yml` corre en cada `push` o `pull_request` a `main`/`master`/`develop`.

`app-dist/` está commiteado. Antes de hacer push con cambios de frontend:

```bash
npm run update
git add app-dist
git commit -m "chore: update app-dist"
```

El reporte HTML se sube como artefacto con 30 días de retención.

---

## Artefactos generados (no se suben al repo)

| Carpeta | Contenido |
|---------|-----------|
| `test-results/` | Screenshots y videos de tests fallidos |
| `playwright-report/` | Reporte HTML completo |
| `blob-report/` | Reporte en formato blob |
