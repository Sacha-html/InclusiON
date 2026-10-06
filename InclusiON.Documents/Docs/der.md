# DER — InclusiON

**Última actualización:** 2026-10-04  
**Fuente:** `InclusiON.Data/AppDbContext.cs` + `InclusiON.Domain/Models/` + Migraciones 2026-10-04  
**Naturaleza:** modelo persistido oficial y completo del sistema InclusiON (48 DbSets en español).

> **Alcance de esta revisión:** representación del modelo persistido vigente al 2026-10-04 con tablas, campos y relaciones traducidos al español. Incorpora todas las entidades y reglas estructurales consolidadas en septiembre de 2026 junto al catálogo de cargos directivos escolares (`RolInstitucional`) introducido en octubre de 2026. La cardinalidad expresa el esquema EF Core; las reglas de negocio de desactivación, validación explícita del roadmap y umbral inicial de desbloqueo se preservan como invariantes del dominio. Configurado con layout ELK y enrutamiento ortogonal (`edgeRouting: ORTHOGONAL`).

```mermaid
---
config:
  layout: elk
  elk:
    edgeRouting: ORTHOGONAL
---
erDiagram

    %% ─── IDENTIDAD / AUTENTICACIÓN ──────────────────────────────────────────

    %% Identidad base de todos los actores del sistema. Contiene credenciales
    %% (ASP.NET Identity) y datos de sesión. Las tres asociaciones de perfil son
    %% opcionales individualmente; la exclusividad mutua y la regla de un perfil
    %% máximo son invariantes de aplicación, no cardinalidades del DER.
    Usuario {
        uuid        Id                      PK  "NOT NULL"
        varchar256  Email                   UK  "nullable"
        varchar100  Nombre                      "nullable"
        varchar100  Apellido                    "nullable"
        varchar256  NumeroTelefono              "nullable"
        bool        Activo                      "NOT NULL"
        bool        DebeCambiarContrasena       "NOT NULL"
        timestamptz FechaCreacion               "NOT NULL"
        timestamptz FechaUltimoLogin            "nullable"
    }

    %% Tokens de renovación de sesión JWT. Cada token tiene vida útil y puede
    %% revocarse individualmente, permitiendo logout desde múltiples dispositivos.
    TokenRenovacion {
        uuid        Id              PK  "NOT NULL"
        uuid        UsuarioId       FK  "NOT NULL"
        varchar512  Token               "NOT NULL"
        timestamptz ExpiraEn            "NOT NULL"
        timestamptz CreadoEn            "NOT NULL"
        timestamptz RevocadoEn          "nullable"
        bool        Activo              "NOT NULL"
    }

    %% Dispositivos autorizados para login asistido. Un supervisor puede autorizar
    %% un dispositivo para que la persona inicie sesión sin credenciales propias.
    DispositivoConfianza {
        int         Id                      PK  "NOT NULL"
        uuid        UsuarioId               FK  "NOT NULL"
        uuid        AutorizadoPorUsuarioId  FK  "nullable"
        varchar256  DispositivoId               "NOT NULL"
        varchar100  NombreDispositivo           "nullable"
        varchar100  Navegador                   "nullable"
        timestamptz RegistradoEn                "NOT NULL"
        timestamptz UltimoUsoEn                 "nullable"
    }

    %% Tokens seguros de restablecimiento de contraseña generados para recuperación de acceso.
    TokenRecuperacionContrasena {
        uuid        Id              PK  "NOT NULL"
        uuid        UsuarioId       FK  "NOT NULL"
        varchar256  HashToken           "NOT NULL - SHA256"
        timestamptz CreadoEn            "NOT NULL"
        timestamptz ExpiraEn            "NOT NULL"
        timestamptz UsadoEn             "nullable"
        bool        EstaUsado           "NOT NULL"
    }

    %% ─── CATÁLOGOS DE REFERENCIA ────────────────────────────────────────────

    %% Catálogo de tipos de discapacidad reconocidos. Valores de referencia
    %% usados en el perfil de la persona y en filtros de actividades.
    TipoDiscapacidad {
        int         Id              PK  "NOT NULL"
        varchar100  Nombre              "NOT NULL"
        text        Descripcion         "nullable"
        bool        Activo              "NOT NULL"
    }

    %% Catálogo de niveles de autonomía. Determina si la persona requiere
    %% supervisión durante el login y la ejecución de actividades.
    NivelAutonomia {
        int         Id                  PK  "NOT NULL"
        varchar100  Nombre                  "NOT NULL"
        bool        RequiereSupervision     "NOT NULL"
        int         OrdenVisualizacion      "NOT NULL"
        bool        Activo                  "NOT NULL"
    }

    %% Catálogo de métodos de autenticación disponibles (STANDARD=1, PIN=2,
    %% ASSISTED=3). Define qué credenciales requiere cada método.
    MetodoLogin {
        int         Id                  PK  "NOT NULL"
        varchar20   Codigo              UK  "NOT NULL"
        varchar100  Nombre                  "NOT NULL"
        bool        RequiereEmail           "NOT NULL"
        bool        RequiereContrasena      "NOT NULL"
        bool        RequierePin             "NOT NULL"
        bool        RequiereSupervisor      "NOT NULL"
        bool        Activo                  "NOT NULL"
    }

    %% Categorías temáticas de actividades. Usadas para organizar el catálogo
    %% de actividades del profesional y como filtro de búsqueda.
    CategoriaActividad {
        int         Id              PK  "NOT NULL"
        varchar100  Nombre              "NOT NULL"
        text        Descripcion         "nullable"
        bool        Activo              "NOT NULL"
    }

    %% Tipos de reporte de progreso disponibles. Define la estructura
    %% y propósito de los reportes clínicos generados por el profesional.
    TipoReporte {
        int         Id              PK  "NOT NULL"
        varchar100  Nombre              "NOT NULL"
        text        Descripcion         "nullable"
        bool        Activo              "NOT NULL"
    }

    %% Áreas de habilidad del sistema (Comunicación, Alfabetización, etc.).
    %% Eje central del radar chart y de la organización del roadmap.
    AreaHabilidad {
        int         Id                  PK  "NOT NULL"
        varchar100  Nombre                  "NOT NULL"
        varchar50   Icono                   "nullable"
        varchar7    Color                   "nullable"
        int         OrdenVisualizacion      "NOT NULL"
        bool        Activo                  "NOT NULL"
    }

    %% Tipos de template con su estructura JSON y componente Angular asociado.
    %% Define qué campos contiene el ContenidoJson de cada actividad según su tipo.
    TipoPlantillaActividad {
        int         Id                  PK  "NOT NULL"
        int         AreaHabilidadId     FK  "NOT NULL"
        varchar100  Nombre                  "NOT NULL"
        varchar50   Codigo              UK  "NOT NULL"
        text        EsquemaContenido        "NOT NULL - JSON Schema"
        varchar100  NombreComponente        "NOT NULL"
        bool        UsaPictogramas          "NOT NULL"
        bool        TieneAudio              "NOT NULL"
        int         OrdenVisualizacion      "NOT NULL"
        bool        Activo                  "NOT NULL"
    }

    %% Estados posibles de ejecución de una asignación de actividad (seed data).
    EstadoAsignacionActividad {
        int         Id              PK  "NOT NULL - semilla"
        varchar50   Nombre              "NOT NULL - UK"
    }

    %% Especialidades clínicas/terapéuticas normalizadas para profesionales.
    Especialidad {
        int         Id              PK  "NOT NULL"
        varchar100  Nombre          UK  "NOT NULL"
        bool        Activo              "NOT NULL"
    }

    %% Catálogo de roles/cargos directivos institucionales escolares
    %% (Director, Vicedirector, Secretario, Preceptor). Define el rol operativo en la sede.
    RolInstitucional {
        int         Id              PK  "NOT NULL"
        varchar100  Nombre          UK  "NOT NULL"
        varchar255  Descripcion         "nullable"
        bool        Activo              "NOT NULL"
    }

    %% Tipos de trabajos en segundo plano gestionados por agentes y workers asíncronos.
    TipoTrabajoSegundoPlano {
        int         Id              PK  "NOT NULL"
        varchar100  Nombre              "NOT NULL"
    }

    %% Estados posibles de ciclo de vida de un background job (Pending, Processing, Completed, Failed).
    EstadoTrabajoSegundoPlano {
        int         Id              PK  "NOT NULL"
        varchar100  Nombre              "NOT NULL"
    }

    %% ─── INSTITUCIONES Y AULAS ──────────────────────────────────────────────

    %% Instituciones educativas registradas en el sistema. Los profesionales
    %% se vinculan a instituciones; los admins institucionales filtran su scope.
    InstitucionEducativa {
        int         Id              PK  "NOT NULL"
        varchar200  Nombre              "NOT NULL"
        varchar300  Direccion           "nullable"
        varchar20   Telefono            "nullable"
        varchar256  Email               "nullable"
        bool        Activo              "NOT NULL"
    }

    %% Relación entre admins y las instituciones que gestionan.
    %% Un admin institucional solo ve datos de sus instituciones asignadas y su cargo directivo.
    AdminInstitucion {
        uuid        UsuarioAdminId      FK  "NOT NULL - PK compuesto"
        int         InstitucionId       FK  "NOT NULL - PK compuesto"
        int         RolInstitucionalId  FK  "nullable"
        timestamptz AsignadoEn              "NOT NULL"
        bool        Activo                  "NOT NULL"
    }

    %% Aulas asociadas a un profesional responsable para agrupar y organizar alumnos.
    Aula {
        uuid        Id              PK  "NOT NULL"
        varchar150  Nombre              "NOT NULL"
        uuid        ProfesionalId   FK  "NOT NULL"
        bool        Activo              "NOT NULL"
        timestamptz CreadoEn            "NOT NULL"
    }

    %% ─── PERFILES DE USUARIO ────────────────────────────────────────────────

    %% Perfil extendido del usuario con rol profesional. Incluye datos académicos
    %% y el estado de validación (Pending → Approved / Rejected).
    Profesional {
        uuid        Id                      PK  "NOT NULL"
        uuid        UsuarioId               FK  "NOT NULL - UK"
        varchar100  Nombre                      "NOT NULL"
        varchar100  Apellido                    "NOT NULL"
        varchar20   NumeroDocumento         UK  "nullable"
        varchar20   Telefono                    "nullable"
        varchar100  Especialidad                "nullable - texto legado"
        int         EspecialidadId          FK  "nullable"
        varchar50   NumeroMatricula             "nullable"
        int         Estado                      "NOT NULL - enum como entero"
        timestamptz ValidadoEn                  "nullable"
        uuid        ValidadoPorUsuarioId    FK  "nullable"
        varchar255  Email                       "nullable - UK filtrado"
        date        FechaNacimiento             "nullable"
        bool        Activo                      "NOT NULL"
    }

    %% Auditoría de cambios de estado del profesional. Registra quién realizó
    %% el cambio (Pending → Approved, etc.) y con qué observación.
    HistorialEstadoProfesional {
        uuid        Id                      PK  "NOT NULL"
        uuid        ProfesionalId           FK  "NOT NULL"
        varchar20   EstadoAnterior              "nullable"
        varchar20   EstadoNuevo                 "NOT NULL"
        text        Observacion                 "nullable"
        uuid        ModificadoPorUsuarioId  FK  "NOT NULL"
    }

    %% Perfil central de la persona atendida. Concentra identidad, discapacidad,
    %% autonomía, preferencias de accesibilidad, perfil sensorial y método de autenticación.
    PersonaConDiscapacidad {
        uuid        Id                          PK  "NOT NULL"
        uuid        UsuarioId                   FK  "NOT NULL - UK"
        int         TipoDiscapacidadId          FK  "nullable"
        int         NivelAutonomiaId            FK  "nullable"
        int         MetodoLoginId               FK  "nullable"
        uuid        SupervisorUsuarioId         FK  "nullable"
        varchar100  Nombre                          "NOT NULL"
        varchar100  Apellido                        "NOT NULL"
        varchar20   NumeroDocumento             UK  "nullable"
        date        FechaNacimiento                 "NOT NULL"
        varchar500  UrlFoto                         "nullable"
        int         NivelAtencion                   "nullable"
        int         NivelComunicacion               "nullable"
        int         NivelMotricidad                 "nullable"
        varchar500  InteresesYMotivadores           "nullable"
        varchar250  EstiloAprendizaje               "nullable"
        varchar255  RecursosDisponibles             "nullable"
        varchar500  TerapiasAdicionales             "nullable"
        varchar255  HashCodigoPin                   "nullable"
        varchar20   ColorAvatar                     "nullable"
        bool        UsaCAA                          "NOT NULL"
        bool        UsaLenguaSenas                  "NOT NULL"
        bool        RequiereAltoContraste           "NOT NULL"
        bool        RequiereFuenteGrande            "NOT NULL"
        bool        Activo                          "NOT NULL"
        bool        SensibilidadRuidoVisual         "NOT NULL"
        bool        SensibilidadSonido              "NOT NULL"
        varchar100  TipoDaltonismo                  "nullable"
    }

    %% Perfil del familiar/tutor. Se vincula a una o más personas con discapacidad
    %% y accede al portal familiar para ver reportes y progreso.
    RepresentanteFamiliar {
        uuid        Id                  PK  "NOT NULL"
        uuid        UsuarioId           FK  "NOT NULL - UK"
        varchar100  Nombre                  "NOT NULL"
        varchar100  Apellido                "NOT NULL"
        varchar20   NumeroDocumento     UK  "nullable"
        varchar20   Telefono                "nullable"
        varchar50   Parentesco              "nullable"
        varchar20   Estado                  "NOT NULL"
        bool        Activo                  "NOT NULL"
    }

    %% Auditoría de cambios de estado del familiar (Active ↔ Terminated).
    %% No tiene flujo de aprobación — el familiar queda Active al registrarse.
    HistorialEstadoFamiliar {
        uuid        Id                      PK  "NOT NULL"
        uuid        FamiliarId              FK  "NOT NULL"
        varchar20   EstadoAnterior              "nullable"
        varchar20   EstadoNuevo                 "NOT NULL"
        text        Observacion                 "nullable"
        uuid        ModificadoPorUsuarioId  FK  "nullable"
    }

    %% ─── RELACIONES ENTRE PERFILES ───────────────────────────────────────────

    %% Relación entre profesionales e instituciones donde trabajan.
    %% Un profesional puede pertenecer a múltiples instituciones.
    ProfesionalInstitucion {
        uuid        ProfesionalId   FK  "NOT NULL - PK compuesto"
        int         InstitucionId   FK  "NOT NULL - PK compuesto"
        timestamptz AsignadoEn          "NOT NULL"
        bool        Activo              "NOT NULL"
    }

    %% Relación de atención entre un profesional y una persona con discapacidad.
    %% Indica si es el profesional principal, si puede supervisar el login y su aula asignada.
    ProfesionalPersona {
        uuid        ProfesionalId           FK  "NOT NULL - PK compuesto"
        uuid        PersonaId               FK  "NOT NULL - PK compuesto"
        uuid        AulaId                  FK  "nullable"
        bool        EsProfesionalPrincipal      "NOT NULL"
        bool        PuedeSupervisarLogin        "NOT NULL"
        timestamptz AsignadoEn                  "NOT NULL"
        bool        Activo                      "NOT NULL"
    }

    %% Vínculo activo entre persona con discapacidad y su familiar/representante.
    %% Registra tipo de relación, consentimiento informado y fecha de vigencia.
    PersonaRepresentante {
        uuid        Id                          PK  "NOT NULL"
        uuid        PersonaId                   FK  "NOT NULL"
        uuid        RepresentanteId             FK  "NOT NULL"
        varchar50   Parentesco                      "nullable"
        bool        EsPrincipal                     "NOT NULL"
        bool        TieneConsentimientoInformado    "NOT NULL"
        bool        PuedeSupervisarLogin            "NOT NULL"
        timestamptz CreadoEn                        "NOT NULL"
        timestamptz FinalizadoEn                    "nullable"
        bool        Activo                          "NOT NULL"
    }

    %% Historial de cambios en el vínculo persona-familiar. Permite auditar
    %% altas, bajas y modificaciones de la relación a lo largo del tiempo.
    HistorialPersonaRepresentante {
        uuid        Id                          PK  "NOT NULL"
        uuid        PersonaRepresentanteId      FK  "NOT NULL"
        uuid        PersonaId                   FK  "NOT NULL"
        uuid        RepresentanteId             FK  "NOT NULL"
        varchar50   TipoCambio                      "NOT NULL"
        varchar50   Parentesco                      "NOT NULL"
        bool        EraPrincipal                    "NOT NULL"
        uuid        ModificadoPorUsuarioId      FK  "NOT NULL"
    }

    %% Áreas de habilidad activas para una persona. Determina qué secciones
    %% del radar chart se muestran y qué áreas tiene disponibles en el roadmap.
    PerfilHabilidadesPersona {
        uuid        PersonaId           FK  "NOT NULL - PK compuesto"
        int         AreaHabilidadId     FK  "NOT NULL - PK compuesto"
        timestamptz AsignadoEn              "NOT NULL"
        bool        Activo                  "NOT NULL"
    }

    %% ─── INVITACIONES ────────────────────────────────────────────────────────

    %% Código generado por el profesional para que un familiar se registre y
    %% quede vinculado automáticamente a una persona. De un solo uso, con vencimiento.
    Invitacion {
        int         Id                          PK  "NOT NULL"
        uuid        CreadoPorProfesionalId      FK  "NOT NULL"
        uuid        ParaPersonaId               FK  "nullable"
        uuid        UsadoPorUsuarioId           FK  "nullable"
        varchar256  Email                           "NOT NULL"
        varchar64   Codigo                      UK  "NOT NULL"
        varchar50   Parentesco                      "NOT NULL"
        timestamptz ExpiraEn                        "NOT NULL"
        bool        EstaUsada                       "NOT NULL"
        bool        Activo                          "NOT NULL"
    }

    %% ─── ACTIVIDADES Y CONTENIDO ─────────────────────────────────────────────

    %% Actividad educativa creada por un profesional o plantilla oficial. Define área de habilidad,
    %% nivel de complejidad, template y configuración de accesibilidad (AAC, audio).
    Actividad {
        int         Id                          PK  "NOT NULL"
        uuid        ProfesionalId               FK  "nullable - null para plantillas globales"
        int         CategoriaId                 FK  "NOT NULL"
        varchar100  ClaveEstandar               UK  "nullable"
        text        Descripcion                     "nullable"
        text        Instrucciones                   "nullable"
        varchar500  UrlRecursos                     "nullable"
        int         AreaHabilidadId             FK  "nullable"
        varchar200  Titulo                          "NOT NULL"
        int         NivelComplejidad                "nullable"
        bool        RequiereSupervision             "NOT NULL"
        int         DuracionEstimadaMinutos         "nullable"
        bool        EsActividadEstandar             "NOT NULL"
        bool        EsPlantilla                     "NOT NULL"
        int         OrdenRoadmap                    "nullable"
        bool        TieneSoporteVisual              "NOT NULL"
        bool        TieneSoporteAudio               "NOT NULL"
        bool        UsaLecturaFacil                 "NOT NULL"
        bool        UsaPictogramas                  "NOT NULL"
        bool        Activo                          "NOT NULL"
    }

    %% Contenido dinámico de la actividad almacenado como JSON. La estructura
    %% varía según el TipoPlantillaActividad (opciones de selección, pares imagen-palabra, etc.).
    ContenidoActividad {
        int         Id                  PK  "NOT NULL"
        int         ActividadId         FK  "NOT NULL - UK (1:1)"
        int         TipoPlantillaId     FK  "NOT NULL"
        jsonb       ContenidoJson           "NOT NULL"
        bool        Activo                  "NOT NULL"
    }

    %% Vector semántico de la actividad para búsqueda por similaridad (pgvector).
    %% Se genera al crear o editar la actividad si el módulo semántico está activo.
    EmbeddingActividad {
        int         ActividadId         PK  "NOT NULL - FK 1:1"
        varchar100  Modelo                  "NOT NULL"
        int         Dimensiones             "NOT NULL"
        text        EmbeddingJson           "NOT NULL - vector pgvector(384)"
    }

    %% Vector semántico del perfil del alumno para recomendaciones y similitud (pgvector).
    EmbeddingPersona {
        uuid        PersonaId           PK  "NOT NULL - FK 1:1"
        varchar100  Modelo                  "NOT NULL"
        int         Dimensiones             "NOT NULL"
    }

    %% ─── ROADMAP (PLAN PERSONALIZADO) ────────────────────────────────────────

    %% Plan de aprendizaje personalizado de una persona. Cada persona tiene
    %% como máximo un roadmap por persona (índice único), organizado por áreas.
    RoadmapPersona {
        int         Id                          PK  "NOT NULL"
        uuid        PersonaId                   FK  "NOT NULL - UK (máximo 1 por persona)"
        uuid        CreadoPorProfesionalId      FK  "NOT NULL"
        bool        Activo                          "NOT NULL"
    }

    %% Sección del roadmap correspondiente a un área de habilidad. Agrupa
    %% las actividades que la persona debe completar en esa área.
    AreaRoadmapPersona {
        int         Id                  PK  "NOT NULL"
        int         RoadmapPersonaId    FK  "NOT NULL"
        int         AreaHabilidadId     FK  "NOT NULL"
        int         OrdenVisualizacion      "NOT NULL"
        bool        Activo                  "NOT NULL"
    }

    %% Actividad dentro del roadmap con configuración propia de dificultad,
    %% umbral de desbloqueo y límites de tiempo e intentos.
    ActividadRoadmapPersona {
        int         Id                          PK  "NOT NULL"
        int         AreaRoadmapPersonaId        FK  "NOT NULL"
        int         ActividadId                 FK  "NOT NULL"
        int         OrdenSecuencial                 "NOT NULL"
        bool        EstaDesbloqueada                "NOT NULL"
        int         UmbralDesbloqueoPorcentaje      "NOT NULL - 0 a 100"
        int         NivelDificultad                 "NOT NULL - 1 a 5"
        bool        MostrarPistas                   "NOT NULL"
        int         TiempoLimiteSegundos            "nullable"
        int         MaximoIntentos                  "nullable"
        timestamptz DesbloqueadaEn                  "nullable"
        bool        Activo                          "NOT NULL"
    }

    %% ─── ASIGNACIONES, EJECUCIÓN Y RESPUESTAS ────────────────────────────────

    %% Asignación directa de una actividad a una persona por parte del profesional.
    %% Independiente del roadmap; permite asignar actividades puntuales o de evaluación.
    AsignacionActividad {
        int         Id                          PK  "NOT NULL"
        int         ActividadId                 FK  "NOT NULL"
        uuid        PersonaId                   FK  "NOT NULL"
        uuid        AsignadoPorProfesionalId    FK  "NOT NULL"
        int         EstadoId                    FK  "NOT NULL - EstadoAsignacionActividad"
        int         OrdenSecuencial                 "nullable"
        bool        EsActividadEvaluacion           "NOT NULL"
        timestamptz AsignadoEn                      "NOT NULL"
        timestamptz FechaVencimiento                "nullable"
        bool        Activo                          "NOT NULL"
        timestamptz AlertaAtendidaEn                "nullable"
        int         DuracionEstimadaMinutos         "nullable"
        bool        TieneSoporteVisual              "nullable - adaptacion alumno"
        bool        TieneSoporteAudio               "nullable - adaptacion alumno"
        bool        UsaLecturaFacil                 "nullable - adaptacion alumno"
        bool        UsaPictogramas                  "nullable - adaptacion alumno"
        bool        RequiereSupervision             "nullable - adaptacion alumno"
        varchar1000 NotasAdaptacionPersonalizada    "nullable"
    }

    %% Trabajos en segundo plano procesados asíncronamente (agentes MAF / tareas diferidas).
    TrabajoSegundoPlano {
        int         Id                  PK  "NOT NULL"
        int         TipoTrabajoId       FK  "NOT NULL"
        int         EstadoId            FK  "NOT NULL"
        jsonb       PayloadJson             "NOT NULL"
        int         CantidadReintentos      "NOT NULL"
        int         MaximoReintentos        "NOT NULL"
        text        MensajeError            "nullable"
        timestamptz ProgramadoEn            "nullable"
        timestamptz CompletadoEn            "nullable"
    }

    %% Resultado de una ejecución de actividad asignada. Almacena éxito, porcentaje,
    %% intentos y nivel de frustración. Datos clínicos cifrados con AES-256-GCM.
    RespuestaActividad {
        int         Id                  PK  "NOT NULL"
        int         AsignacionId        FK  "NOT NULL"
        varchar20   Resultado               "nullable - cifrado: Exito/Parcial/Fallido"
        numeric5_2  PorcentajeExito         "nullable - 0.00 a 100.00"
        int         CantidadIntentos        "NOT NULL"
        bool        RequirioSoporte         "NOT NULL"
        int         NivelFrustracion        "nullable"
        timestamptz IniciadoEn              "NOT NULL"
        timestamptz CompletadoEn            "nullable"
        bool        Activo                  "NOT NULL"
    }

    %% Resultado detallado de un intento sobre una actividad del roadmap.
    %% Alimenta el radar chart y es el input principal del motor adaptativo.
    ResultadoActividad {
        int         Id                          PK  "NOT NULL"
        int         ActividadRoadmapPersonaId   FK  "NOT NULL"
        int         NumeroIntento                   "NOT NULL"
        float4      PuntajePorcentaje               "NOT NULL - 0.0 a 1.0"
        int         TiempoEmpleadoSegundos          "NOT NULL"
        timestamptz CompletadoEn                    "NOT NULL"
    }

    %% Sesión y métricas analíticas de una actividad finalizada por un alumno.
    %% Alimenta dashboards de KPIs pedagógicos (GAS, tasa de éxito, tiempos y errores).
    SesionActividad {
        int         Id                      PK  "NOT NULL"
        uuid        AlumnoId                FK  "NOT NULL"
        uuid        ProfesionalId           FK  "NOT NULL"
        int         ActividadId             FK  "NOT NULL"
        timestamptz FechaCompletado             "NOT NULL"
        numeric5_2  TasaExito                   "NOT NULL - 0.00 a 100.00"
        int         CantidadErrores             "NOT NULL"
        int         TiempoEmpleadoSegundos      "NOT NULL"
        int         PuntajeGAS                  "NOT NULL - [-2 a +2]"
    }

    %% ─── MOTOR DE DIFICULTAD ADAPTATIVA (MDA) ────────────────────────────────

    %% Configuración del motor de dificultad adaptativa para una actividad del roadmap.
    %% Define rangos y umbrales para ajustar la dificultad automáticamente.
    ConfiguracionMotorAdaptativo {
        int         Id                              PK  "NOT NULL"
        int         ActividadRoadmapPersonaId       FK  "NOT NULL - UK (1:1)"
        bool        EstaHabilitado                      "NOT NULL"
        int         NivelDificultadMinimo               "NOT NULL"
        int         NivelDificultadMaximo               "NOT NULL"
        int         AciertosConsecutivosParaSubir       "NOT NULL"
        int         FracasosConsecutivosParaBajar       "NOT NULL"
        int         UmbralExitoPorcentaje               "NOT NULL - 0 a 100"
        int         UmbralFrustracion                   "NOT NULL - 0 a 5"
        bool        Activo                              "NOT NULL"
    }

    %% Registro de cada ajuste realizado por el motor adaptativo. Permite trazar
    %% el historial de cambios de dificultad para auditoría y visualización.
    RegistroAjusteAdaptativo {
        int         Id                          PK  "NOT NULL"
        int         ActividadRoadmapPersonaId   FK  "NOT NULL"
        int         RespuestaActividadId        FK  "NOT NULL"
        varchar50   TipoAjuste                      "NOT NULL"
        text        ValorAnterior                   "NOT NULL"
        text        ValorNuevo                      "NOT NULL"
        text        Motivo                          "NOT NULL"
        timestamptz AjustadoEn                      "NOT NULL"
        bool        Activo                          "NOT NULL"
    }

    %% ─── CLÍNICO Y EVALUACIONES ──────────────────────────────────────────────

    %% Diagnóstico funcional registrado por el profesional. El texto clínico
    %% se cifra automáticamente con AES-256-GCM vía la annotation [Encrypted].
    Diagnostico {
        int         Id                      PK  "NOT NULL"
        uuid        PersonaId               FK  "NOT NULL"
        uuid        ProfesionalId           FK  "NOT NULL"
        date        FechaDiagnostico            "NOT NULL"
        text        DiagnosticoPrincipal        "NOT NULL - cifrado AES-256-GCM"
        bool        Activo                      "NOT NULL"
    }

    %% Reporte de progreso con flujo de aprobación (Draft → Submitted → Approved/Rejected).
    %% El familiar recibe email al aprobarse; el profesional al rechazarse.
    Reporte {
        int         Id                      PK  "NOT NULL"
        uuid        PersonaId               FK  "NOT NULL"
        uuid        ProfesionalId           FK  "NOT NULL"
        int         TipoReporteId           FK  "NOT NULL"
        varchar200  Titulo                      "NOT NULL"
        varchar20   Estado                      "NOT NULL - Borrador/Enviado/Aprobado/Rechazado"
        date        FechaReporte                "NOT NULL"
        date        FechaInicioPeriodo          "nullable"
        date        FechaFinPeriodo             "nullable"
        uuid        AprobadoPorUsuarioId    FK  "nullable"
        bool        Activo                      "NOT NULL"
    }

    %% ─── COMUNICACIÓN ────────────────────────────────────────────────────────

    %% Mensaje interno entre usuarios del sistema. Soporta hilos mediante
    %% MensajePadreId y puede estar relacionado a una persona como contexto.
    Mensaje {
        int         Id                      PK  "NOT NULL"
        uuid        RemitenteId             FK  "NOT NULL"
        uuid        DestinatarioId          FK  "NOT NULL"
        uuid        PersonaRelacionadaId    FK  "nullable"
        int         MensajePadreId          FK  "nullable - hilo"
        varchar200  Asunto                      "nullable"
        bool        EstaLeido                   "NOT NULL"
        timestamptz EnviadoEn                   "NOT NULL"
        bool        Activo                      "NOT NULL"
    }

    %% ─── AGENDA Y CALENDARIO ─────────────────────────────────────────────────

    %% Eventos de calendario y agenda vinculados a profesionales y alumnos.
    EventoCalendario {
        uuid        Id                          PK  "NOT NULL"
        varchar150  Titulo                          "NOT NULL"
        varchar50   Tipo                            "NOT NULL"
        timestamptz Fecha                           "NOT NULL"
        varchar10   Hora                            "NOT NULL"
        text        Descripcion                     "nullable"
        uuid        AlumnoId                    FK  "nullable"
        varchar100  NombreAlumno                    "nullable"
        uuid        CreadoPorProfesionalId      FK  "NOT NULL"
        varchar20   AlcanceDestinatarios            "NOT NULL"
        bool        Activo                          "NOT NULL"
        timestamptz CreadoEn                        "NOT NULL"
    }

    %% ─── AUDITORÍA DE SEGURIDAD ──────────────────────────────────────────────

    %% Registro de auditoría de acceso a recursos (IN-172). Detecta accesos
    %% indebidos y permite trazar quién accedió a datos de qué persona y cuándo.
    AuditoriaAcceso {
        int         Id                  PK  "NOT NULL"
        uuid        UsuarioId           FK  "NOT NULL"
        uuid        PersonaAccedidaId   FK  "nullable"
        varchar50   Rol                     "nullable"
        varchar50   TipoAccion              "NOT NULL"
        varchar20   Resultado               "NOT NULL - Permitido/Denegado"
        varchar100  TablaAfectada           "nullable"
        timestamptz Timestamp               "NOT NULL"
    }


    %% ═══ RELACIONES ENTRE ENTIDADES ══════════════════════════════════════════

    %% Autenticación
    Usuario ||--o{ TokenRenovacion             : "tokens"
    Usuario ||--o{ DispositivoConfianza         : "dispositivos"
    Usuario ||--o{ AdminInstitucion             : "admin de"
    Usuario ||--o{ TokenRecuperacionContrasena : "solicita reset"

    Usuario ||--o| Profesional                 : "perfil"
    Usuario ||--o| PersonaConDiscapacidad       : "perfil"
    Usuario ||--o| RepresentanteFamiliar        : "perfil"

    Usuario ||--o{ PersonaConDiscapacidad       : "supervisa login"

    %% Catálogos → PersonaConDiscapacidad
    TipoDiscapacidad ||--o{ PersonaConDiscapacidad : "tipo discapacidad"
    NivelAutonomia   ||--o{ PersonaConDiscapacidad : "nivel autonomia"
    MetodoLogin      ||--o{ PersonaConDiscapacidad : "metodo auth"

    %% Catálogos → TipoPlantillaActividad
    AreaHabilidad ||--o{ TipoPlantillaActividad : "agrupa plantillas"

    %% Instituciones y Aulas
    InstitucionEducativa ||--o{ AdminInstitucion        : "administradores"
    InstitucionEducativa ||--o{ ProfesionalInstitucion  : "profesionales"
    RolInstitucional     ||--o{ AdminInstitucion        : "cargo directivo escolar"
    Profesional          ||--o{ ProfesionalInstitucion  : "trabaja en"
    Profesional          ||--o{ Aula                    : "gestiona"
    Aula                 ||--o{ ProfesionalPersona      : "alumnos"

    %% Historiales de estado
    Profesional           ||--o{ HistorialEstadoProfesional : "historial estado"
    RepresentanteFamiliar ||--o{ HistorialEstadoFamiliar    : "historial estado"

    %% Relaciones persona
    Profesional            ||--o{ ProfesionalPersona : "atiende"
    PersonaConDiscapacidad ||--o{ ProfesionalPersona : "atendida por"

    PersonaConDiscapacidad ||--o{ PersonaRepresentante          : "representada por"
    RepresentanteFamiliar  ||--o{ PersonaRepresentante          : "representa a"
    PersonaRepresentante   ||--o{ HistorialPersonaRepresentante : "historial"

    PersonaConDiscapacidad ||--o{ PerfilHabilidadesPersona : "perfil habilidades"
    AreaHabilidad          ||--o{ PerfilHabilidadesPersona : "asignada a"
    PersonaConDiscapacidad ||--o| EmbeddingPersona         : "embedding (1:1)"

    %% Invitaciones
    Profesional            ||--o{ Invitacion : "crea"
    PersonaConDiscapacidad ||--o{ Invitacion : "destino"
    Usuario                ||--o{ Invitacion : "usada por"

    %% Actividades
    Profesional            ||--o{ Actividad          : "crea"
    CategoriaActividad     ||--o{ Actividad          : "clasifica"
    AreaHabilidad          ||--o{ Actividad          : "area"
    Actividad              ||--o| ContenidoActividad : "contenido (1:1)"
    Actividad              ||--o| EmbeddingActividad : "embedding (1:1)"
    TipoPlantillaActividad ||--o{ ContenidoActividad : "define estructura"

    %% Roadmap
    PersonaConDiscapacidad  ||--o{ RoadmapPersona          : "planes (UK por persona)"
    Profesional             ||--o{ RoadmapPersona          : "crea"
    RoadmapPersona          ||--o{ AreaRoadmapPersona      : "areas"
    AreaHabilidad           ||--o{ AreaRoadmapPersona      : "define area"
    AreaRoadmapPersona      ||--o{ ActividadRoadmapPersona : "actividades"
    Actividad               ||--o{ ActividadRoadmapPersona : "incluida en"

    %% Asignaciones y Sesiones Analíticas
    Actividad                 ||--o{ AsignacionActividad : "asignada"
    PersonaConDiscapacidad    ||--o{ AsignacionActividad : "recibe"
    Profesional               ||--o{ AsignacionActividad : "asigna"
    AsignacionActividad       ||--o{ RespuestaActividad  : "respuestas"
    EstadoAsignacionActividad ||--o{ AsignacionActividad : "estado"

    PersonaConDiscapacidad ||--o{ SesionActividad : "ejecuta"
    Profesional            ||--o{ SesionActividad : "supervisa"
    Actividad              ||--o{ SesionActividad : "sesion"

    %% Resultados roadmap
    ActividadRoadmapPersona ||--o{ ResultadoActividad : "resultados"

    %% MDA
    ActividadRoadmapPersona ||--o| ConfiguracionMotorAdaptativo : "config MDA (1:1)"
    ActividadRoadmapPersona ||--o{ RegistroAjusteAdaptativo      : "ajustes"
    RespuestaActividad      ||--o{ RegistroAjusteAdaptativo      : "dispara"

    %% Clínico
    PersonaConDiscapacidad ||--o{ Diagnostico : "evaluada"
    Profesional            ||--o{ Diagnostico : "registra"
    PersonaConDiscapacidad ||--o{ Reporte     : "reportada"
    Profesional            ||--o{ Reporte     : "genera"
    TipoReporte            ||--o{ Reporte     : "tipo"

    %% Agenda y Calendario
    Profesional            ||--o{ EventoCalendario : "organiza"
    PersonaConDiscapacidad ||--o{ EventoCalendario : "participa"

    %% Comunicación
    Usuario                ||--o{ Mensaje : "envia"
    Usuario                ||--o{ Mensaje : "recibe"
    PersonaConDiscapacidad ||--o{ Mensaje : "tema"
    Mensaje                ||--o{ Mensaje : "hilo"

    %% Auditoría
    Usuario                ||--o{ AuditoriaAcceso : "genera"
    PersonaConDiscapacidad ||--o{ AuditoriaAcceso : "accedida en"

    %% Background Jobs y Especialidades
    TipoTrabajoSegundoPlano   ||--o{ TrabajoSegundoPlano : "tipo"
    EstadoTrabajoSegundoPlano ||--o{ TrabajoSegundoPlano : "estado"
    Especialidad              ||--o{ Profesional         : "especialidad catalogada"
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

### Persistencia de enumeraciones

| Propiedad en C# / Español | Persistencia |
|---|---|
| `Profesional.Estado` (`Professional.Status`) e historiales | `integer` mediante `HasConversion<int>()` |
| `RepresentanteFamiliar.Estado` y `HistorialPersonaRepresentante.TipoCambio` | `integer` mediante `HasConversion<int>()` |
| `Reporte.Estado` (`Report.Status`) y `RespuestaActividad.Resultado` | `varchar` mediante `HasConversion<string>()` |
| `AsignacionActividad.EstadoId` (`ActivityAssignment.StatusId`) | FK `integer` a `EstadoAsignacionActividad`; no es un enum persistido |

### Semántica vigente de actividades y roadmap

- `ProfesionalId` de `Actividad` es nullable: las plantillas globales (`EsPlantilla`) y actividades estándar pueden no tener creador profesional.
- `ClaveEstandar` (`StandardKey`) es nullable, pero cuando existe es único; `OrdenRoadmap` y `EsPlantilla` identifican y ordenan las plantillas oficiales.
- `RoadmapPersona.PersonaId` tiene índice único: puede existir como máximo un roadmap por persona. La creación automática del roadmap estándar ocurre después de una asignación efectiva a un profesional.
- Los overrides de `AsignacionActividad` son nullable para conservar la configuración de la actividad cuando no hay adaptación específica.

---

## DbSets por nivel (AppDbContext) y Correspondencia

| Nivel | Entidades en Español (DER) | DbSets C# Originales | Total |
|-------|----------------------------|----------------------|:-----:|
| 1 — Catálogos | TipoDiscapacidad, CategoriaActividad, TipoReporte, InstitucionEducativa, NivelAutonomia, MetodoLogin, AreaHabilidad, TipoPlantillaActividad, EstadoTrabajoSegundoPlano, TipoTrabajoSegundoPlano, Especialidad, RolInstitucional | DisabilityType, ActivityCategory, ReportType, EducationalInstitution, AutonomyLevel, LoginMethod, SkillArea, ActivityTemplateType, BackgroundJobStatus, JobType, Specialty, InstitutionalRole | 12 |
| 2 — Auth/Perfiles | TokenRenovacion, TokenRecuperacionContrasena, Profesional, PersonaConDiscapacidad, RepresentanteFamiliar, Invitacion | RefreshToken, PasswordResetToken, Professional, PersonWithDisability, FamilyRepresentative, Invitation | 6 |
| 3 — Relaciones | AdminInstitucion, DispositivoConfianza, Aula, ProfesionalInstitucion, ProfesionalPersona, PersonaRepresentante, PerfilHabilidadesPersona, Diagnostico, Actividad, ContenidoActividad, RoadmapPersona, AreaRoadmapPersona, ActividadRoadmapPersona | AdminInstitution, TrustedDevice, Classroom, ProfessionalInstitution, ProfessionalPerson, PersonRepresentative, PersonSkillProfile, Diagnosis, Activity, ActivityContent, PersonRoadmap, PersonRoadmapArea, PersonRoadmapActivity | 13 |
| 4 — Ejecución/Mensajería | AsignacionActividad, EstadoAsignacionActividad, Reporte, Mensaje, AuditoriaAcceso, EventoCalendario, HistorialEstadoProfesional, HistorialEstadoFamiliar, HistorialPersonaRepresentante | ActivityAssignment, ActivityAssignmentStatus, Report, Message, AccessAudit, CalendarEvent, ProfessionalStatusHistory, FamilyStatusHistory, PersonRepresentativeHistory | 9 |
| 5 — Respuestas/Embeddings | RespuestaActividad, ResultadoActividad, SesionActividad, EmbeddingActividad, EmbeddingPersona | ActivityResponse, ActivityResult, ActivitySession, ActivityEmbedding, PersonEmbedding | 5 |
| 6 — MDA | ConfiguracionMotorAdaptativo, RegistroAjusteAdaptativo | AdaptiveEngineConfig, AdaptiveAdjustmentLog | 2 |
| 7 — Background Jobs | TrabajoSegundoPlano | BackgroundJob | 1 |

**Total:** 48 `DbSet` explícitos en `AppDbContext` (más la entidad `Usuario` / `User` administrada por `IdentityDbContext`).
