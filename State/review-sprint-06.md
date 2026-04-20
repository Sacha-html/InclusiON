# Review Sprint 6 — Checklist de Verificación

**Fecha:** 2026-04-17  
**HUs a revisar:** IN-149, IN-150, IN-148

---

## IN-149 — Auto-registro Profesional

**Archivos clave:**
- BE: `ProfessionalsController.cs` → `POST /api/Professionals/register` (AllowAnonymous)
- BE: `ProfessionalValidationController.cs` → `GET /email`, `GET /license-number`
- FE: `views/pages/register-professional/register-professional.component.ts`
- FE: Ruta pública `/register-professional` (sin auth guard)

### Happy path
- [ ] La ruta `/register-professional` es accesible sin estar logueado
- [ ] El formulario muestra todos los campos requeridos
- [ ] Completar y enviar crea el profesional → aparece modal de éxito
- [ ] El modal redirige a `/admin-login?role=professional`
- [ ] El profesional queda en estado **Pending** (visible en tab Validaciones)
- [ ] El usuario asociado queda **inactivo** (no puede hacer login)
- [ ] Se recibe email de confirmación de registro pendiente

### Validaciones asíncronas
- [ ] Ingresar un email ya registrado → muestra error `emailExists` debajo del campo
- [ ] Ingresar una matrícula ya registrada → muestra error `licenseExists`
- [ ] El botón "Registrar" permanece deshabilitado mientras los validators están pendientes (`pending`)
- [ ] Con email/matrícula únicos → validators muestran estado OK

### Edge cases
- [ ] Enviar el formulario con todos los campos vacíos → validaciones síncronas visibles
- [ ] Campo institución es **opcional** → puede enviarse sin seleccionar ninguna
- [ ] Email con mayúsculas → se registra correctamente (case-insensitive)

---

## IN-150 — Validación por Administrador

**Archivos clave:**
- BE: `ProfessionalsController.cs` → `GET /pending`, `PUT /{id}/validate`, `PUT /{id}/reactivate`
- BE: `ValidateProfessionalCommandHandler.cs`
- FE: `views/admin/professionals/list/list.component.ts` → tab "Validaciones"

### Happy path — Aprobación
- [ ] Tab "Validaciones" visible en la lista de profesionales
- [ ] El badge del tab muestra la cantidad de pendientes correcta
- [ ] El profesional recién registrado (IN-149) aparece en la lista
- [ ] Click en "Aprobar" → modal de confirmación (con campo observación opcional)
- [ ] Confirmar aprobación → profesional desaparece del tab Validaciones
- [ ] El profesional aprobado aparece en la lista "Activos" con estado Approved
- [ ] El usuario asociado queda **activo** y con `MustChangePassword = true`
- [ ] Se recibe email con contraseña temporal al profesional aprobado
- [ ] El profesional puede hacer login con la contraseña temporal → redirige a cambio de contraseña

### Happy path — Rechazo
- [ ] Click en "Rechazar" → modal con campo **observación obligatoria**
- [ ] No se puede confirmar rechazo sin ingresar motivo
- [ ] Confirmar rechazo → profesional desaparece del tab Validaciones
- [ ] Se recibe email al profesional con el motivo del rechazo
- [ ] El profesional rechazado **no puede** hacer login

### Historial de estados
- [ ] En el detalle del profesional, la pestaña de historial muestra las transiciones de estado
- [ ] Se registra: Pending → Approved (o Rejected) con fecha y admin responsable

### Edge cases
- [ ] Admin institucional solo ve pendientes de **su institución**
- [ ] Admin global ve todos los pendientes (incluso sin institución asignada)
- [ ] Intentar aprobar un profesional ya aprobado → error controlado (no 500)

---

## IN-148 — Agrupación del Núcleo Familiar (desde Profesional)

**Archivos clave:**
- BE: `FamilyController.cs` → `/professional/available`, `/professional/link/{fId}/{pId}`, `/professional/unlink/{fId}/{pId}`
- BE: `LinkFamilyToPersonCommandHandler.cs`, `UnlinkFamilyFromPersonCommandHandler.cs`
- FE: `views/professional/person-detail/components/family-tab.component.ts`

### Happy path — Vinculación
- [ ] En el detalle de persona (vista profesional), la pestaña "Familia" es visible
- [ ] El botón "Vincular" aparece solo si el profesional tiene permiso `family:link`
- [ ] Click en "Vincular" → modal con buscador de familiares
- [ ] Buscar con menos de 3 caracteres → no trae resultados (mín. 3)
- [ ] Buscar con 3+ caracteres → lista familiares disponibles
- [ ] Los familiares ya vinculados a esta persona **no aparecen** en la lista (filtro por `personId`)
- [ ] Seleccionar familiar → se habilitan campos de relación e isPrimary
- [ ] Confirmar → familiar vinculado aparece en la tabla de la pestaña
- [ ] Si se marca como primario, el anterior primario deja de serlo

### Happy path — Desvinculación
- [ ] Botón "Desvincular" visible en la tabla (con permiso `family:unlink`)
- [ ] Click en "Desvincular" → modal con campo observación **obligatorio**
- [ ] No se puede confirmar sin ingresar motivo
- [ ] Confirmar → familiar desaparece de la lista activa

### Historial de vinculación
- [ ] El historial registra cada vinculación con fecha, relación y quién vinculó
- [ ] El historial registra cada desvinculación con fecha y motivo

### Reglas de negocio
- [ ] No se pueden vincular 2 familiares con relación "Madre" a la misma persona → error
- [ ] No se pueden vincular 2 familiares con relación "Padre" a la misma persona → error
- [ ] Un familiar previamente desvinculado **puede** re-vincularse (reactivación del vínculo)
- [ ] Al re-vincular, el historial muestra el vínculo anterior + el nuevo

### Edge cases
- [ ] Familiar inactivo (desactivado) → **no aparece** en la lista de disponibles
- [ ] Buscar familiar sin resultados → mensaje "sin resultados" en el modal

---

## Checklist transversal (aplica a las 3 HUs)

- [ ] Los endpoints protegidos devuelven **401** si se acceden sin token
- [ ] Los endpoints con policy devuelven **403** si el rol no tiene permiso
- [ ] Los errores de validación del servidor se muestran como toast en el frontend
- [ ] No hay errores en la consola del navegador durante el flujo normal
- [ ] Los emails de notificación tienen tildes y ñ correctas (encoding UTF-8)

---

## Resultado

| HU | BE | FE | Emails | Edge Cases | Veredicto |
|----|----|----|--------|------------|-----------|
| IN-149 | | | | | |
| IN-150 | | | | | |
| IN-148 | | | | | |
