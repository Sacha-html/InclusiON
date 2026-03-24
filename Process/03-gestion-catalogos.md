# Proceso 03 — Gestión de Catálogos

**Área:** Configuración del Sistema

## Descripción
Proceso de administración de las tablas de referencia del sistema que alimentan formularios, dropdowns y configuraciones. Los catálogos son gestionados exclusivamente por el admin global y consultados por todos los roles autenticados.

## Participantes
- **Admin Global** — CRUD completo de catálogos (protegido por policy `global-admin`)
- **Admin Institucional** — Solo lectura
- **Profesional** — Solo lectura (usa los catálogos en formularios)

## Catálogos del sistema ✅ Implementado

| Catálogo | Endpoint lectura | Endpoint escritura | Uso en el sistema |
|----------|-----------------|-------------------|-------------------|
| Tipos de Discapacidad | `GET /api/catalogs/disability-types` | `POST/PUT /api/admin/catalogs/disability-types` | Alta de persona |
| Niveles de Autonomía | `GET /api/catalogs/autonomy-levels` | `POST/PUT /api/admin/catalogs/autonomy-levels` | Configuración de login |
| Categorías de Actividad | `GET /api/catalogs/activity-categories` | `POST/PUT /api/admin/catalogs/activity-categories` | Clasificación de actividades |
| Áreas de Habilidad | `GET /api/catalogs/skill-areas` | `POST/PUT /api/admin/catalogs/skill-areas` | Skill profile, roadmap |
| Tipos de Template | `GET /api/catalogs/activity-template-types` | `POST/PUT /api/admin/catalogs/activity-template-types` | Formulario dinámico de actividad |
| Métodos de Login | `GET /api/catalogs/login-methods` | `PUT /api/admin/catalogs/login-methods/{id}` | Configuración de acceso |

## Pasos del proceso

### 1. Consulta de Catálogos ✅ Implementado
Cualquier usuario autenticado consulta los catálogos para llenar dropdowns.
- **Controlador:** `CatalogsController`
- **6 endpoints GET** listados en la tabla anterior

### 2. Alta de Item ✅ Implementado
El admin global crea nuevos items desde el panel admin.
- **Controlador:** `CatalogAdminController`
- **Endpoint:** `POST /api/admin/catalogs/{tipo}`
- **Validación:** Nombre único por tipo de catálogo
- **Frontend:** `/admin/catalogs/{tipo}` (modal de creación)

### 3. Edición de Item ✅ Implementado
El admin global edita items existentes. Se valida nombre único.
- **Endpoint:** `PUT /api/admin/catalogs/{tipo}/{id}`
- **Frontend:** `/admin/catalogs/{tipo}` (modal de edición)

## Frontend — Sidebar Admin

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
    TIPO --> LIST[DataTable: lista de items]

    LIST -->|Botón Nuevo| MODAL_NEW[Modal: crear item]
    LIST -->|Botón Editar| MODAL_EDIT[Modal: editar item]

    MODAL_NEW -->|POST /api/admin/catalogs/tipo| API[CatalogAdminController]
    MODAL_EDIT -->|PUT /api/admin/catalogs/tipo/id| API

    API -->|Valida nombre único| CHECK{¿Duplicado?}
    CHECK -->|No| SAVE[Guardar]
    CHECK -->|Sí| ERR[Error 409]

    subgraph Lectura — todos los roles
        ANY[Usuario autenticado] -->|GET /api/catalogs/tipo| READ[CatalogsController]
    end
```

## Estado resumen

| Paso | Estado |
|------|--------|
| Consulta de catálogos | ✅ Implementado |
| Alta de item | ✅ Implementado |
| Edición de item | ✅ Implementado |
