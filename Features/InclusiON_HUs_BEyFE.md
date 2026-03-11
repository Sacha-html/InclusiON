# InclusiON — Historias de Usuario

**Backend + Frontend** — Separadas por track, ordenadas por dependencia
Institucion Cervantes - Analista de Sistemas - 2025/2026

## Como usar este documento

Las historias estan separadas en dos tracks: **Backend (BE)** y **Frontend (FE)**. Cada track esta ordenado por dependencias: una HU nunca aparece antes de las HUs que necesita para estar completa.

### Resumen — Dependencias Cruzadas BE <> FE

Estas son las HUs donde el frontend necesita que el backend este listo para conectar el servicio real (reemplazar el mock). El FE puede avanzar con mock antes, pero la integracion depende del BE.

| Campo | Significado |
|---|---|
| Dependencias | HUs que deben estar completadas antes de empezar esta |
| Al completar... | Que otras HUs quedan desbloqueadas cuando esta se termina |
| Mock FE | Datos hardcodeados que el FE puede usar para avanzar mientras el BE no esta listo |
| Criterios de aceptacion | Condiciones concretas y verificables que deben cumplirse para dar la HU por terminada |

---

## TRACK BACKEND (BE)

**17 Historias de Usuario** - Endpoints - Handlers CQRS - Logica de negocio - Ordenadas por dependencia

---

### Sprint 1 — 3 Historias

---

#### BE-01 — Catalogos de Referencia (lectura) — CRITICA

**Como** desarrollador de backend
**Quiero** exponer los catalogos de referencia del sistema como endpoints GET simples
**Para** que el frontend pueda poblar dropdowns sin tener datos hardcodeados

**Que construye el Backend:**
CatalogsController -> GET endpoints sin handler (Dapper directo):
- `GET /api/catalogs/disability-types`
- `GET /api/catalogs/autonomy-levels`
- `GET /api/catalogs/activity-categories`
- `GET /api/catalogs/report-types`
- `GET /api/educational-institutions`

Response: `[ { id: int, name: string } ]`
Sin paginacion. Cache con `[ResponseCache(Duration=300)]`

**Criterios de aceptacion:**
- Cada endpoint devuelve un array con todos los registros activos.
- Response shape uniforme: `[{ "id": N, "name": "..." }]`.
- Si no hay registros: devuelve array vacio `[]`, no 404.
- Requieren JWT valido excepto los de login (ya existentes).
- Los 5 endpoints responden en menos de 100ms (datos en memoria / seed).

**Al completar esta HU se desbloquea:** BE-02, BE-03, BE-08, FE-01 (dropdowns del formulario de persona), FE-04 (formulario de actividad)

---

#### BE-02 — CRUD de Profesionales — CRITICA

**Como** administrador del sistema
**Quiero** poder registrar, consultar, editar y dar de baja profesionales
**Para** que puedan acceder a la plataforma y comenzar a gestionar personas

**Que construye el Backend:**
ProfessionalsController:
- `GET  /api/professionals` -> lista paginada con filtros
- `GET  /api/professionals/{id}` -> detalle + instituciones
- `GET  /api/professionals/me` -> perfil propio (del JWT)
- `POST /api/professionals` -> crea User + Professional en transaccion
- `PUT  /api/professionals/{id}` -> edita datos del profesional
- `PUT  /api/professionals/{id}/deactivate` -> soft delete + revoca tokens

Handlers: CreateProfessionalCommand, UpdateProfessionalCommand, DeactivateProfessionalCommand
Queries: GetProfessionalsQuery, GetProfessionalByIdQuery

**Criterios de aceptacion:**
- POST crea User (ASP.NET Identity) + Professional en una sola transaccion (rollback si falla cualquier parte).
- POST asigna rol "Professional" al usuario creado.
- POST devuelve contrasena temporal generada en el response (solo una vez, no se guarda en texto plano).
- DNI y email deben ser unicos; si ya existen: 400 con ErrorCode especifico.
- GET lista soporta filtros: search (nombre/apellido), specialty, institutionId, isActive.
- GET lista es paginado: page, pageSize, devuelve `{ data, total, page, pageSize }`.
- PUT deactivate: IsActive = false + revoca todos los RefreshTokens activos del profesional.
- Toda modificacion se audita en AccessAudit.

**Dependencias:** BE-01 (GET /educational-institutions para asignacion a institucion)
**Al completar esta HU se desbloquea:** BE-03, FE-02 (lista de profesionales), FE-03 (formulario)

---

#### BE-03 — Asignacion Profesional <> Institucion y Persona — ALTA

**Como** administrador del sistema
**Quiero** poder asociar profesionales a instituciones educativas y a personas con discapacidad
**Para** que queden registradas las relaciones de trabajo y el profesional pueda operar sobre sus personas asignadas

**Que construye el Backend:**
- `POST /api/professionals/{id}/institutions` -> crea ProfessionalInstitution
- `DELETE /api/professionals/{id}/institutions/{instId}` -> desasociar
- `POST /api/persons/{id}/professionals` -> crea ProfessionalPerson
- `PUT  /api/persons/{id}/professionals/{profId}/deactivate` -> desasociar
- `POST /api/persons/{id}/representatives` -> crea PersonRepresentative
- `PUT  /api/persons/{id}/deactivate` -> soft delete de persona

**Criterios de aceptacion:**
- Un profesional puede asociarse a multiples instituciones (sin duplicados).
- Una persona puede tener multiples profesionales asignados.
- La relacion ProfessionalPerson incluye: IsPrimaryProfessional, CanSuperviseLogin.
- Desasociar no elimina registros: IsActive = false con auditoria.
- No se puede desasociar si el profesional tiene assignments activos para esa persona (validacion).

**Dependencias:** BE-01 (instituciones), BE-02 (profesionales)
**Al completar esta HU se desbloquea:** FE-03 (seccion de instituciones y personas en detalle de profesional)

---

### Sprint 2 — 6 Historias

---

#### BE-04 — Areas de Habilidad (SkillAreas) — CRITICA

**Como** profesional / administrador
**Quiero** gestionar las areas de habilidad disponibles en el sistema y consultarlas
**Para** que los formularios de actividad y el radar chart sean completamente dinamicos

**Que construye el Backend:**
- `GET  /api/skill-areas` -> lista activas con id, name, icon, color, displayOrder
- `GET  /api/skill-areas/{id}` -> detalle
- `POST /api/skill-areas` -> crear (AdminOnly)
- `PUT  /api/skill-areas/{id}` -> editar nombre, icon, color, orden (AdminOnly)
- `PUT  /api/skill-areas/{id}/deactivate` -> soft delete (AdminOnly)

Seed ya cargado: Comunicacion, Alfabetizacion, Logica-Matematica, Conducta, Motricidad

