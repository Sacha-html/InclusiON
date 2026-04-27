using InclusiON.DTOs.Requests.Admin;
using InclusiON.DTOs.Requests.Assignments;
using InclusiON.DTOs.Requests.Auth;
using InclusiON.DTOs.Requests.Catalogs;
using InclusiON.DTOs.Requests.Diagnoses;
using InclusiON.DTOs.Requests.Family;
using InclusiON.DTOs.Requests.Institutions;
using InclusiON.DTOs.Requests.Invitations;
using InclusiON.DTOs.Requests.Persons;
using InclusiON.DTOs.Requests.Professionals;
using InclusiON.DTOs.Requests.Reports;
using InclusiON.DTOs.Requests.Roles;

namespace InclusiON.Api.Scalar
{
    /// <summary>
    /// Ejemplos de request para documentación y tests manuales.
    /// Los IDs (Guid/int) corresponden a los datos del DatabaseSeeder.
    /// </summary>
    public static class RequestExamples
    {
        // ═══════════════════════════════════════════════════════════════
        // AUTH
        // ═══════════════════════════════════════════════════════════════

        public static LoginRequest LoginAdmin => new()
        {
            Email = "admin@inclusion.com",
            Password = "Admin123!",
            RememberMe = false
        };

        public static LoginRequest LoginProfesionalPedro => new()
        {
            Email = "profesional@test.com",
            Password = "Prof123!",
            RememberMe = false
        };

        public static LoginRequest LoginProfesionalLaura => new()
        {
            Email = "docente@test.com",
            Password = "Doc123!",
            RememberMe = false
        };

        public static LoginRequest LoginFamiliarRosa => new()
        {
            Email = "familia@test.com",
            Password = "Fam123!",
            RememberMe = false
        };

        public static LoginRequest LoginFamiliarMiguel => new()
        {
            Email = "tutor@test.com",
            Password = "Tutor123!",
            RememberMe = false
        };

        public static RegisterRequest Register => new()
        {
            Name = "Nuevo",
            Surname = "Usuario",
            Email = "nuevo@inclusion.com",
            Password = "Nuevo123!",
            ConfirmPassword = "Nuevo123!",
            PhoneNumber = "1122334455"
        };

        public static IdentifyUserRequest IdentifyUserMariaPin => new()
        {
            Identifier = "Maria",
            UserType = "Person"
        };

        public static IdentifyUserRequest IdentifyUserJuanStandard => new()
        {
            Identifier = "Juan",
            UserType = "Person"
        };

        public static IdentifyUserRequest IdentifyUserAnaAssisted => new()
        {
            Identifier = "Ana",
            UserType = "Person"
        };

        public static PinLoginRequest PinLogin => new()
        {
            UserId = Guid.Parse("00000000-0000-0000-0000-000000000010"),
            Pin = "1234",
            RememberDevice = false
        };

        public static VisualStandardLoginRequest VisualStandardLogin => new()
        {
            UserId = Guid.Parse("00000000-0000-0000-0000-000000000011"),
            Password = "Juan123!",
            RememberDevice = false
        };

        public static FamilyLoginRequest FamilyLoginRosa => new()
        {
            UserId = Guid.Parse("00000000-0000-0000-0000-000000000030"),
            Password = "Fam123!",
            RememberDevice = false
        };

        public static FamilyLoginRequest FamilyLoginMiguel => new()
        {
            UserId = Guid.Parse("00000000-0000-0000-0000-000000000031"),
            Password = "Tutor123!",
            RememberDevice = false
        };

        public static AssistedLoginRequest AssistedLogin => new()
        {
            UserId = Guid.Parse("00000000-0000-0000-0000-000000000012"),
            SupervisorEmail = "profesional@test.com",
            SupervisorPassword = "Prof123!"
        };

        public static RefreshTokenRequest RefreshToken => new()
        {
            RefreshToken = "<pegar refresh token obtenido del login>"
        };

        public static ChangePasswordRequest ChangePassword => new()
        {
            CurrentPassword = "Admin123!",
            NewPassword = "Admin456!",
            ConfirmNewPassword = "Admin456!"
        };

        // ═══════════════════════════════════════════════════════════════
        // ADMIN INSTITUTIONS
        // ═══════════════════════════════════════════════════════════════

