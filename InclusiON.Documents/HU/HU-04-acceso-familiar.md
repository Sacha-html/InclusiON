# HU-04 — Acceso Familiar e Invitaciones

| Campo | Contenido |
|---|---|
| ID | HU-04 |
| Épica | Gestión de Usuarios |
| Título | Acceso Familiar e Invitaciones |
| Prioridad | Alta |
| Estimación | 5 puntos de historia |
| Sprint asignado | Sprint 2 |
| Estado | Completada |

**Proceso relacionado:** 06, 07

---

## Historia de Usuario

**Como** profesional
**Quiero** invitar a los familiares de mis personas asignadas para que se registren en la plataforma
**Para** que puedan acceder al seguimiento del progreso de forma controlada sin intervención del administrador

---

## Descripción funcional

El profesional genera una invitación ingresando el email, nombre, apellido y relación del familiar, junto con la persona con discapacidad asociada. El sistema:

1. **Envía un email** con un link único de registro
2. **El familiar abre el link** y ve un formulario con sus datos pre-llenados (nombre, apellido, relación en modo solo lectura)
3. **Completa el registro** eligiendo una contraseña
4. **El sistema crea automáticamente** la cuenta del familiar, su perfil y la vinculación con la persona

Las invitaciones tienen un ciclo de vida:
- **Enviada** — Email enviado, pendiente de aceptación
- **Aceptada** — El familiar completó el registro
- **Expirada** — Pasaron 7 días sin ser usada

El profesional puede consultar el estado de sus invitaciones. El administrador ve todas las invitaciones del sistema.

---

## Criterios de Aceptación

- [x] El profesional puede generar invitaciones desde su portal
- [x] El sistema envía un email con link único al familiar
- [x] El link de invitación es accesible sin necesidad de tener cuenta (ruta pública)
- [x] Los datos del familiar aparecen pre-llenados y en modo solo lectura
- [x] La contraseña debe tener mínimo 8 caracteres, 1 mayúscula y 1 número
- [x] Al aceptar la invitación, se crea la cuenta, el perfil familiar y la vinculación con la persona automáticamente
- [x] Una invitación no puede usarse dos veces
- [x] Una invitación expira a los 7 días si no se acepta
- [x] El profesional puede ver el estado de sus invitaciones
- [x] El administrador puede ver todas las invitaciones con filtro por institución