**Criterios de aceptacion:**
- GET /api/skill-areas responde sin autenticacion de admin: cualquier ProfOrAbove puede leer.
- Un area con PersonSkillProfile activos no puede desactivarse (devuelve 409 con mensaje explicativo).
- El campo "icon" acepta strings de nombre de icono (ej: "book", "calculator").
- El campo "color" acepta formato hex #RRGGBB validado por regex.
- Seed inicial responde correctamente desde el primer dotnet run.

**Al completar esta HU se desbloquea:** BE-05, BE-06, FE-04 (paso 1 del form de actividad), FE-07 (SkillProfile)

---

#### BE-05 — Tipos de Plantilla (ActivityTemplateTypes) — CRITICA

**Como** profesional
**Quiero** consultar los tipos de plantilla disponibles filtrados por area
**Para** que el formulario dinamico de actividad sepa que campos mostrar segun el template elegido

**Que construye el Backend:**
- `GET /api/activity-templates` -> lista activos, query: `?skillAreaId=1`
  Response: id, name, code, description, componentName, usesPictograms, hasAudio, displayOrder
- `GET /api/activity-templates/{id}` -> detalle (incluye ContentSchema completo)
- `GET /api/activity-templates/by-code/{code}` -> por codigo (SELECT_FIGURE, etc.)

Solo lectura para profesionales. Escritura solo por admin via seed/migration.

**Criterios de aceptacion:**
- GET /api/activity-templates?skillAreaId=1 devuelve solo los templates de esa area.
- Sin filtro devuelve todos los activos de todas las areas.
- ContentSchema se devuelve como string JSON (el frontend lo parsea).
- Codigos disponibles en seed: SELECT_FIGURE, VISUAL_SUM, MATCH_IMAGE_WORD, ORDER_SEQUENCE, COMPLETE_LETTER.
- GET by-code/{code} devuelve 404 si el codigo no existe.

**Dependencias:** BE-04 (SkillAreas deben existir para filtrar)
**Al completar esta HU se desbloquea:** BE-06, FE-04 (paso 2 y 3 del form de actividad — DynamicActivityForm)

---

#### BE-06 — CRUD de Actividades con Contenido Dinamico — CRITICA

**Como** profesional
**Quiero** crear, editar y gestionar actividades vinculadas a una plantilla, con el contenido generado por el formulario dinamico
**Para** tener un catalogo de actividades reutilizables adaptadas a cada area de habilidad

**Que construye el Backend:**
- `GET /api/activities` -> lista paginada del prof + estandar
  Filtros: skillAreaId, templateTypeId, complexityLevel, mine/standard, search
- `GET /api/activities/{id}` -> detalle con ContentJson incluido
- `POST /api/activities` -> crea Activity + ActivityContent en transaccion
  Body: `{ title, description, skillAreaId, categoryId, templateTypeId, contentJson, complexityLevel, estimatedDurationMinutes, requiresSupervision, isStandardActivity }`
- `PUT  /api/activities/{id}` -> edita (solo si es propia)
- `PUT  /api/activities/{id}/deactivate` -> soft delete

Handler: CreateActivityCommand, UpdateActivityCommand

**Criterios de aceptacion:**
- POST valida que templateTypeId sea valido y corresponda al skillAreaId indicado.
- POST crea Activity y ActivityContent en la misma transaccion (rollback si falla).
- ContentJson no se valida contra el schema en el backend (confia en el frontend): se guarda como nvarchar(max).
- Un profesional solo puede editar/desactivar sus propias actividades (no las estandar).
- Actividades con assignments activos (Pendiente/EnProgreso) no pueden desactivarse.
- GET lista devuelve actividades propias del profesional autenticado + las isStandardActivity=true.

**Dependencias:** BE-04 (SkillAreas), BE-05 (Templates), BE-01 (activity-categories)
**Al completar esta HU se desbloquea:** BE-09, FE-04 (puede guardar el formulario), FE-05 (lista de actividades)

---

#### BE-07 — Perfil de Habilidades de la Persona (SkillProfile) — CRITICA

**Como** profesional
**Quiero** asignar y gestionar que areas de habilidad se van a trabajar para cada persona a mi cargo
**Para** que el radar chart y el roadmap sean personalizados para cada estudiante

**Que construye el Backend:**
- `GET /api/persons/{id}/skill-profile` -> `[{ skillAreaId, skillAreaName, color, icon, addedBy, isActive }]`
- `POST /api/persons/{id}/skill-profile` -> Body: `{ skillAreaId }` -> Crea PersonSkillProfile
- `PUT  /api/persons/{id}/skill-profile/{areaId}` -> Desactiva el area (IsActive=false)

**Criterios de aceptacion:**
- No puede asignarse un area que ya esta activa para esa persona (409 con mensaje).
- Solo el profesional asignado a esa persona puede gestionar su skill profile.
- Desactivar un area no elimina las respuestas historicas (no afecta ActivityResponse).
- GET devuelve solo las areas activas (IsActive=true) por defecto; query `?all=true` para todas.

**Dependencias:** BE-04 (SkillAreas), BE-03 (persona debe existir y tener profesional asignado)
**Al completar esta HU se desbloquea:** BE-09 (roadmap solo puede agregar actividades de areas en el profile), FE-07 (seccion skill profile)

---

#### BE-08 — Sistema de Invitaciones para Familias — ALTA

**Como** profesional
**Quiero** generar invitaciones para que los representantes familiares se registren vinculados directamente a la persona
**Para** que el acceso familiar sea controlado y no requiera configuracion manual del admin

**Que construye el Backend:**
- `POST /api/invitations` -> Body: `{ personId, email, firstName, lastName, relationship }` -> Genera codigo unico (GUID o token) + ExpiresAt (7 dias)
- `GET  /api/invitations` -> Lista del profesional autenticado con estado: Enviada/Aceptada/Expirada
- `GET  /api/invitations/{code}` (publico, sin JWT) -> Valida codigo + devuelve datos pre-llenados para el form
- `POST /api/invitations/{code}/accept` (publico) -> Crea User + FamilyRepresentative + PersonRepresentative en transaccion

**Criterios de aceptacion:**
- GET /invitations/{code}: si el codigo expiro -> 400 con "INVITATION_EXPIRED". Si ya fue usado -> 400 con "INVITATION_ALREADY_USED".
- POST /accept no requiere JWT: es el registro del familiar.
- Email del usuario nuevo debe ser unico en el sistema.
- Al aceptar: Invitation.IsUsed = true, UsedAt = now, UsedByUserId = nuevo userId.
- La relacion PersonRepresentative se crea con Relationship del campo de la invitacion.

**Dependencias:** BE-01 (GET /persons para selector de persona), BE-03 (persona debe existir)
**Al completar esta HU se desbloquea:** FE-10 (formulario de invitacion y pagina de registro familiar)

---

#### BE-09 — Roadmap de la Persona — CRITICA

**Como** profesional
**Quiero** armar el roadmap educativo de cada persona agregando actividades en secuencia por area
**Para** que el plan de aprendizaje sea progresivo y las actividades se desbloqueen automaticamente

