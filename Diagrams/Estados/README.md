# Diagramas de Estado — InclusiON

Diagramas de estado de las entidades principales del sistema. Cada archivo cubre una entidad con ciclo de vida no trivial.

---

## Índice

| Archivo | Entidad | Estados |
|---------|---------|---------|
| [casos-borde.md](casos-borde.md) | Análisis transversal | 11 casos derivados de los diagramas |
| [professional.md](professional.md) | `Professional` | Pending · Approved · Rejected · Suspended · Terminated |
| [family-representative.md](family-representative.md) | `FamilyRepresentative` | Active · Terminated |
| [invitation.md](invitation.md) | `Invitation` | Pending · Used · Expired · Cancelled |
| [report.md](report.md) | `Report` | Draft · Submitted · Approved · Rejected |
| [activity-assignment.md](activity-assignment.md) | `ActivityAssignment` | Pending · InProgress · Completed · Cancelled |
| [roadmap-activity.md](roadmap-activity.md) | `PersonRoadmapActivity` | Locked · Unlocked |
| [refresh-token.md](refresh-token.md) | `RefreshToken` | Active · Revoked · Expired |

---

## Entidades sin diagrama de estado

Las siguientes entidades usan únicamente el flag `IsActive` para soft-delete, sin máquina de estados propia:

| Entidad | Razón de exclusión |
|---------|-------------------|
| `PersonWithDisability` | Solo `IsActive` (no tiene campo `Status`) |
| `Activity` | Solo `IsActive` |
| `EducationalInstitution` | Solo `IsActive` |
| `ActivityResult` | Entidad inmutable: se registra y no cambia |
| `Diagnosis` | Entidad inmutable: texto cifrado de solo escritura |
| `Message` | Solo `IsRead` e `IsActive`; no hay flujo de aprobación |
| `AccessAudit` | Registro de auditoría inmutable |
