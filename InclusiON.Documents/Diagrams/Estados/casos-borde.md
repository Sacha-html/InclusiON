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

## CB-03 — Familiar desvinculado siendo el único tutor registrado (Regla de Custodia y Protección Legal)

**Origen:** [`family-representative.md`](family-representative.md) + `PersonRepresentative` + `PersonWithDisability`

**Escenario:** Por una situación judicial o administrativa se desvincula o desactiva al único tutor que tenía acceso a ver el progreso del estudiante en la plataforma y responder legalmente por él.

**Resolución Implementada (Enfoque 1 — Suspensión Preventiva y Reactivación Automática):**
1. **Verificación de Tutores Activos Restantes:** Al ejecutar la desvinculación de un tutor (`UnlinkFamilyFromPersonCommandHandler`) o la desactivación general del familiar (`DeactivateFamilyCommandHandler`), el sistema verifica la cantidad de representantes legales activos que le quedan a cada persona vinculada (`GetActiveRepresentativesCountForPersonAsync`).
2. **Suspensión Preventiva del Alumno:** Si el alumno queda sin tutores activos restantes (contador = 0):
   - Se suspende automáticamente la persona con discapacidad (`Person.IsActive = false`) y su cuenta de usuario asociada (`User.IsActive = false`).
   - El endpoint retorna `studentSuspended: true` (o la lista `SuspendedStudents` en la desactivación del familiar).
   - Se emite una **Alerta de Custodia Escolar** en la interfaz (Toast Warning extendido):
     > *"Familiar desvinculado exitosamente. Al ser el único tutor activo registrado, el alumno [Nombre Alumno] fue dado de baja automáticamente hasta que se registre o invite a un nuevo responsable legal"*
3. **Reactivación Automática:**
   - **Vía Alta/Vinculación Directa:** Al vincular un nuevo familiar activo (`LinkFamilyToPersonCommandHandler`), el sistema detecta si la persona estaba inactiva y la reactiva automáticamente junto a su cuenta de usuario (`Person.IsActive = true`, `User.IsActive = true`).
   - **Vía Invitación:** Cuando un nuevo familiar acepta una invitación asociada al alumno (`AcceptInvitationCommandHandler`), el sistema también reactiva automáticamente a la persona y su usuario si se encontraban suspendidos.

---

## CB-04 — ActivityAssignment sin transición por vencimiento de `DueDate`

**Origen:** [`activity-assignment.md`](activity-assignment.md)

`ActivityAssignment` tiene `DueDate` opcional. Los estados documentados son `Pending → InProgress → Completed / Cancelled`. No existe un estado `Overdue` ni una transición automática cuando `DueDate ≤ now()`.

Una asignación puede permanecer en `Pending` o `InProgress` indefinidamente después de su fecha límite sin cambio de estado.

**Brecha:** El diagrama de estado no tiene salida para el vencimiento de `DueDate`. El sistema no define si esto es intencionado (el profesional gestiona manualmente) o un gap.

---

## CB-05 — Docente dado de baja o reasignación mientras el alumno está en actividad / Transferencia de reportes

**Origen:** [`activity-assignment.md`](activity-assignment.md) × [`professional.md`](professional.md) × [`report.md`](report.md)

**Escenario:** La dirección desactiva a un docente justo en el momento en que uno de sus alumnos está en el aula resolviendo un juego interactivo, o se transfieren alumnos y reportes pendientes a otro profesional.

**Resolución Implementada:**
1. **Preservación del Esfuerzo del Alumno:** El esfuerzo del alumno no se pierde. Las respuestas y métricas (`ActivityResponse`) registradas por el alumno son inmutables y persisten en la base de datos con sus tiempos y aciertos.
2. **Transferencia y Reasignación Sin Conflicto de Change Tracker (EF Core):** Al reasignar un alumno o transferir reportes de un profesional cesante a otro docente o coordinador pedagógico:
   - Se resolvió el error `InvalidOperationException: The instance of entity type 'ProfessionalPerson' cannot be tracked because another instance with the key value '{ProfessionalId, PersonId}' is already being tracked` consultando primero la colección en memoria `_context.ProfessionalPersons.Local` antes de consultar la base de datos en `ReportsRepository.ReassignReportAsync`.
   - Se removió `.AsNoTracking()` en `AssignmentsRepository.GetAssignmentAsync` para garantizar que la desactivación del vínculo anterior (`oldLink.IsActive = false`) y la creación/actualización del nuevo profesional sean rastreados y confirmados en la misma transacción atómica de persistencia.

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

## CB-09 — Invitation `Cancelled` vs `Used` en ventana de concurrencia y Ciclo de Cancelación

**Origen:** [`invitation.md`](invitation.md)

**Resolución Implementada:**
1. **Cancelación por Profesional:** El profesional emisor puede cancelar invitaciones en estado `Enviada` mediante `POST /api/invitations/{code}/cancel`, marcando `IsActive = false`.
2. **Validación Atómica y Bloqueo Inmediato:** Tanto `ValidateInvitationQueryHandler` como `AcceptInvitationCommandHandler` validan en tiempo real `invitation.IsActive`. Si la invitación fue cancelada:
   - Se rechaza inmediatamente retornando `ErrorCode.InvitationCancelled` (803).
   - Se previene cualquier consumo concurrente o registro tardío por parte del familiar.
3. **Visibilidad y Auditoría Directiva:** El Administrador visualiza el emisor (`createdByProfessionalName`), el estado con badge gris (`Cancelada`) y puede filtrar por invitaciones canceladas en el panel administrativo.

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

| ID | Entidades involucradas | Tipo de borde | Estado de Resolución |
|----|----------------------|---------------|----------------------|
| CB-01 | Professional + Report | Bloqueo preventivo (Hard Stop) por reportes pendientes | Resuelto (Hard stop 710) |
| CB-02 | Professional + PersonWithDisability | Estado terminal bloquea entidad dependiente | Resuelto (Hard stop con alerta) |
| CB-03 | FamilyRepresentative + PersonWithDisability | Tutor único desvinculado (Custodia escolar) | Resuelto (Suspensión preventiva y reactivación automática) |
| CB-04 | ActivityAssignment | Condición temporal sin transición documentada | Brecha analizada |
| CB-05 | ActivityAssignment + Professional | Baja docente durante juego / reasignación con reportes | Resuelto (Inmutabilidad de respuestas + fix Change Tracker EF Core) |
| CB-06 | PersonRoadmapActivity + Activity | Bloqueo permanente por dependencia inactiva | Brecha analizada |
| CB-07 | PersonRoadmapActivity | Estado sin salida por restricción de intentos | Brecha analizada |
| CB-08 | AdaptiveEngineConfig | Configuración válida que produce comportamiento inútil | Brecha analizada |
| CB-09 | Invitation | Cancelación y concurrencia Cancelled vs Used | Resuelto (Cancelación POST, error 803 y auditoría) |
| CB-10 | Report | Acción con efecto secundario vacío sin error | Brecha analizada |
| CB-11 | RefreshToken + Professional | Transición de estado que no propaga a entidad relacionada | Brecha analizada |
