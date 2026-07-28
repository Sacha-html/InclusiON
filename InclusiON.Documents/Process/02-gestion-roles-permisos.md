# Proceso 02 — Gestión de Roles y Permisos

**Área:** Configuración del Sistema

## Descripción
Proceso de configuración de los roles del sistema y asignación de permisos a cada rol. Los permisos se agrupan por módulo y controlan el acceso a funcionalidades específicas de la plataforma. Solo el admin global puede modificar permisos; los roles están predefinidos (Admin, Professional, FamilyRepresentative, PersonWithDisability).

## Participantes
- **Admin Global** — Consulta roles y asigna permisos

## Pasos del proceso

### 1. Consulta de Roles
El admin global visualiza los roles del sistema con sus permisos actuales.
- **Listar roles:** `GET /api/roles`
- **Detalle de rol:** `GET /api/roles/{id}`
- **Permisos disponibles:** `GET /api/roles/available-permissions`
- **Frontend:** `/admin/roles` (DataTable con roles)

### 2. Asignación de Permisos
El admin global selecciona permisos mediante checkboxes agrupados por módulo y los asigna al rol.
- **Endpoint:** `PUT /api/roles/{id}/permissions`
- **Protección:** `[Authorize(Policy = "global-admin")]`
- **Frontend:** `/admin/roles/{id}` (checkboxes por módulo)

### 3. Jerarquía Admin Global / Institucional

| Capacidad | Admin Global | Admin Institucional |
|-----------|-------------|-------------------|

### 4. Creación de Admins Institucionales
El admin global crea usuarios admin y los vincula a instituciones. Se genera contraseña temporal.
- **Crear admin:** `POST /api/admin/institutions-assignments/users`
- **Asignar institución:** `POST /api/admin/institutions-assignments/{adminUserId}`
- **Desasignar:** `DELETE /api/admin/institutions-assignments/{adminUserId}/{instId}`
- **Listar admins:** `GET /api/admin/institutions-assignments/admins`
- **Mis instituciones:** `GET /api/admin/institutions-assignments/me`
- **Frontend:** `/admin/admins`

### 5. Autorización por Recurso — Capa 3 (HU-IN-172)

Más allá de los permisos del rol, el acceso a entidades sensibles requiere un **vínculo explícito** entre el usuario y el recurso solicitado. Esta es la tercera capa de seguridad, aplicada después de verificar JWT y política de permiso.

#### Fuentes de verdad por rol

| Rol | Tabla de vínculo | Condición |
|-----|-----------------|-----------|
| Professional | `ProfessionalPersons` | `ProfessionalId` + `PersonId` + `IsActive = true` |
| FamilyRepresentative | `PersonRepresentatives` | `RepresentativeId` + `PersonId` + `IsActive = true` |
| Admin Institucional | `AdminInstitutions` | `AdminUserId` + `InstitutionId` |
| GlobalAdmin | — | Bypass total (siempre permitido, pero auditado) |

#### Entidades sensibles en scope

`PersonWithDisability`, `PersonSkillProfile`, `Diagnosis`, `Report`, `ActivityResponse`, `PersonRoadmap`, `ActivityAssignment`, `Invitation`, `User` (consulta de terceros)

#### Implementación técnica

- **Interfaz:** `IResourceAuthorizationService` (Application)
- **Implementación:** `ResourceAuthorizationService` (Infrastructure) — inyectado vía DI
- **Filtros declarativos** en controllers (evitan lógica repetida en handlers):
  - `[PersonAccess(AccessMode.Read/Write)]` — lee `{personId}` de la ruta
  - `[DiagnosisAccess(AccessMode.Read/Write)]` — lee `{id}`, resuelve PersonId del diagnóstico
  - `[ReportAccess(AccessMode.Read/Write)]` — lee `{reportId}`, resuelve PersonId del reporte
- **Listados:** `GetAccessiblePersonIdsAsync()` filtra en el repositorio (no post-filtrado en memoria)
- **Cache por request:** el resultado de `ProfessionalAssignments` se cachea en el scope del request para evitar consultas repetidas

#### Política de respuesta

- Roles internos (Professional, Admin) → **403 Forbidden**
- Roles externos (FamilyRepresentative, PersonWithDisability) → **404 Not Found** (oculta existencia)

Ver detalle completo en [References/REF-autenticacion.md](../References/REF-autenticacion.md).

### 6. Filtrado por Institución
Los admins institucionales solo ven datos de sus instituciones asignadas.
```
Admin Institucional
  → AdminInstitution (sus instituciones)
    → ProfessionalInstitution (profesionales de esas instituciones)
      → ProfessionalPerson (personas de esos profesionales)
        → PersonRepresentative (familiares de esas personas)
        → Invitation (invitaciones de esos profesionales)
```

## Diagrama de flujo

```mermaid
flowchart TD
    AG[Admin Global] -->|GET /api/roles| ROLES[Consultar Roles]
    ROLES -->|PUT /api/roles/id/permissions| PERM[Asignar Permisos]
    PERM -->|Checkboxes por módulo| SAVE[Guardar configuración]

    AG -->|POST /api/admin/.../users| AI[Crear Admin Institucional]
    AI -->|POST /api/admin/.../adminUserId| VINC[Asignar a Institución]
    VINC -->|Determina alcance| FILTRO[Filtrado por Institución]

    subgraph Protección JWT
        JWT[JWT Token] -->|Claim| GLA[isGlobalAdmin]
        GLA -->|Policy| POL[global-admin]
    end
```


