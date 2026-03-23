# Proceso 09 — Gestión de Catálogos

**Origen:** Implementación del sistema (no definido en proyecto final original)

## Descripción

Administración de las tablas de referencia del sistema que alimentan los formularios y configuraciones. Los catálogos son gestionados exclusivamente por el admin global y consultados por todos los roles autenticados. Todo el proceso está completamente implementado.

## Participantes

- **Admin Global** — CRUD completo de catálogos (protegido por policy `global-admin`)
- **Admin Institucional** — Solo lectura
- **Profesional** — Solo lectura (usa los catálogos en formularios)

## Catálogos del sistema ✅ Implementado

| Catálogo | Endpoint lectura | Endpoint escritura | Uso |
|----------|-----------------|-------------------|-----|
| Tipos de Discapacidad | `GET /api/catalogs/disability-types` | `POST/PUT /api/admin/catalogs/disability-types` | Alta de persona |
| Niveles de Autonomía | `GET /api/catalogs/autonomy-levels` | `POST/PUT /api/admin/catalogs/autonomy-levels` | Configuración de login |
| Categorías de Actividad | `GET /api/catalogs/activity-categories` | `POST/PUT /api/admin/catalogs/activity-categories` | Clasificación de actividades |
| Áreas de Habilidad | `GET /api/catalogs/skill-areas` | `POST/PUT /api/admin/catalogs/skill-areas` | Skill profile, roadmap |
| Tipos de Template | `GET /api/catalogs/activity-template-types` | `POST/PUT /api/admin/catalogs/activity-template-types` | Formulario dinámico de actividad |
| Métodos de Login | `GET /api/catalogs/login-methods` | `PUT /api/admin/catalogs/login-methods/{id}` | Configuración de acceso |

## Pasos del proceso

### 1. Consulta de catálogos (todos los roles) ✅ Implementado
Los catálogos se consultan desde `CatalogsController`. Cualquier usuario autenticado puede leerlos.
- **Controlador:** `CatalogsController`
- **6 endpoints GET** listados en la tabla anterior

### 2. Alta de item en catálogo (solo admin global) ✅ Implementado
El admin global crea nuevos items desde el panel admin (`/admin/catalogs/{tipo}`).
- **Controlador:** `CatalogAdminController`
- **Protección:** `[Authorize(Policy = "global-admin")]`
- Se valida nombre único por tipo de catálogo.

### 3. Edición de item en catálogo (solo admin global) ✅ Implementado
El admin global edita items existentes. Se valida nombre único.
- **Endpoint:** `PUT /api/admin/catalogs/{tipo}/{id}`

### 4. Uso en formularios ✅ Implementado
Los catálogos alimentan dropdowns en:
- Alta/edición de persona: tipo de discapacidad, nivel de autonomía, método de login
- Perfil de habilidades: áreas de habilidad
- ⏳ Pendiente: Creación de actividades usará categorías y tipos de template (BE-06)

## Estructura del sidebar admin

```
Catálogos ▼
  ├── Tipos de Discapacidad     /admin/catalogs/disability-types
  ├── Niveles de Autonomía      /admin/catalogs/autonomy-levels
  ├── Categorías de Actividad   /admin/catalogs/activity-categories
  ├── Áreas de Habilidad        /admin/catalogs/skill-areas
  ├── Tipos de Template         /admin/catalogs/template-types
  └── Métodos de Login          /admin/catalogs/login-methods
```

## Diagrama de flujo

```mermaid
flowchart TD
    AG[Admin Global] -->|Accede a| SIDEBAR[Sidebar: Catálogos]
    SIDEBAR --> TIPO{Tipo de catálogo}

    TIPO --> DT[Tipos de Discapacidad]
    TIPO --> AL[Niveles de Autonomía]
    TIPO --> AC[Categorías de Actividad]
    TIPO --> SA[Áreas de Habilidad]
    TIPO --> TT[Tipos de Template]
    TIPO --> LM[Métodos de Login]

    DT --> LIST[DataTable: lista de items]
    AL --> LIST
    AC --> LIST
    SA --> LIST
    TT --> LIST
    LM --> LIST

    LIST -->|Botón Nuevo| MODAL_NEW[Modal: crear item]
    LIST -->|Botón Editar| MODAL_EDIT[Modal: editar item]

    MODAL_NEW -->|POST /api/admin/catalogs/tipo| API[CatalogAdminController]
    MODAL_EDIT -->|PUT /api/admin/catalogs/tipo/id| API

    API -->|Valida nombre único| CHECK{¿Duplicado?}
    CHECK -->|No| SAVE[Guardar]
    CHECK -->|Sí| ERR[Error 409: nombre duplicado]

    SAVE --> LIST

    subgraph Protección
        POLICY[Policy: global-admin]
        POLICY -->|Bloquea| AI[Admin Institucional — solo lectura]
        POLICY -->|Permite| AG
    end

    subgraph Lectura — todos los roles
        ANY[Usuario autenticado] -->|GET /api/catalogs/tipo| READ[CatalogsController]
        READ -->|Devuelve| ITEMS[Lista de items]
    end
```
