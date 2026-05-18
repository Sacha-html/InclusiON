# DER — InclusiON

**Última actualización:** 2026-05-15  
**Fuente:** `InclusiON.Data/AppDbContext.cs` + `InclusiON.Domain/Models/`

```mermaid
erDiagram

    %% ─── IDENTITY / AUTH ────────────────────────────────────────────────────

    %% Identidad base de todos los actores del sistema. Contiene credenciales
    %% (ASP.NET Identity) y datos de sesión. Cada User tiene exactamente un
    %% perfil de rol (Professional, PersonWithDisability o FamilyRepresentative).
    User {
        uuid        Id                  PK  "NOT NULL"
        varchar256  Email               UK  "nullable"
        varchar100  Name                    "nullable"
        varchar100  Surname                 "nullable"
        bool        IsActive                "NOT NULL"
        bool        MustChangePassword      "NOT NULL"
        timestamptz CreatedAt               "NOT NULL"
        timestamptz LastLoginDate           "nullable"
    }

    %% Tokens de renovación de sesión JWT. Cada token tiene vida útil y puede
    %% revocarse individualmente, permitiendo logout desde múltiples dispositivos.
    RefreshToken {
        uuid        Id          PK  "NOT NULL"
        uuid        UserId      FK  "NOT NULL"
        varchar512  Token           "NOT NULL"
        timestamptz ExpiresAt       "NOT NULL"
        timestamptz CreatedAt       "NOT NULL"
        timestamptz RevokedAt       "nullable"
        bool        IsActive        "NOT NULL"
    }

    %% Dispositivos autorizados para login asistido. Un supervisor puede autorizar
    %% un dispositivo para que la persona inicie sesión sin credenciales propias.
    TrustedDevice {
        int         Id                  PK  "NOT NULL"
        uuid        UserId              FK  "NOT NULL"
        uuid        AuthorizedByUserId  FK  "nullable"
        varchar256  DeviceId                "NOT NULL"
        varchar100  DeviceName              "nullable"
        varchar100  Browser                 "nullable"
        timestamptz RegisteredAt            "NOT NULL"
        timestamptz LastUsedAt             "nullable"
    }

    %% ─── CATÁLOGOS ──────────────────────────────────────────────────────────

    %% Catálogo de tipos de discapacidad reconocidos. Valores de referencia
    %% usados en el perfil de la persona y en filtros de actividades.
    DisabilityType {
        int     Id          PK  "NOT NULL"
        varchar100 Name         "NOT NULL"
        text    Description     "nullable"
        bool    IsActive        "NOT NULL"
    }

    %% Catálogo de niveles de autonomía. Determina si la persona requiere
    %% supervisión durante el login y la ejecución de actividades.
    AutonomyLevel {
        int     Id                  PK  "NOT NULL"
        varchar100 Name                 "NOT NULL"
        bool    RequiresSupervision     "NOT NULL"
        int     DisplayOrder            "NOT NULL"
        bool    IsActive                "NOT NULL"
    }

    %% Catálogo de métodos de autenticación disponibles (STANDARD, PIN,
    %% ASSISTED, FAMILY). Define qué credenciales requiere cada método.
    LoginMethod {
        int    Id                  PK  "NOT NULL"
        varchar20 Code             UK  "NOT NULL"
        varchar100 Name                "NOT NULL"
        bool   RequiresEmail           "NOT NULL"
        bool   RequiresPassword        "NOT NULL"
        bool   RequiresPin             "NOT NULL"
        bool   RequiresSupervisor      "NOT NULL"
        bool   IsActive                "NOT NULL"
    }

    %% Categorías temáticas de actividades. Usadas para organizar el catálogo
    %% de actividades del profesional y como filtro de búsqueda.
    ActivityCategory {
        int    Id           PK  "NOT NULL"
        varchar100 Name         "NOT NULL"
        text   Description      "nullable"
        bool   IsActive         "NOT NULL"
    }

    %% Tipos de reporte de progreso disponibles. Define la estructura
    %% y propósito de los reportes clínicos generados por el profesional.
    ReportType {
        int    Id           PK  "NOT NULL"
        varchar100 Name         "NOT NULL"
        text   Description      "nullable"
        bool   IsActive         "NOT NULL"
    }

    %% Áreas de habilidad del sistema (Comunicación, Alfabetización, etc.).
    %% Eje central del radar chart y de la organización del roadmap.
    SkillArea {
        int    Id           PK  "NOT NULL"
        varchar100 Name         "NOT NULL"
        varchar50 Icon           "nullable"
        varchar7  Color          "nullable"
        int    DisplayOrder      "NOT NULL"
        bool   IsActive          "NOT NULL"
    }

    %% Tipos de template con su estructura JSON y componente Angular asociado.
    %% Define qué campos contiene el ContentJson de cada actividad según su tipo.
    ActivityTemplateType {
        int    Id             PK  "NOT NULL"
        int    SkillAreaId    FK  "NOT NULL"
        varchar100 Name           "NOT NULL"
        varchar50 Code        UK  "NOT NULL"
        text   ContentSchema      "NOT NULL - JSON Schema"
        varchar100 ComponentName  "NOT NULL"
        bool   UsesPictograms     "NOT NULL"
        bool   HasAudio           "NOT NULL"
        int    DisplayOrder       "NOT NULL"
        bool   IsActive           "NOT NULL"
    }

    %% ─── INSTITUCIONES ───────────────────────────────────────────────────────

    %% Instituciones educativas registradas en el sistema. Los profesionales
    %% se vinculan a instituciones; los admins institucionales filtran su scope.
    EducationalInstitution {
        int    Id       PK  "NOT NULL"
        varchar200 Name     "NOT NULL"
        varchar300 Address  "nullable"
        varchar20 Phone     "nullable"
        varchar256 Email    "nullable"
        bool   IsActive     "NOT NULL"
    }

    %% Relación entre admins y las instituciones que gestionan.
    %% Un admin institucional solo ve datos de sus instituciones asignadas.
    AdminInstitution {
        uuid        AdminUserId     FK  "NOT NULL - PK compuesto"
        int         InstitutionId   FK  "NOT NULL - PK compuesto"
        timestamptz AssignedAt          "NOT NULL"
        bool        IsActive            "NOT NULL"
    }

    %% ─── PERFILES DE USUARIO ─────────────────────────────────────────────────

    %% Perfil extendido del usuario con rol profesional. Incluye datos académicos
    %% y el estado de validación (Pending → Approved / Rejected).
    Professional {
        uuid        Id              PK  "NOT NULL"
        uuid        UserId          FK  "NOT NULL - UK"
        varchar100  FirstName           "NOT NULL"
        varchar100  LastName            "NOT NULL"
        varchar20   DocumentNumber  UK  "nullable"
        varchar200  Specialty           "nullable"
        varchar50   LicenseNumber       "nullable"
        varchar20   Status              "NOT NULL - Pending/Approved/Rejected"
        timestamptz ValidatedAt         "nullable"
        bool        IsActive            "NOT NULL"
    }

    %% Auditoría de cambios de estado del profesional. Registra quién realizó
    %% el cambio (Pending → Approved, etc.) y con qué observación.
    ProfessionalStatusHistory {
        uuid   Id              PK  "NOT NULL"
        uuid   ProfessionalId  FK  "NOT NULL"
        varchar20 OldStatus        "NOT NULL"
        varchar20 NewStatus        "NOT NULL"
        text   Observation         "nullable"
        uuid   ChangedByUserId FK  "NOT NULL"
    }

    %% Perfil central de la persona atendida. Concentra identidad, discapacidad,
    %% autonomía, preferencias de accesibilidad y método de autenticación.
    PersonWithDisability {
        uuid   Id               PK  "NOT NULL"
        uuid   UserId           FK  "NOT NULL - UK"
        int    DisabilityTypeId FK  "nullable"
        int    AutonomyLevelId  FK  "nullable"
        int    LoginMethodId    FK  "nullable"
        uuid   SupervisorUserId FK  "nullable"
        varchar100 FirstName        "NOT NULL"
        varchar100 LastName         "NOT NULL"
        varchar20 DocumentNumber UK "nullable"
        date   BirthDate            "NOT NULL"
        varchar7 AvatarColor        "nullable"
        bool   UsesAAC              "NOT NULL"
        bool   UsesSignLanguage     "NOT NULL"
        bool   RequiresHighContrast "NOT NULL"
        bool   RequiresLargeFont    "NOT NULL"
        bool   IsActive             "NOT NULL"
    }

    %% Perfil del familiar/tutor. Se vincula a una o más personas con discapacidad
    %% y accede al portal familiar para ver reportes y progreso.
    FamilyRepresentative {
        uuid   Id              PK  "NOT NULL"
        uuid   UserId          FK  "NOT NULL - UK"
        varchar100 FirstName       "NOT NULL"
        varchar100 LastName        "NOT NULL"
        varchar20 DocumentNumber UK "nullable"
        varchar20 Phone             "nullable"
        varchar50 Relationship      "nullable"
        varchar20 Status            "NOT NULL"
        bool   IsActive             "NOT NULL"
    }

    %% Auditoría de cambios de estado del familiar. Flujo análogo al del
    %% profesional (Pending → Approved / Rejected).
    FamilyStatusHistory {
        uuid   Id              PK  "NOT NULL"
        uuid   FamilyId        FK  "NOT NULL"
        varchar20 OldStatus        "NOT NULL"
        varchar20 NewStatus        "NOT NULL"
        text   Observation         "nullable"
        uuid   ChangedByUserId FK  "NOT NULL"
    }

    %% ─── RELACIONES ENTRE PERFILES ───────────────────────────────────────────

    %% Relación entre profesionales e instituciones donde trabajan.
    %% Un profesional puede pertenecer a múltiples instituciones.
    ProfessionalInstitution {
        uuid        ProfessionalId  FK  "NOT NULL - PK compuesto"
        int         InstitutionId   FK  "NOT NULL - PK compuesto"
        timestamptz AssignedAt          "NOT NULL"
        bool        IsActive            "NOT NULL"
    }

    %% Relación de atención entre un profesional y una persona con discapacidad.
    %% Indica si es el profesional principal y si puede supervisar el login.
    ProfessionalPerson {
        uuid        ProfessionalId          FK  "NOT NULL - PK compuesto"
        uuid        PersonId                FK  "NOT NULL - PK compuesto"
        bool        IsPrimaryProfessional       "NOT NULL"
        bool        CanSuperviseLogin           "NOT NULL"
        timestamptz AssignedAt                  "NOT NULL"
        bool        IsActive                    "NOT NULL"
    }

    %% Vínculo activo entre persona con discapacidad y su familiar/representante.
    %% Registra tipo de relación, consentimiento informado y fecha de vigencia.
    PersonRepresentative {
        uuid        Id               PK  "NOT NULL"
        uuid        PersonId         FK  "NOT NULL"
        uuid        RepresentativeId FK  "NOT NULL"
        varchar50   Relationship         "nullable"
        bool        IsPrimary            "NOT NULL"
        bool        HasInformedConsent   "NOT NULL"
        bool        CanSuperviseLogin    "NOT NULL"
        timestamptz CreatedAt            "NOT NULL"
        timestamptz EndedAt              "nullable"
        bool        IsActive             "NOT NULL"
    }

    %% Historial de cambios en el vínculo persona-familiar. Permite auditar
    %% altas, bajas y modificaciones de la relación a lo largo del tiempo.
    PersonRepresentativeHistory {
        uuid   Id                       PK  "NOT NULL"
        uuid   PersonRepresentativeId   FK  "NOT NULL"
        uuid   PersonId                 FK  "NOT NULL"
        uuid   RepresentativeId         FK  "NOT NULL"
        varchar50 ChangeType                "NOT NULL"
        varchar50 Relationship              "NOT NULL"
        bool   WasPrimary                   "NOT NULL"
        uuid   ChangedByUserId          FK  "NOT NULL"
    }

    %% Áreas de habilidad activas para una persona. Determina qué secciones
    %% del radar chart se muestran y qué áreas tiene disponibles en el roadmap.
    PersonSkillProfile {
        uuid        PersonId    FK  "NOT NULL - PK compuesto"
        int         SkillAreaId FK  "NOT NULL - PK compuesto"
        timestamptz AssignedAt      "NOT NULL"
        bool        IsActive        "NOT NULL"
    }

    %% ─── INVITACIONES ────────────────────────────────────────────────────────

    %% Código generado por el profesional para que un familiar se registre y
    %% quede vinculado automáticamente a una persona. De un solo uso, con vencimiento.
    Invitation {
        int         Id                      PK  "NOT NULL"
        uuid        CreatedByProfessionalId FK  "NOT NULL"
        uuid        ForPersonId             FK  "nullable"
        uuid        UsedByUserId            FK  "nullable"
        varchar256  Email                       "NOT NULL"
        varchar64   Code                    UK  "NOT NULL"
        varchar50   Relationship                "NOT NULL"
        timestamptz ExpiresAt                   "NOT NULL"
        bool        IsUsed                      "NOT NULL"
        bool        IsActive                    "NOT NULL"
    }

    %% ─── ACTIVIDADES ─────────────────────────────────────────────────────────

    %% Actividad educativa creada por un profesional. Define área de habilidad,
    %% nivel de complejidad, template y configuración de accesibilidad (AAC, audio).
    Activity {
        int    Id                   PK  "NOT NULL"
        uuid   ProfessionalId       FK  "NOT NULL"
        int    CategoryId           FK  "NOT NULL"
        int    SkillAreaId          FK  "nullable"
        varchar200 Title                "NOT NULL"
        int    ComplexityLevel          "nullable"
        bool   RequiresSupervision      "NOT NULL"
        bool   IsStandardActivity       "NOT NULL"
        bool   HasVisualSupport         "NOT NULL"
        bool   HasAudioSupport          "NOT NULL"
        bool   UsesPictograms           "NOT NULL"
        bool   IsActive                 "NOT NULL"
    }

    %% Contenido dinámico de la actividad almacenado como JSON. La estructura
    %% varía según el TemplateType (opciones de selección, pares imagen-palabra, etc.).
    ActivityContent {
        int    Id             PK  "NOT NULL"
        int    ActivityId     FK  "NOT NULL - UK (1:1)"
        int    TemplateTypeId FK  "NOT NULL"
        jsonb  ContentJson        "NOT NULL"
        bool   IsActive           "NOT NULL"
    }

    %% Vector semántico de la actividad para búsqueda por similaridad (pgvector).
    %% Se genera al crear o editar la actividad si el módulo semántico está activo.
    ActivityEmbedding {
        int    ActivityId     PK  "NOT NULL - FK 1:1"
        varchar100 Model          "NOT NULL"
        int    Dimensions         "NOT NULL"
        text   EmbeddingJson      "NOT NULL - vector serializado"
    }

    %% ─── ROADMAP ─────────────────────────────────────────────────────────────

    %% Plan de aprendizaje personalizado de una persona. Cada persona tiene
    %% exactamente un roadmap activo, organizado por áreas de habilidad.
    PersonRoadmap {
        int  Id                         PK  "NOT NULL"
        uuid PersonId                   FK  "NOT NULL - UK (1:1)"
        uuid CreatedByProfessionalId    FK  "NOT NULL"
        bool IsActive                       "NOT NULL"
    }

    %% Sección del roadmap correspondiente a un área de habilidad. Agrupa
    %% las actividades que la persona debe completar en esa área.
    PersonRoadmapArea {
        int  Id             PK  "NOT NULL"
        int  PersonRoadmapId FK "NOT NULL"
        int  SkillAreaId    FK  "NOT NULL"
        int  DisplayOrder       "NOT NULL"
        bool IsActive           "NOT NULL"
    }

    %% Actividad dentro del roadmap con configuración propia de dificultad,
    %% umbral de desbloqueo y límites de tiempo e intentos.
    PersonRoadmapActivity {
        int         Id                      PK  "NOT NULL"
        int         PersonRoadmapAreaId     FK  "NOT NULL"
        int         ActivityId              FK  "NOT NULL"
        int         SequenceOrder               "NOT NULL"
        bool        IsUnlocked                  "NOT NULL"
        int         UnlockThresholdPercent      "NOT NULL - 0 a 100"
        int         DifficultyLevel             "NOT NULL - 1 a 5"
        bool        ShowHints                   "NOT NULL"
        int         TimeLimitSeconds            "nullable"
        int         MaxAttempts                 "nullable"
        timestamptz UnlockedAt                  "nullable"
        bool        IsActive                    "NOT NULL"
    }

    %% ─── ASIGNACIONES Y RESPUESTAS ───────────────────────────────────────────

    %% Asignación directa de una actividad a una persona por parte del profesional.
    %% Independiente del roadmap; permite asignar actividades puntuales o de evaluación.
    ActivityAssignment {
        int         Id                          PK  "NOT NULL"
        int         ActivityId                  FK  "NOT NULL"
        uuid        PersonId                    FK  "NOT NULL"
        uuid        AssignedByProfessionalId    FK  "NOT NULL"
        varchar20   Status                          "NOT NULL - Pending/InProgress/Completed"
        bool        IsEvaluationActivity            "NOT NULL"
        timestamptz AssignedAt                      "NOT NULL"
        timestamptz DueDate                         "nullable"
        bool        IsActive                        "NOT NULL"
    }

    %% Resultado de una ejecución de actividad asignada. Almacena éxito, porcentaje,
    %% intentos y nivel de frustración. Datos clínicos cifrados con AES-256-GCM.
    ActivityResponse {
        int         Id                  PK  "NOT NULL"
        int         AssignmentId        FK  "NOT NULL"
        varchar20   Result                  "nullable - cifrado: Correct/Incorrect/Partial"
        numeric5_2  SuccessPercentage       "nullable - 0.00 a 100.00"
        int         AttemptCount            "NOT NULL"
        bool        RequiredSupport         "NOT NULL"
        int         FrustrationLevel        "nullable"
        timestamptz StartedAt               "NOT NULL"
        timestamptz CompletedAt             "nullable"
        bool        IsActive                "NOT NULL"
    }

    %% Resultado detallado de un intento sobre una actividad del roadmap.
    %% Alimenta el radar chart y es el input principal del motor adaptativo.
    ActivityResult {
        int         Id                      PK  "NOT NULL"
        int         PersonRoadmapActivityId FK  "NOT NULL"
        int         AttemptNumber               "NOT NULL"
        float4      ScorePercent                "NOT NULL - 0.0 a 1.0"
        int         TimeSpentSeconds            "NOT NULL"
        timestamptz CompletedAt                 "NOT NULL"
    }

    %% ─── MDA ─────────────────────────────────────────────────────────────────

    %% Configuración del motor de dificultad adaptativa para una actividad del roadmap.
    %% Define rangos y umbrales para ajustar la dificultad automáticamente.
    AdaptiveEngineConfig {
        int  Id                             PK  "NOT NULL"
        int  PersonRoadmapActivityId        FK  "NOT NULL - UK (1:1)"
        bool IsEnabled                          "NOT NULL"
        int  MinDifficultyLevel                 "NOT NULL"
        int  MaxDifficultyLevel                 "NOT NULL"
        int  ConsecutiveSuccessToUpgrade        "NOT NULL"
        int  ConsecutiveFailuresToDowngrade     "NOT NULL"
        int  SuccessThresholdPercent            "NOT NULL - 0 a 100"
        int  FrustrationThreshold               "NOT NULL - 0 a 5"
        bool IsActive                           "NOT NULL"
    }

    %% Registro de cada ajuste realizado por el motor adaptativo. Permite trazar
    %% el historial de cambios de dificultad para auditoría y visualización.
    AdaptiveAdjustmentLog {
        int         Id                      PK  "NOT NULL"
        int         PersonRoadmapActivityId FK  "NOT NULL"
        int         ActivityResponseId      FK  "NOT NULL"
        varchar50   AdjustmentType              "NOT NULL"
        text        PreviousValue               "NOT NULL"
        text        NewValue                    "NOT NULL"
        text        Reason                      "NOT NULL"
        timestamptz AdjustedAt                  "NOT NULL"
        bool        IsActive                    "NOT NULL"
    }

    %% ─── CLÍNICO ─────────────────────────────────────────────────────────────

    %% Diagnóstico funcional registrado por el profesional. El texto clínico
    %% se cifra automáticamente con AES-256-GCM vía la annotation [Encrypted].
    Diagnosis {
        int  Id             PK  "NOT NULL"
        uuid PersonId       FK  "NOT NULL"
        uuid ProfessionalId FK  "NOT NULL"
        date DiagnosisDate      "NOT NULL"
        text PrimaryDiagnosis   "NOT NULL - cifrado AES-256-GCM"
        bool IsActive           "NOT NULL"
    }

    %% Reporte de progreso con flujo de aprobación (Draft → Submitted → Approved/Rejected).
    %% El familiar recibe email al aprobarse; el profesional al rechazarse.
    Report {
        int    Id               PK  "NOT NULL"
        uuid   PersonId         FK  "NOT NULL"
        uuid   ProfessionalId   FK  "NOT NULL"
        int    ReportTypeId     FK  "NOT NULL"
        varchar200 Title            "NOT NULL"
        varchar20 Status            "NOT NULL - Draft/Submitted/Approved/Rejected"
        date   ReportDate           "NOT NULL"
        date   PeriodStartDate      "nullable"
        date   PeriodEndDate        "nullable"
        uuid   ApprovedBy       FK  "nullable"
        bool   IsActive             "NOT NULL"
    }

    %% ─── COMUNICACIÓN ────────────────────────────────────────────────────────

    %% Mensaje interno entre usuarios del sistema. Soporta hilos mediante
    %% ParentMessageId y puede estar relacionado a una persona como contexto.
    Message {
        int         Id              PK  "NOT NULL"
        uuid        SenderId        FK  "NOT NULL"
        uuid        ReceiverId      FK  "NOT NULL"
        uuid        RelatedPersonId FK  "nullable"
        int         ParentMessageId FK  "nullable - hilo"
        varchar200  Subject             "nullable"
        bool        IsRead              "NOT NULL"
        timestamptz SentAt              "NOT NULL"
        bool        IsActive            "NOT NULL"
    }

    %% ─── AUDITORÍA ───────────────────────────────────────────────────────────

    %% Registro de auditoría de acceso a recursos (IN-172). Detecta accesos
    %% indebidos y permite trazar quién accedió a datos de qué persona y cuándo.
    AccessAudit {
        int         Id              PK  "NOT NULL"
        uuid        UserId          FK  "NOT NULL"
        uuid        AccessedPersonId FK "nullable"
        varchar50   Role                "nullable"
        varchar50   ActionType          "NOT NULL"
        varchar20   Result              "NOT NULL - Allowed/Denied"
        varchar100  AffectedTable       "nullable"
        timestamptz Timestamp           "NOT NULL"
    }


    %% ═══ RELACIONES ═══════════════════════════════════════════════════════════

    %% Auth
    User ||--o{ RefreshToken       : "tokens"
    User ||--o{ TrustedDevice      : "dispositivos"
    User ||--o{ AdminInstitution   : "admin de"

    %% User → perfiles (1:1)
    User ||--o| Professional         : "perfil"
    User ||--o| PersonWithDisability : "perfil"
    User ||--o| FamilyRepresentative : "perfil"

    %% Supervisor (User → PersonWithDisability)
    User ||--o{ PersonWithDisability : "supervisa login"

    %% Catálogos → PersonWithDisability
    DisabilityType ||--o{ PersonWithDisability : "tipo discapacidad"
    AutonomyLevel  ||--o{ PersonWithDisability : "nivel autonomía"
    LoginMethod    ||--o{ PersonWithDisability : "método auth"

    %% Catálogos → ActivityTemplateType
    SkillArea ||--o{ ActivityTemplateType : "agrupa templates"

    %% Instituciones
    EducationalInstitution ||--o{ AdminInstitution        : "admins"
    EducationalInstitution ||--o{ ProfessionalInstitution : "profesionales"
    Professional           ||--o{ ProfessionalInstitution : "trabaja en"

    %% Historiales de estado
    Professional         ||--o{ ProfessionalStatusHistory   : "historial estado"
    FamilyRepresentative ||--o{ FamilyStatusHistory          : "historial estado"

    %% Relaciones persona
    Professional         ||--o{ ProfessionalPerson : "atiende"
    PersonWithDisability ||--o{ ProfessionalPerson : "atendida por"

    PersonWithDisability ||--o{ PersonRepresentative      : "representada por"
    FamilyRepresentative ||--o{ PersonRepresentative      : "representa a"
    PersonRepresentative ||--o{ PersonRepresentativeHistory : "historial"

    PersonWithDisability ||--o{ PersonSkillProfile : "perfil habilidades"
    SkillArea            ||--o{ PersonSkillProfile : "asignada a"

    %% Invitaciones
    Professional         ||--o{ Invitation : "crea"
    PersonWithDisability ||--o{ Invitation : "destino"
    User                 ||--o{ Invitation : "usada por"

    %% Actividades
    Professional      ||--o{ Activity        : "crea"
    ActivityCategory  ||--o{ Activity        : "clasifica"
    SkillArea         ||--o{ Activity        : "área"
    Activity          ||--o| ActivityContent  : "contenido (1:1)"
    Activity          ||--o| ActivityEmbedding : "embedding (1:1)"
    ActivityTemplateType ||--o{ ActivityContent : "define estructura"

    %% Roadmap
    PersonWithDisability ||--|| PersonRoadmap     : "plan (1:1)"
    Professional         ||--o{ PersonRoadmap     : "crea"
    PersonRoadmap        ||--o{ PersonRoadmapArea : "áreas"
    SkillArea            ||--o{ PersonRoadmapArea : "define área"
    PersonRoadmapArea    ||--o{ PersonRoadmapActivity : "actividades"
    Activity             ||--o{ PersonRoadmapActivity : "incluida en"

    %% Asignaciones
    Activity             ||--o{ ActivityAssignment : "asignada"
    PersonWithDisability ||--o{ ActivityAssignment : "recibe"
    Professional         ||--o{ ActivityAssignment : "asigna"
    ActivityAssignment   ||--o{ ActivityResponse   : "respuestas"

    %% Resultados roadmap
    PersonRoadmapActivity ||--o{ ActivityResult : "resultados"

    %% MDA
    PersonRoadmapActivity ||--o| AdaptiveEngineConfig  : "config MDA (1:1)"
    PersonRoadmapActivity ||--o{ AdaptiveAdjustmentLog : "ajustes"
    ActivityResponse      ||--o{ AdaptiveAdjustmentLog : "dispara"

    %% Clínico
    PersonWithDisability ||--o{ Diagnosis : "evaluada"
    Professional         ||--o{ Diagnosis : "registra"
    PersonWithDisability ||--o{ Report    : "reportada"
    Professional         ||--o{ Report    : "genera"
    ReportType           ||--o{ Report    : "tipo"

    %% Comunicación
    User                 ||--o{ Message : "envía"
    User                 ||--o{ Message : "recibe"
    PersonWithDisability ||--o{ Message : "tema"
    Message              ||--o{ Message : "hilo"

    %% Auditoría
    User                 ||--o{ AccessAudit : "genera"
    PersonWithDisability ||--o{ AccessAudit : "accedida en"
```