**Que construye el Backend:**
- `GET /api/persons/{id}/roadmap` -> Agrupado por area: `[{ area, items: [{ id, activityTitle, sequenceOrder, isUnlocked, completedAt, successPercentage }] }]`
- `POST /api/persons/{id}/roadmap` -> Body: `{ activityId, skillAreaId, sequenceOrder, unlockThresholdPercent }` -> Si es el primer item del area: IsUnlocked=true + crea ActivityAssignment Pendiente
- `PUT /api/persons/{id}/roadmap/{itemId}/reorder` -> cambia SequenceOrder
- `PUT /api/persons/{id}/roadmap/{itemId}/unlock` -> desbloqueo manual (override)
- `DELETE /api/persons/{id}/roadmap/{itemId}` -> eliminar (solo si no fue completado)

**Criterios de aceptacion:**
- Solo se pueden agregar actividades de areas en el PersonSkillProfile de la persona.
- El sequenceOrder debe ser unico por (personId, skillAreaId).
- Al agregar el primer item de un area: IsUnlocked=true automaticamente.
- PUT unlock tambien crea un ActivityAssignment en estado Pendiente para la actividad.
- No se puede reordenar ni eliminar un item con ActivityResponse ya registrada.

**Dependencias:** BE-06 (actividades), BE-07 (skill profile), BE-03 (persona)
**Al completar esta HU se desbloquea:** BE-10 (assignments), FE-08 (RoadmapManager del prof), FE-09 (RoadmapView del alumno)

---

### Sprint 3 — 3 Historias

---

#### BE-10 — Assignments — Consulta y Gestion — CRITICA

**Como** persona con discapacidad / profesional
**Quiero** consultar las actividades asignadas con todo el contenido necesario para reproducirlas
**Para** que el player pueda cargar cualquier actividad sin multiples llamadas al API

**Que construye el Backend:**
- `GET /api/persons/{id}/assignments` -> Filtros: status, skillAreaId, isEvaluation -> Lista con: titulo, area, status, dueDate, completedAt
- `GET /api/assignments/{id}` -> Detalle COMPLETO: assignment + activity + ContentJson + templateType.code + templateType.componentName (este endpoint es el que llama el player al abrir)
- `POST /api/assignments` -> asignacion directa (fuera del roadmap)
- `PUT  /api/assignments/{id}/status` -> cambio manual de estado (Cancelada)

**Criterios de aceptacion:**
- GET /assignments/{id} incluye en un solo response: assignment, activity (titulo, instrucciones, flags), activityContent.contentJson, templateType.code y componentName.
- La persona solo puede ver sus propios assignments. El profesional puede ver los de sus personas.
- Status validos: Pendiente, EnProgreso, Completada, Cancelada.

**Dependencias:** BE-09 (roadmap genera assignments automaticamente)
**Al completar esta HU se desbloquea:** BE-11 (responses), FE-11 (ActivityPlayerShell)

---

#### BE-11 — Responses — Ciclo de Vida de una Actividad — CRITICA

**Como** persona con discapacidad
**Quiero** registrar el inicio, progreso y resultado de cada actividad realizada
**Para** que quede trazabilidad completa del aprendizaje y se dispare el desbloqueo del siguiente paso

**Que construye el Backend:**
- `POST /api/assignments/{id}/responses/start` -> Crea ActivityResponse (StartedAt=now, AttemptCount=1) -> Assignment.Status = EnProgreso -> Response: `{ responseId }`
- `PUT  /api/assignments/{id}/responses/{resId}` -> Actualiza durante la actividad: FrustrationLevel, AttemptCount, ResponsePattern
- `POST /api/assignments/{id}/responses/{resId}/complete` -> Body: `{ successPercentage, timeSpentSeconds, result, responsePattern, observations }` -> Assignment.Status = Completada
  **LOGICA DE DESBLOQUEO:**
  ```
  if successPercentage >= roadmapItem.UnlockThresholdPercent:
    nextItem.IsUnlocked = true
    nextItem.UnlockedAt = now
    create ActivityAssignment(nextItem.ActivityId, Pendiente)
  ```

**Criterios de aceptacion:**
- POST /start devuelve error si el assignment no esta en estado Pendiente o EnProgreso.
- POST /complete evalua el umbral y desbloquea el siguiente item del roadmap en la misma transaccion.
- Si no hay siguiente item en el area: no hace nada extra (area completada).
- Si SuccessPercentage < UnlockThresholdPercent: assignment queda Completada pero el siguiente no se desbloquea.
- ResponsePattern se guarda como JSON string (el backend no lo parsea).
- FrustrationLevel acepta valores 1-5.

**Dependencias:** BE-10 (assignments), BE-09 (roadmap para la logica de desbloqueo)
**Al completar esta HU se desbloquea:** BE-12 (radar tiene datos), FE-11 (players pueden completar actividades)

---

#### BE-12 — Radar Chart y Dashboard con Datos Reales — ALTA

**Como** profesional
**Quiero** ver el perfil de habilidades de una persona como un grafico radar calculado desde las respuestas reales
**Para** tener una vista rapida del estado de cada estudiante sin revisar actividad por actividad

**Que construye el Backend:**
- `GET /api/persons/{id}/radar` -> `[{ areaId, areaName, color, icon, score, activityCount, hasData }]` -> score = AVG(SuccessPercentage) de responses Completadas de esa area -> hasData = false si no hay ninguna response para esa area
- `GET /api/dashboard/professional` -> `{ personCount, pendingAssignments, lastCompleted[5], upcomingDue[5] }`
- `GET /api/dashboard/family` -> `{ personName, lastActivities[3], unreadMessages, newReports }`

**Criterios de aceptacion:**
- GET /radar solo devuelve ejes de las areas en el PersonSkillProfile activo de esa persona.
- El score se calcula on-demand (no se persiste en una columna extra).
- Si una area no tiene responses: score=0, hasData=false (el frontend muestra eje en gris).
- Dashboard: datos reales, no mocks. El endpoint debe funcionar aunque no haya datos (devuelve arrays vacios).

**Dependencias:** BE-11 (responses con SuccessPercentage), BE-07 (PersonSkillProfile para los ejes)
**Al completar esta HU se desbloquea:** FE-12 (RadarChartComponent), FE-13 (dashboards con datos reales)

---

### Sprint 4 — 5 Historias

---

#### BE-13 — Diagnosticos Funcionales — CRITICA

**Como** profesional
**Quiero** registrar y consultar diagnosticos funcionales de cada persona
**Para** que quede documentado el punto de partida del estudiante como base del plan educativo

**Que construye el Backend:**
- `GET /api/persons/{id}/diagnoses` -> lista desc. por fecha
- `GET /api/diagnoses/{id}` -> detalle completo
- `POST /api/persons/{id}/diagnoses` -> Body: `{ diagnosisDate, primaryDiagnosis, initialObservations, identifiedCapabilities, identifiedChallenges, requiredSupports, pedagogicalObjectives, recommendedStrategies }`
- `PUT /api/diagnoses/{id}` -> edita (solo el creador)

