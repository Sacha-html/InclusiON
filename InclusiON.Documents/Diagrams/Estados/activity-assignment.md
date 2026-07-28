# Diagrama de Estado — ActivityAssignment

**Entidad:** `ActivityAssignment`  
**Campo de estado:** `Status` (varchar20)  
**Entidades relacionadas:** `ActivityResponse` (resultado de cada ejecución)

---

## Estados

| Estado | Descripción |
|--------|-------------|
| `Pending` | Asignación creada por el profesional. La persona aún no inició la actividad. |
| `InProgress` | La persona inició la ejecución de la actividad. |
| `Completed` | La persona completó la actividad (al menos un intento finalizado). |
| `Cancelled` | Cancelada por el profesional antes de ser completada. Soft-delete (`IsActive = false`). |

---

## Diagrama — ActivityAssignment

```mermaid
stateDiagram-v2
    direction LR

    [*] --> Pending : Profesional asigna actividad

    Pending    --> InProgress : Persona inicia ejecución
    Pending    --> Cancelled  : Profesional cancela
    InProgress --> Completed  : Persona finaliza ejecución
    InProgress --> Cancelled  : Profesional cancela

    Completed --> [*]
    Cancelled --> [*]

    note right of Pending
        DueDate opcional
        IsEvaluationActivity indica si es tarea de evaluación
        No puede existir ya una asignación activa de la misma actividad para la misma persona
    end note

    note right of Completed
        No puede cancelarse una vez completada
        Cada ejecución genera un ActivityResponse
    end note
```

---

## Reglas de Transición

| Desde | Hacia | Actor | Condición |
|-------|-------|-------|-----------|
| — | `Pending` | Profesional | No puede existir asignación activa duplicada para misma actividad + persona |
| `Pending` | `InProgress` | Sistema (auto) | La persona inicia la ejecución en el frontend |
| `Pending` | `Cancelled` | Profesional | Soft-delete: `IsActive = false` |
| `InProgress` | `Completed` | Sistema (auto) | La persona finaliza el intento |
| `InProgress` | `Cancelled` | Profesional | Soft-delete: `IsActive = false` |
| `Completed` | `Cancelled` | — | **No permitido** |

El estado se actualiza automáticamente por el sistema; el profesional no lo modifica directamente (excepto para cancelar).

---

## ActivityResponse — Resultado de cada Ejecución

Cada vez que la persona ejecuta la actividad asignada, el sistema registra un `ActivityResponse`. Una asignación puede tener múltiples respuestas (múltiples intentos).

```mermaid
stateDiagram-v2
    direction LR

    [*] --> InProgress : Persona inicia ejecución (StartedAt registrado)

    InProgress --> Completed : Persona finaliza (CompletedAt registrado)

    state Completed {
        [*] --> Correct    : SuccessPercentage alto
        [*] --> Partial    : SuccessPercentage intermedio
        [*] --> Incorrect  : SuccessPercentage bajo
        [*] --> Abandoned  : Persona abandona sin completar
    }

    Completed --> [*]

    note right of Completed
        Result cifrado con AES-256-GCM
        FrustrationLevel registrado (1-5)
        RequiredSupport indica si hubo intervención del supervisor
    end note
```

> `ActivityResponse` es inmutable una vez registrado. El profesional puede agregar `Observations` (notas post-sesión), pero no puede modificar el resultado.