---

## Convención de tipos

| Tipo en diagrama | Tipo PostgreSQL real | Notas |
|---|---|---|
| `uuid` | `uuid` | PKs y FKs de entidades de dominio |
| `int` | `integer` | PKs de catálogos y entidades de ejecución |
| `varchar(n)` | `character varying(n)` | Strings acotados; el `n` indica límite |
| `text` | `text` | Strings sin límite (JSON, contenido clínico) |
| `jsonb` | `jsonb` | Contenido dinámico de actividades |
| `bool` | `boolean` | Flags y soft-delete |
| `timestamptz` | `timestamp with time zone` | Fechas con zona horaria (UTC en DB) |
| `date` | `date` | Fechas sin hora (diagnósticos, reportes) |
| `numeric5_2` | `numeric(5,2)` | Porcentajes de éxito (0.00–100.00) |
| `float4` | `real` | Scores normalizados (0.0–1.0) |

---

## DbSets por nivel (AppDbContext)

| Nivel | Entidades |
|-------|-----------|
| 1 — Catálogos | DisabilityType, ActivityCategory, ReportType, EducationalInstitution, AutonomyLevel, LoginMethod, SkillArea, ActivityTemplateType |
| 2 — Auth/Perfiles | RefreshToken, Professional, PersonWithDisability, FamilyRepresentative, Invitation |
| 3 — Relaciones | AdminInstitution, TrustedDevice, ProfessionalInstitution, ProfessionalPerson, PersonRepresentative, PersonSkillProfile, Diagnosis, Activity, ActivityContent, PersonRoadmap, PersonRoadmapArea, PersonRoadmapActivity |
| 4 — Ejecución/Mensajería | ActivityAssignment, Report, Message, AccessAudit, ProfessionalStatusHistory, FamilyStatusHistory, PersonRepresentativeHistory |
| 5 — Respuestas/Embeddings | ActivityResponse, ActivityResult, ActivityEmbedding |
| 6 — MDA | AdaptiveEngineConfig, AdaptiveAdjustmentLog |