**Criterios de aceptacion:**
- Una persona puede tener multiples diagnosticos en el tiempo.
- Solo el profesional que creo el diagnostico puede editarlo.
- Los diagnosticos de otros profesionales son visibles pero de solo lectura.
- Todos los campos de texto son nvarchar(max), ninguno es obligatorio excepto primaryDiagnosis y diagnosisDate.

**Dependencias:** BE-03 (persona con profesional asignado)
**Al completar esta HU se desbloquea:** FE-14 (DiagnosisFormComponent)

---

#### BE-14 — Reportes de Progreso — ALTA

**Como** profesional
**Quiero** generar reportes de progreso de cada persona para un periodo determinado
**Para** poder comunicar formalmente los avances a la familia de forma estandarizada

**Que construye el Backend:**
- `GET /api/persons/{id}/reports` -> lista (visible para prof Y familia)
- `GET /api/reports/{id}` -> detalle completo
- `POST /api/persons/{id}/reports` -> Body: `{ reportTypeId, title, periodStart, periodEnd, content, achievedGoals, areasToReinforce, futureRecommendations, nextObjectives }`
- `GET /api/reports/{id}/export/pdf` -> genera PDF (QuestPDF o HTML)

**Criterios de aceptacion:**
- El representante familiar puede acceder a los reportes de su familiar (sin crear ni editar).
- GET lista devuelve: id, titulo, tipo, fecha, profesional.
- El PDF en primera version puede ser HTML con window.print() si QuestPDF implica demasiado tiempo.

**Dependencias:** BE-01 (GET /report-types), BE-03 (persona)
**Al completar esta HU se desbloquea:** FE-15 (ReportFormComponent, ReportDetailComponent)

---

#### BE-15 — Mensajeria Interna — ALTA

**Como** profesional / representante familiar
**Quiero** enviar y recibir mensajes dentro de la plataforma con la familia de mis personas a cargo
**Para** centralizar la comunicacion y evitar el uso de canales externos no auditables

**Que construye el Backend:**
- `GET /api/messages` -> inbox del usuario autenticado. Query: `?unreadOnly=true`
- `GET /api/messages/sent` -> enviados
- `GET /api/messages/{id}` -> detalle + marca ReadAt=now si destinatario
- `POST /api/messages` -> Body: `{ receiverId, subject, content, relatedPersonId?, parentMessageId? }`
- `GET /api/messages/unread-count` -> `{ count: N }` para el badge del sidebar

**Criterios de aceptacion:**
- Un usuario solo puede leer mensajes donde es sender o receiver.
- GET /messages/{id} setea ReadAt = now si el usuario autenticado es el receiver.
- parentMessageId permite armar hilos: un mensaje puede ser respuesta de otro.
- GET /unread-count es el endpoint que el frontend llama cada 30s para el badge.

**Al completar esta HU se desbloquea:** FE-16 (MessageInboxComponent, MessageComposeComponent)

---

#### BE-16 — Busqueda Semantica de Actividades — ALTA (NUEVA)

**Como** profesional
**Quiero** buscar actividades por significado semantico usando lenguaje natural
**Para** encontrar actividades relevantes sin depender de keywords exactos o filtros manuales

**Que construye el Backend:**
- `GET /api/activities/search?text=...&topN=10` -> busqueda semantica por similitud coseno
- Generacion automatica de embeddings al crear/editar Activity via `CreateActivityCommandHandler`
- Interface `IEmbeddingService` + `ISimilarityCalculator` en Application
- Query handler `SearchActivitiesSemanticQueryHandler` (CQRS)
- Registro de servicios via `AddSemanticSearch()` en Program.cs

Usa ONNX + all-MiniLM-L6-v2 (384 dimensiones). Entidades ya existentes: `ActivityEmbedding`, `ActivityResult` (con migracion `Add-Embedding`).
Library en `InclusiON.SemanticSearch/`: OnnxEmbeddingProvider, CosineSimilarityCalculator, WordPieceTokenizer.

**Criterios de aceptacion:**
- El endpoint devuelve actividades ordenadas por similitud semantica descendente.
- topN limita los resultados (default 10).
- Si no hay embeddings generados: devuelve array vacio.
- La generacion de embeddings no bloquea la respuesta del POST de actividad (puede ser asincrona).
- Configuracion en appsettings.json: ModelPath, VocabPath, EmbeddingDimension: 384.

**Dependencias:** BE-06 (actividades deben existir)
**Al completar esta HU se desbloquea:** BE-17 (el MDA usa datos de actividades encontradas semanticamente)

---

#### BE-17 — Motor de Dificultad Adaptativa (MDA) — ALTA (NUEVA)

**Como** profesional
**Quiero** que el sistema ajuste automaticamente la dificultad, tiempo, pistas e intentos de cada actividad segun el desempeno del estudiante
**Para** mantener al estudiante en su zona de desarrollo proximo sin intervencion manual constante

**Que construye el Backend:**
- Interface `IAdaptiveEngineService` + DTOs de resultado (`AdaptiveAdjustmentResult`, `AdaptiveState` enum) en Application
- Implementacion `AdaptiveEngineService` en Infrastructure con maquina de estados (ESTABLE/PROGRESANDO/DIFICULTAD/FRUSTRACION)
- Integracion en `CompleteActivityResponseCommandHandler` via `IAdaptiveEngineService.EvaluateAndAdjustAsync()`
- `GET /api/persons/{id}/adaptive-log` -> historial de ajustes adaptativos para consulta del profesional

Entidades ya existentes (migracion `Add-AdaptiveEngine`):
- `AdaptiveEngineConfig` — Configuracion de rangos y umbrales (1:0..1 con PersonRoadmapActivity)
- `AdaptiveAdjustmentLog` — Auditoria de cada ajuste realizado

**Maquina de estados:**
| Estado | Condicion | Accion |
|---|---|---|
| ESTABLE | Inicial, sin condiciones cumplidas | Sin cambios |
| PROGRESANDO | N exitos consecutivos >= SuccessThresholdPercent | Sube dificultad, reduce tiempo, desactiva hints, reduce intentos |
| DIFICULTAD | N fallos consecutivos < SuccessThresholdPercent | Activa hints, mas intentos, mas tiempo, baja dificultad |
| FRUSTRACION | FrustrationLevel >= threshold o 3+ abandonos | Intervencion total: todo al minimo accesible + alerta al profesional |

**Reglas de escalamiento (PROGRESANDO):**
1. DifficultyLevel += 1 (max MaxDifficultyLevel)
2. TimeLimitSeconds -= 10% (min MinTimeLimitSeconds)
3. ShowHints = false (si ultimas 3 no usaron hints)
4. MaxAttempts -= 1 (min 1, si resolvio al primer intento)

**Reglas de desescalamiento (DIFICULTAD):**
1. ShowHints = true
2. MaxAttempts += 1 (max 5)
3. TimeLimitSeconds += 15% (max MaxTimeLimitSeconds)
4. DifficultyLevel -= 1 (min MinDifficultyLevel)

