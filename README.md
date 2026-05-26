# InclusiON Frontend

Angular 20 + CoreUI para el panel de administración inclusivo.

## Requisitos

- Node.js 20+
- npm 10+

## Instalación

```bash
npm install
npm update
```

## Desarrollo

```bash
npm start
# http://localhost:4200
```

## Build

```bash
npm run build      # Production (dist/)
npm run build:dev  # Development
```

## Testing

```bash
ng test                                                    # Todos los tests
ng test --include="**/foo.spec.ts"                        # Un archivo específico
npx karma start --single-run --browsers=ChromeHeadless    # Headless
```

## Tipado

```bash
npx tsc --noEmit
```

---

## Estructura

```
src/
├── app/
│   ├── constants/     # Constantes de dominio (avatar colors, shapes, etc.)
│   ├── guards/        # Route guards (auth, role)
│   ├── interceptors/  # HTTP interceptors (JWT, error handling)
│   ├── layout/        # Layout principal (CoreUI sidebar/navbar)
│   ├── models/        # Interfaces, Types, Enums, Requests, Responses
│   ├── services/      # Servicios HTTP
│   ├── shared/        # Componentes, pipes y directivas reutilizables
│   └── views/         # Páginas (admin/, auth/, visual-login/, professional/, family/)
├── assets/
├── environments/
└── styles/
```

---

## Servicios

| Servicio | Uso |
|----------|-----|
| `AuthService` | Login, JWT, refresh token, cambio de método de login |
| `PersonsService` | CRUD Personas, asignación de profesionales, actividades recomendadas |
| `ProfessionalsService` | CRUD Profesionales, validación, historial de estados |
| `FamilyService` | CRUD Representantes familiares |
| `ActivitiesService` | CRUD Actividades, búsqueda semántica, personas compatibles |
| `ReportsService` | Informes (admin, profesional, familiar) |
| `InstitutionsService` | Instituciones educativas |
| `AssignmentsService` | Asignaciones profesional ↔ persona |
| `InvitationsService` | Invitaciones de profesionales |
| `UserManagementService` | Gestión de usuarios admin (reset password, activar/desactivar) |
| `CatalogsService` | Catálogos del sistema (métodos de login, colores de avatar, etc.) |
| `CatalogAdminService` | Gestión de catálogos desde admin |
| `AccessibilityService` | Preferencias de accesibilidad del usuario |
| `ToastService` | Notificaciones toast |

---

## API

El frontend se conecta al backend en `environments/environment.ts`:

```typescript
export const environment = {
  production: false,
  apiUrl: 'http://localhost:5000/api'
};
```

### Endpoints usados

| Endpoint | Método | Descripción |
|----------|--------|-------------|
| `/auth/identify` | POST | Identificar usuario (puede retornar múltiples candidatos) |
| `/auth/login/visual-standard` | POST | Login con contraseña |
| `/auth/login/visual-pin` | POST | Login con PIN |
| `/auth/login/visual-assisted` | POST | Login asistido |
| `/auth/refresh` | POST | Renovar JWT |
| `/auth/users/{userId}/login-method` | PUT | Cambiar método de login |
| `/persons` | GET/POST | Listar / crear personas |
| `/persons/{id}` | GET/PUT | Obtener / actualizar persona |
| `/persons/{id}/professionals` | GET | Profesionales asignados |
| `/persons/{id}/supervisor-candidates` | GET | Candidatos a supervisor de login |
| `/professionals` | GET/POST | Listar / crear profesionales |
| `/professionals/pending` | GET | Solicitudes pendientes |
| `/professionals/{id}/validate` | POST | Aprobar o rechazar profesional |
| `/family` | GET/POST | Listar / crear representantes familiares |
| `/reports` | GET/POST | Listar / crear informes |
| `/reports/family` | GET | Informes del familiar autenticado |
| `/institutions` | GET/POST | Instituciones educativas |
| `/admin/users` | GET | Usuarios del sistema |
| `/catalogs/login-methods` | GET | Métodos de login activos |
| `/catalogs/avatar-colors` | GET | Colores de avatar |
| `/activities/{id}/similar` | GET | Actividades similares (búsqueda semántica) |
| `/activities/{id}/compatible-persons` | GET | Personas compatibles para una actividad |
| `/persons/{id}/recommended-activities` | GET | Actividades recomendadas para una persona |

---

## Autenticación

El proyecto usa JWT con refresh tokens. Flujo del login visual:

1. **Identify** — el usuario ingresa su nombre; si hay homónimos retorna una lista de candidatos con avatar de color para seleccionar
2. **Login** — según el método configurado (Estándar / PIN / Asistido) se obtienen los tokens
3. **Refresh** — el interceptor renueva automáticamente el token al expirar

### Métodos de login

| Método | Descripción |
|--------|-------------|
| Estándar | Contraseña convencional |
| PIN | Código numérico de 4 dígitos |
| Asistido | Un supervisor (profesional o familiar) ingresa su contraseña |

### Roles

| Rol | Descripción |
|-----|-------------|
| `ADMIN` | Administrador del sistema |
| `PROFESSIONAL` | Profesional / tutor |
| `FAMILY` | Representante familiar |
| `PERSON` | Persona con discapacidad |

---

## Estilos

El proyecto usa CoreUI + variables SCSS personalizadas:

```scss
$primary:            #2E5FA3;
$secondary:          #6C757D;
$success:            #28A745;
$danger:             #DC3545;
$warning:            #FFC107;

// Accesibilidad
$high-contrast-bg:   #000000;
$high-contrast-text: #FFFFFF;
```

---

## Extensiones Angular

```bash
ng generate component component-name
ng generate service service-name
ng generate directive directive-name
ng generate pipe pipe-name
```

---

## Git

Commits convencionales:

```bash
git commit -m "feat: agregar login asistido"
git commit -m "fix: corregir filtro de profesionales"
git commit -m "docs: actualizar README"
```
