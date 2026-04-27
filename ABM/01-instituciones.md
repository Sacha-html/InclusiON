# ABM — Instituciones Educativas

**Actor:** Administrador Global  
**Justificación:** El Administrador Global es el único con visibilidad sobre toda la plataforma. Necesita crear y gestionar las instituciones educativas (escuelas, centros de rehabilitación) que albergarán profesionales y personas con discapacidad. Sin este ABM, no es posible incorporar ningún profesional al sistema, ya que todo profesional debe pertenecer a al menos una institución.

**Entidades:** `EducationalInstitution`, `AdminInstitution`

---

## Alta — Nueva Institución

**Actor:** Administrador Global

| Campo | Tipo | Requerido | Validaciones |
|-------|------|:---------:|--------------|
| Nombre | Texto (255) | Sí | No vacío; único en el sistema |
| Dirección | Texto (255) | No | — |
| Teléfono | Texto (20) | No | — |
| Email | Texto (100) | No | Formato email válido si se ingresa |

**Validaciones de integridad:**
- El nombre de la institución debe ser único (case-insensitive).
- Se persiste con `Activo = true`.

**Resultado:** Se crea un registro en `EducationalInstitution`.

---

## Alta — Asignación de Administrador Institucional

**Actor:** Administrador Global

| Campo | Tipo | Requerido | Validaciones |
|-------|------|:---------:|--------------|
| Institución | Referencia | Sí | Debe existir y estar activa |
| Usuario administrador | Referencia | Sí | Debe existir, estar activo y tener rol Admin |

**Validaciones de integridad:**
- No puede existir ya una asignación activa entre el mismo admin y la misma institución.
- La institución debe estar activa.

**Resultado:** Se crea un registro en `AdminInstitution`.

---

## Baja — Institución

**Actor:** Administrador Global

- Se establece `Activo = false` en `EducationalInstitution` (baja lógica).
- **Impacto en cadena:** Los profesionales vinculados quedan sin institución activa; sus asignaciones a esa institución en `ProfessionalInstitution` también se desactivan.
- **Validación:** No se puede dar de baja una institución con personas con discapacidad activas asignadas a profesionales de esa institución.

---

## Baja — Asignación de Administrador Institucional

**Actor:** Administrador Global

- Se establece `Activo = false` en `AdminInstitution`.
- El usuario admin no se elimina; pierde acceso a la institución específica.

---

## Modificación — Institución

**Actor:** Administrador Global

Campos editables:

| Campo | Validaciones |
|-------|--------------|
| Nombre | No vacío; único (excluyendo el registro actual) |
| Dirección | — |
| Teléfono | — |
| Email | Formato válido si se ingresa |

---

## Listado — Instituciones

**Actor:** Administrador Global

| Columna | Descripción |
|---------|-------------|
| Nombre | Nombre de la institución |
| Email | Email de contacto |
| Teléfono | Teléfono de contacto |
| Admins asignados | Cantidad de admins institucionales activos |
| Profesionales activos | Cantidad de profesionales activos vinculados |
| Estado | Activo / Inactivo |

**Filtros disponibles:** nombre, estado (activo/inactivo).  
**Persistencia:** Consulta directa a `EducationalInstitution` con joins a `AdminInstitution` y `ProfessionalInstitution`.
