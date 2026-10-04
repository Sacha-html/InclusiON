# Reglas de Negocio — InclusiON

**Artefacto:** 07 — Reglas de Negocio  
**Práctica Profesionalizante II — Institución Cervantes**  
**Última actualización:** Octubre 2026

---

## ¿Qué es una regla de negocio?

Una restricción o condición que el negocio impone y que el sistema debe respetar siempre. No es un paso del proceso: es una ley que aplica en cualquier momento del flujo.

> **Trazabilidad:** Cada regla incluye la Historia de Usuario que la originó. Esto garantiza que ninguna restricción del sistema fue inventada por el equipo técnico: todas surgieron del relevamiento institucional con la escuela.

> **Verificación:** La columna **"Validada por"** indica si la regla fue contrastada contra el código fuente del backend (`InclusiON.Server`).  
> - `Verificado en código` = Regla implementada, activa y validada en el backend.  
> - `⚠️ Pendiente` = Regla definida por el negocio pero pendiente de codificación.

---

## Módulo: Acceso y Autorización

| Regla en lenguaje del cliente | Cómo el sistema la implementa | Qué pasa si se viola | Origen (HU) | Validada por |
|---|---|---|---|---|
| Un profesional solo puede ver y editar datos de los alumnos que tiene asignados. | Al recibir cualquier request sobre un alumno, el sistema verifica `CanAccessPersonAsync(professionalId, personId)`. Si falla, devuelve `403 Forbidden`. | El profesional accede a información clínica o pedagógica de un alumno que no está a su cargo. Violación de privacidad (Ley 25.326). | [HU-IN-172](../HU/HU-IN-172-autorizacion-por-recurso.md) | Verificado en código · `ResourceAuthorizationService.cs:47` |
| Un familiar solo puede ver información de las personas que representa activamente. | Cada consulta filtra forzosamente por `PersonRepresentative.IsActive = true` usando el `entityId` del JWT. | El familiar visualiza datos de un menor con el que no mantiene vínculo legal activo. | [HU-04](../HU/HU-04-acceso-familiar.md) | Verificado en código · `FamilyReportsQueryHandler.cs` |
| Un admin institucional solo puede gestionar usuarios y datos de su propia institución escolar. | El JWT incluye `institutionId`. El sistema filtra todas las consultas y operaciones directivas usando ese identificador (aislamiento multi-tenant estricto; no existe acceso global irrestricto). | El directivo de la institución A visualiza o modifica datos del personal o alumnos de la institución B. El backend rechaza con `403 Forbidden`. | [HU-11](../HU/HU-11-gestion-usuarios.md) · [HU-IN-172](../HU/HU-IN-172-autorizacion-por-recurso.md) | Verificado en código · `AdminUsersController.cs` |
| Una persona con discapacidad (estudiante) solo puede ver sus propias actividades asignadas y trayectoria. | Las consultas a `/api/my/activity-assignments` y `/api/my/roadmap` filtran por `entityId` del JWT, que corresponde al ID del estudiante autenticado. | El estudiante accede a actividades de otro compañero, alterando registros o perfiles ajenos. | [HU-06](../HU/HU-06-ejecucion-actividades.md) | Verificado en código · `MyAssignmentsQueryHandler.cs` |

---

## Módulo: Gestión de Profesionales

