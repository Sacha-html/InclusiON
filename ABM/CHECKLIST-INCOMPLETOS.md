# IN-185 — Reparar ABMs incompletos

**Jira:** IN-185  
**Rama:** `feature/IN-185-reparar-abms`  
**Objetivo:** completar los ABMs parcialmente implementados — cada entidad con Alta + Baja lógica + Modificación + Listado con persistencia real.

---

## ABM 01 — Instituciones

### Backend (`InclusiON.Server`)
- [ ] `PUT /api/institutions/{id}/deactivate` — baja lógica (`Activo = false`)

### Frontend (`InclusiON.Client`)
- [ ] `institutions/detail.component` — detalle de institución (admins asignados, profesionales, datos)
- [ ] Botón "Dar de baja" en `institutions/list.component` con confirmación

---

## ABM 02 — Administradores

### Backend
- [ ] `PUT /api/admin/users/{userId}` — editar nombre, apellido y email del administrador

### Frontend
- [ ] `admins/edit.component` (o modal de edición) — formulario reactivo con nombre, apellido, email
- [ ] Botón "Editar" en `admin-users.component`

---

## ABM 03 — Catálogos

### Backend — endpoints de baja lógica
- [ ] `PUT /api/catalogs/admin/disability-types/{id}/deactivate`
- [ ] `PUT /api/catalogs/admin/autonomy-levels/{id}/deactivate`
- [ ] `PUT /api/catalogs/admin/activity-categories/{id}/deactivate`
- [ ] `PUT /api/catalogs/admin/skill-areas/{id}/deactivate`
- [ ] `PUT /api/catalogs/admin/activity-template-types/{id}/deactivate`
- [ ] `PUT /api/catalogs/admin/report-types/{id}/deactivate`

### Backend — validaciones de integridad en baja
- [ ] `DisabilityType`: rechazar si hay `PersonWithDisability` activos con ese tipo
- [ ] `ActivityCategory`: rechazar si hay `Activity` activas con esa categoría
- [ ] `SkillArea`: rechazar si hay `PersonSkillProfile` activos o `ActivityTemplateType` activos asociados
- [ ] `ActivityTemplateType`: rechazar si hay `ActivityContent` activos asociados
- [ ] `ReportType`: rechazar si hay `Report` activos asociados

### Frontend
- [ ] Toggle activo/inactivo en `catalogs.component` (aplica a todos los tipos)
- [ ] Confirmación de baja mostrando cuántos registros quedarían afectados

---

## ABM 11 — Diagnósticos

### Backend
- [ ] `PUT /api/diagnoses/{id}/deactivate` — baja lógica

### Frontend
- [ ] Botón "Dar de baja" en `professional-diagnoses.component` con confirmación
- [ ] Botón "Dar de baja" en `admin-diagnoses.component` con confirmación

---

## ABM 12 — Reportes

### Backend
- [ ] `PUT /api/reports/{id}/deactivate` — baja lógica

### Frontend
- [ ] `pro/reports/edit.component` — formulario reactivo para editar reporte en estado `Borrador` o `Rechazado`
- [ ] Botón "Editar" en `pro/reports/detail.component` (visible solo si el estado lo permite)
- [ ] Botón "Dar de baja" en `pro/reports/detail.component` (con advertencia si ya fue leído por el familiar)

---

## Resumen

| # | Tarea | ABM | Layer |
|---|-------|-----|-------|
| 1 | `PUT /institutions/{id}/deactivate` | 01 | BE |
| 2 | `institutions/detail.component` | 01 | FE |
| 3 | Botón baja instituciones | 01 | FE |
| 4 | `PUT /admin/users/{id}` editar admin | 02 | BE |
| 5 | `admins/edit.component` | 02 | FE |
| 6 | Endpoints deactivate × 6 catálogos | 03 | BE |
| 7 | Validaciones de integridad × 5 catálogos | 03 | BE |
| 8 | Toggle activo/inactivo en catálogos | 03 | FE |
| 9 | `PUT /diagnoses/{id}/deactivate` | 11 | BE |
| 10 | Botón baja diagnósticos (2 componentes) | 11 | FE |
| 11 | `PUT /reports/{id}/deactivate` | 12 | BE |
| 12 | `pro/reports/edit.component` | 12 | FE |
| 13 | Botón baja + botón editar reportes | 12 | FE |

**Total: 13 tareas**
