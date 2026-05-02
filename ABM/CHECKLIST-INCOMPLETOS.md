# IN-185 — Reparar ABMs incompletos

**Jira:** IN-185  
**Rama:** `feature/IN-185-reparar-abms`  
**Objetivo:** completar los ABMs parcialmente implementados — cada entidad con Alta + Baja lógica + Modificación + Listado con persistencia real.

---

## ABM 01 — Instituciones

### Backend (`InclusiON.Server`)
- [x] `PATCH /api/institutions/{id}` — máquina de estados (`{ isActive: bool }`, valida no-op + integridad en baja)
- [x] Tests unitarios `PatchInstitutionStatusCommandHandlerTests` (10 casos)

### Frontend (`InclusiON.Client`)
- [x] `institutions/detail.component` — detalle de institución (admins asignados, profesionales, datos)
- [x] Botón "Dar de baja" en `institutions/detail.component` con confirmación (409 del BE surfacea mensaje directo)

---

## ABM 02 — Administradores ✅

### Backend
- [x] `PUT /api/admin/users/{userId}` — editar nombre, apellido, email (`AdminUpdateUserCommand`)

### Frontend
- [x] `admin-users/edit/edit.component` — formulario reactivo con nombre, apellido, email
- [x] Botón "Editar" en `admin-users.component` (visible solo para el propio usuario)

---

## ABM 03 — Catálogos

### Backend — endpoints de estado (PATCH con máquina de estados)
- [x] `PATCH /api/admin/catalogs/disability-types/{id}` — baja + reactivación con no-op check
- [x] `PATCH /api/admin/catalogs/autonomy-levels/{id}`
- [x] `PATCH /api/admin/catalogs/activity-categories/{id}`
- [x] `PATCH /api/admin/catalogs/skill-areas/{id}`
- [x] `PATCH /api/admin/catalogs/activity-template-types/{id}`
- [x] `PATCH /api/admin/catalogs/report-types/{id}`

### Backend — validaciones de integridad en baja
- [x] `DisabilityType`: rechazar si hay `PersonWithDisability` con ese tipo
- [x] `ActivityCategory`: rechazar si hay `Activity` con esa categoría
- [x] `SkillArea`: rechazar si hay `PersonSkillProfile` activos o `ActivityTemplateType` activos asociados
- [x] `ActivityTemplateType`: rechazar si hay `ActivityContent` asociados
- [x] `ReportType`: rechazar si hay `Report` asociados
- [x] Tests unitarios `CatalogAdminControllerPatchStatusTests` (13 casos — DisabilityType + AutonomyLevel representativos)

### Frontend
- [x] Botón "Dar de baja" + modal de confirmación en `catalogs.component` (5 tipos con deactivate)
- [x] Error del 409 surfacea el mensaje del backend directamente en el toast

---

## ABM 11 — Diagnósticos ✅

### Backend
- [x] `PATCH /api/diagnoses/{id}` — máquina de estados (`{ isActive: bool }`, valida no-op + autoría)
- [x] `PatchDiagnosisStatusCommandHandler` con verificación de profesional creador
- [x] Tests unitarios `PatchDiagnosisStatusCommandHandlerTests`

### Frontend
- [x] Botón "Dar de baja" en `professional-diagnoses.component` — solo visible para el creador, con `ConfirmModalComponent`
- [x] Botón "Dar de baja" en `admin-diagnoses.component` — visible para todos los diagnósticos
- [x] Fix TS2322: `date` pipe devolvía `string | null` en `[itemName]` → corregido con `?? ''`

---

## ABM 12 — Reportes ✅

### Backend
- [x] `PUT /api/reports/{id}/deactivate` — baja lógica con validaciones (no permite baja en estado Enviado, verifica autoría)

### Frontend
- [x] `pro/reports/edit.component` — formulario completo, valida que el reporte sea Borrador o Rechazado antes de mostrar
- [x] Botón "Editar" en `pro/reports/detail.component` (visible solo si Draft o Rejected)
- [x] Botón "Dar de baja" en `pro/reports/detail.component` (visible si no está Enviado, con modal de confirmación, surfacea mensaje del backend)

---

## Resumen

| # | Tarea | ABM | Layer |
|---|-------|-----|-------|
| 1 | ~~`PATCH /institutions/{id}` (máquina de estados)~~ ✅ | 01 | BE |
| 2 | ~~Tests `PatchInstitutionStatusCommandHandlerTests`~~ ✅ | 01 | BE |
| 3 | ~~`institutions/detail.component`~~ ✅ | 01 | FE |
| 4 | ~~Botón baja instituciones~~ ✅ | 01 | FE |
| 5 | ~~`PUT /admin/users/{id}` editar admin~~ ✅ | 02 | BE |
| 6 | ~~`admins/edit.component`~~ ✅ | 02 | FE |
| 7 | ~~Endpoints PATCH × 6 catálogos~~ ✅ | 03 | BE |
| 8 | ~~Validaciones de integridad × 5 catálogos~~ ✅ | 03 | BE |
| 9 | ~~Tests `CatalogAdminControllerPatchStatusTests`~~ ✅ | 03 | BE |
| 10 | ~~Toggle activo/inactivo en catálogos~~ ✅ | 03 | FE |
| 11 | ~~`PATCH /diagnoses/{id}`~~ ✅ | 11 | BE |
| 12 | ~~Botón baja diagnósticos (2 componentes)~~ ✅ | 11 | FE |
| 13 | ~~`PUT /reports/{id}/deactivate`~~ ✅ | 12 | BE |
| 14 | ~~`pro/reports/edit.component`~~ ✅ | 12 | FE |
| 15 | ~~Botón baja + botón editar reportes~~ ✅ | 12 | FE |

**Total: 15 tareas — 15 completadas ✅ — 0 pendientes** 🎉
