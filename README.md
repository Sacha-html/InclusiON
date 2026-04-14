# InclusiON Frontend

Angular 20 + CoreUI para el panel de administración inclusivo.

## Requisitos

- Node.js 20+
- npm 10+

##Instalación

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
npm run build:dev # Development
```

## Testing

```bash
ng test                           # Todos los tests
ng test --include="**/foo.spec.ts" # Un archivo específico
npx karma start --single-run --browsers=ChromeHeadless # Headless
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
│   ├── components/     # Componentes reutilizables
│   ├── guards/         # Route guards
│   ├── interceptors/  # HTTP interceptors
│   ├── layout/        # Layout (CoreUI)
│   ├── models/        # Interfaces, Types, Enums
│   ├── services/      # Servicios HTTP
│   ├── shared/        # Pipes, Directivas
│   └── views/         # Páginas (admin, auth, visual-login)
├── assets/
├── environments/
└── styles/
```

---

## Servicios

| Servicio | Uso |
|----------|-----|
| `AuthService` | Login, JWT, refresh token |
| `PersonsService` | CRUD Personas |
| `ProfessionalsService` | CRUD Profesionales |
| `AssignmentsService` | Asignaciones |
| `ReportsService` | Reportes |
| `ToastService` | Notificaciones |

## Modelos

```typescript
// Ejemplo de interfaz
interface Person {
  id: string;
  firstName: string;
  lastName: string;
  loginMethod?: LoginMethod;
  disabilityType?: DisabilityType;
}
```

---

## API

El frontend conectarse al backend en `environments/environment.ts`:

```typescript
export const environment = {
  production: false,
  apiUrl: 'http://localhost:5000/api'
};
```

### Endpoints常用ados

| Endpoint | Método | Descripción |
|----------|--------|-------------|
| `/api/auth/login` | POST | Login JWT |
| `/api/auth/identify` | POST | Identificar usuario |
| `/api/persons` | GET | Listar personas |
| `/api/persons` | POST | Crear persona |
| `/api/persons/{id}/professionals` | GET | Profesionales asignados |
| `/api/persons/{id}/change-login-method` | PUT | Cambiar método login |
| `/api/professionals` | GET | Listar profesionales |
| `/api/family` | GET | Listar representantes |
| `/api/institutions` | GET | Listar instituciones |

---

## Autenticación

El proyecto usa JWT con refresh tokens. Flujo:

1. `IdentifyUser` - Identificar usuario por nombre
2. `Login` - Obtener tokens según método de login
3. Refresh token automático si expira

### Roles

- `ADMIN` - Administrador del sistema
- `PROFESSIONAL` - Profesional/tutor
- `FAMILY` - Representante familiar
- `PERSON` - Persona con discapacidad

---

## Estilos

El proyecto usa CoreUI + variables personalizadas:

```scss
// Colores del sistema
$primary: #2E5FA3;
$secondary: #6C757D;
$success: #28A745;
$danger: #DC3545;
$warning: #FFC107;

// Accesibilidad
$high-contrast-bg: #000000;
$high-contrast-text: #FFFFFF;
```

---

## Extensiones Angular

Para crear nuevos componentes:

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