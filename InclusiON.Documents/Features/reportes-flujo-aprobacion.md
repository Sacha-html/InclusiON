# Reportes — Flujo de Aprobación

## Descripción

Los reportes de progreso siguen un flujo de revisión de tres actores antes de quedar disponibles para el familiar. El profesional genera un borrador, lo envía al admin para revisión, y el familiar solo accede a los reportes aprobados.

---

## Actores

| Actor | Rol en el flujo |
|---|---|
| **Profesional** | Crea y edita borradores. Envía al admin para revisión. Recibe notificación si es rechazado. |
| **Admin** | Revisa los reportes enviados. Aprueba o rechaza con comentario obligatorio. |
| **Familiar** | Consulta los reportes aprobados de su persona a cargo. Recibe notificación cuando uno es aprobado. |

---

## Máquina de Estados

```
                    ┌─────────────────────────────────────────┐
                    │                                         │
                    ▼                                         │
             ┌────────────┐                                   │
  POST /reports│            │                                   │
  ────────────►│   DRAFT    │  PUT /reports/{id}               │
               │  (Borrador)│◄─────────────────────────────────┘
               └─────┬──────┘  (solo editable en este estado)
                     │
                     │ PATCH /reports/{id}/submit
                     │ (Profesional → Admin)
                     ▼
               ┌─────────────┐
               │  SUBMITTED  │
               │  (Enviado)  │
               └──────┬──────┘
                      │
          ┌───────────┴────────────┐
          │                        │
          │ PATCH /approve          │ PATCH /reject
          │ (Admin aprueba)         │ (Admin rechaza + comentario)
          ▼                        ▼
   ┌─────────────┐          ┌─────────────┐
   │  APPROVED   │          │  REJECTED   │
   │  (Aprobado) │          │  (Rechazado)│
   └─────────────┘          └──────┬──────┘
                                   │
                                   │ El profesional crea
                                   │ un nuevo borrador
                                   ▼
                            POST /reports (nuevo)
                            Status: DRAFT
```

### Reglas de transición

| Desde | Hacia | Acción | Actor | Condición |
|---|---|---|---|---|
| — | `Draft` | `POST /api/reports` | Profesional | — |
| `Draft` | `Draft` | `PUT /api/reports/{id}` | Profesional | Solo el autor |
| `Draft` | `Submitted` | `PATCH /api/reports/{id}/submit` | Profesional | Solo el autor |
| `Submitted` | `Approved` | `PATCH /api/reports/{id}/approve` | Admin | — |
| `Submitted` | `Rejected` | `PATCH /api/reports/{id}/reject` | Admin | `comment` obligatorio |
| `Rejected` | — | — | — | No se puede reabrir. Se crea un nuevo `Draft` |

> **Nota:** Un reporte en estado `Submitted`, `Approved` o `Rejected` **no puede editarse**. El handler devuelve `400 InvalidOperation` si se intenta.

---

## Visibilidad por actor

| Estado | Profesional | Admin | Familiar |
|---|:---:|:---:|:---:|
| `Draft` | ✅ Ve y edita | ❌ | ❌ |
| `Submitted` | ✅ Ve (solo lectura) | ✅ Ve y decide | ❌ |
| `Approved` | ✅ Ve | ✅ Ve | ✅ Ve |
| `Rejected` | ✅ Ve (con motivo) | ✅ Ve | ❌ |

### Endpoints por actor

**Profesional** (`/api/reports` con permiso `reports:read`, `reports:create`, `reports:submit`, `reports:export`):
- `GET /api/reports` — lista sus reportes (todos los estados)
- `POST /api/reports` — crea borrador
- `PUT /api/reports/{id}` — edita borrador (solo `Draft`)
- `PATCH /api/reports/{id}/submit` — envía al admin

**Admin** (`/api/reports` con permiso `reports:approve`, `reports:reject`):
- `GET /api/reports?status=Submitted` — cola de pendientes
- `PATCH /api/reports/{id}/approve` — aprueba
- `PATCH /api/reports/{id}/reject` — rechaza con motivo

**Familiar** (`/api/reports/family` con permiso `reports:read`, `reports:export`):
- `GET /api/reports/family` — solo reportes `Approved` de sus personas a cargo
- Filtros disponibles: `dateFrom`, `dateTo`, `reportTypeId`

---

## Notificaciones por email

### 1. Reporte aprobado → Familiar

**Cuándo:** Inmediatamente después de que el admin ejecuta `PATCH /approve`.

**Destinatarios:** Todos los familiares activos (`PersonRepresentative.IsActive = true`) vinculados a la persona del reporte.

**Template:** `ReportApproved.html`

**Datos incluidos:**
- Nombre del familiar
- Nombre de la persona con discapacidad
- Título del reporte
- Tipo de reporte
- Fecha del reporte
- Nombre del profesional

**Implementación:** Fire-and-forget con `Task.Run()` — no bloquea la respuesta HTTP. Errores logueados sin relanzar.

---

### 2. Reporte rechazado → Profesional

**Cuándo:** Inmediatamente después de que el admin ejecuta `PATCH /reject`.

**Destinatario:** El profesional autor del reporte (`Report.ProfessionalId`).

**Template:** `ReportRejected.html`

**Datos incluidos:**
- Nombre del profesional
- Título del reporte
- Nombre de la persona
- Fecha del reporte
- **Motivo del rechazo** (campo `AdminComment`, destacado en rojo en el template)

**Implementación:** Fire-and-forget con `Task.Run()` — no bloquea la respuesta HTTP.

---

## Pendientes / TODO

- **Notificaciones en tiempo real:** Actualmente solo se envía email. Pendiente integrar notificaciones in-app (WebSocket / SignalR).
- **Refactorizar envío de emails:** Todos los puntos de envío tienen un `TODO` para migrar a **Microsoft.Extensions.AI / Semantic Kernel Agent Framework**, que permitirá orquestación inteligente (reintentos automáticos, múltiples canales, priorización).
- **Notificación al profesional cuando se aprueba:** Actualmente solo el familiar es notificado al aprobar. Se puede agregar también una notificación al profesional.
- **Paginación en familia:** El endpoint `GET /api/reports/family` soporta paginación pero el frontend aún no tiene la vista implementada.

---

## Archivos clave

| Capa | Archivo |
|---|---|
| Dominio | `InclusiON.Domain/Models/Report.cs` |
| Dominio | `InclusiON.Domain/Enums/ReportStatus.cs` |
| Aplicación | `UseCases/Reports/Commands/` — CreateReport, UpdateReport, SubmitReport, ApproveReport, RejectReport |
| Aplicación | `UseCases/Reports/Handlers/` — un handler por comando/query |
| Aplicación | `UseCases/Reports/Queries/` — GetReportsQuery, GetFamilyReportsQuery, GetReportByIdQuery |
| Infraestructura | `Data/Repositories/ReportsRepository.cs` |
| Infraestructura | `Templates/Emails/ReportApproved.html` |
| Infraestructura | `Templates/Emails/ReportRejected.html` |
| API | `Controllers/ReportsController.cs` |
| Migración DB | `Data/Migrations/20260414034350_AddReportApprovalWorkflow.cs` |
| Permisos | `Application/Constants/Permissions.cs` → `Reports.*` |
| Seeder | `Data/Seeders/DatabaseSeeder.cs` → permisos por rol |
