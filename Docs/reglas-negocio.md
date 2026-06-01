# Reglas de Negocio — InclusiON

**Artefacto:** 07 — Reglas de Negocio  
**Práctica Profesionalizante II — Institución Cervantes**  
**Última actualización:** 2026-05-31

---

## ¿Qué es una regla de negocio?

Una restricción o condición que el negocio impone y que el sistema debe respetar siempre. No es un paso del proceso: es una ley que aplica en cualquier momento del flujo.

> **Trazabilidad:** cada regla incluye la Historia de Usuario que la originó. Esto garantiza que ninguna restricción del sistema fue inventada por el equipo técnico: todas surgieron del relevamiento con el cliente.

---

## Módulo: Acceso y Autorización

| Regla en lenguaje del cliente | Cómo el sistema la implementa | Qué pasa si se viola | Origen (HU) | Validada por |
|---|---|---|---|---|
| Un profesional solo puede ver y editar datos de las personas que tiene asignadas. | Al recibir cualquier request sobre una persona, el sistema verifica `CanAccessPersonAsync(professionalId, personId)`. Si falla, devuelve 403 Forbidden. | El profesional accede a información clínica de un paciente que no es suyo. Violación de privacidad. | [HU-IN-172](../HU/HU-IN-172-autorizacion-por-recurso.md) | Equipo — 2026-05-31 |
| Un familiar solo puede ver información de las personas que representa activamente. | Cada consulta filtra por `PersonRepresentative.IsActive = true` usando el `entityId` del JWT. | El familiar visualiza datos de una persona con la que no tiene vínculo activo. | [HU-04](../HU/HU-04-acceso-familiar.md) | Equipo — 2026-05-31 |
| Un admin institucional solo puede gestionar usuarios y datos de su propia institución. | El JWT incluye `institutionId`. El sistema filtra todas las consultas usando ese valor. El admin global tiene `isGlobalAdmin = true` y sin filtro. | El admin de institución A ve o modifica datos de institución B. | [HU-11](../HU/HU-11-gestion-usuarios.md) · [HU-IN-172](../HU/HU-IN-172-autorizacion-por-recurso.md) | Equipo — 2026-05-31 |
| Una persona con discapacidad solo puede ver sus propias actividades asignadas. | `GET /api/my/activity-assignments` filtra por `entityId` del JWT, que corresponde al id de la persona autenticada. | La persona ve actividades de otra persona. | [HU-06](../HU/HU-06-ejecucion-actividades.md) | Equipo — 2026-05-31 |

---

## Módulo: Gestión de Profesionales

| Regla en lenguaje del cliente | Cómo el sistema la implementa | Qué pasa si se viola | Origen (HU) | Validada por |
|---|---|---|---|---|
| Un profesional que se registra por sí mismo empieza en estado Pendiente y no puede operar hasta que el admin lo apruebe. | `POST /api/professionals/register` crea el profesional con `Status = Pending`. Las rutas protegidas verifican que el profesional tenga `Status = Approved`. | Un profesional no validado crea actividades o accede a datos de personas. | [HU-IN-149](../HU/HU-IN-149-auto-registro-profesional.md) | Equipo — 2026-05-31 |
| Un profesional creado directamente por el admin queda aprobado de inmediato. | `POST /api/professionals` (ruta admin) crea con `Status = Approved`. | — | [HU-01](../HU/HU-01-catalogos-configuracion.md) | Equipo — 2026-05-31 |
| Un profesional no puede validarse a sí mismo. | `PUT /api/professionals/{id}/validate` verifica que el `userId` del JWT no coincida con el `Professional.UserId` del profesional siendo validado. Si coincide, devuelve 403. | El profesional aprueba su propio auto-registro. | [HU-IN-150](../HU/HU-IN-150-validacion-admin.md) | Equipo — 2026-05-31 |
| El email y el número de matrícula deben ser únicos en todo el sistema. | Al crear o editar, el sistema valida unicidad en `Professional.Email` (vía User) y `Professional.LicenseNumber`. Devuelve 409 Conflict si ya existe. | Dos profesionales comparten email o matrícula, generando confusión en la identidad. | [HU-IN-149](../HU/HU-IN-149-auto-registro-profesional.md) | Equipo — 2026-05-31 |
| Un profesional sin login durante 90 días se suspende automáticamente. | El job `POST /api/professionals/suspend-inactive` compara `LastLoginDate` con fecha actual. Si la diferencia supera 90 días, cambia `Status = Suspended`. | Un profesional inactivo mantiene acceso activo indefinidamente. | [HU-11](../HU/HU-11-gestion-usuarios.md) | Equipo — 2026-05-31 |

