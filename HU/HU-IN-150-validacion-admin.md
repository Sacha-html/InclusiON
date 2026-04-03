# HU IN-150 — Selección Institucional y Validación por Administrador

**Proceso relacionado:** 04
**Prioridad:** Alta
**Estado:** ✅ Completada

---

## Historia de Usuario

**Como Profesional**, quiero seleccionar la institución a la que pertenezco durante mi registro, para que los responsables de dicha entidad puedan verificar mi identidad y otorgarme el alta definitiva.

**Como Administrador de Institución**, quiero visualizar una lista de solicitudes de registro Pending (pendientes), para validar los datos del profesional y activar su acceso al sistema o rechazar la solicitud si los datos son incorrectos.

---

## Descripción funcional

El profesional selecciona su institución al registrarse. El admin institucional ve solo las solicitudes pendientes de su institución en el tab "Validaciones". Puede aprobar (crea usuario activo, envía credenciales) o rechazar (con motivo, notifica al solicitante).

---

## Criterios de Aceptación

- [x] El profesional puede seleccionar institución opcional en el registro
- [x] Si no selecciona institución, un GlobalAdmin puede validar la solicitud
- [x] Admin institucional ve solo solicitudes pendientes de su institución
- [x] GlobalAdmin ve todas las solicitudes pendientes
- [x] Tab "Activos" muestra profesionales aprobados con filtro por estado (Activos, Suspendidos, Dados de baja)
- [x] Tab "Validaciones" muestra solicitudes pendientes con badge contador
- [x] Al aprobar: se activa el User, se envía email con credenciales temporales
- [x] Al rechazar: se desactiva la relación con la institución, se envía email con motivo
- [x] El admin puede desactivar profesionales activos (con motivo obligatorio)
- [x] El admin puede reactivar profesionales dados de baja
- [x] Historial de estados disponible para cada profesional
- [x] Ordenamiento por columnas en ambas tablas (backend)
- [x] Exportar a CSV desde la tabla de activos
- [x] Envío de emails en background (no bloquea la respuesta del API)

---

## Endpoints

| Método | Endpoint | Auth | Descripción |
|--------|----------|------|-------------|
| GET | `/api/Professionals` | `professionals:read` | Listado paginado de aprobados |
| GET | `/api/Professionals/pending` | `professionals:read` | Listado de pendientes |
| PUT | `/api/Professionals/{id}/validate` | `professionals:update` | Aprobar/rechazar solicitud |
| PUT | `/api/Professionals/{id}/deactivate` | `professionals:update` | Desactivar profesional |
| PUT | `/api/Professionals/{id}/reactivate` | `professionals:update` | Reactivar profesional |
| GET | `/api/Professionals/{id}/status-history` | `professionals:read` | Historial de estados |
| POST | `/api/Professionals/suspend-inactive?days=90` | `professionals:update` | Suspender inactivos |

---

## Componentes Frontend

| Componente | Ruta | Descripción |
|------------|------|-------------|
| `ListComponent` | `/admin/professionals` | Listado con tabs Activos/Pendientes |
| `ConfirmModalComponent` | `@shared/components` | Modal reutilizable con soporte para observación |

---

## Estados del Profesional

| Estado | Descripción | Transiciones |
|--------|-------------|--------------|
| Pending | Registrado, pendiente de validación | → Approved, Rejected |
| Approved | Validado, acceso activo | → Suspended, Terminated |
| Rejected | Rechazado por el admin | — |
| Suspended | Suspendido por inactividad | → Approved (reactivación) |
| Terminated | Dado de baja por el admin | → Approved (reactivación) |

---

## Flujo de validación

### Aprobar
1. Admin selecciona "Aprobar" en una fila pendiente
2. Se abre modal de confirmación
3. Al confirmar:
   - `User.IsActive = true`
   - `Professional.Status = Approved`
   - Se genera contraseña temporal
   - Se envía email con credenciales (background)
   - Se registra en historial de estados

### Rechazar
1. Admin selecciona "Rechazar" en una fila pendiente
2. Se abre modal con campo obligatorio de motivo
3. Al confirmar:
   - `Professional.Status = Rejected`
   - Se desactivan las `ProfessionalInstitutions`
   - Se envía email con motivo del rechazo (background)
   - Se registra en historial de estados

