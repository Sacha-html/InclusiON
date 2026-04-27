# CLAUDE.md — Frontend (Angular 20)

Instrucciones para agentes AI y desarrolladores trabajando en `InclusiON.Client/`.

---

## Stack

- **Angular 20** con standalone components
- **CoreUI Angular** como template base de admin
- **SCSS** con variables de accesibilidad (`--a11y-*`)
- **TypeScript** estricto

---

## Comandos

```bash
cd InclusiON.Client
npm install
npm start                         # Dev server en http://localhost:4200
npm run build                     # Build de producción
ng test                           # Tests con Karma
ng generate component <nombre>    # Generar componente
```

---

## Estructura del Proyecto

```
src/app/
├── components/                   ← Componentes globales (accessibility-panel, toaster)
├── guards/                       ← auth.guard, guest.guard, role.guard
├── interceptors/                 ← auth.interceptor (JWT en headers)
├── models/                       ← Interfaces TS (requests/, responses/)
├── services/                     ← Servicios Angular (auth, persons, accessibility, etc.)
├── shared/
│   ├── components/               ← Componentes reutilizables (avatar, big-button, shape, visual-card)
│   └── constants/                ← avatar-colors, roles
├── layouts/                      ← Layouts por rol (aac, default, family, professional)
├── views/
│   ├── aac/                      ← Portal del estudiante (PersonWithDisability)
│   ├── dashboard/                ← Dashboard admin
│   ├── family/                   ← Portal familia
│   ├── pages/                    ← Login, register, visual-login, 404, 500
│   └── professional/             ← Portal profesional
└── app.routes.ts                 ← Rutas principales con lazy loading
```

---

## Rutas y Roles

| Ruta | Layout | Rol requerido | Guard |
|------|--------|---------------|-------|
| `/login`, `/admin-login` | — | Ninguno | `guestGuard` |
| `/app/**` | `AacLayout` | `PersonWithDisability` | `authGuard` + `roleGuard` |
| `/pro/**` | `ProfessionalLayout` | `Professional`, `Admin` | `authGuard` + `roleGuard` |
| `/family/**` | `FamilyLayout` | `FamilyRepresentative`, `Admin` | `authGuard` + `roleGuard` |
| `/admin/**` | `DefaultLayout` | `Admin` | `authGuard` + `roleGuard` |

---

## Autenticación

- JWT almacenado en `localStorage`: `access_token`, `refresh_token`, `current_user`
- `auth.interceptor.ts` agrega el token a cada request HTTP
- `auth.service.ts` maneja login/logout/refresh
- `ConfigService` gestiona la URL base de la API

---

## Sistema de Accesibilidad

### Variables CSS

Todos los componentes usan variables `--a11y-*` que cambian según el perfil activo:

```scss
// Backgrounds
--a11y-bg, --a11y-bg-secondary, --a11y-surface

// Texto
--a11y-text, --a11y-text-secondary, --a11y-text-muted

// Colores de acción
--a11y-primary, --a11y-success, --a11y-warning, --a11y-danger

// Bordes y focus
--a11y-border, --a11y-focus-color, --a11y-focus-width

// Tipografía
--a11y-font-family, --a11y-font-size, --a11y-line-height
```

### Atributos HTML

```html
<html data-color-mode="light|dark" data-profile="default|high-contrast|dyslexia|low-vision|deuteranopia|protanopia|tritanopia">
```

### 7 Perfiles de accesibilidad

| Perfil | Descripción |
|--------|-------------|
| `default` | Sin ajustes especiales |
| `high-contrast` | Contraste máximo 21:1 |
| `dyslexia` | Fuente Lexend, spacing aumentado |
| `low-vision` | Fuente 20px base, targets 44px+ |
| `deuteranopia` | Daltonismo rojo-verde (más común) |
| `protanopia` | Daltonismo rojo-verde (sensibilidad reducida al rojo) |
| `tritanopia` | Daltonismo azul-amarillo |

### Archivos clave

- `src/scss/_accessibility-themes.scss` — Variables y estilos por perfil
- `src/app/services/accessibility.service.ts` — Servicio con signals reactivos
- `src/app/components/accessibility-panel/` — Panel de usuario (atajo Alt+A)

### Reglas WCAG al desarrollar

