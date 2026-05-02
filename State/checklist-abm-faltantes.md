# Checklist — ABMs Faltantes (MVP)

**Última actualización:** 2026-05-01

> Todos los ítems de este checklist fueron completados. Archivo conservado como referencia histórica.

---

## Gestión de Actividades (IN-105, 107, 108, 109) — ✅ Completo

| Item | Estado |
|------|--------|
| `CreateActivityCommand` + handler + `POST /api/activities` | ✅ |
| `GetActivitiesQuery` + handler + `GET /api/activities` (paginado + filtros) | ✅ |
| `UpdateActivityCommand` + handler + `PUT /api/activities/{id}` | ✅ |
| `PatchActivityStatusCommand` + handler + `PATCH /api/activities/{id}` (baja/reactivación) | ✅ |
| Wizard FE: área → template → contenido → metadatos | ✅ |
| Lista FE con filtros + búsqueda semántica IA | ✅ |
| Edición FE | ✅ |
| Botón desactivar FE con confirm modal | ✅ |
| Players: SELECT_FIGURE, ORDER_SEQUENCE, MATCH_IMAGE_WORD, VISUAL_SUM | ✅ |
| Arquitectura dinámica: PlayerBaseComponent + registry + primitivas | ✅ |
| Embedding generación al crear/editar (fire-and-forget) | ✅ |
| Búsqueda semántica: `GET /api/activities/search?text=` + handler + toggle FE | ✅ |

---

## Asignación de Actividad (IN-118) — ✅ Completo

| Item | Estado |
|------|--------|
| `CreateActivityAssignmentCommand` + handler + `POST /api/activity-assignments` | ✅ |
| Modal asignar desde lista profesional | ✅ |
| Lista asignaciones estudiante (`/app/activities`) | ✅ |
| `GET /api/my/activity-assignments` (sin GUID, por token) | ✅ |
| Tab "Actividades" en person-detail del profesional (historial + resultados) | ✅ |

---

## Ver Resultado — ✅ Completo

| Item | Estado |
|------|--------|
| `GET /api/persons/{id}/activity-assignments` | ✅ |
| Panel historial de asignaciones en detalle persona (tab Actividades) | ✅ |
| Intentos expandibles, score %, tiempo, estado | ✅ |

---

## Plan de Trabajo / Roadmap (IN-110 a IN-115, IN-117) — ✅ Completo

| Item | Jira | Estado |
|------|------|--------|
| Crear roadmap por persona | IN-110 | ✅ |
| Agregar actividades al roadmap por área | IN-111 | ✅ |
| Orden secuencial y umbral de desbloqueo | IN-112 | ✅ |
| Reordenamiento drag-drop | IN-113 | ✅ |
| Desbloqueo manual de actividad | IN-114 | ✅ |
| Eliminación de actividad del roadmap | IN-115 | ✅ |
| Visualización roadmap (vista estudiante, estilo Duolingo) | IN-117 | ✅ |
| Auto-unlock por umbral de rendimiento | IN-127 | ✅ |
