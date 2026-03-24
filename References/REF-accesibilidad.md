# Proceso 07 — Accesibilidad

**Origen:** Implementación del sistema (derivado del alcance "Accesibilidad y usabilidad" del proyecto final + documentación CIF)

## Descripción
Sistema de accesibilidad implementado que permite a la plataforma ser utilizable por personas con diferentes tipos de discapacidad. Incluye 7 perfiles de accesibilidad, modo claro/oscuro, métodos de login adaptativo y directivas Angular para control de permisos en la UI.

## Perfiles de accesibilidad ✅ Implementado

| Perfil | Descripción | Ajustes principales |
|--------|-------------|---------------------|
| Default | Perfil estándar | Base WCAG AA |
| High-contrast | Máximo contraste | Colores puros (blanco/negro/amarillo) |
| Dyslexia | Lectura fácil | Fuente Lexend, colores Solarized, espaciado amplio |
| Low-vision | Visión reducida | Fuente 20px, bordes gruesos |
| Deuteranopia | Daltonismo rojo-verde (6% hombres) | Azules y naranjas en vez de verde/rojo |
| Protanopia | Sensibilidad roja reducida | Azules y amarillos |
| Tritanopia | Daltonismo azul-amarillo | Rojos/magentas y cianes |

Cada perfil tiene variante **Light** y **Dark** = **14 combinaciones** totales.

## Implementación técnica

### Variables CSS ✅ Implementado
- `--a11y-*` — Variables propias del sistema (background, text, primary, success, danger, warning, info)
- `--cui-*` — Variables de CoreUI sobreescritas por perfil (badges, botones, alerts, form controls)
- Aplicadas vía atributos `data-profile` y `data-color-mode` en el elemento `<html>`

### Directivas Angular ✅ Implementado
- `*appHasPermission="'permission:action'"` — Muestra elemento solo si el usuario tiene el permiso
- `*appIfGlobalAdmin` — Muestra solo para admin global
- `*appIfInstitutionalAdmin` — Muestra solo para admin institucional

### Guards de ruta ✅ Implementado
- `authGuard` — Requiere autenticación
- `roleGuard` — Verifica rol requerido por la ruta
- `guestGuard` — Solo usuarios no autenticados (login pages)
- `globalAdminGuard` — Solo admin global
- `permissionGuard` — Verifica permiso específico de la ruta

### Componentes UI reutilizables ✅ Implementado
- **DataTable** — Componente con título, botones en header, búsqueda con debounce, paginación
- **Toasts** — Notificaciones con colores e iconos según tipo (success, error, warning, info)

## Métodos de login adaptativo ✅ Implementado

| Método | Nivel de autonomía | Endpoint | Descripción |
|--------|-------------------|----------|-------------|
| Standard | Alta | `POST /api/auth/login/visual-standard` | Contraseña visual tras identificación por nombre |
| PIN | Media | `POST /api/auth/login/pin` | PIN numérico de 4 dígitos con pad visual |
| Assisted | Baja | `POST /api/auth/login/assisted` | Supervisor autoriza el login |
| Family | N/A | `POST /api/auth/login/family` | Contraseña del familiar vinculado |

El método de login se configura por persona:
- **Endpoint:** `PUT /api/persons/{id}/login-method` o `PUT /api/persons/me/login-method`
- Los métodos disponibles se obtienen del catálogo: `GET /api/catalogs/login-methods`

## Rutas del frontend por portal

| Portal | Ruta base | Guard | Rol |
|--------|-----------|-------|-----|
| Admin | `/admin/*` | authGuard + roleGuard | Admin |
| Profesional | `/pro/*` | authGuard + roleGuard | Professional |
| Familia | `/family/*` | authGuard + roleGuard | FamilyRepresentative |
| AAC (persona) | `/app/*` | authGuard + roleGuard | PersonWithDisability |
| Login visual | `/login` | guestGuard | — |
| Login admin | `/admin-login` | guestGuard | — |
| Invitación | `/invite/:code` | (público) | — |

## Diagrama de flujo

```mermaid
flowchart TD
    USER[Usuario] -->|Abre app| SEL{Selección de Rol}

    SEL -->|Soy Persona| ID[POST /api/auth/identify]
    SEL -->|Soy Profesional| LOGIN_P[/admin-login: Email + Contraseña]
    SEL -->|Soy Familia| ID

    ID -->|Identifica usuario| METHOD{Método de login}

    METHOD -->|Standard| STD[Contraseña visual]
    METHOD -->|PIN| PIN[PIN 4 dígitos]
    METHOD -->|Assisted| ASS[Supervisor autoriza]
    METHOD -->|Family| FAM[Contraseña familiar]

    STD -->|OK| DASH_P[Portal Persona /app]
    PIN -->|OK| DASH_P
    ASS -->|OK| DASH_P
    FAM -->|OK| DASH_F[Portal Familia /family]
    LOGIN_P -->|OK| DASH_PR[Portal Profesional /pro]

    subgraph Accesibilidad Visual ✅
        PROFILE[7 perfiles × 2 modos = 14 combinaciones]
        PROFILE -->|data-profile| CSS[Variables --a11y-* y --cui-*]
    end

    subgraph Control de Acceso ✅
        GUARDS[authGuard, roleGuard, guestGuard]
        DIRECTIVES[*appHasPermission, *appIfGlobalAdmin]
    end
```
