# Casos Borde — Derivados de los Diagramas de Estado

Cada caso borde está justificado por una transición o condición documentada en los diagramas de estado. Ninguno es especulativo.

---

## CB-01 — Bloqueo preventivo de baja para profesionales con reportes pendientes

**Origen:** [`professional.md`](professional.md) × [`report.md`](report.md)

**Resolución:** Se implementa un bloqueo preventivo (*hard stop*). Cuando se solicita la baja de un profesional (`IsActive = false` / `Terminated`), el sistema valida si posee informes de progreso en estado borrador o en revisión. De ser así, se aborta la transacción y se retorna un error preventivo con un Alert informativo (`ErrorCode.HasPendingReports` / `710`), detallando la cantidad de borradores para que el profesional los finalice/elimine o el administrador los resuelva.

| Estado del Report | ¿Bloquea la baja? | Comportamiento del Sistema |
|-------------------|:-----------------:|---------------------------|
| `Draft` (Borrador) | **SÍ** | Bloquea la baja. El borrador debe enviarse o descartarse previamente. |
| `Submitted` (Enviado) | **SÍ** | Bloquea la baja. El administrador debe aprobarlo o rechazarlo antes del cese. |
| `Approved` (Aprobado) | **NO** | Permite la baja. El reporte es histórico y permanece inalterable en el legajo. |
| `Rejected` (Rechazado) | **NO** | Permite la baja. El reporte fue evaluado y cerrado; no impide el cese. |

---

## CB-02 — Bloqueo preventivo de baja por alumnos a cargo y supervisión de acceso

**Origen:** [`professional.md`](professional.md) + `ProfessionalPerson` + `PersonWithDisability.SupervisorUserId`

**Resolución:** Se implementa un bloqueo preventivo (*hard stop*). No se permite dar de baja a un profesional si:
1. **Tiene alumnos asignados activamente a su cargo (`ProfessionalPerson.IsActive == true`):** Ya sea en aulas o en seguimiento pedagógico/terapéutico individual. Deben ser reasignados a otro profesional activo previamente.
2. **Es supervisor exclusivo de inicio de sesión asistido (`LoginMethod = ASSISTED` con `SupervisorUserId == professional.UserId`):** Si es el único docente que puede habilitar el ingreso al alumno en el aula, la persona quedaría sin acceso. Debe reasignarse la supervisión a otro profesional activo antes de la baja.

Si alguna de estas condiciones no se cumple, el sistema aborta la operación y emite un Alert informativo indicando la cantidad de alumnos asignados que deben reasignarse.

---

## CB-03 — FamilyRepresentative `Terminated` como supervisor de login

**Origen:** [`family-representative.md`](family-representative.md) + `PersonRepresentative.CanSuperviseLogin`

Caso análogo al CB-02 pero para `LoginMethod = FAMILY`. Al llegar `FamilyRepresentative` a `Terminated`, todos sus `PersonRepresentative` se desactivan. Si era el **único supervisor familiar** de una persona, esa persona pierde acceso.

**Brecha:** La baja del familiar en cascada no verifica si deja personas sin supervisor.

---

## CB-04 — ActivityAssignment sin transición por vencimiento de `DueDate`

**Origen:** [`activity-assignment.md`](activity-assignment.md)

`ActivityAssignment` tiene `DueDate` opcional. Los estados documentados son `Pending → InProgress → Completed / Cancelled`. No existe un estado `Overdue` ni una transición automática cuando `DueDate ≤ now()`.

Una asignación puede permanecer en `Pending` o `InProgress` indefinidamente después de su fecha límite sin cambio de estado.

**Brecha:** El diagrama de estado no tiene salida para el vencimiento de `DueDate`. El sistema no define si esto es intencionado (el profesional gestiona manualmente) o un gap.

---

## CB-05 — ActivityAssignment `InProgress` al producirse la baja del Professional

**Origen:** [`activity-assignment.md`](activity-assignment.md) × [`professional.md`](professional.md)

El ABM de profesionales documenta que al llegar a `Terminated` se desactivan los `ProfessionalPerson`. Sin embargo, las `ActivityAssignment` en estado `InProgress` **no tienen una transición de cancelación automática** documentada.

Una persona puede estar ejecutando una actividad asignada por un profesional que ya no existe en el sistema. El resultado (`ActivityResponse`) se registraría sin profesional asignador activo.

---

## CB-06 — PersonRoadmapActivity `Locked` con `Activity.IsActive = false`

**Origen:** [`roadmap-activity.md`](roadmap-activity.md) + `Activity.IsActive`

La condición de desbloqueo es:

```
ScorePercent(actividad N-1) ≥ UnlockThresholdPercent / 100
```

Si la `Activity` a la que apunta un nodo `Locked` es dada de baja (`IsActive = false`), **la actividad nunca podrá desbloquearse**: la persona no puede ejecutar la actividad anterior (que ahora es inactiva) para generar el score necesario.

