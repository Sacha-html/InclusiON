# Proceso 08 — Autenticación y Gestión de Sesiones

**Origen:** Implementación del sistema (no definido en proyecto final original)

## Descripción

Sistema de autenticación multi-método adaptado a diferentes niveles de autonomía. Cada tipo de usuario accede a la plataforma con un método de login acorde a sus capacidades. La autenticación se basa en JWT con refresh tokens. Todo el flujo está completamente implementado.

## Participantes

- **Persona con Discapacidad** — Login visual (PIN, estándar o asistido) desde `/login`
- **Profesional** — Login con email y contraseña desde `/admin-login`
- **Familiar** — Login visual con contraseña familiar desde `/login`
- **Admin** — Login con email y contraseña desde `/admin-login`

## Endpoints de autenticación ✅ Implementado

| Método | Endpoint | Autonomía | Descripción |
|--------|----------|-----------|-------------|
| Estándar | `POST /api/auth/login` | Alta | Email + contraseña (admins y profesionales) |
| Visual Estándar | `POST /api/auth/login/visual-standard` | Alta | Identificación por nombre + contraseña visual |
| PIN | `POST /api/auth/login/pin` | Media | Identificación por nombre + PIN 4 dígitos |
| Asistido | `POST /api/auth/login/assisted` | Baja | Supervisor autoriza el acceso |
| Familiar | `POST /api/auth/login/family` | N/A | Identificación por nombre + contraseña |
| Identificación | `POST /api/auth/identify` | Todos | Paso previo: busca usuario por nombre y devuelve método de login configurado |
| Refresh | `POST /api/auth/refresh` | Todos | Renueva JWT antes de expiración |
| Cambiar contraseña | `PUT /api/auth/change-password` | Todos | Cambio obligatorio o voluntario |
| Registro | `POST /api/auth/register` | — | Registro vía invitación |

## Pasos del proceso

### 1. Selección de rol ✅ Implementado
El usuario elige "Soy Persona", "Soy Profesional" o "Soy Familia" en la pantalla de login visual (`/login`). Profesionales y admins también pueden acceder directamente por `/admin-login`.

### 2. Identificación del usuario ✅ Implementado
Para personas y familiares: escriben su nombre y el sistema los identifica, mostrando su avatar y determinando el método de login configurado.
- **Endpoint:** `POST /api/auth/identify`
- Devuelve: userId, nombre, avatar, loginMethod configurado

### 3. Autenticación según método ✅ Implementado
Dependiendo del método configurado, se muestra la interfaz correspondiente:
- **Standard:** Campo de contraseña visual
- **PIN:** Pad numérico de 4 dígitos
- **Assisted:** Pantalla de supervisor (profesional con CanSuperviseLogin autoriza)
- **Family:** Campo de contraseña del familiar vinculado

### 4. Generación de JWT ✅ Implementado
Al autenticarse correctamente, se genera un JWT con claims y un refresh token.

### 5. Redirección por rol ✅ Implementado
Cada rol redirige a su portal correspondiente:
- Admin → `/admin`
- Professional → `/pro`
- FamilyRepresentative → `/family`
- PersonWithDisability → `/app`

### 6. Refresh de token ✅ Implementado
El token se renueva automáticamente antes de expirar.
- **Endpoint:** `POST /api/auth/refresh`

### 7. Cambio de contraseña obligatorio ✅ Implementado
Si el usuario tiene contraseña temporal (`MustChangePassword = true`), se redirige a la pantalla de cambio de contraseña antes de acceder al portal.
- **Endpoint:** `PUT /api/auth/change-password`

## Claims del JWT

| Claim | Descripción |
|-------|-------------|
| `userId` | ID del usuario |
| `role` | Rol: Admin, Professional, FamilyRepresentative, PersonWithDisability |
| `permission` | Permisos del rol (múltiples claims) |
| `isGlobalAdmin` | true/false — determina acceso a funciones de admin global |
| `institutionId` | IDs de instituciones asignadas (para admins institucionales) |

## Diagrama de flujo

```mermaid
flowchart TD
    START[Usuario abre la app] --> ROUTE{¿Qué ruta?}

    ROUTE -->|/login| SEL{Selección de rol}
    ROUTE -->|/admin-login| LOGIN_E[Email + contraseña]

    SEL -->|Soy Persona| ID_P[Identificarse por nombre]
    SEL -->|Soy Profesional| LOGIN_E
    SEL -->|Soy Familia| ID_F[Identificarse por nombre]

    ID_P --> IDENTIFY[POST /api/auth/identify]
    ID_F --> IDENTIFY

    IDENTIFY --> METHOD{Método configurado}

    METHOD -->|STANDARD| STD[POST /api/auth/login/visual-standard]
    METHOD -->|PIN| PIN[POST /api/auth/login/pin]
    METHOD -->|ASSISTED| ASS[POST /api/auth/login/assisted]
    METHOD -->|FAMILY| FAM_L[POST /api/auth/login/family]

    LOGIN_E --> AUTH_E[POST /api/auth/login]

    STD --> JWT[Generar JWT + Refresh Token]
    PIN --> JWT
    ASS --> JWT
    FAM_L --> JWT
    AUTH_E --> JWT

    JWT --> MUST{¿MustChangePassword?}
    MUST -->|Sí| CHANGE[PUT /api/auth/change-password]
    MUST -->|No| REDIRECT{Redirección por rol}

    CHANGE --> REDIRECT

    REDIRECT -->|Admin| ADMIN[/admin]
    REDIRECT -->|Professional| PRO[/pro]
    REDIRECT -->|Family| FAMILY[/family]
    REDIRECT -->|Person| APP[/app]
```