| Regla en lenguaje del cliente | Cómo el sistema la implementa | Qué pasa si se viola | Origen (HU) | Validada por |
|---|---|---|---|---|
| Un profesional creado directamente por el directivo escolar queda aprobado de inmediato con clave temporal. | `POST /api/professionals` (ruta directiva) crea la ficha con `Status = Approved`, genera clave provisoria y marca `MustChangePassword = true`. | Docentes habilitados administrativamente quedan demorados sin acceso al aula o gabinete. | [HU-01](../HU/HU-01-catalogos-configuracion.md) | Verificado en código · `ProfessionalsController.cs` |
| Un profesional que se auto-registra o preinscribe inicia en estado Pendiente y no puede operar hasta que el directivo lo apruebe. | `POST /api/professionals/register` crea al profesional con `Status = Pending`. Las rutas de asignación y reportes exigen `Status = Approved`. | Un profesional sin matrícula verificada por la institución crea actividades o accede a datos sensibles de menores. | [HU-IN-149](../HU/HU-IN-149-auto-registro-profesional.md) | Verificado en código · `ValidateProfessionalCommandHandler.cs` |
| Un profesional no puede validarse ni aprobarse a sí mismo. | `PUT /api/professionals/{id}/validate` verifica que el `adminUserId` del JWT no coincida con el `Professional.UserId` del profesional siendo validado. Si coincide, devuelve `403 Forbidden`. | Un docente con privilegios mixtos aprueba su propia solicitud de registro eludiendo el control directivo. | [HU-IN-150](../HU/HU-IN-150-validacion-admin.md) | Verificado en código · `ValidateProfessionalCommandHandler.cs:72` |
| El email y el número de matrícula habilitante deben ser únicos en todo el sistema. | Al crear o editar, el sistema valida unicidad en `Professional.Email` (vía `Identity User`) y `Professional.LicenseNumber`. Devuelve `409 Conflict` si ya existe. | Dos docentes comparten identidad o matrícula profesional, impidiendo la trazabilidad legal y deontológica de informes y diagnósticos. | [HU-IN-149](../HU/HU-IN-149-auto-registro-profesional.md) | Verificado en código · `ProfessionalsRepository.cs` |
| Un profesional sin actividad durante 90 días se suspende automáticamente. | El job programado compara `LastLoginDate` con la fecha actual. Si la diferencia supera 90 días, actualiza `Status = Suspended` y revoca tokens activos. | Un profesional desvinculado o inactivo conserva acceso activo a legajos escolares indefinidamente. | [HU-11](../HU/HU-11-gestion-usuarios.md) | Verificado en código · `SuspendInactiveProfessionalsJob.cs` |
| Un profesional con alumnos asignados o informes pendientes no puede ser dado de baja sin reasignación previa. | Al solicitar baja lógica, el sistema valida que no existan asignaciones activas (`ProfessionalPerson`) ni reportes en estado `Submitted`. Devuelve `409 Conflict`. | Alumnos quedan sin docente a cargo o informes pedagógicos quedan huérfanos sin firma responsable. | [HU-11](../HU/HU-11-gestion-usuarios.md) | Verificado en código · `DeactivateProfessionalCommandHandler.cs` |

---

## Módulo: Gestión de Personas con Discapacidad (Estudiantes)

| Regla en lenguaje del cliente | Cómo el sistema la implementa | Qué pasa si se viola | Origen (HU) | Validada por |
|---|---|---|---|---|
| El método de login de un estudiante debe ser compatible con su nivel de autonomía. | Al configurar el perfil de accesibilidad, el sistema valida que si `LoginMethod.RequiresSupervisor = true`, la persona tenga configurado `AutonomyLevel.RequiresSupervision = true`. Si no coinciden, devuelve `422 UnprocessableEntity`. | Un estudiante con alta necesidad de supervisión intenta ingresar sin asistencia o un alumno autónomo queda bloqueado por configuraciones erróneas. | [HU-14](../HU/HU-14-accesibilidad-por-persona.md) | Verificado en código · `UpdateAccessibilityProfileCommandHandler.cs` |
| Un supervisor autorizado activo debe estar configurado si el alumno usa login asistido. | Si `LoginMethod.Code = ASSISTED` (id=3), el campo `PersonWithDisability.SupervisorUserId` es obligatorio y debe corresponder a un docente/terapeuta activo con permiso `CanSuperviseLogin = true`. | El estudiante no puede iniciar sesión en la tableta AAC o se inician sesiones sin un adulto responsable presente. | [HU-14](../HU/HU-14-accesibilidad-por-persona.md) | Verificado en código · `PersonsController.cs` |
| Prohibición absoluta de exigir correo electrónico al estudiante. | La entidad `PersonWithDisability` no almacena ni exige email; la autenticación se realiza exclusivamente por PIN numérico visual de 4 dígitos o sesión asistida. | Se vulnera la privacidad del menor y se excluye a estudiantes sin lectoescritura convencional. | [HU-12](../HU/HU-12-login-pin.md) · [HU-14](../HU/HU-14-accesibilidad-por-persona.md) | Verificado en código · `CreatePersonCommandHandler.cs` |
| El legajo pedagógico y las respuestas del alumno son inmutables de por vida (prohibición de Hard Delete). | Ante desvinculación escolar, el alumno pasa a estado `Inactive / Egresado`. Se prohíbe el borrado físico (`HARD DELETE`) de legajos, tareas y diagnósticos. | Pérdida irreparable del historial evolutivo escolar y violación de normativas educativas y de salud. | [HU-11](../HU/HU-11-gestion-usuarios.md) | Verificado en código · `DeletePersonCommandHandler.cs` |

---

## Módulo: Invitaciones y Vinculación Familiar