El ABM de actividades restringe la baja si hay `ActivityAssignment` activas, pero **no cubre las `PersonRoadmapActivity`** que referencian esa actividad.

**Brecha:** No hay transición documentada en `Locked` para el caso en que la actividad referenciada se desactive.

---

## CB-07 — PersonRoadmapActivity `Unlocked` con `MaxAttempts` agotados sin score suficiente

**Origen:** [`roadmap-activity.md`](roadmap-activity.md)

`PersonRoadmapActivity.MaxAttempts` limita la cantidad de intentos. Si la persona agota todos los intentos sin superar `UnlockThresholdPercent`:

- La actividad queda `Unlocked` (técnicamente) pero la persona no puede ejecutarla más.
- La siguiente actividad en la secuencia queda `Locked` indefinidamente porque el score requerido nunca se alcanzó.
- No existe un estado `Blocked` ni una transición de recuperación documentada.

**Brecha:** La intersección `MaxAttempts agotados + score insuficiente` no tiene estado ni salida en el diagrama.

---

## CB-08 — MDA con `MinDifficultyLevel = MaxDifficultyLevel`

**Origen:** [`roadmap-activity.md`](roadmap-activity.md) — sección Motor Adaptativo

`AdaptiveEngineConfig` define:

```
ConsecutiveSuccessToUpgrade → sube DifficultyLevel
ConsecutiveFailuresToDowngrade → baja DifficultyLevel
```

Si `MinDifficultyLevel = MaxDifficultyLevel` (rango de un solo nivel), ambas condiciones de ajuste se evalúan correctamente, **se detecta el umbral, pero el ajuste resulta en un no-op**. El sistema registraría entradas en `AdaptiveAdjustmentLog` con `PreviousValue = NewValue`.

**Brecha:** Configuración válida según el schema que produce logs vacíos de forma indefinida.

---

## CB-09 — Invitation `Cancelled` vs `Used` en ventana de concurrencia

**Origen:** [`invitation.md`](invitation.md)

El diagrama documenta las transiciones `Pending → Cancelled` (profesional cancela) y `Pending → Used` (familiar completa registro) como independientes. Si ambas ocurren simultáneamente:

- El familiar envía el código en el mismo instante que el profesional cancela (`IsActive = false`).
- Dependiendo del orden de ejecución, el familiar podría completar el registro antes de que la cancelación se persista.

**Brecha:** No hay documentación de qué operación tiene precedencia ni si existe un lock de exclusión mutua.

---

## CB-10 — Report `Approved` sin `PersonRepresentative` activos

**Origen:** [`report.md`](report.md) — sección Notificaciones

Al aprobar un `Report`, el sistema envía email **a todos los `PersonRepresentative.IsActive = true`** de la persona. Si la persona no tiene familiares vinculados activos (aún no se generaron vínculos, o todos fueron dados de baja), la notificación de aprobación se envía a una **lista vacía sin error ni advertencia**.

El familiar nunca se entera del reporte aprobado, pero el reporte queda `Approved` y visible (sin destinatario real).

---

## CB-11 — RefreshToken: Professional `Suspended` sin revocación de tokens

**Origen:** [`refresh-token.md`](refresh-token.md) × [`professional.md`](professional.md)

`Professional → Suspended` no establece `IsActive = false` en `User` (a diferencia de `Terminated`). El diagrama de `RefreshToken` documenta que la revocación forzada ocurre ante "suspensión o baja de usuario", pero `Suspended` en `Professional` **no equivale a `IsActive = false` en `User`**.

Un profesional `Suspended` podría seguir autenticándose con tokens activos si el backend no fuerza la revocación en esa transición específica.

**Brecha:** La relación entre `Professional.Status = Suspended` y el estado de sus `RefreshToken` no está documentada.

---

## Resumen

| ID | Entidades involucradas | Tipo de borde |
|----|----------------------|---------------|
| CB-01 | Professional + Report | Bloqueo preventivo (Hard Stop) por reportes pendientes |
| CB-02 | Professional + PersonWithDisability | Estado terminal bloquea entidad dependiente |
| CB-03 | FamilyRepresentative + PersonWithDisability | Estado terminal bloquea entidad dependiente |
| CB-04 | ActivityAssignment | Condición temporal sin transición documentada |
| CB-05 | ActivityAssignment + Professional | Estado activo sin dueño al llegar a terminal |
| CB-06 | PersonRoadmapActivity + Activity | Bloqueo permanente por dependencia inactiva |
| CB-07 | PersonRoadmapActivity | Estado sin salida por restricción de intentos |
| CB-08 | AdaptiveEngineConfig | Configuración válida que produce comportamiento inútil |
| CB-09 | Invitation | Condición de carrera entre dos transiciones |
| CB-10 | Report | Acción con efecto secundario vacío sin error |
| CB-11 | RefreshToken + Professional | Transición de estado que no propaga a entidad relacionada |