        public static CreateAdminUserRequest CreateAdminUser => new()
        {
            FirstName = "Luciana",
            LastName = "Torres",
            Email = "luciana.torres@inclusion.com",
            InstitutionId = 1
        };

        // ═══════════════════════════════════════════════════════════════
        // ASSIGNMENTS
        // ═══════════════════════════════════════════════════════════════

        public static AssignPersonRequest AssignPerson => new()
        {
            PersonId = Guid.Parse("00000000-0000-0000-0000-000000000010"),
            IsPrimaryProfessional = true,
            CanSuperviseLogin = true
        };

        public static AssignInstitutionRequest AssignInstitution => new()
        {
            InstitutionId = 1
        };

        // ═══════════════════════════════════════════════════════════════
        // CATALOG ADMIN
        // ═══════════════════════════════════════════════════════════════

        public static CreateDisabilityTypeRequest CreateDisabilityType => new()
        {
            Name = "Trastorno del Espectro Autista",
            Description = "Condición del neurodesarrollo que afecta la comunicación y conducta"
        };

        public static UpdateDisabilityTypeRequest UpdateDisabilityType => new()
        {
            Name = "Trastorno del Espectro Autista (TEA)",
            Description = "Condición del neurodesarrollo que afecta la comunicación y conducta",
            IsActive = true
        };

        public static CreateAutonomyLevelRequest CreateAutonomyLevel => new()
        {
            Name = "Apoyo Total",
            Description = "Requiere asistencia permanente en todas las actividades",
            RequiresSupervision = true,
            DisplayOrder = 1
        };

        public static UpdateAutonomyLevelRequest UpdateAutonomyLevel => new()
        {
            Name = "Apoyo Total",
            Description = "Requiere asistencia permanente en todas las actividades",
            RequiresSupervision = true,
            DisplayOrder = 1,
            IsActive = true
        };

        public static CreateActivityCategoryRequest CreateActivityCategory => new()
        {
            Name = "Vida Cotidiana",
            Description = "Actividades de autonomía personal y habilidades de la vida diaria"
        };

        public static UpdateActivityCategoryRequest UpdateActivityCategory => new()
        {
            Name = "Vida Cotidiana",
            Description = "Actividades de autonomía personal y habilidades de la vida diaria",
            IsActive = true
        };

        public static CreateSkillAreaRequest CreateSkillArea => new()
        {
            Name = "Comunicación",
            Description = "Habilidades de comunicación verbal y no verbal",
            Icon = "chat-bubble",
            Color = "#2196F3",
            DisplayOrder = 1
        };

        public static UpdateSkillAreaRequest UpdateSkillArea => new()
        {
            Name = "Comunicación",
            Description = "Habilidades de comunicación verbal y no verbal",
            Icon = "chat-bubble",
            Color = "#2196F3",
            DisplayOrder = 1,
            IsActive = true
        };

        public static CreateActivityTemplateTypeRequest CreateActivityTemplateType => new()
        {
            SkillAreaId = 1,
            Name = "Asociación de imágenes",
            Code = "IMG_MATCH",
            Description = "El alumno asocia imágenes con sus conceptos correspondientes",
            ContentSchema = "{\"images\": [], \"pairs\": []}",
            ComponentName = "ImageMatchActivity",
            UsesPictograms = true,
            HasAudio = false,
            DisplayOrder = 1
        };

        public static UpdateActivityTemplateTypeRequest UpdateActivityTemplateType => new()
        {
            SkillAreaId = 1,
            Name = "Asociación de imágenes",
            Code = "IMG_MATCH",
            Description = "El alumno asocia imágenes con sus conceptos correspondientes",
            ContentSchema = "{\"images\": [], \"pairs\": []}",
            ComponentName = "ImageMatchActivity",
            UsesPictograms = true,
            HasAudio = true,
            DisplayOrder = 1,
            IsActive = true
        };

        public static UpdateLoginMethodCatalogRequest UpdateLoginMethodCatalog => new()
        {
            Name = "PIN",
            Description = "Acceso mediante código numérico de 4 dígitos",
            DisplayOrder = 1,
            IsActive = true
        };

        // ═══════════════════════════════════════════════════════════════
        // DIAGNOSES
        // ═══════════════════════════════════════════════════════════════