| Regla en lenguaje del cliente | Cómo el sistema la implementa | Qué pasa si se viola | Origen (HU) | Validada por |
|---|---|---|---|---|
| Una invitación a tutor familiar es de un solo uso. | Al canjear un código de invitación, el sistema marca `Invitation.IsUsed = true` y estampa `UsedAt = UtcNow`. Si ya fue utilizada, devuelve `409 Conflict`. | Dos personas distintas utilizan el mismo enlace para vincularse al expediente del menor. | [HU-04](../HU/HU-04-acceso-familiar.md) | Verificado en código · `AcceptInvitationCommandHandler.cs` |
| Una invitación familiar expira indefectiblemente a los 7 días. | Al intentar el canje, el sistema verifica que `Invitation.ExpiresAt > DateTime.UtcNow`. Si venció, devuelve `410 Gone`. | Un familiar o tercero accede mediante un enlace caduco fuera del ciclo lectivo o tras un cambio de tutela legal. | [HU-04](../HU/HU-04-acceso-familiar.md) | Verificado en código · `AcceptInvitationCommandHandler.cs` |
| Un alumno matriculado nunca puede quedar sin al menos un tutor activo. | Al revocar la tutela de un familiar (`IsActive = false`), el sistema verifica que exista al menos otro representante activo vinculado. Devuelve `409 Conflict`. | Un menor queda institucionalmente desamparado sin responsable legal registrado para comunicaciones y consentimientos. | [HU-04](../HU/HU-04-acceso-familiar.md) | Verificado en código · `RevokeFamilyRepresentativeCommandHandler.cs` |

---

## Módulo: Actividades, Asignaciones y Trayectoria (Roadmap)

| Regla en lenguaje del cliente | Cómo el sistema la implementa | Qué pasa si se viola | Origen (HU) | Validada por |
|---|---|---|---|---|
| Solo se puede cancelar una asignación en estado Pendiente. | `PATCH /api/activity-assignments/{id}/cancel` verifica `StatusId == AssignmentStatuses.Pendiente`. Si ya está `EnProgreso` o `Completada`, devuelve `409 Conflict` con `ErrorCode.BusinessRuleViolation`. | Se intenta cancelar una actividad que el estudiante ya inició o completó, perdiendo el registro de intento y esfuerzo. | [HU-06](../HU/HU-06-ejecucion-actividades.md) | Verificado en código · `CancelActivityAssignmentCommandHandler.cs` |
| Una asignación completada es inmutable y no puede retornar a un estado previo. | El estado `Completada` es terminal en la máquina de estados. No existe ningún endpoint de transición que permita salir de ese estado. | El sistema permite alterar resultados ya consolidados, comprometiendo la auditoría pedagógica y clínica. | [HU-06](../HU/HU-06-ejecucion-actividades.md) | Verificado en código · `ActivityAssignmentStateMachine.cs` |
| Cada estudiante tiene exactamente un Roadmap oficial activo. | `POST /api/persons/{id}/roadmap` devuelve `409 Conflict` si ya existe un `PersonRoadmap` con `IsActive = true` para esa persona. Se inicializa automáticamente en la matrícula escolar con 10 niveles DUA. | Se crean múltiples planes curriculares contradictorios para el mismo alumno. | [HU-05](../HU/HU-05-roadmap.md) | Verificado en código · `RoadmapInitializer.cs` |
| No se puede alterar el orden de estaciones del Roadmap que ya fueron ejecutadas. | Al reordenar nodos, el sistema valida que las actividades modificadas no cuenten con sesiones previas finalizadas (`ActivityResponses`). Devuelve `409 Conflict`. | Se rompe la coherencia cronológica y secuencial de la trayectoria evolutiva del alumno. | [HU-05](../HU/HU-05-roadmap.md) | Verificado en código · `ReorderRoadmapActivitiesCommandHandler.cs` |

---

## Módulo: Motor Adaptativo (MDA)

| Regla en lenguaje del cliente | Cómo el sistema la implementa | Qué pasa si se viola | Origen (HU) | Validada por |
|---|---|---|---|---|
| El motor no puede subir la dificultad más allá del máximo configurado, ni bajarla por debajo del mínimo. | Antes de persistir el ajuste, el sistema verifica que `newLevel ∈ [AdaptiveEngineConfig.MinDifficultyLevel, MaxDifficultyLevel]`. Clampea estrictamente el valor dentro del rango configurado (escala 1 a 10). | La persona recibe actividades de dificultad imposible o trivial fuera del umbral pedagógico. | [HU-10](../HU/HU-10-motor-adaptativo.md) | Verificado en código · `AdaptiveAdjustmentAgent.cs:101-105` |
| El motor adaptativo solo actúa si está habilitado para esa actividad del roadmap. | Antes de ejecutar el pipeline de ajuste, se verifica `AdaptiveEngineConfig.IsEnabled == true`. Si no existe configuración o está deshabilitada, no hay ajuste algorítmico. | El motor ajusta arbitrariamente actividades que el profesional configuró en modalidad estática o manual. | [HU-10](../HU/HU-10-motor-adaptativo.md) | Verificado en código · `AdaptiveAdjustmentAgent.cs:34` |
| Intervención inmediata y alerta en tiempo real ante detección de frustración del alumno. | Si `FrustrationLevel >= FrustrationThreshold` (escala 1 a 5), el agente activa `FrustrationIntervention`, reduce la dificultad al mínimo configurado y despacha notificación push SignalR al profesional a cargo. | El estudiante sufre sobrecarga emocional o abandono de tarea sin asistencia pedagógica oportuna. | [HU-10](../HU/HU-10-motor-adaptativo.md) | Verificado en código · `AdaptiveAdjustmentAgent.cs:76-80, 127-148` |

