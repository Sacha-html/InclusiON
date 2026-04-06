# Sprint 4 — Evaluación, Diagnóstico y Dashboard (IN-81 a IN-104)

**Período:** 

**Objetivo:** Diagnósticos, dashboard profesional y gestión admin

---

## Tareas

| Código | Task | Backend | Frontend | Estado |
|--------|------|---------|----------|--------|
| IN-81 | Configuración del perfil de habilidades | CatalogsController GET /skill-areas | persons service | ✅ |
| IN-82 | Edición del perfil funcional | PUT /api/persons/{id} | persons/detail | ✅ |
| IN-83 | Registro de diagnóstico funcional | POST /api/persons/{id}/diagnoses | diagnoses service | ✅ |
| IN-84 | Consulta de historial de diagnósticos | GET /api/persons/{id}/diagnoses | diagnoses service | ✅ |
| IN-85 | Edición de diagnóstico por su creador | PUT /api/diagnoses/{id} | diagnoses service | ✅ |
| IN-86 | Timeline de diagnósticos en perfil de persona | - | - | ⏳ PENDIENTE |
| IN-87 | Dashboard del profesional con contadores reales | AssignmentsService | pro-dashboard | ✅ |
| IN-88 | Mi Aula (cards personas asignadas) | AssignmentsService | classroom component | ✅ |
| IN-89 | Detalle de persona con edición inline | GET /api/persons/{id} | persons/detail | ✅ (solo lectura) |
| IN-90 | Radar chart de habilidades | - | - | ⏳ PENDIENTE |
| IN-91 | Dashboard familiar | - | - | ⏳ PENDIENTE |
| IN-92 | Portal familia con progreso completo | - | - | ⏳ PENDIENTE |
| IN-93 | Listado centralizado de usuarios | GET /api/admin/users | admin-users | ✅ |
| IN-94 | Detalle de usuario con entidad asociada | GET /api/admin/users/{id} | admin-users detail | ✅ |
| IN-95 | Reseteo de contraseña | PUT /api/admin/users/{id}/reset-password | admin-users | ✅ |
| IN-96 | Desactivación de cuenta | PUT /api/admin/users/{id}/deactivate | admin-users | ✅ |
| IN-97 | Reactivación de cuenta | PUT /api/admin/users/{id}/reactivate | admin-users | ✅ |
| IN-98 | Consulta de actividad reciente del usuario | - | - | ⏳ PENDIENTE |
| IN-99 | Wizard de completado de perfil (profesional) | - | - | ⏳ PENDIENTE |
| IN-100 | Tour guiado del portal (profesional) | - | - | ⏳ PENDIENTE |
| IN-101 | Pantalla de bienvenida (familiar) | - | - | ⏳ PENDIENTE |
| IN-102 | Pantalla de bienvenida (persona) | - | - | ⏳ PENDIENTE |
| IN-103 | Consulta de tipos de template | GET /api/catalogs/activity-template-types | catalogs service | ✅ |
| IN-104 | Consulta de categorías de actividad | GET /api/catalogs/activity-categories | catalogs service | ✅ |

---

## Resumen

| Métrica | Valor |
|---------|-------|
| Total tareas | 24 |
| Completadas | 15 |
| Pendientes | 9 |

---

## Pendientes a Sprint 7

- IN-86, IN-90, IN-91, IN-92, IN-98, IN-99, IN-100, IN-101, IN-102

---

## Épicas padre

- **IN-7:** Evaluación, Diagnóstico y Dashboard
- **IN-8:** Administración de Cuentas y Onboarding
- **IN-9:** Gestión de Actividades (parcial)