# Diagrama de Estado — Invitation

**Entidad:** `Invitation`  
**Campos de estado:** `IsUsed` (bool), `IsActive` (bool), `ExpiresAt` (timestamptz)  
**Estado derivado** (no hay campo `Status` explícito — se infiere de los tres campos)

---

## Estados

| Estado | Condición | Descripción |
|--------|-----------|-------------|
| `Pending` | `IsUsed = false` AND `IsActive = true` AND `ExpiresAt > now()` | Invitación activa, a la espera de ser usada. |
| `Used` | `IsUsed = true` | Familiar completó el registro con esta invitación. |
| `Expired` | `IsUsed = false` AND `ExpiresAt ≤ now()` | Venció sin ser usada (TTL: 7 días desde creación). |
| `Cancelled` | `IsActive = false` AND `IsUsed = false` | Cancelada manualmente antes de ser usada. |

---

## Diagrama

```mermaid
stateDiagram-v2
    direction LR

    [*] --> Pending : Profesional crea invitación

    Pending --> Used     : Familiar completa registro con el código
    Pending --> Expired  : ExpiresAt ≤ now()
    Pending --> Cancelled : Profesional cancela

    Used      --> [*]
    Expired   --> [*]
    Cancelled --> [*]

    note right of Pending
        Código único (varchar64)
        Vence a los 7 días
        Email enviado al familiar
    end note

    note right of Used
        IsUsed = true
        UsedByUserId registrado
        FamilyRepresentative creado → Status Active
        PersonRepresentative creado si tenía ForPersonId
        Reactivación automática del alumno si estaba suspendido (CB-03)
    end note
```

---

## Reglas de Negocio

- Una invitación es de **un solo uso**: una vez `Used`, no puede reutilizarse.
- No puede existir más de una invitación `Pending` para el mismo par `Email + ForPersonId`.
- Las invitaciones `Expired` y `Cancelled` no bloquean crear una nueva invitación para el mismo email.
- El TTL de 7 días se evalúa comparando `ExpiresAt` con la fecha actual; no hay job que cambie el estado en base de datos.
- **Cancelación:** El profesional emisor puede cancelar una invitación en estado `Pending` / `Enviada`. Al cancelarse, `IsActive` pasa a `false`.
- **Bloqueo de Códigos Cancelados:** Cualquier intento de validar (`GET /api/invitations/{code}`) o aceptar (`POST /api/invitations/{code}/accept`) una invitación cancelada es rechazado inmediatamente con `ErrorCode.InvitationCancelled` (código 803).
- **Custodia Escolar (CB-03):** Si el estudiante asociado a la invitación se encontraba suspendido preventivamente por haber quedado sin tutores legales activos, al completarse el registro del familiar el alumno y su cuenta de usuario son reactivados automáticamente (`Person.IsActive = true`, `User.IsActive = true`).

| Transition | Actor | Endpoint |
|------------|-------|----------|
| → `Pending` | Profesional | `POST /api/invitations` |
| `Pending` → `Used` | Familiar (registro completado) | `POST /api/invitations/{code}/accept` |
| `Pending` → `Cancelled` | Profesional | `POST /api/invitations/{code}/cancel` |
| `Pending` → `Expired` | Sistema (evaluación pasiva en lectura) | — |