- Usar siempre variables `--a11y-*` para colores, nunca valores hardcodeados
- Contraste mínimo: 4.5:1 (AA), idealmente 7:1 (AAA)
- Botones/targets: mínimo 44px en perfil low-vision
- No transmitir información solo con color — usar iconos + texto
- Agregar `aria-label` a elementos interactivos sin texto visible
- Focus visible: 3-4px solid outline

---

## Convenciones de Desarrollo

### Crear un nuevo componente/vista

1. Usar `ng generate component views/<portal>/<nombre>` (standalone por defecto en Angular 20)
2. Agregar la ruta en el archivo `routes.ts` del portal correspondiente
3. Usar lazy loading: `loadComponent: () => import('./nombre.component').then(m => m.NombreComponent)`

### Crear un servicio para un nuevo endpoint

1. Crear en `src/app/services/<nombre>.service.ts`
2. Inyectar `HttpClient` y `ConfigService`
3. Usar `ConfigService.apiUrl` como base URL
4. Exportar desde `src/app/services/index.ts`
5. Los tipos de request/response van en `src/app/models/requests/` y `responses/`

### Ejemplo de servicio

```typescript
@Injectable({ providedIn: 'root' })
export class ActivitiesService {
  private readonly http = inject(HttpClient);
  private readonly config = inject(ConfigService);

  getAll(params: PagedRequest) {
    return this.http.get<PagedResponse<ActivityResponse>>(
      `${this.config.apiUrl}/activities`, { params: { ...params } }
    );
  }
}
```

---

## Componentes Existentes

### Layouts (4)
- `AacLayoutComponent` — Para estudiantes, con `aac-header` y `aac-nav`
- `ProfessionalLayoutComponent` — Para profesionales
- `FamilyLayoutComponent` — Para familia
- `DefaultLayoutComponent` — Para admin

### Vistas implementadas
- **Login visual completo:** identify-user (con multi-match por homónimos), role-selection, login-method-selector, pin-login, visual-standard-login, assisted-login, family-login
- **Admin — Personas:** list (con modal cambiar método de login), detail, edit, new
- **Admin — Profesionales:** list (tabs activos/pendientes, validación), detail (tabs personas/instituciones/reportes), edit, new
- **Admin — Familiares:** list, detail, edit, new
- **Admin — Instituciones:** list, detail, edit, new
- **Admin — Reportes:** list, detail, new
- **Admin — Usuarios:** list paginado con acciones (reset, desactivar, reactivar)
- **Admin — Catálogos:** submenú por tipo (6 tipos) con CRUD
- **Admin — Roles:** listado con checkboxes de permisos por módulo
- **Portal Profesional:** dashboard con datos reales, Mi Aula (personas asignadas), lista de reportes, invitaciones
- **Portal Familiar:** registro por invitación (/invite/:code), lista de reportes
- **Dashboards:** admin, profesional con datos reales
- **AAC:** home, activities, calendar, communication (stubs)

### Shared components
- `AvatarComponent` — Avatar con colores de catálogo
- `BigButtonComponent` — Botón accesible grande
- `ShapeComponent` — Formas para login visual
- `VisualCardComponent` — Tarjeta visual para login
- `DataTableComponent` — Tabla paginada con sort, búsqueda, acciones y botones de header
- `ConfirmModalComponent` — Modal de confirmación reutilizable
- `InstitutionFilterComponent` — Selector de institución para admin global/institucional

---

## Lo que FALTA construir (por portal)

### Portal Profesional (`/pro/`)
- Listado y CRUD de actividades
- Perfil de habilidades del estudiante
- Gestor de roadmap drag-and-drop
- Radar chart de habilidades
- Mensajería inbox
- Panel config MDA
- Timeline ajustes adaptativos

### Portal Estudiante AAC (`/app/`)
- Roadmap visual estilo Duolingo
- ActivityPlayerShell + 5 players

### Portal Familia (`/family/`)
- Dashboard con datos de progreso

---

## Lo que NO hacer

- No usar colores directos (`#fff`, `red`) — usar variables `--a11y-*`
- No crear componentes como módulos — usar standalone components
- No importar CoreUI sin verificar que sea compatible con accesibilidad
- No hardcodear URLs de API — usar `ConfigService.apiUrl`
- No guardar datos sensibles en localStorage (solo tokens y user básico)
- No olvidar `aria-label` en botones con solo icono