---

## Módulo: Gestión de Personas con Discapacidad

| Regla en lenguaje del cliente | Cómo el sistema la implementa | Qué pasa si se viola | Origen (HU) | Validada por |
|---|---|---|---|---|
| El método de login de una persona debe ser compatible con su nivel de autonomía. | Al configurar el método de login, el sistema valida que si `LoginMethod.RequiresSupervisor = true`, la persona tenga `AutonomyLevel.RequiresSupervision = true`. Si no coinciden, devuelve 422. | Una persona con autonomía alta tiene login asistido configurado, o una persona que necesita supervisión usa login estándar sin supervisor. | [HU-14](../HU/HU-14-accesibilidad-por-persona.md) | Equipo — 2026-05-31 |
| Un supervisor autorizado debe estar configurado si la persona usa login asistido. | Si `LoginMethod.Code = ASSISTED` (id=3), el campo `PersonWithDisability.SupervisorUserId` es obligatorio y debe corresponder a un usuario activo con permiso `CanSuperviseLogin = true`. | La persona no puede iniciar sesión con el método asistido, o queda sin supervisión real. | [HU-14](../HU/HU-14-accesibilidad-por-persona.md) | Equipo — 2026-05-31 |

---

## Módulo: Invitaciones

| Regla en lenguaje del cliente | Cómo el sistema la implementa | Qué pasa si se viola | Origen (HU) | Validada por |
|---|---|---|---|---|
| Una invitación es de un solo uso. | Al usar un código de invitación, el sistema marca `Invitation.IsUsed = true`. Si ya fue usada, devuelve 409 Conflict. | Dos personas distintas usan el mismo código y quedan vinculadas al mismo familiar. | [HU-04](../HU/HU-04-acceso-familiar.md) | Equipo — 2026-05-31 |
| Una invitación tiene fecha de vencimiento. | Al registrarse con el código, el sistema verifica que `Invitation.ExpiresAt > DateTime.UtcNow`. Si venció, devuelve 410 Gone. | Un familiar accede con un código generado hace meses, potencialmente inválido. | [HU-04](../HU/HU-04-acceso-familiar.md) | Equipo — 2026-05-31 |

---

## Módulo: Actividades y Asignaciones

| Regla en lenguaje del cliente | Cómo el sistema la implementa | Qué pasa si se viola | Origen (HU) | Validada por |
|---|---|---|---|---|
| Solo se puede cancelar una asignación en estado Pendiente. | `PATCH /api/activity-assignments/{id}/cancel` verifica `StatusId == AssignmentStatuses.Pendiente`. Si ya está EnProgreso o Completada, devuelve 409 con `ErrorCode.BusinessRuleViolation`. | Se intenta cancelar una actividad que la persona ya inició, perdiendo el registro de intento iniciado. | [HU-06](../HU/HU-06-ejecucion-actividades.md) | Equipo — 2026-05-31 |
| Una asignación completada no puede volver a estado anterior. | El estado `Completada` es final en la máquina de estados. No existe ningún endpoint de transición que permita salir de ese estado. | El sistema permite editar resultados ya registrados, comprometiendo la auditoría clínica. | [HU-06](../HU/HU-06-ejecucion-actividades.md) | Equipo — 2026-05-31 |
| Cada persona tiene exactamente un roadmap activo. | `POST /api/persons/{id}/roadmap` devuelve 409 Conflict si ya existe un `PersonRoadmap` con `IsActive = true` para esa persona. | Se crean múltiples planes de aprendizaje contradictorios para la misma persona. | [HU-05](../HU/HU-05-roadmap.md) | Equipo — 2026-05-31 |