---

## Módulo: Diagnósticos y Reportes Escolares

| Regla en lenguaje del cliente | Cómo el sistema la implementa | Qué pasa si se viola | Origen (HU) | Validada por |
|---|---|---|---|---|
| Un informe en revisión (`Submitted`) o aprobado (`Approved`) no puede editarse directamente. | `PUT /api/reports/{id}` verifica que `Status` sea `Draft` o `Rejected`. Si se encuentra en `Submitted` o `Approved`, devuelve `422 UnprocessableEntity`. | El profesional modifica retroactivamente un reporte que ya está siendo evaluado por la Dirección o que ya fue notificado a los padres. | [HU-08](../HU/HU-08-diagnosticos-reportes.md) | Verificado en código · `UpdateReportCommandHandler.cs:52` |
| Solo el profesional autor del reporte puede enviarlo a revisión directiva. | `PATCH /api/reports/{id}/submit` verifica que el `ProfessionalId` del reporte coincida con el `entityId` del token JWT autenticado. Devuelve `403 Forbidden` si difiere. | Un profesional remite a revisión un informe ajeno sin la firma ni consentimiento del profesional que evaluó al alumno. | [HU-08](../HU/HU-08-diagnosticos-reportes.md) | Verificado en código · `SubmitReportCommandHandler.cs` |
| Un reporte rechazado por la Dirección pasa a estado Rechazado con observaciones obligatorias y habilita su corrección. | `PATCH /api/reports/{id}/reject` exige el motivo (`Comment` no vacío), fija `Status = Rejected` y guarda `AdminComment`. `UpdateReportCommandHandler` permite al autor editarlo y reenviarlo. | El directivo rechaza un informe sin justificación formal, o el informe queda bloqueado impidiendo subsanar las observaciones. | [HU-08](../HU/HU-08-diagnosticos-reportes.md) | Verificado en código · `RejectReportCommandHandler.cs:52-56` y `UpdateReportCommandHandler.cs:52` |
| Un familiar solo puede ver reportes aprobados, y su apertura registra acuse de lectura. | `GET /api/reports/family` filtra estrictamente `Status == Approved`. Al abrir el informe, se ejecuta `PATCH /api/reports/{id}/mark-read`, estampando `isReadByFamily = true`. | La familia accede a apreciaciones preliminares no avaladas por la Dirección, o la institución carece de constancia de recepción formal. | [HU-04](../HU/HU-04-acceso-familiar.md) · [HU-08](../HU/HU-08-diagnosticos-reportes.md) | Verificado en código · `ReportsController.cs:98-125, 308-319` |
| Solo el profesional autor puede modificar un diagnóstico funcional propio. | `PUT /api/diagnoses/{id}` valida que `Diagnosis.ProfessionalId == currentProfessionalId`. Para otros profesionales asignados, se presenta en modo solo lectura. | Se vulnera la autoría clínica y la opinión especializada del profesional evaluador original. | [HU-08](../HU/HU-08-diagnosticos-reportes.md) | Verificado en código · `UpdateDiagnosisCommandHandler.cs:63` |

---

## Módulo: Datos Clínicos y Seguridad

| Regla en lenguaje del cliente | Cómo el sistema la implementa | Qué pasa si se viola | Origen (HU) | Validada por |
|---|---|---|---|---|
| Los datos clínicos sensibles (diagnósticos, observaciones) deben almacenarse cifrados en reposo. | Los campos marcados con `[Encrypted]` se cifran automáticamente con **AES-256-GCM** antes de persistirse en la base de datos PostgreSQL. El descifrado es transparente en memoria al autorizarse la lectura. | Una copia de seguridad o acceso no autorizado a la base de datos expone diagnósticos médicos y de discapacidad en texto plano (Violación Ley 25.326). | [HU-IN-173](../HU/HU-IN-173-hardening-seguridad.md) | Verificado en código · `AesGcmEncryptionService.cs`, `Diagnosis.cs:35` |
| Cada acceso a datos de un alumno queda registrado en auditoría inalterable. | El middleware de autorización registra cada operación en `AccessAudit` con resultado `Allowed`/`Denied`, usuario, persona accedida y sellado de tiempo UTC. | Imposibilidad de determinar la trazabilidad ante peritajes legales o auditorías ministeriales sobre quién consultó datos de menores protegidos. | [HU-IN-172](../HU/HU-IN-172-autorizacion-por-recurso.md) | Verificado en código · `AccessAuditLogger.cs` |
