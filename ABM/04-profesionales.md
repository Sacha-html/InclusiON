# ABM — Profesionales

**Actor:** Administrador Institucional  
**Justificación:** El Administrador Institucional gestiona el plantel de profesionales (docentes, terapeutas, psicólogos) de su institución. Sin este ABM, no es posible incorporar ni habilitar a los profesionales que trabajan directamente con las personas con discapacidad. También gestiona su estado (aprobación, suspensión, baja) y sus vinculaciones con la institución.

**Entidades:** `Professional`, `ProfessionalStatusHistory`, `ProfessionalInstitution`, `User`

---

## Alta — Profesional

**Actor:** Administrador Institucional

El alta de un profesional inicia un flujo de dos pasos: (1) el admin crea el registro y envía invitación por email, (2) el profesional completa su registro.

| Campo | Tipo | Requerido | Validaciones |
|-------|------|:---------:|--------------|
| Nombre | Texto (100) | Sí | No vacío |
| Apellido | Texto (100) | Sí | No vacío |
| Email | Texto (255) | Sí | Formato válido; único en `User` |
| DNI | Texto (20) | No | Único si se ingresa |
| Teléfono | Texto (20) | No | — |
| Especialidad | Texto (100) | No | — |
| Matrícula | Texto (50) | No | Único si se ingresa |
| Fecha de nacimiento | Fecha | No | Debe ser una fecha pasada |
| Institución | Referencia | Sí | La institución del Admin Institucional |

**Validaciones de integridad:**
- El email no puede existir ya en `User`.
- El DNI, si se ingresa, no puede existir en otro `Professional`.
- La matrícula, si se ingresa, no puede existir en otro `Professional`.

**Resultado:**
- Se crea `Professional` con `Status = Pending`.
- Se crea `User` con `MustChangePassword = true`.
- Se crea `ProfessionalInstitution` vinculando el profesional a la institución.
- Se registra entrada en `ProfessionalStatusHistory` (primer estado: `Pending`).
- Se envía email de bienvenida con link de activación.

---

## Cambio de Estado — Profesional

**Actor:** Administrador Institucional

Transiciones válidas:

| Estado actual | Estado destino | Acción |
|---------------|----------------|--------|
| `Pending` | `Approved` | Aprobar profesional |
| `Pending` | `Rejected` | Rechazar profesional |
| `Approved` | `Suspended` | Suspender temporalmente |
| `Suspended` | `Approved` | Reactivar |
| `Approved` | `Terminated` | Dar de baja definitiva |
| `Suspended` | `Terminated` | Dar de baja definitiva |

Cada transición requiere una **observación** (texto libre) que se registra en `ProfessionalStatusHistory`.

**Impacto de `Terminated`:**
- `IsActive = false` en `User`.
- `Activo = false` en `Professional`.
- Se desactivan las `ProfessionalPerson` del profesional (sus personas quedan sin profesional principal y deben ser reasignadas).

---

## Baja — Profesional

Equivale al cambio de estado a `Terminated` (ver arriba). No existe eliminación física.

---

## Modificación — Profesional

**Actor:** Administrador Institucional (datos básicos) / Profesional (su propio perfil)

Campos editables por el Admin Institucional:

| Campo | Validaciones |
|-------|--------------|
| Nombre | No vacío |
| Apellido | No vacío |
| DNI | Único (excluyendo registro actual) |
| Teléfono | — |
| Especialidad | — |
| Matrícula | Único (excluyendo registro actual) |
| Fecha de nacimiento | Fecha pasada |

El email solo puede modificarse si el profesional aún no activó su cuenta (`Status = Pending`).

---

## Listado — Profesionales

**Actor:** Administrador Institucional

| Columna | Descripción |
|---------|-------------|
| Nombre y Apellido | Identidad del profesional |
| DNI | Documento |
| Especialidad | Área de trabajo |
| Matrícula | Número de matrícula |
| Estado | Pending / Approved / Rejected / Suspended / Terminated |
| Personas asignadas | Cantidad de personas activas bajo su cargo |
| Institución | Institución a la que pertenece |

**Filtros disponibles:** nombre/DNI/email, estado, institución (solo para Admin Global).  
**Persistencia:** Consulta a `Professional` con join a `ProfessionalInstitution` filtrado por institución del admin que realiza la consulta.

---

## Listado — Historial de Estados

Para cada profesional, se puede consultar la tabla `ProfessionalStatusHistory`:

| Columna | Descripción |
|---------|-------------|
| Estado anterior | Estado previo al cambio |
| Estado nuevo | Estado resultante |
| Observación | Motivo del cambio |
| Modificado por | Admin que realizó el cambio |
| Fecha | Cuándo se realizó |