        public static CreateDiagnosisRequest CreateDiagnosis => new()
        {
            DiagnosisDate = new DateTime(2024, 3, 10),
            PrimaryDiagnosis = "Trastorno del Espectro Autista nivel 2",
            InitialObservations = "Presenta dificultades en la comunicación verbal y conductas repetitivas",
            IdentifiedCapabilities = "Buena memoria visual, interés en actividades estructuradas, habilidad con dispositivos tecnológicos",
            IdentifiedChallenges = "Comunicación verbal limitada, dificultad en transiciones, sensibilidad sensorial",
            RequiredSupports = "Apoyo en comunicación aumentativa, estructura visual, anticipación de cambios",
            PedagogicalObjectives = "Incrementar vocabulario funcional, mejorar tolerancia a cambios de rutina",
            RecommendedStrategies = "Uso de pictogramas, agenda visual, refuerzo positivo"
        };

        public static UpdateDiagnosisRequest UpdateDiagnosis => new()
        {
            DiagnosisDate = new DateTime(2024, 9, 15),
            PrimaryDiagnosis = "Trastorno del Espectro Autista nivel 2",
            InitialObservations = "Progreso significativo en comunicación funcional",
            IdentifiedCapabilities = "Buena memoria visual, comunicación con pictogramas mejorada",
            IdentifiedChallenges = "Aún presenta dificultad en situaciones no estructuradas",
            RequiredSupports = "Continuar con apoyo en CAA",
            PedagogicalObjectives = "Ampliar vocabulario CAA a 100 símbolos",
            RecommendedStrategies = "Continuar agenda visual, incorporar actividades grupales graduales"
        };

        // ═══════════════════════════════════════════════════════════════
        // FAMILY
        // ═══════════════════════════════════════════════════════════════

        public static CreateFamilyRequest CreateFamily => new()
        {
            FirstName = "Carmen",
            LastName = "Gomez",
            Email = "carmen.gomez@gmail.com",
            DocumentNumber = "28456123",
            Phone = "1133445566",
            Relationship = "Madre",
            PersonId = Guid.Parse("00000000-0000-0000-0000-000000000010")
        };

        public static UpdateFamilyRequest UpdateFamily => new()
        {
            FirstName = "Carmen",
            LastName = "Gomez",
            Email = "carmen.gomez@gmail.com",
            DocumentNumber = "28456123",
            Phone = "1144556677",
            Relationship = "Madre"
        };

        public static LinkFamilyToPersonRequest LinkFamilyToPerson => new()
        {
            Relationship = "Padre",
            IsPrimary = true
        };

        public static UnlinkFamilyFromPersonRequest UnlinkFamilyFromPerson => new()
        {
            Observation = "El familiar solicitó desvincularse del seguimiento"
        };

        // ═══════════════════════════════════════════════════════════════
        // INSTITUTIONS
        // ═══════════════════════════════════════════════════════════════

        public static CreateInstitutionRequest CreateInstitution => new()
        {
            Name = "Escuela Especial N° 12",
            Address = "Av. Rivadavia 1500, CABA",
            Phone = "011-4567-8901",
            Email = "contacto@escuela12.edu.ar"
        };

        public static UpdateInstitutionRequest UpdateInstitution => new()
        {
            Name = "Escuela Especial N° 12",
            Address = "Av. Rivadavia 1500, CABA",
            Phone = "011-4567-8902",
            Email = "info@escuela12.edu.ar"
        };

        // ═══════════════════════════════════════════════════════════════
        // INVITATIONS
        // ═══════════════════════════════════════════════════════════════

        public static CreateInvitationRequest CreateInvitation => new()
        {
            PersonId = Guid.Parse("00000000-0000-0000-0000-000000000010"),
            Email = "maria.garcia@gmail.com",
            FirstName = "María",
            LastName = "García",
            Relationship = "Madre"
        };

        public static AcceptInvitationRequest AcceptInvitation => new()
        {
            Email = "maria.garcia@gmail.com",
            Password = "Maria123!",
            ConfirmPassword = "Maria123!"
        };

        // ═══════════════════════════════════════════════════════════════
        // PERSONS
        // ═══════════════════════════════════════════════════════════════