**Intervencion por frustracion:**
- DifficultyLevel = MinDifficultyLevel
- ShowHints = true
- TimeLimitSeconds = MaxTimeLimitSeconds (o null)
- MaxAttempts = 5 (o null para ilimitado)
- Notificacion al profesional via SignalR o flag en dashboard

**Criterios de aceptacion:**
- Si AdaptiveEngineConfig es null para una actividad: el motor no interviene (configuracion estatica).
- Si IsEnabled = false: el motor no interviene aunque exista la config.
- Cada ajuste genera un AdaptiveAdjustmentLog con tipo, valor anterior, valor nuevo y razon.
- El motor nunca excede los rangos configurados por el profesional (MinDifficultyLevel..MaxDifficultyLevel, etc).
- La evaluacion se ejecuta dentro de la misma transaccion del UnitOfWork (atomicidad).
- GET /adaptive-log devuelve el historial filtrado por persona y opcionalmente por actividad del roadmap.

**Dependencias:** BE-11 (responses con SuccessPercentage y FrustrationLevel), BE-09 (roadmap)
**Al completar esta HU se desbloquea:** FE-17 (panel de configuracion de rangos), FE-18 (timeline de ajustes adaptativos)

---

## TRACK FRONTEND (FE)

**18 Historias de Usuario** - Componentes Angular - Services - Mocks de desarrollo - Ordenadas por dependencia

---

### Sprint 1 — 3 Historias

---

#### FE-01 — CatalogsService y Conexion de Formulario de Persona — CRITICA

**Como** developer frontend
**Quiero** un servicio centralizado que exponga todos los catalogos del sistema con cache en memoria
**Para** que los formularios no hagan multiples llamadas repetidas y no tengan datos hardcodeados

**Que construye el Frontend:**
CatalogsService (singleton):
- `getDisabilityTypes(): Observable<Catalog[]>`
- `getAutonomyLevels(): Observable<Catalog[]>`
- `getActivityCategories(): Observable<Catalog[]>`
- `getReportTypes(): Observable<Catalog[]>`
- `getEducationalInstitutions(): Observable<Catalog[]>`

Internamente usa BehaviorSubject: primera llamada al API, siguientes devuelven el cache sin nueva request.
Conectar PersonFormComponent existente con el service real -> Reemplazar arrays hardcodeados por CatalogsService.getXxx()

**Criterios de aceptacion:**
- El servicio tiene un BehaviorSubject por catalogo. Si ya tiene datos, no hace nueva request.
- Todos los selectores del formulario de persona (disability type, autonomy level, login method) usan el servicio.
- Si el backend devuelve error: el selector muestra "No se pudieron cargar las opciones" y no bloquea el form.
- Interfaz Catalog: `{ id: number, name: string }`.

**Dependencias:** BE-01 (catalogos deben estar disponibles para conectar)
**Al completar esta HU se desbloquea:** FE-02, FE-04 (ambos necesitan catalogos)

---

#### FE-02 — Lista y Formulario de Profesionales — CRITICA

**Como** administrador
**Quiero** ver el listado de profesionales con filtros y poder crear o editar uno
**Para** gestionar el equipo profesional sin intervencion tecnica

**Que construye el Frontend:**
ProfessionalsService: getAll(filters), getById(id), create(dto), update(id, dto), deactivate(id)

ProfessionalsListComponent (/admin/professionals):
- Tabla CoreUI con columnas: nombre, especialidad, email, matricula, estado
- Filtros: input search (debounce 300ms), select especialidad/institucion/estado
- Paginacion: ngb-pagination o CoreUI pager
- Acciones por fila: Ver, Editar, Dar de baja

ProfessionalFormComponent (modal o pagina separada):
- Reactive Form con validacion
- Modo crear: todos los campos vacios
- Modo editar: pre-carga desde GET /professionals/{id}
- Al crear: muestra toast con contrasena temporal + boton copiar

ProfessionalDetailComponent:
- Info del profesional + lista de instituciones asignadas

**Criterios de aceptacion:**
- La tabla muestra "Sin resultados" con opcion de limpiar filtros cuando no hay datos.
- El formulario valida en tiempo real: email valido, DNI solo numeros (7-8 digitos), campos requeridos.
- Al dar de baja: dialog de confirmacion con nombre del profesional y advertencia de perdida de acceso.
- DNI no es editable en modo edicion (campo readonly).
- Toast de exito al guardar con boton "Ver detalle".
- Con mock: lista con 3 profesionales hardcodeados funciona sin BE.

**Dependencias:** BE-02 debe estar listo para conectar el service real
**Al completar esta HU se desbloquea:** FE-03 (requiere que el form exista para agregar secciones)

---

#### FE-03 — Asignaciones de Institucion y Persona en Detalle del Profesional — ALTA

**Como** administrador
**Quiero** desde el detalle de un profesional poder asignarle instituciones y ver sus personas a cargo
**Para** que la gestion de relaciones quede centralizada en un solo lugar

**Que construye el Frontend:**
En ProfessionalDetailComponent, dos secciones:
- Seccion "Instituciones": Multi-select de instituciones disponibles, chip por institucion asignada con boton "x" para desasociar, boton "Agregar institucion"
- Seccion "Personas a cargo": Lista de PersonWithDisability vinculados, boton "Asignar persona" abre modal de busqueda

ProfessionalsService.assignInstitution() / removeInstitution()
ProfessionalsService.assignPerson() / removePerson()

**Criterios de aceptacion:**
- El multi-select no muestra instituciones ya asignadas.
- Al quitar una institucion: confirm dialog.
- La lista de personas muestra: nombre, discapacidad, estado.
- La seccion funciona con mock antes de que BE-03 este listo.

**Dependencias:** FE-02 (ProfessionalDetailComponent debe existir), BE-03 para conectar

---

### Sprint 2 — 5 Historias

---

#### FE-04 — Formulario de Actividad con Plantilla Dinamica — CRITICA

**Como** profesional
**Quiero** crear una actividad eligiendo area, plantilla y completando el contenido generado automaticamente por el schema
**Para** disenar actividades sin necesitar conocimientos tecnicos ni forms hardcodeados

**Que construye el Frontend:**
ActivityFormComponent (wizard 4 pasos):
1. Elegir SkillArea: cards con color e icono de cada area
2. Elegir Template: cards filtradas por area, badge de picto/audio
3. Completar contenido: DynamicActivityFormComponent
4. Config general: titulo, complejidad (stars 1-5), duracion, supervision

DynamicActivityFormComponent: Lee ContentSchema (JSON) del template seleccionado, genera campos dinamicamente segun type (text, number, boolean, select, pictogram, array).

PictogramPickerComponent: Input de busqueda -> GET https://api.arasaac.org/api/pictograms/search/{term}?locale=es -> Grid de resultados (pictogramas 80x80px)

ActivitiesService.create(dto): Observable<Activity>

