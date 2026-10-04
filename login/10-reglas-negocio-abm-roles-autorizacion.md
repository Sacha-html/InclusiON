# 📋 Reglas de Negocio, ABMs Justificados y Arquitectura de Roles — InclusiON

**Práctica Profesionalizante II — Institución Cervantes**  
**Proyecto:** InclusiON — Plataforma de Gestión Educativa y Terapéutica Inclusiva  
**Artefactos integrados:** 
- Artefacto 07: Reglas de Negocio (Validado en Código)
- Justificación de ABMs de Gestión Institucional (Director)
- IN-188: Justificación de Roles del Sistema respecto a los Actores de Negocio
- IN-190: Justificación de Permisos y Modelo de Autorización en 3 Capas
- IN-193: Decisión Arquitectónica de Roles (ADR)
- CU-48: Configuración del Motor Adaptativo (MDA)
**Fecha de consolidación:** Octubre 2026

---

## ÍNDICE GENERAL

1. [Reglas de Negocio Validadas en Código (Artefacto 07)](#1-reglas-de-negocio-validadas-en-código-artefacto-07)
   - 1.1. Definición y Metodología de Verificación
   - 1.2. Módulo: Acceso y Autorización
   - 1.3. Módulo: Gestión de Profesionales
   - 1.4. Módulo: Gestión de Personas con Discapacidad (Estudiantes)
   - 1.5. Módulo: Invitaciones y Vinculación Familiar
   - 1.6. Módulo: Actividades, Asignaciones y Trayectoria (Roadmap)
   - 1.7. Módulo: Motor Adaptativo (MDA)
   - 1.8. Módulo: Diagnósticos y Reportes Escolares
   - 1.9. Módulo: Datos Clínicos y Seguridad
2. [Justificación Funcional de ABMs Escolares (Equipo Directivo)](#2-justificación-funcional-de-abms-escolares-equipo-directivo)
   - 2.1. Matriz de Justificación de los 10 ABMs
   - 2.2. Reglas de Cuidado y Prevención de Desastres Operativos
3. [Arquitectura de Roles y Modelo de Autorización (IN-188 / IN-190 / IN-193)](#3-arquitectura-de-roles-y-modelo-de-autorización-in-188--in-190--in-193)
   - 3.1. Ecosistema de 4 Actores de Negocio y 4 Roles Técnicos
   - 3.2. Modelo de Autorización en Tres Capas Apiladas
   - 3.3. Principio Rector de Mínimo Privilegio (Ley 25.326)
   - 3.4. Justificación Detallada por Rol
4. [Motor Adaptativo (MDA) — Caso de Uso CU-48](#4-motor-adaptativo-mda--caso-de-uso-cu-48)
   - 4.1. Ficha del Caso de Uso CU-48
   - 4.2. Flujo Principal, Alternativos y Reglas Algorítmicas

---

## 1. Reglas de Negocio Validadas en Código (Artefacto 07)

### 1.1. Definición y Metodología de Verificación

Una **regla de negocio** es una restricción o condición imperativa que el dominio institucional impone y que el sistema informático debe garantizar de forma irrestricta. No representa un paso cronológico de un proceso, sino una invariante que aplica transversalmente.

* **Trazabilidad:** Cada regla deriva directamente de una Historia de Usuario (HU) relevada con los directivos y profesionales escolares.
* **Verificación:** La columna *Validada por* certifica el contraste contra el código fuente activo del backend (`InclusiON.Server`):
  * `Verificado en código`: Regla activa, testeada e implementada en controladores, handlers o domain services.
  * `Deprecada / Corregida`: Regla ajustada para alinearse al modelo de seguridad institucional (ej. eliminación del rol global genérico y corrección del ciclo de rechazo de informes).

---

### 1.2. Módulo: Acceso y Autorización

| Regla en lenguaje del cliente | Cómo el sistema la implementa | Qué pasa si se viola | Origen (HU) | Validada por |
|---|---|---|---|---|
| Un profesional solo puede ver y editar datos de los alumnos que tiene asignados. | Al recibir cualquier request sobre un alumno, el sistema verifica `CanAccessPersonAsync(professionalId, personId)`. Si falla, devuelve `403 Forbidden`. | El profesional accede a información clínica o pedagógica de un alumno que no está a su cargo. Violación de privacidad (Ley 25.326). | [HU-IN-172](../InclusiON.Documents/Docs/HU/HU-IN-172-autorizacion-por-recurso.md) | Verificado en código — `ResourceAuthorizationService.cs:47` |
| Un familiar solo puede ver información de las personas que representa activamente. | Cada consulta filtra forzosamente por `PersonRepresentative.IsActive = true` usando el `entityId` del JWT. | El familiar visualiza datos de un menor con el que no mantiene vínculo legal activo. | [HU-04](../InclusiON.Documents/Docs/HU/HU-04-acceso-familiar.md) | Verificado en código — `FamilyReportsQueryHandler.cs` |
| Un admin institucional solo puede gestionar usuarios y datos de su propia sede escolar. | El JWT incluye `institutionId`. El sistema filtra todas las consultas y operaciones directivas usando ese identificador (aislamiento multi-tenant estricto; no existe acceso global irrestricto). | El directivo de la institución A visualiza o modifica datos del personal o alumnos de la institución B. El backend rechaza con `403 Forbidden`. | [HU-11](../InclusiON.Documents/Docs/HU/HU-11-gestion-usuarios.md) · [HU-IN-172](../InclusiON.Documents/Docs/HU/HU-IN-172-autorizacion-por-recurso.md) | Verificado en código — `AdminUsersController.cs` |
| Una persona con discapacidad (estudiante) solo puede ver sus propias actividades asignadas y trayectoria. | Las consultas a `/api/my/activity-assignments` y `/api/my/roadmap` filtran por el `entityId` del JWT, correspondiente al ID del estudiante autenticado. | El estudiante accede a actividades de otro compañero, alterando registros o métricas ajenas. | [HU-06](../InclusiON.Documents/Docs/HU/HU-06-ejecucion-actividades.md) | Verificado en código — `MyAssignmentsQueryHandler.cs` |

---

### 1.3. Módulo: Gestión de Profesionales

| Regla en lenguaje del cliente | Cómo el sistema la implementa | Qué pasa si se viola | Origen (HU) | Validada por |
|---|---|---|---|---|
| Un profesional creado directamente por el directivo escolar queda aprobado de inmediato con clave provisoria. | `POST /api/professionals` (ruta directiva) crea la ficha con `Status = Approved`, genera clave temporal y marca `MustChangePassword = true`. | Docentes habilitados formalmente quedan demorados sin acceso al aula o gabinete. | [HU-01](../InclusiON.Documents/Docs/HU/HU-01-catalogos-configuracion.md) | Verificado en código — `ProfessionalsController.cs` |
| Un profesional que se auto-registra o preinscribe inicia en estado Pendiente y no opera hasta que el directivo lo apruebe. | `POST /api/professionals/register` crea al profesional con `Status = Pending`. Las rutas de asignación y reportes exigen `Status = Approved`. | Un profesional sin matrícula verificada por la institución crea actividades o accede a legajos confidenciales. | [HU-IN-149](../InclusiON.Documents/Docs/HU/HU-IN-149-auto-registro-profesional.md) | Verificado en código — `ValidateProfessionalCommandHandler.cs` |
| Un profesional no puede validarse ni aprobarse a sí mismo. | `PUT /api/professionals/{id}/validate` verifica que el `adminUserId` del JWT no coincida con el `Professional.UserId` del profesional siendo evaluado. Si coincide, devuelve `403 Forbidden`. | Un docente con privilegios mixtos aprueba su propia solicitud de ingreso eludiendo el control directivo. | [HU-IN-150](../InclusiON.Documents/Docs/HU/HU-IN-150-validacion-admin.md) | Verificado en código — `ValidateProfessionalCommandHandler.cs:72` |
| El email y el número de matrícula habilitante deben ser únicos en todo el sistema. | Al crear o editar, el sistema valida unicidad en `Professional.Email` (vía `Identity User`) y `Professional.LicenseNumber`. Devuelve `409 Conflict` si ya existe. | Dos docentes comparten identidad o matrícula profesional, impidiendo la trazabilidad legal y deontológica de informes. | [HU-IN-149](../InclusiON.Documents/Docs/HU/HU-IN-149-auto-registro-profesional.md) | Verificado en código — `ProfessionalsRepository.cs` |
| Un profesional sin actividad durante 90 días se suspende preventivamente. | El job programado compara `LastLoginDate` con la fecha actual. Si la diferencia supera 90 días, actualiza `Status = Suspended` y revoca tokens activos. | Un profesional desvinculado o inactivo conserva acceso activo a legajos escolares indefinidamente. | [HU-11](../InclusiON.Documents/Docs/HU/HU-11-gestion-usuarios.md) | Verificado en código — `SuspendInactiveProfessionalsJob.cs` |
| Un profesional con alumnos asignados o informes pendientes no puede ser dado de baja sin reasignación previa. | Al solicitar baja lógica, el sistema valida que no existan asignaciones activas (`ProfessionalPerson`) ni reportes en estado `Submitted`. Devuelve `409 Conflict`. | Alumnos quedan sin docente a cargo o informes pedagógicos quedan huérfanos sin firma responsable. | [HU-11](../InclusiON.Documents/Docs/HU/HU-11-gestion-usuarios.md) | Verificado en código — `DeactivateProfessionalCommandHandler.cs` |

---

### 1.4. Módulo: Gestión de Personas con Discapacidad (Estudiantes)

| Regla en lenguaje del cliente | Cómo el sistema la implementa | Qué pasa si se viola | Origen (HU) | Validada por |
|---|---|---|---|---|
| El método de login de un estudiante debe ser compatible con su nivel de autonomía. | Al configurar el perfil de accesibilidad, el sistema valida que si `LoginMethod.RequiresSupervisor = true`, la persona tenga configurado `AutonomyLevel.RequiresSupervision = true`. Si no coinciden, devuelve `422 UnprocessableEntity`. | Un estudiante con alta necesidad de supervisión intenta ingresar sin asistencia o un alumno autónomo queda bloqueado por configuraciones erróneas. | [HU-14](../InclusiON.Documents/Docs/HU/HU-14-accesibilidad-por-persona.md) | Verificado en código — `UpdateAccessibilityProfileCommandHandler.cs` |
| Un supervisor autorizado activo debe estar configurado si el alumno usa login asistido. | Si `LoginMethod.Code = ASSISTED` (id=3), el campo `PersonWithDisability.SupervisorUserId` es obligatorio y debe corresponder a un docente activo con permiso `CanSuperviseLogin = true`. | El estudiante no puede iniciar sesión en la tableta AAC o se inician sesiones sin un adulto responsable presente. | [HU-14](../InclusiON.Documents/Docs/HU/HU-14-accesibilidad-por-persona.md) | Verificado en código — `PersonsController.cs` |
| Prohibición absoluta de exigir correo electrónico al estudiante. | La entidad `PersonWithDisability` no almacena ni exige email; la autenticación se realiza exclusivamente por PIN numérico visual de 4 dígitos o sesión asistida. | Se vulnera la privacidad del menor y se excluye a estudiantes sin lectoescritura convencional. | [HU-12](../InclusiON.Documents/Docs/HU/HU-12-login-pin.md) · [HU-14](../InclusiON.Documents/Docs/HU/HU-14-accesibilidad-por-persona.md) | Verificado en código — `CreatePersonCommandHandler.cs` |
| El legajo pedagógico y las respuestas del alumno son inmutables de por vida (prohibición de Hard Delete). | Ante desvinculación escolar, el alumno pasa a estado `Inactive / Egresado`. Se prohíbe el borrado físico (`HARD DELETE`) de legajos, tareas y diagnósticos. | Pérdida irreparable del historial evolutivo escolar y violación de normativas educativas y de salud. | [HU-11](../InclusiON.Documents/Docs/HU/HU-11-gestion-usuarios.md) | Verificado en código — `DeletePersonCommandHandler.cs` |

---

### 1.5. Módulo: Invitaciones y Vinculación Familiar

| Regla en lenguaje del cliente | Cómo el sistema la implementa | Qué pasa si se viola | Origen (HU) | Validada por |
|---|---|---|---|---|
| Una invitación a tutor familiar es de un solo uso. | Al canjear un código de invitación, el sistema marca `Invitation.IsUsed = true` y estampa `UsedAt = UtcNow`. Si ya fue utilizada, devuelve `409 Conflict`. | Dos personas distintas utilizan el mismo enlace para vincularse al expediente del menor. | [HU-04](../InclusiON.Documents/Docs/HU/HU-04-acceso-familiar.md) | Verificado en código — `AcceptInvitationCommandHandler.cs` |
| Una invitación familiar expira indefectiblemente a los 7 días. | Al intentar el canje, el sistema verifica que `Invitation.ExpiresAt > DateTime.UtcNow`. Si venció, devuelve `410 Gone`. | Un familiar o tercero accede mediante un enlace caduco fuera del ciclo lectivo o tras un cambio de tutela legal. | [HU-04](../InclusiON.Documents/Docs/HU/HU-04-acceso-familiar.md) | Verificado en código — `AcceptInvitationCommandHandler.cs` |
| Un alumno matriculado nunca puede quedar sin al menos un tutor activo. | Al revocar la tutela de un familiar (`IsActive = false`), el sistema verifica que exista al menos otro representante activo vinculado. Devuelve `409 Conflict`. | Un menor queda institucionalmente desamparado sin responsable legal registrado para comunicaciones y consentimientos. | [HU-04](../InclusiON.Documents/Docs/HU/HU-04-acceso-familiar.md) | Verificado en código — `RevokeFamilyRepresentativeCommandHandler.cs` |

---

### 1.6. Módulo: Actividades, Asignaciones y Trayectoria (Roadmap)

| Regla en lenguaje del cliente | Cómo el sistema la implementa | Qué pasa si se viola | Origen (HU) | Validada por |
|---|---|---|---|---|
| Solo se puede cancelar una asignación en estado Pendiente. | `PATCH /api/activity-assignments/{id}/cancel` verifica `StatusId == AssignmentStatuses.Pendiente`. Si ya está `EnProgreso` o `Completada`, devuelve `409 Conflict` con `ErrorCode.BusinessRuleViolation`. | Se intenta cancelar una actividad que el estudiante ya inició o completó, perdiendo el registro de intento y esfuerzo. | [HU-06](../InclusiON.Documents/Docs/HU/HU-06-ejecucion-actividades.md) | Verificado en código — `CancelActivityAssignmentCommandHandler.cs` |
| Una asignación completada es inmutable y no puede retornar a un estado previo. | El estado `Completada` es terminal en la máquina de estados. No existe ningún endpoint de transición que permita salir de ese estado. | El sistema permite alterar resultados ya consolidados, comprometiendo la auditoría pedagógica y clínica. | [HU-06](../InclusiON.Documents/Docs/HU/HU-06-ejecucion-actividades.md) | Verificado en código — `ActivityAssignmentStateMachine.cs` |
| Cada estudiante tiene exactamente un Roadmap oficial activo. | `POST /api/persons/{id}/roadmap` devuelve `409 Conflict` si ya existe un `PersonRoadmap` con `IsActive = true` para esa persona. Se inicializa automáticamente en la matrícula con 10 niveles DUA. | Se crean múltiples planes curriculares contradictorios para el mismo alumno. | [HU-05](../InclusiON.Documents/Docs/HU/HU-05-roadmap.md) | Verificado en código — `RoadmapInitializer.cs` |
| No se puede alterar el orden de estaciones del Roadmap que ya fueron ejecutadas. | Al reordenar nodos, el sistema valida que las actividades modificadas no cuenten con sesiones previas finalizadas (`ActivityResponses`). Devuelve `409 Conflict`. | Se rompe la coherencia cronológica y secuencial de la trayectoria evolutiva del alumno. | [HU-05](../InclusiON.Documents/Docs/HU/HU-05-roadmap.md) | Verificado en código — `ReorderRoadmapActivitiesCommandHandler.cs` |

---

### 1.7. Módulo: Motor Adaptativo (MDA)

| Regla en lenguaje del cliente | Cómo el sistema la implementa | Qué pasa si se viola | Origen (HU) | Validada por |
|---|---|---|---|---|
| El motor no puede subir la dificultad más allá del máximo configurado, ni bajarla por debajo del mínimo. | Antes de persistir el ajuste, el sistema verifica que `newLevel ∈ [AdaptiveEngineConfig.MinDifficultyLevel, MaxDifficultyLevel]`. Clampea estrictamente el valor dentro del rango configurado (escala 1 a 10). | La persona recibe actividades de dificultad imposible o trivial fuera del umbral pedagógico. | [HU-10](../InclusiON.Documents/Docs/HU/HU-10-motor-adaptativo.md) | Verificado en código — `AdaptiveAdjustmentAgent.cs:101-105` |
| El motor adaptativo solo actúa si está habilitado para esa actividad del roadmap. | Antes de ejecutar el pipeline de ajuste, se verifica `AdaptiveEngineConfig.IsEnabled == true`. Si no existe configuración o está deshabilitada, no hay ajuste algorítmico. | El motor ajusta arbitrariamente actividades que el profesional configuró en modalidad estática o manual. | [HU-10](../InclusiON.Documents/Docs/HU/HU-10-motor-adaptativo.md) | Verificado en código — `AdaptiveAdjustmentAgent.cs:34` |
| Intervención inmediata y alerta en tiempo real ante detección de frustración del alumno. | Si `FrustrationLevel >= FrustrationThreshold` (escala 1 a 5), el agente activa `FrustrationIntervention`, reduce la dificultad al mínimo configurado y despacha notificación push SignalR al profesional a cargo. | El estudiante sufre sobrecarga emocional o abandono de tarea sin asistencia pedagógica oportuna. | [HU-10](../InclusiON.Documents/Docs/HU/HU-10-motor-adaptativo.md) | Verificado en código — `AdaptiveAdjustmentAgent.cs:76-80, 127-148` |

---

### 1.8. Módulo: Diagnósticos y Reportes Escolares

| Regla en lenguaje del cliente | Cómo el sistema la implementa | Qué pasa si se viola | Origen (HU) | Validada por |
|---|---|---|---|---|
| Un informe en revisión (`Submitted`) o aprobado (`Approved`) no puede editarse directamente. | `PUT /api/reports/{id}` verifica que `Status` sea `Draft` o `Rejected`. Si se encuentra en `Submitted` o `Approved`, devuelve `422 UnprocessableEntity`. | El profesional modifica retroactivamente un reporte que ya está siendo evaluado por la Dirección o que ya fue notificado a los padres. | [HU-08](../InclusiON.Documents/Docs/HU/HU-08-diagnosticos-reportes.md) | Verificado en código — `UpdateReportCommandHandler.cs:52` |
| Solo el profesional autor del reporte puede enviarlo a revisión directiva. | `PATCH /api/reports/{id}/submit` verifica que el `ProfessionalId` del reporte coincida con el `entityId` del token JWT autenticado. Devuelve `403 Forbidden` si difiere. | Un profesional remite a revisión un informe ajeno sin la firma ni consentimiento del profesional evaluador original. | [HU-08](../InclusiON.Documents/Docs/HU/HU-08-diagnosticos-reportes.md) | Verificado en código — `SubmitReportCommandHandler.cs` |
| Un reporte rechazado por la Dirección pasa a estado Rechazado con observaciones obligatorias y habilita su corrección. | `PATCH /api/reports/{id}/reject` exige el motivo (`Comment` no vacío), fija `Status = Rejected` y guarda `AdminComment`. `UpdateReportCommandHandler` permite al autor editarlo y reenviarlo. | El directivo rechaza un informe sin justificación formal, o el informe queda bloqueado impidiendo subsanar las observaciones. | [HU-08](../InclusiON.Documents/Docs/HU/HU-08-diagnosticos-reportes.md) | Verificado en código — `RejectReportCommandHandler.cs:52-56` y `UpdateReportCommandHandler.cs:52` |
| Un familiar solo puede ver reportes aprobados, y su apertura registra acuse de lectura. | `GET /api/reports/family` filtra estrictamente `Status == Approved`. Al abrir el informe, se ejecuta `PATCH /api/reports/{id}/mark-read`, estampando `isReadByFamily = true`. | La familia accede a apreciaciones preliminares no avaladas por la Dirección, o la institución carece de constancia de recepción formal. | [HU-04](../InclusiON.Documents/Docs/HU/HU-04-acceso-familiar.md) · [HU-08](../InclusiON.Documents/Docs/HU/HU-08-diagnosticos-reportes.md) | Verificado en código — `ReportsController.cs:98-125, 308-319` |
| Solo el profesional autor puede modificar un diagnóstico funcional propio. | `PUT /api/diagnoses/{id}` valida que `Diagnosis.ProfessionalId == currentProfessionalId`. Para otros profesionales asignados, se presenta en modo solo lectura. | Se vulnera la autoría clínica y la opinión especializada del profesional evaluador original. | [HU-08](../InclusiON.Documents/Docs/HU/HU-08-diagnosticos-reportes.md) | Verificado en código — `UpdateDiagnosisCommandHandler.cs:63` |

---

### 1.9. Módulo: Datos Clínicos y Seguridad

| Regla en lenguaje del cliente | Cómo el sistema la implementa | Qué pasa si se viola | Origen (HU) | Validada por |
|---|---|---|---|---|
| Los datos clínicos sensibles (diagnósticos, observaciones) deben almacenarse cifrados en reposo. | Los campos marcados con `[Encrypted]` se cifran automáticamente con **AES-256-GCM** antes de persistirse en la base de datos PostgreSQL. El descifrado es transparente en memoria al autorizarse la lectura. | Una copia de seguridad o acceso no autorizado a la base de datos expone diagnósticos médicos y de discapacidad en texto plano (Violación Ley 25.326). | [HU-IN-173](../InclusiON.Documents/Docs/HU/HU-IN-173-hardening-seguridad.md) | Verificado en código — `AesGcmEncryptionService.cs`, `Diagnosis.cs:35` |
| Cada acceso a datos de un alumno queda registrado en auditoría inalterable. | El middleware de autorización registra cada operación en `AccessAudit` con resultado `Allowed`/`Denied`, usuario, persona accedida y sellado de tiempo UTC. | Imposibilidad de determinar la trazabilidad ante peritajes legales o auditorías ministeriales sobre quién consultó datos de menores protegidos. | [HU-IN-172](../InclusiON.Documents/Docs/HU/HU-IN-172-autorizacion-por-recurso.md) | Verificado en código — `AccessAuditLogger.cs` |

---

## 2. Justificación Funcional de ABMs Escolares (Equipo Directivo)

El análisis del rol directivo escolar arrojó 10 ABMs (Altas, Bajas, Modificaciones y Consultas) esenciales para la gobernanza de la institución especial e inclusiva:

### 2.1. Matriz de Justificación de los 10 ABMs

| ID | ABM | Actor que lo opera | ¿Para qué lo usa en su trabajo diario? | ¿Qué problema real de la escuela resuelve? | Valor para la Institución |
|---|---|---|---|---|---|
| **01** | **Perfil y Sede de la Institución** | Administrador (Director) | Configura y actualiza datos oficiales de la sede (nombre, CUE/habilitación ministerial, dirección, teléfonos y logo). | Evita papelería desactualizada: cada informe o constancia emitido incluye el membrete legal oficial. | **Identidad formal:** Respaldo ministerial y formalidad ante obras sociales y ministerios. |
| **02** | **Cuentas de Usuario y Seguridad** | Administrador (Director) | Desbloquea cuentas de docentes, genera claves temporales y da de baja accesos de personal desvinculado. | La rotación docente o pérdida de claves deja a la dirección sin control de accesos. | **Gobernanza escolar:** Control perimetral inmediato ante renuncias o licencias. |
| **03** | **Personal Docente y Terapéutico** | Administrador (Director) | Da de alta psicopedagogos, terapeutas y docentes, registrando su matrícula provincial habilitante. | Ingreso de personal no verificado a expedientes de menores protegidos. | **Seguridad profesional:** Certeza legal de que quien evalúa es idóneo y matriculado. |
| **04** | **Legajos de Alumnos (Estudiantes)** | Administrador (Director) | Matricula a nuevos alumnos, define métodos de acceso (PIN/Asistido) y vincula tutores legales. | La dispersión de fichas en carpetas de papel y pérdida de datos ante cambios de sala. | **Legajo centralizado:** Registro pedagógico y clínico unificado de por vida. |
| **05** | **Trayectorias Educativas (Roadmap)** | Administrador (Director) | Supervisa los senderos DUA de 10 niveles y autoriza adecuaciones curriculares significativas. | Falta de visión global sobre si los alumnos avanzan según el plan pedagógico anual. | **Garantía de inclusión:** Asegura la continuidad pedagógica sin estancamiento. |
| **06** | **Catálogo de Actividades y Juegos** | Administrador (Director) | Aprueba o archiva plantillas de actividades multisensoriales creadas por el equipo docente. | Actividades obsoletas o no adaptadas al Diseño Universal para el Aprendizaje (DUA). | **Calidad curricular:** Banco de recursos validado institucionalmente. |
| **07** | **Comunicaciones y Circulares** | Administrador (Director) | Emite avisos institucionales, fechas de reuniones de gabinete y suspensiones de actividades a familias. | El cuaderno de comunicaciones físico se pierde, rompe o no es leído a tiempo por los tutores. | **Canal formal auditado:** Notificaciones con acuse de recepción y lectura. |
| **08** | **Auditoría y Aprobación de Informes** | Administrador (Director) | Revisa, aprueba con firma digital o rechaza con observaciones los informes evolutivos de los docentes. | Informes con errores conceptuales o de redacción entregados prematuramente a padres o mutuales. | **Control de calidad:** Ningún documento sale de la escuela sin aval directivo. |
| **09** | **Catálogos de Discapacidad y DUA** | Administrador (Director) | Mantiene actualizados los tipos de discapacidad, escalas sensoriales y configuraciones de accesibilidad. | Criterios diagnósticos dispares entre profesionales de distintas disciplinas del gabinete. | **Estandarización diagnóstica:** Lenguaje común interdisciplinario en toda la escuela. |
| **10** | **Auditoría y Trazabilidad Forense** | Administrador (Director) | Monitorea quién ingresó a ver diagnósticos, a qué hora y desde qué dispositivo. | Desconocimiento ante denuncias de filtración de información médica confidencial. | **Cumplimiento Ley 25.326:** Respaldo probatorio inalterable ante inspecciones ministeriales. |

---

### 2.2. Reglas de Cuidado y Prevención de Desastres Operativos

1. **Bloqueo de Cierre Institucional:** No se puede dar de baja una sede escolar si registra alumnos o docentes activos matriculados. Se exige el pase, egreso o reasignación previa de la totalidad de las personas.
2. **Inmutabilidad del CUE y Códigos Oficiales:** Una vez creada la institución, su Código Único de Establecimiento (CUE) no puede modificarse para prevenir fraude registral.
3. **Bloqueo de Baja Docente con Carga Asignada:** Se prohíbe desactivar la cuenta de un profesional si mantiene alumnos a cargo o reportes en estado `Submitted` pendientes de dictamen.
4. **Prohibición de Eliminación de Legajos de Alumnos:** El egreso de un alumno es siempre una baja lógica (`Inactive / Egresado`). El borrado físico de la historia clínica o escolar está estrictamente vedado por regulaciones educativas.

---

## 3. Arquitectura de Roles y Modelo de Autorización (IN-188 / IN-190 / IN-193)

### 3.1. Ecosistema de 4 Actores de Negocio y 4 Roles Técnicos

La arquitectura del sistema descarta intermediarios globales genéricos y adopta un modelo centrado en los **4 actores canónicos de la comunidad educativa inclusiva**:

```
        ┌────────────────────────────────────────────────────────┐
        │            COMUNIDAD EDUCATIVA INCLUSIÒN               │
        └───────────────────────────┬────────────────────────────┘
                                    │
         ┌───────────────┬──────────┴────────┬───────────────┐
         ▼               ▼                   ▼               ▼
   [Estudiante]    [Profesional]         [Familia]     [Equipo Directivo]
   (Persona con     (Docente /        (Padre / Madre /      (Director /
   Discapacidad)    Terapeuta)            Tutor)        Admin Institucional)
         │               │                   │               │
         ▼               ▼                   ▼               ▼
   Rol: persona    Rol: profesional    Rol: familia     Rol: admin
   (Portal AAC)   (Gestión y Aula)    (Portal Familias) (Gestión Escolar)
```

| Actor de Negocio | Rol Técnico en Sistema | Claim JWT emitido | Alcance de Autorización |
|---|---|---|---|
| **Persona con Discapacidad** | `persona` | `role: persona`, `entityId: <PersonId>` | Portal adaptativo AAC. Únicamente sus actividades asignadas y su trayectoria personal. |
| **Profesional Docente / Terapeuta** | `profesional` | `role: profesional`, `entityId: <ProfessionalId>` | Gestión de aula, roadmap, ejecución de sesiones y emisión de diagnósticos/reportes de sus alumnos asignados. |
| **Familia / Tutor Legal** | `familia` | `role: familia`, `entityId: <RepresentativeId>` | Portal de tutores. Consulta de progreso e informes aprobados de los menores que representa activamente. |
| **Equipo Directivo Escolar** | `admin` | `role: admin`, `institutionId: <SchoolId>` | Gestión institucional completa, matrículas, usuarios, auditoría e informes dentro de su propia sede escolar. |

---

### 3.2. Modelo de Autorización en Tres Capas Apiladas

InclusiON ejecuta una validación secuencial rigurosa en cada petición entrante:

```
[Request Entrante]
        │
        ▼
┌───────────────────────────────────────────────────────────────────────────┐
│ [Capa 1] Autenticación JWT (Identity)                                     │
│ ¿Quién es el usuario y está su firma criptográfica vigente?               │
│ ➔ Si falla: 401 Unauthorized                                              │
└─────────────────────────────────────┬─────────────────────────────────────┘
                                      │ OK
                                      ▼
┌───────────────────────────────────────────────────────────────────────────┐
│ [Capa 2] Política de Rol y Permiso (Claims-based Authorization)           │
│ ¿El rol técnico cuenta con el permiso para invocar este módulo/endpoint?  │
│ ➔ Si falla: 403 Forbidden                                                 │
└─────────────────────────────────────┬─────────────────────────────────────┘
                                      │ OK
                                      ▼
┌───────────────────────────────────────────────────────────────────────────┐
│ [Capa 3] Autorización por Recurso (Resource-based Authorization)          │
│ ¿Tiene el usuario un vínculo activo y legal con el dato específico?       │
│ • Profesional ➔ CanAccessPersonAsync(profId, personId)                    │
│ • Familia ➔ IsActiveRepresentative(repId, personId)                      │
│ • Admin ➔ SameInstitution(adminInstId, targetInstId)                      │
│ ➔ Si falla: 403 Forbidden                                                 │
└─────────────────────────────────────┬─────────────────────────────────────┘
                                      │ OK
                                      ▼
                         [Ejecución de Lógica de Negocio]
```

---

### 3.3. Principio Rector de Mínimo Privilegio (Ley 25.326)

Cada rol parte de **cero permisos predeterminados** y se le habilitan exclusivamente las operaciones indispensables para su misión pedagógica. 

La fuente jurídica de referencia es la **Ley 25.326 de Protección de Datos Personales (Argentina), Art. 8**, que estipula que los datos sensibles relativos a salud, diagnósticos médicos y menores de edad solo pueden ser tratados por quienes guarden un vínculo directo, profesional y confidencial con el titular.

---

### 3.4. Justificación Detallada por Rol

#### A. Persona con Discapacidad (`persona`)
* **Función:** Ejecución de actividades lúdico-pedagógicas adaptadas y visualización de logros.
* **Privilegios habilitados:**
  * Acceso al portal AAC con soporte visual de pictogramas Arasaac.
  * Ejecución de tareas interactivas asignadas (`/api/my/activity-assignments`).
  * Autenticación simplificada mediante PIN visual de 4 dígitos o sesión asistida por supervisor.
* **Privilegios denegados:**
  * Cero acceso a datos de otros compañeros.
  * Cero acceso a paneles de configuración o administración.
  * Prohibición absoluta de exigir correo electrónico o contraseñas alfanuméricas complejas.

#### B. Profesional Docente y Terapéutico (`profesional`)
* **Función:** Planificación, intervención en sala/gabinete y redacción de diagnósticos.
* **Privilegios habilitados:**
  * Creación y adaptación de actividades lúdicas.
  * Asignación y reordenamiento de nodos pendientes del Roadmap.
  * Configuración de parámetros de dificultad y umbrales del Motor Adaptativo (MDA).
  * Redacción de diagnósticos funcionales y emisión de reportes pedagógicos.
* **Privilegios denegados:**
  * Acceso vedado a legajos de alumnos no asignados expresamente por la Dirección escolar.
  * No puede auto-aprobar sus propios informes frente a la familia (exige aprobación directiva).
  * No puede auto-aprobar su matrícula habilitante ni validarse a sí mismo.

#### C. Familia / Cuidador (`familia`)
* **Función:** Acompañamiento en el hogar, consulta de evoluciones y canal formal con la escuela.
* **Privilegios habilitados:**
  * Visualización de reportes escolares en estado `Approved` (aprobados por Dirección).
  * Consulta del panel de progreso y logros alcanzados por su hijo/a.
  * Registro de acuse de lectura digital de circulares e informes.
* **Privilegios denegados:**
  * No puede ver reportes en borrador (`Draft`) ni en revisión (`Submitted`).
  * No puede editar actividades, diagnósticos ni alterar el Roadmap del alumno.
  * No puede acceder a datos de otros estudiantes de la institución.

#### D. Equipo Directivo / Admin Institucional (`admin`)
* **Función:** Gobierno pedagógico, administrativo y de seguridad de la sede escolar.
* **Privilegios habilitados:**
  * Matriculación de alumnos y altas directivas de profesionales matriculados.
  * Vinculación de tutores y emisión de enlaces de invitación familiares (TTL 7 días).
  * Dictamen sobre informes docentes: Aprobación formal (`Approved`) o Rechazo con observaciones (`Rejected` con `AdminComment`).
  * Gestión de sedes, salas y configuración de catálogos institucionales.
  * Auditoría transversal de accesos clínicos (`AccessAudit`).
* **Privilegios denegados:**
  * Acceso vedado a cualquier otra institución escolar ajena a su `institutionId`.
  * No puede alterar retroactivamente respuestas de actividades ya ejecutadas por alumnos.

---

## 4. Motor Adaptativo (MDA) — Caso de Uso CU-48

### 4.1. Ficha del Caso de Uso CU-48

| Campo | Detalle |
|---|---|
| **Caso de Uso** | **CU-48: Configurar motor adaptativo para una actividad** |
| **Módulo** | Módulo 11 — Motor Adaptativo (MDA) |
| **Actor principal** | Profesional Docente / Terapeuta |
| **Actores secundarios** | — |
| **HU de referencia** | [HU-10](../InclusiON.Documents/Docs/HU/HU-10-motor-adaptativo.md) |
| **Prioridad** | Alta |
| **Precondiciones** | La actividad se encuentra en el Roadmap de un alumno asignado activamente al Profesional. |

---

### 4.2. Flujo Principal, Alternativos y Reglas Algorítmicas

#### Flujo Principal
1. El Profesional accede al editor de trayectorias (Roadmap) del alumno asignado.
2. Selecciona una actividad específica y presiona **"Configurar motor adaptativo"**.
3. El sistema despliega el panel de parametrización con los siguientes controles:
   * **Interruptor de estado:** Activar / Desactivar motor adaptativo para este nodo.
   * **Rango de dificultad:** Nivel mínimo y nivel máximo (escala de 1 a 10).
   * **Rango de tiempo límite:** Mínimo y máximo admisible por consigna (en segundos).
   * **Umbral de éxitos consecutivos:** Cantidad de aciertos seguidos para escalar dificultad (ej. 3).
   * **Umbral de fracasos consecutivos:** Cantidad de fallos seguidos para desescalar (ej. 2).
   * **Porcentaje de acierto mínimo:** Precisión esperada para considerar consolidada la estación (ej. 75%).
   * **Umbral de frustración:** Nivel límite de estrés o abandono que dispara la intervención (escala 1 a 5).
4. El Profesional ajusta los parámetros en función del perfil sensorial del estudiante y guarda los cambios.
5. El sistema valida que el valor mínimo no supere al valor máximo en ningún rango configurado.
6. El sistema persiste la entidad `AdaptiveEngineConfig` y confirma con notificación de éxito.

#### Flujos Alternativos y Excepcionales
* **4a. Motor desactivado:** Si el docente apaga el motor, la actividad opera con parámetros fijos estáticos. Las métricas de tiempo y aciertos se continúan registrando para estadística, pero el agente no altera la dificultad.
* **5a. Rango inválido (Mínimo > Máximo):** El sistema resalta los campos inconsistentes, bloquea el guardado y emite mensaje: *"El valor mínimo no puede superar al valor máximo"*.
* **5b. Pérdida de conexión:** Si la red se interrumpe, el cliente retiene la configuración localmente y reintenta el despacho al restablecer enlace.

#### Reglas de Ejecución en Background (AdaptiveAdjustmentAgent)
1. **Clampeo Estricto:** `newLevel = Math.Clamp(calculatedLevel, config.MinDifficultyLevel, config.MaxDifficultyLevel)`. El algoritmo jamás desbordará los límites fijados por el docente.
2. **Intervención Inmediata por Frustración:** Si el análisis de interacción detecta `FrustrationLevel >= config.FrustrationThreshold`:
   * Se fuerza el nivel de dificultad al mínimo configurado (`config.MinDifficultyLevel`).
   * Se adapta el entorno con refuerzos positivos y simplificación de estímulos visuales.
   * Se despacha una alerta push SignalR en tiempo real al panel del docente con el estado crítico del estudiante.