---

## Módulo: Motor Adaptativo (MDA)

| Regla en lenguaje del cliente | Cómo el sistema la implementa | Qué pasa si se viola | Origen (HU) | Validada por |
|---|---|---|---|---|
| El motor no puede subir la dificultad más allá del máximo configurado, ni bajarla por debajo del mínimo. | Antes de persistir el ajuste, el sistema verifica que `newLevel ∈ [AdaptiveEngineConfig.MinDifficultyLevel, MaxDifficultyLevel]`. Clampea el valor si supera el rango. | La persona recibe actividades de dificultad imposible o trivial. | [HU-10](../HU/HU-10-motor-adaptativo.md) | Equipo — 2026-05-31 |
| El motor solo actúa si está habilitado para esa actividad del roadmap. | Antes de ejecutar el pipeline de ajuste, se verifica `AdaptiveEngineConfig.IsEnabled = true`. Si no existe config o está deshabilitado, no hay ajuste. | El motor ajusta actividades para las que no fue configurado. | [HU-10](../HU/HU-10-motor-adaptativo.md) | Equipo — 2026-05-31 |

---

## Módulo: Reportes

| Regla en lenguaje del cliente | Cómo el sistema la implementa | Qué pasa si se viola | Origen (HU) | Validada por |
|---|---|---|---|---|
| Un reporte enviado (Submitted) no puede editarse. | `PUT /api/reports/{id}` verifica que `Status = DRAFT`. Si está en otro estado, devuelve 422. | El profesional modifica retroactivamente un reporte ya enviado a revisión. | [HU-08](../HU/HU-08-diagnosticos-reportes.md) | Equipo — 2026-05-31 |
| Solo el profesional que creó el reporte puede enviarlo a revisión. | `PATCH /api/reports/{id}/submit` verifica que `ProfessionalId` del reporte coincida con el `entityId` del JWT. | Otro profesional envía un reporte que no generó. | [HU-08](../HU/HU-08-diagnosticos-reportes.md) | Equipo — 2026-05-31 |
| Un reporte rechazado vuelve a borrador para corrección. | Al ejecutar `PATCH /api/reports/{id}/reject`, el sistema cambia `Status = DRAFT` y habilita nuevamente la edición. | El reporte rechazado queda bloqueado y el profesional no puede corregirlo. | [HU-08](../HU/HU-08-diagnosticos-reportes.md) | Equipo — 2026-05-31 |
| Un familiar solo puede ver reportes aprobados, no borradores ni enviados. | `GET /api/reports` con rol `family` filtra `Status = APPROVED` únicamente. Los reportes Draft o Submitted son invisibles para el familiar. | El familiar ve un reporte en borrador incompleto o con errores, antes de que el profesional lo revise. | [HU-04](../HU/HU-04-acceso-familiar.md) · [HU-08](../HU/HU-08-diagnosticos-reportes.md) | Equipo — 2026-05-31 |

---

## Módulo: Datos Clínicos y Seguridad

| Regla en lenguaje del cliente | Cómo el sistema la implementa | Qué pasa si se viola | Origen (HU) | Validada por |
|---|---|---|---|---|
| Los datos clínicos sensibles (diagnósticos, observaciones) deben almacenarse cifrados. | Los campos marcados con `[Encrypted]` se cifran automáticamente con AES-256-GCM antes de persistirse en la base de datos. El descifrado es transparente al leer. | La base de datos queda expuesta con diagnósticos médicos en texto plano. | [HU-IN-173](../HU/HU-IN-173-hardening-seguridad.md) | Equipo — 2026-05-31 |
| Cada acceso a datos de una persona queda registrado para auditoría. | El middleware de autorización registra cada operación en `AccessAudit` con resultado Allowed/Denied, usuario, persona accedida y timestamp. | No se puede rastrear quién accedió a información clínica confidencial. | [HU-IN-172](../HU/HU-IN-172-autorizacion-por-recurso.md) | Equipo — 2026-05-31 |
