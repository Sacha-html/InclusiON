# ABM — Asignación Profesional–Persona

**Actor:** Profesional / Administrador Institucional  
**Justificación:** Un profesional puede atender a múltiples personas con discapacidad, y una persona puede tener más de un profesional asignado (ej: docente + terapeuta). El Administrador Institucional necesita gestionar estas asignaciones para garantizar que cada persona tenga al menos un profesional principal, y que las reasignaciones (por licencias, bajas, etc.) se realicen correctamente. El propio Profesional también puede solicitar nuevas asignaciones.

**Entidades:** `ProfessionalPerson`

---

## Alta — Asignación

**Actor:** Profesional / Administrador Institucional

| Campo | Tipo | Requerido | Validaciones |
|-------|------|:---------:|--------------|
| Profesional | Referencia | Sí | Debe existir, estar activo y tener `Status = Approved` |
| Persona con discapacidad | Referencia | Sí | Debe existir y estar activa |
| Es profesional principal | Booleano | Sí | — |
| Puede supervisar login | Booleano | Sí | — |

**Validaciones de integridad:**
- No puede existir ya una asignación activa entre el mismo profesional y la misma persona.
- Si `EsPrincipal = true`, la asignación principal anterior se actualiza a `EsPrincipal = false`.
- El profesional y la persona deben pertenecer a la misma institución.

**Resultado:** Se crea registro en `ProfessionalPerson` con `Activo = true`.

---

## Baja — Asignación

**Actor:** Administrador Institucional

- Se establece `Activo = false` en `ProfessionalPerson`.
- **Validación:** Si el profesional es el principal de la persona y no queda ningún otro profesional asignado, se debe especificar un nuevo profesional principal antes de proceder, o la persona queda marcada sin profesional principal (alerta en el sistema).

---

## Modificación — Asignación

**Actor:** Administrador Institucional

| Campo | Validaciones |
|-------|--------------|
| Es profesional principal | Si se activa, se desactiva la bandera del anterior principal |
| Puede supervisar login | — |

---

## Listado — Asignaciones por Persona

**Actor:** Profesional / Administrador Institucional

| Columna | Descripción |
|---------|-------------|
| Profesional | Nombre y especialidad |
| Es principal | Sí / No |
| Puede supervisar login | Sí / No |
| Fecha de asignación | Cuándo se vincularon |
| Estado | Activo / Inactivo |

---

## Listado — Personas por Profesional

**Actor:** Profesional

| Columna | Descripción |
|---------|-------------|
| Persona | Nombre y apellido |
| Edad | Calculada |
| Tipo de discapacidad | Del catálogo |
| Soy principal | Sí / No |
| Tiene roadmap | Sí / No |
| Estado | Activo / Inactivo |

**Persistencia:** Consulta a `ProfessionalPerson` con filtros por `ProfessionalId` o `PersonWithDisabilityId`.