**Criterios de aceptacion:**
- Si el campo de tipo "array" tiene min definido en el schema, el boton "Agregar item" esta deshabilitado si ya se llego al max.
- Los campos required del schema muestran error de validacion antes de avanzar al paso siguiente.
- PictogramPicker muestra loading spinner durante la busqueda y "Sin resultados" si la API no devuelve nada.
- El ContentJson que se envia al backend es el objeto JS tal como lo armo el form (JSON.stringify).
- Se puede construir TODO el wizard con ContentSchema hardcodeado de SELECT_FIGURE sin necesitar BE.
- Al guardar: navega a la lista de actividades con toast de exito.

**Dependencias:** BE-04 (SkillAreas) para paso 1, BE-05 (Templates) para paso 2 y el schema, BE-06 para guardar
**Al completar esta HU se desbloquea:** FE-05 (la lista necesita que se puedan crear actividades)

---

#### FE-05 — Catalogo de Actividades del Profesional — ALTA

**Como** profesional
**Quiero** ver todas mis actividades y las estandar, con filtros, y poder asignarlas al roadmap
**Para** reutilizar actividades ya creadas sin tener que crearlas desde cero cada vez

**Que construye el Frontend:**
ActivityListComponent (/pro/activities): Tabla/grid con titulo, area (badge color), template (codigo), complejidad (stars), duracion. Toggle "Mis actividades" / "Estandar". Filtros: area, template, complejidad. Acciones: Ver detalle, Editar (si propia), Asignar a persona.

ActivityDetailComponent: Info general + preview del ContentJson segun template (renderiza el contenido de forma legible, no como JSON crudo).

ActivitiesService.getAll(filters), getById(id), update(id,dto), deactivate(id)

**Criterios de aceptacion:**
- El badge del area usa el color de SkillArea.color.
- La complejidad se muestra como N estrellas (1-5).
- El preview del ContentJson en detalle muestra los campos del schema de forma legible.
- Con mock: 4 actividades hardcodeadas, 2 propias y 2 estandar.

**Dependencias:** FE-04 (formulario para crear/editar), BE-06 para datos reales
**Al completar esta HU se desbloquea:** FE-08 (modal de asignar actividad al roadmap)

---

#### FE-06 — Seccion Skill Profile en Perfil de Persona — CRITICA

**Como** profesional
**Quiero** desde el perfil de una persona definir que areas de habilidad se van a trabajar y monitorear
**Para** que el radar chart y el roadmap sean especificos para las necesidades de ese estudiante

**Que construye el Frontend:**
En PersonDetailComponent, nueva pestana/seccion "Habilidades":
- Lista de areas activas en chips con el color de cada area
- Boton "Agregar area" -> select de areas disponibles (no asignadas aun)
- Chip clickeable: confirm -> desactiva el area

SkillProfileService: getProfile(personId), assign(personId, skillAreaId), deactivate(personId, areaId)

**Criterios de aceptacion:**
- Las areas disponibles en el select son las que existen en el sistema menos las ya asignadas.
- Al desactivar: dialog de confirmacion "Desactivar area X? Se conservara el historial de respuestas."
- Los chips usan el color de SkillArea.color como background o borde.
- Con mock: 2 areas asignadas hardcodeadas.

**Dependencias:** BE-04 (SkillAreas), BE-07 para datos reales
**Al completar esta HU se desbloquea:** FE-08 (roadmap solo muestra areas del skill profile)

---

#### FE-07 — Roadmap Manager (Vista del Profesional) — CRITICA

**Como** profesional
**Quiero** armar y gestionar el roadmap de actividades para cada persona por area
**Para** planificar la progresion educativa de forma visual e intuitiva

**Que construye el Frontend:**
RoadmapManagerComponent (pestana en PersonDetail):
- Una seccion por area del skill profile de la persona
- Cada seccion: lista de nodos en orden (drag & drop o flechas)
- Nodo: icono estado + titulo + tipo de template + resultado (si completado)
- Boton "+" al final de cada area -> RoadmapAddActivityModalComponent
- Nodo completado: no reordenable, no eliminable
- Nodo pendiente/desbloqueado: boton "Forzar desbloqueo" (override)

RoadmapAddActivityModalComponent: Busca en catalogo de actividades del area seleccionada. Campo: umbral de desbloqueo (default 60%, slider 0-100).

RoadmapService: getRoadmap(), addActivity(), reorder(), forceUnlock(), remove()

**Criterios de aceptacion:**
- El orden visual de los nodos refleja el sequenceOrder.
- Al agregar una actividad que ya existe en el roadmap de esa area: error con mensaje.
- El modal de agregar actividad filtra las actividades disponibles por el skillAreaId de la seccion.
- Con mock: roadmap con 3 nodos en distintos estados (bloqueado, desbloqueado pendiente, completado).

**Dependencias:** FE-05 (catalogo de actividades para el modal), FE-06 (areas del skill profile), BE-09 para datos reales
**Al completar esta HU se desbloquea:** FE-08 (roadmap del alumno necesita datos cargados por el prof)

---

#### FE-08 — Registro por Invitacion (Pagina Publica Familiar) — ALTA

**Como** representante familiar
**Quiero** registrarme en la plataforma usando el link de invitacion enviado por el profesional
**Para** acceder a seguir el progreso de mi familiar sin necesitar que el admin me cree la cuenta

**Que construye el Frontend:**
InvitationListComponent (profesional, en seccion Comunicacion): Lista de invitaciones con estado. Boton "Nueva invitacion" -> InvitationFormComponent (modal).

RegisterByInvitationComponent (/invite/:code, ruta publica): Al cargar valida codigo. Si valido: form con datos pre-llenados + email + contrasena. Submit: POST /accept. Redirect a /login con toast.

InvitationsService: getAll(), create(), validateCode(), accept()

**Criterios de aceptacion:**
- La URL del link es del tipo: `https://[dominio]/invite/{code}`.
- El form de registro no permite cambiar nombre/apellido/relacion (campos readonly).
- La contrasena debe tener minimo 8 caracteres, una mayuscula y un numero.
- Se puede construir completo con mock.

**Dependencias:** BE-08 para conectar el service real

---

### Sprint 3 — 5 Historias

---

#### FE-09 — Roadmap Visual (Vista del Alumno, estilo Duolingo) — CRITICA

**Como** persona con discapacidad
**Quiero** ver mi progreso como nodos visuales por area y poder iniciar la actividad que tengo disponible
**Para** entender de forma intuitiva donde estoy y que sigue sin necesitar leer texto complejo

**Que construye el Frontend:**
RoadmapViewComponent (layout AAC, ruta /app/roadmap):
- Tabs o accordion por area (con color e icono del area)
- Columna vertical de nodos: Completado (check verde + porcentaje), Desbloqueado pendiente (brillante + pulse), Bloqueado (candado gris, no tappable)
- Tap en nodo desbloqueado -> navega a /app/activity/:assignmentId

CelebrationOverlayComponent: Animacion de confeti al volver de actividad completada.

