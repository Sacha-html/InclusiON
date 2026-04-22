# InclusiON.Testing — Documentación de Tests E2E

## Qué es

`InclusiON.Testing` es el repositorio de pruebas end-to-end (E2E) de la plataforma InclusiON. Usa [Playwright](https://playwright.dev/) para automatizar pruebas sobre el frontend Angular corriendo en un servidor local.

---

## Repositorio

```
InclusiON.Testing/
├── tests/
│   └── frontend/            ← Tests E2E organizados por módulo
│       └── homepage.spec.ts
├── app-dist/                ← Build del frontend (copiado desde InclusiON.Client)
├── scripts/
│   └── copy-dist.js         ← Script para copiar el build del cliente
├── .github/
│   └── workflows/
│       └── playwright.yml   ← GitHub Actions CI
├── playwright.config.ts     ← Configuración de Playwright
├── tsconfig.json
└── package.json
```

---

## Dependencias

| Herramienta | Versión | Rol |
|---|---|---|
| Playwright | ^1.59.1 | Framework de tests E2E |
| @types/node | ^25.6.0 | Tipos de Node.js para TypeScript |
| serve | (via npx) | Servidor estático para `app-dist/` |

---

## Cómo funciona

Los tests no levantan la app Angular en modo desarrollo. En su lugar:

1. Se buildea `InclusiON.Client` → genera `dist/inclusion-client/browser/`
2. Se copia ese build a `InclusiON.Testing/app-dist/`
3. Playwright levanta `npx serve app-dist/browser -p 4200 --single` antes de correr los tests
4. Los tests corren contra `http://localhost:4200`

Este enfoque permite que CI corra los tests sin necesidad de clonar ni tener acceso al repo del cliente.

---

## Configuración

### playwright.config.ts

| Parámetro | Valor |
|---|---|
| `baseURL` | `http://localhost:4200` |
| `trace` | `on-first-retry` |
| `screenshot` | `only-on-failure` |
| `video` | `retain-on-failure` |
| `retries` (CI) | 2 |
| `workers` (CI) | 1 |
| Browsers | Chromium, Microsoft Edge |

### webServer

```ts
webServer: {
  command: 'npx serve app-dist/browser -p 4200 --single',
  url: 'http://localhost:4200',
  reuseExistingServer: !process.env.CI,
}
```

El flag `--single` redirige todas las rutas al `index.html`, necesario para el routing de Angular.

---

## Scripts disponibles

| Comando | Descripción |
|---|---|
| `npm test` | Corre todos los tests |
| `npm run test:headed` | Corre los tests con el browser visible |
| `npm run test:ui` | Abre la UI interactiva de Playwright |
| `npm run test:debug` | Modo debug |
| `npm run test:chromium` | Solo Chromium |
| `npm run report` | Abre el último reporte HTML |
| `npm run copy-dist` | Copia el build de InclusiON.Client a `app-dist/` |
| `npm run build:client` | Buildea InclusiON.Client |
| `npm run update` | Buildea el cliente y copia el dist (todo en uno) |
| `npm run typecheck` | Chequeo de tipos TypeScript |

---

## Flujo de trabajo local

```bash
# Desde InclusiON.Testing
npm run update      # build del cliente + copia el dist
npm test            # corre los tests
npm run report      # abre el reporte HTML
```

---

## CI/CD — GitHub Actions

El workflow `.github/workflows/playwright.yml` se ejecuta automáticamente en cada `push` o `pull_request` a las ramas `main`, `master` y `develop`.

### Pasos del workflow

1. Checkout del repo
2. Setup Node.js LTS
3. `npm ci` — instala dependencias
4. `npx playwright install --with-deps` — instala browsers
5. `npx playwright test` — corre los tests
6. Sube el reporte HTML como artefacto (30 días de retención)

### Importante

`app-dist/` está commiteado en el repositorio. Antes de cada push que requiera reflejar cambios del frontend, correr:

```bash
npm run update
git add app-dist
git commit -m "chore: update app-dist"
```

---

## Estructura de tests

Los tests se organizan dentro de `tests/` por módulo o sección de la app:

```
tests/
└── frontend/
    └── homepage.spec.ts    ← Validación del título de la página principal
```

### Convención de nombres

- Archivos: `<modulo>.spec.ts`
- Tests: describen el comportamiento esperado en lenguaje natural
- Carpeta: `tests/frontend/` para tests del cliente Angular

---

## Generación de tests con Codegen

Playwright incluye una herramienta para grabar tests interactuando con el browser:

```bash
npx playwright codegen --output tests/frontend/mi-test.spec.ts http://localhost:4200
```

Esto abre el browser y graba cada acción como código Playwright, guardándolo directamente en el archivo indicado.

---

## Artefactos generados (no se suben al repo)

| Carpeta | Contenido |
|---|---|
| `test-results/` | Screenshots y videos de tests fallidos |
| `playwright-report/` | Reporte HTML completo |
| `blob-report/` | Reporte en formato blob |
| `trace/` | Archivos de trace para debugging |