### Desactivar
1. Admin selecciona "Desactivar" en profesional activo
2. Se abre modal con campo obligatorio de motivo
3. Al confirmar:
   - `User.IsActive = false`
   - `Professional.Status = Terminated`
   - Se desactivan las `ProfessionalInstitutions`
   - Se registra en historial de estados

### Reactivar ✅
1. Admin selecciona "Reactivar" en profesional Terminated/Suspended
2. Se abre modal de confirmación
3. Al confirmar:
   - `User.IsActive = true`
   - `Professional.Status = Approved`
   - Se reactivan las `ProfessionalInstitutions`
   - Se registra en historial de estados

### Suspender por inactividad ⚠️ (pendiente automatización)
- Endpoint implementado: `POST /api/Professionals/suspend-inactive?days=90`
- Busca profesionales con `Status == Approved && User.IsActive && (LastLoginDate == null || LastLoginDate < cutoffDate)`
- Suspende y desactiva instituciones
- **Falta:** Background job o tarea programada que ejecute automáticamente
- Por ahora se ejecuta manualmente vía API

---

## Filtro por institución

### Admin Institucional
- Ve solo solicitudes pendientes donde `ProfessionalInstitution.InstitutionId` coincide con sus instituciones asignadas
- Ve solo profesionales activos de sus instituciones

### GlobalAdmin
- Ve todas las solicitudes pendientes sin filtro
- Ve todos los profesionales activos

---

## Cambios en Proceso 04 — Gestión de Profesionales

El proceso se actualiza para incluir:

1. **Auto-registro público** — El profesional se registra sin intervención del admin
2. **Validación por admin** — El admin aprueba/rechaza solicitudes pendientes
3. **Filtro por institución** — Admin institucional ve solo solicitudes de su institución
4. **Desactivación con motivo** — Se requiere motivo al desactivar
5. **Reactivación** — Se puede reactivar un profesional dado de baja
6. **Suspensión por inactividad** — Endpoint para suspender profesionales que no acceden en X días
7. **Historial de estados** — Registro de todos los cambios de estado
8. **Validación asíncrona** — Email y matrícula se validan en tiempo real durante el registro
9. **Emails en background** — El envío de emails no bloquea la respuesta del API

---

## Archivos modificados

### Backend
- `Professional.cs` — Agregado campo `Email`
- `ProfessionalsRepository.cs` — Métodos `ExistsLicenseNumberAsync`, `ExistsProfessionalEmailAsync`, `GetInactiveProfessionalsAsync`, `GetStatusHistoryAsync`, `AddStatusHistoryAsync`
- `RegisterProfessionalCommandHandler.cs` — Crea User + Professional + ProfessionalInstitution
- `ValidateProfessionalCommandHandler.cs` — Aprueba/rechaza con email en background
- `DeactivateProfessionalCommandHandler.cs` — Desactiva con motivo, desactiva instituciones
- `ReactivateProfessionalCommandHandler.cs` — Reactiva profesional e instituciones
- `SuspendInactiveProfessionalsCommandHandler.cs` — Suspende por inactividad
- `GetPendingProfessionalsQueryHandler.cs` — Filtra por institución del admin
- `GetProfessionalsQueryHandler.cs` — Mapea `Status` al response
- `ProfessionalValidationController.cs` — Endpoints de validación asíncrona
- `CreateProfessionalRequest.cs`, `UpdateProfessionalRequest.cs` — Agregado `InstitutionIds`
- `ProfessionalListItemResponse.cs` — Agregado `Status`, `CreatedAt`
- `ProfessionalStatusHistoryResponse.cs` — Nuevo DTO
- `SuspendResult.cs` — Nuevo DTO

### Frontend
- `register-professional.component.ts/html` — Formulario público con validación async
- `list.component.ts/html` — Tabs Activos/Pendientes, sorting, export CSV, modales
- `new.component.ts/html` — Alta desde admin con validación async
- `professionals.service.ts` — Métodos de validación, reactivación, historial
- `confirm-modal.component.ts` — Soporte para campo de observación
- `data-table.component.ts/html` — Sorting con flechas, loading overlay
- `date.validators.ts` — Validadores async de email y matrícula
- `ProfessionalListItemResponse` — Agregado `status`