**Criterios de aceptacion:**
- El roadmap respeta el perfil de accesibilidad: alto contraste, fuente grande, etc.
- Los nodos bloqueados no muestran el titulo de la actividad (solo "Bloqueada").
- La animacion pulse respeta prefers-reduced-motion.
- Se puede construir con 4 nodos hardcodeados en distintos estados.

**Dependencias:** BE-09 (roadmap), BE-10 (assignments) para datos reales
**Al completar esta HU se desbloquea:** FE-10 (el alumno navega desde aqui al player)

---

#### FE-10 — ActivityPlayerShell — Cargador Dinamico de Players — CRITICA

**Como** persona con discapacidad
**Quiero** que la app cargue automaticamente el tipo de actividad correcto segun el template
**Para** realizar cualquier tipo de actividad sin que yo tenga que elegir como se visualiza

**Que construye el Frontend:**
ActivityPlayerShellComponent (ruta /app/activity/:assignmentId):
1. GET /api/assignments/{id} -> recibe assignment + ContentJson + templateType.code
2. POST /start -> registra ActivityResponse
3. Mapa de componentes por code (SELECT_FIGURE, VISUAL_SUM, etc.)
4. ViewContainerRef: carga componente dinamicamente
5. Pasa @Input: content y config de accesibilidad
6. Escucha @Output completed: PlayerResult
7. POST /complete con el resultado

FrustrationMonitorService: Si > 3 intentos muestra overlay de pausa. Incrementa FrustrationLevel via PUT /responses/{id}.

**Criterios de aceptacion:**
- Si el templateType.code no esta en el mapa: muestra error "Tipo de actividad no soportado".
- El shell es agnostico al tipo de actividad: no tiene logica de juego propia.
- La accessibilityConfig se pasa a todos los players.
- Con mock: assignment hardcodeado con templateType.code = "SELECT_FIGURE".

**Dependencias:** BE-10 (GET /assignments/{id} con ContentJson), BE-11 (POST /start y /complete)
**Al completar esta HU se desbloquea:** FE-11 (cada player necesita el shell para integrarse)

---

#### FE-11 — Players de Actividad (5 Componentes) — CRITICA

**Como** persona con discapacidad
**Quiero** realizar cada tipo de actividad de forma interactiva, visual y con feedback inmediato
**Para** aprender de manera motivadora respetando mi perfil cognitivo y de accesibilidad

**Que construye el Frontend:**
Interfaz comun a todos los players:
- `@Input() content: any` (ContentJson parseado)
- `@Input() config: AccessibilityConfig`
- `@Output() completed: PlayerResult` (`{ successPercentage, attempts, responsePattern }`)
- `@Output() attemptFailed: void` (para FrustrationMonitor)

**SelectFigurePlayerComponent:** Instruccion + TTS + grid 2x2 de opciones con pictogramas. Feedback visual verde/rojo.

**VisualSumPlayerComponent:** Pictogramas animados + botones de respuesta numericos con distractores.

**MatchImageWordPlayerComponent:** Columna pictogramas + columna palabras desordenadas. Tap para match.

**OrderSequencePlayerComponent:** Tarjetas CDK DragDrop + boton "Verificar".

**CompleteLetterPlayerComponent:** Pictograma + palabra con espacio + botones de letras.

**PictogramImageComponent** (compartido): URL ARASAAC + fallback.

**Criterios de aceptacion:**
- Cada player puede ejecutarse standalone con ContentJson hardcodeado.
- Las animaciones respetan prefers-reduced-motion.
- El SuccessPercentage considera todos los intentos.
- El audio TTS usa la Web Speech API.

**Dependencias:** FE-10 (ActivityPlayerShell)
**Al completar esta HU se desbloquea:** FE-12 (el radar necesita respuestas generadas por los players)

---

#### FE-12 — Radar Chart de Habilidades — ALTA

**Como** profesional
**Quiero** ver el perfil de habilidades de cada persona como un grafico radar con ejes dinamicos
**Para** tener una vista rapida y visual de las fortalezas y areas a trabajar

**Que construye el Frontend:**
RadarChartComponent: Usa Chart.js, ejes dinamicos con colores de cada area, tooltip con score y cantidad de actividades. Estado vacio si no hay datos.

En PersonDetailComponent: nueva seccion/pestana "Radar de Habilidades".

**Criterios de aceptacion:**
- El chart se actualiza cuando cambian los datos (reactivo).
- Respeta el perfil de accesibilidad: modo alto contraste usa paleta distinguible.
- Con mock: 3 ejes hardcodeados con scores 75, 60, 45.
- Solo visible para rol Profesional.

**Dependencias:** BE-12 (GET /radar), BE-11 (responses)
**Al completar esta HU se desbloquea:** FE-13 (dashboard usa el radar como componente)

---

#### FE-13 — Dashboards con Datos Reales — ALTA

**Como** profesional / representante familiar
**Quiero** ver en mi dashboard estadisticas reales del estado actual sin datos de ejemplo
**Para** tomar decisiones pedagogicas rapidas desde la pantalla principal

**Que construye el Frontend:**
DashboardProfessionalComponent: 4 KPI cards, tabla ultimas 5 completadas, lista proximas con vencimiento, accesos rapidos.

DashboardFamilyComponent: Nombre y foto del familiar, ultimas 3 actividades, badge mensajes, link a reportes.

**Criterios de aceptacion:**
- Si no hay datos: cada seccion muestra estado vacio con mensaje y accion sugerida.
- Las KPIs usan datos reales del GET /dashboard/professional.
- Carga en menos de 2 segundos con skeleton loader.

**Dependencias:** BE-12 (GET /dashboard con datos reales)

---

### Sprint 4 — 5 Historias

---

#### FE-14 — Diagnosticos — Timeline y Formulario — CRITICA

**Como** profesional
**Quiero** registrar y consultar diagnosticos funcionales de cada persona desde su perfil
**Para** que quede documentado el historial clinico como base de cada plan educativo

**Que construye el Frontend:**
DiagnosisTimelineComponent (pestana en PersonDetail): Cards cronologicas, click abre detalle, boton "Nuevo diagnostico".

DiagnosisFormComponent: 7 campos de texto largo + campo fecha. Editable solo si currentUser === creador. Banner "Solo lectura" si es de otro profesional.

DiagnosisService: getAll(), getById(), create(), update()

**Criterios de aceptacion:**
- Los diagnosticos de otros profesionales se muestran con banner amarillo "Solo lectura — creado por [nombre]".
- Solo fecha y diagnostico principal son requeridos.
- Con mock: 1 diagnostico propio y 1 de otro profesional.

**Dependencias:** BE-13 (GET /diagnoses + POST) para datos reales

---

#### FE-15 — Reportes de Progreso — Creacion y Vista Familiar — ALTA

**Como** profesional / representante familiar
**Quiero** que el profesional pueda generar reportes formales y la familia pueda leerlos y descargarlos
**Para** que exista un canal oficial de comunicacion sobre el progreso educativo