        public static CreatePersonRequest CreatePerson => new()
        {
            FirstName = "Sofia",
            LastName = "Ramirez",
            DocumentNumber = "40123456",
            BirthDate = new DateTime(2015, 5, 20),
            DisabilityTypeId = 1,
            AttentionLevel = 3,
            CommunicationLevel = 2,
            UsesAAC = true,
            UsesSignLanguage = false,
            MotorSkillLevel = 4,
            InterestsAndMotivators = "Musica, colores, animales",
            LearningStyle = "Visual",
            RequiresLargeFont = false,
            RequiresHighContrast = false,
            VisualNoiseSensitivity = true,
            SoundSensitivity = false,
            AutonomyLevelId = 2,
            LoginMethodId = 2,
            Pin = "1234",
            AvatarColor = "#4CAF50"
        };

        public static UpdatePersonRequest UpdatePerson => new()
        {
            FirstName = "Sofia",
            AttentionLevel = 4,
            CommunicationLevel = 3,
            LearningStyle = "Auditivo"
        };

        public static UpdateLoginMethodRequest UpdateLoginMethod => new()
        {
            LoginMethodId = 2,
            Pin = "5678"
        };

        public static AddSkillAreaRequest AddSkillArea => new()
        {
            SkillAreaId = 1
        };

        // ═══════════════════════════════════════════════════════════════
        // PROFESSIONALS
        // ═══════════════════════════════════════════════════════════════

        public static CreateProfessionalRequest CreateProfessional => new()
        {
            FirstName = "Carolina",
            LastName = "Mendez",
            DocumentNumber = "30456789",
            Phone = "1199887766",
            Specialty = "Psicopedagogia",
            LicenseNumber = "MP-12345",
            BirthDate = new DateTime(1985, 8, 15),
            Email = "carolina.mendez@inclusion.com"
        };

        public static UpdateProfessionalRequest UpdateProfessional => new()
        {
            Phone = "1188776655",
            Specialty = "Terapia Ocupacional"
        };

        public static RegisterProfessionalRequest RegisterProfessional => new()
        {
            FirstName = "Diego",
            LastName = "Fernandez",
            DocumentNumber = "32567890",
            Phone = "1155443322",
            Specialty = "Fonoaudiologia",
            LicenseNumber = "MN-67890",
            BirthDate = new DateTime(1990, 3, 22),
            Email = "diego.fernandez@gmail.com",
            InstitutionId = 1
        };

        public static DeactivateProfessionalRequest DeactivateProfessional => new()
        {
            Observation = "Licencia por motivos personales"
        };

        public static ValidateProfessionalRequest ValidateProfessional => new()
        {
            IsApproved = true,
            Observation = "Documentación verificada correctamente"
        };

        public static ReactivateProfessionalRequest ReactivateProfessional => new()
        {
            Observation = "Retoma actividades tras licencia"
        };

        // ═══════════════════════════════════════════════════════════════
        // REPORTS
        // ═══════════════════════════════════════════════════════════════

        public static CreateReportRequest CreateReport => new()
        {
            PersonId = Guid.Parse("00000000-0000-0000-0000-000000000010"),
            Title = "Informe de Progreso — Primer Trimestre 2025",
            Content = "Durante el período se observó avance significativo en comunicación funcional y participación en actividades grupales.",
            ReportTypeId = 1,
            ReportDate = new DateTime(2025, 3, 31),
            PeriodStartDate = new DateTime(2025, 1, 1),
            PeriodEndDate = new DateTime(2025, 3, 31),
            AchievedGoals = "Amplió vocabulario CAA a 80 símbolos. Mejoró tolerancia en actividades de 20 a 35 minutos.",
            AreasToReinforce = "Transiciones entre actividades. Interacción con pares.",
            FutureRecommendations = "Incorporar actividades grupales 2 veces por semana.",
            NextObjectives = "Alcanzar 100 símbolos CAA. Iniciar proyecto de integración parcial."
        };

        // ═══════════════════════════════════════════════════════════════
        // ROLES
        // ═══════════════════════════════════════════════════════════════

        public static UpdateRolePermissionsRequest UpdateRolePermissions => new()
        {
            Permissions = new List<string>
            {
                "persons.read",
                "persons.create",
                "persons.update",
                "professionals.read",
                "reports.read",
                "reports.create",
                "diagnoses.read",
                "diagnoses.create"
            }
        };
    }
}
