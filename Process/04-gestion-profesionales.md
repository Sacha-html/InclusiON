# Proceso 04 — Gestión de Profesionales

**Área:** Gestión de Usuarios

## Descripción
Proceso de alta, edición y desactivación de profesionales en la plataforma. El profesional es el actor principal del sistema: evalúa personas con discapacidad, crea planes de trabajo, asigna actividades y monitorea el progreso. El admin (global o institucional) gestiona el ciclo de vida del profesional.

## Participantes
- **Admin Global** — CRUD completo de profesionales
- **Admin Institucional** — CRUD de profesionales dentro de su institución

## Pasos del proceso

### 1. Alta de Profesional ✅ Implementado
El admin crea el profesional con datos personales y credenciales. Se genera contraseña temporal que el profesional debe cambiar en su primer login.
- **Endpoint:** `POST /api/professionals`
- **Transacción:** Crea `User` + `Professional` en la misma operación
- **Frontend:** `/admin/professionals` (formulario)

### 2. Consulta de Profesionales ✅ Implementado
Listado paginado con búsqueda, filtrado por especialidad, institución y estado activo/inactivo.
- **Listado:** `GET /api/professionals` (paginado)
- **Detalle:** `GET /api/professionals/{id}`
- **Mi perfil:** `GET /api/professionals/me`
- **Frontend:** `/admin/professionals` (DataTable)

### 3. Edición de Profesional ✅ Implementado
El admin modifica los datos personales del profesional.
- **Endpoint:** `PUT /api/professionals/{id}`
- **Frontend:** `/admin/professionals/{id}` (formulario de edición)

### 4. Desactivación de Profesional ✅ Implementado
El admin desactiva al profesional (soft-delete). No se eliminan datos históricos.
- **Endpoint:** `PUT /api/professionals/{id}/deactivate`

## Diagrama de flujo

```mermaid
flowchart TD
    ADMIN[Admin] -->|POST /api/professionals| ALTA[Alta de Profesional]
    ALTA -->|Genera contraseña temporal| USER[User + Professional]

    ADMIN -->|GET /api/professionals| LIST[Listado paginado]
    LIST -->|Búsqueda, filtros| FILTRO[Filtrado por inst./especialidad/estado]

    ADMIN -->|PUT /api/professionals/id| EDIT[Editar datos]
    ADMIN -->|PUT /api/professionals/id/deactivate| DEACT[Desactivar]
    DEACT -->|IsActive = false| SOFT[Soft-delete]
```

## Estado resumen

| Paso | Estado |
|------|--------|
| Alta de profesional | ✅ Implementado |
| Consulta de profesionales | ✅ Implementado |
| Edición de profesional | ✅ Implementado |
| Desactivación | ✅ Implementado |