**Que construye el Frontend:**
ReportListComponent (profesional Y familiar): Lista con tipo, titulo, periodo, fecha, badge "Nuevo".
ReportFormComponent (solo profesional): Periodo date range, select tipo, 5 textareas, toggle incluir resumen.
ReportDetailComponent: Vista completa + boton "Descargar PDF".

ReportsService: getAll(), getById(), create(), exportPdf()

**Criterios de aceptacion:**
- El familiar solo ve reportes de su familiar vinculado.
- Badge "Nuevo" desaparece cuando la familia abre el reporte.
- Con mock: 2 reportes hardcodeados.

**Dependencias:** BE-01 (report-types), BE-14 (GET/POST reportes) para datos reales

---

#### FE-16 — Mensajeria Interna — Inbox y Redaccion — ALTA

**Como** profesional / representante familiar
**Quiero** enviar y recibir mensajes con la contraparte dentro de la plataforma
**Para** centralizar la comunicacion y tener historial sin depender de WhatsApp o email

**Que construye el Frontend:**
MessageInboxComponent: Lista de conversaciones agrupadas por persona/hilo. Columna izquierda: conversaciones. Derecha: mensajes del hilo. Mensajes no leidos: fondo destacado + punto azul.

MessageComposeComponent (boton flotante + modal): Selector destinatario, persona relacionada, asunto + contenido. Reply pre-llena destinatario + ParentMessageId.

UnreadBadgeComponent: Polling cada 30s a GET /messages/unread-count.

MessagesService: getInbox(), getSent(), getById(), send(), getUnreadCount()

**Criterios de aceptacion:**
- El polling se detiene cuando el usuario esta en la ruta /messages.
- El inbox agrupa por hilo (parentMessageId).
- Con mock: 2 conversaciones, 1 con mensajes no leidos.

**Dependencias:** BE-15 (todos los endpoints de mensajeria) para conectar

---

#### FE-17 — Panel de Configuracion del Motor Adaptativo — ALTA (NUEVA)

**Como** profesional
**Quiero** configurar los rangos y umbrales del motor de dificultad adaptativa para cada actividad del roadmap
**Para** personalizar como el sistema ajusta automaticamente la dificultad segun mi criterio pedagogico

**Que construye el Frontend:**
AdaptiveConfigPanelComponent (dentro de RoadmapManager, por nodo de actividad):
- Toggle "Habilitar motor adaptativo" (crea/elimina AdaptiveEngineConfig)
- Sliders/inputs para: MinDifficultyLevel, MaxDifficultyLevel, MinTimeLimitSeconds, MaxTimeLimitSeconds
- Inputs numericos: ConsecutiveSuccessToUpgrade, ConsecutiveFailuresToDowngrade, SuccessThresholdPercent, FrustrationThreshold
- Preview visual de los rangos configurados
- Boton "Restaurar valores por defecto"

AdaptiveConfigService: getConfig(personRoadmapActivityId), createConfig(dto), updateConfig(id, dto), deleteConfig(id)

**Criterios de aceptacion:**
- Si no hay config: muestra boton "Activar motor adaptativo" que crea con valores por defecto.
- Los sliders de dificultad validan que Min <= Max.
- Los sliders de tiempo validan que Min <= Max (si se configuran).
- Con mock: panel con valores por defecto hardcodeados.

**Dependencias:** BE-17 (endpoints de AdaptiveEngineConfig), FE-07 (RoadmapManager)
**Al completar esta HU se desbloquea:** FE-18 (timeline de ajustes necesita que haya config activa)

---

#### FE-18 — Timeline de Ajustes Adaptativos — ALTA (NUEVA)

**Como** profesional
**Quiero** ver el historial cronologico de todos los ajustes automaticos que el motor hizo para cada estudiante
**Para** entender como evoluciono la configuracion y tomar decisiones pedagogicas informadas

**Que construye el Frontend:**
AdaptiveLogTimelineComponent (pestana en PersonDetail o seccion en RoadmapManager):
- Timeline vertical cronologico descendente
- Cada entrada: icono segun tipo de ajuste + timestamp + descripcion legible
- Colores: verde (escalamiento), amarillo (desescalamiento), rojo (frustracion)
- Filtros: por actividad del roadmap, por tipo de ajuste, por rango de fechas
- Detalle expandible: valores anterior y nuevo, respuesta que lo disparo

AdaptiveLogService: getLog(personId, filters)

**Criterios de aceptacion:**
- El timeline muestra el AdjustmentType en lenguaje legible (ej: "Dificultad subio de 2 a 3").
- Los ajustes de frustracion se destacan visualmente con borde rojo.
- Si no hay ajustes: muestra "El motor adaptativo aun no ha realizado ajustes".
- Con mock: 5 entradas hardcodeadas con distintos tipos de ajuste.

**Dependencias:** BE-17 (GET /adaptive-log), FE-17 (panel de configuracion)

---

## Dependencias Cruzadas BE <> FE

| FE necesita | BE que lo provee | Endpoint clave | Para que lo usa el FE |
|---|---|---|---|
| FE-01 | BE-01 | GET /api/catalogs/* | Poblar todos los dropdowns de catalogos |
| FE-02 | BE-02 | GET/POST /api/professionals | Conectar lista y formulario de profesionales |
| FE-04 | BE-04 | GET /api/skill-areas | Paso 1 del wizard de actividad (areas) |
| FE-04 | BE-05 | GET /api/activity-templates | Paso 2 y schema para DynamicForm |
| FE-04 | BE-06 | POST /api/activities | Guardar la actividad creada |
| FE-06 | BE-07 | GET/POST /api/persons/{id}/skill-profile | Areas asignadas a la persona |
| FE-07 | BE-09 | GET/POST /api/persons/{id}/roadmap | Datos del roadmap del prof |
| FE-08 | BE-08 | GET /invitations/{code} + POST /accept | Validar y completar registro familiar |
| FE-09 | BE-09 | GET /api/persons/{id}/roadmap | Roadmap del alumno |
| FE-10 | BE-10 | GET /api/assignments/{id} | ContentJson para cargar el player |
| FE-10 | BE-11 | POST /start + POST /complete | Ciclo de vida de la actividad |
| FE-12 | BE-12 | GET /api/persons/{id}/radar | Datos para el radar chart |
| FE-13 | BE-12 | GET /api/dashboard/* | KPIs y listas del dashboard |
| FE-14 | BE-13 | GET/POST /api/persons/{id}/diagnoses | Historial y creacion de diagnosticos |
| FE-15 | BE-14 | GET/POST /api/persons/{id}/reports | Lista y creacion de reportes |
| FE-16 | BE-15 | GET/POST /api/messages | Inbox y envio de mensajes |
| FE-17 | BE-17 | GET/PUT /api/roadmap-activities/{id}/adaptive-config | Panel de configuracion del MDA |
| FE-18 | BE-17 | GET /api/persons/{id}/adaptive-log | Timeline de ajustes adaptativos |
