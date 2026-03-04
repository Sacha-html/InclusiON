using Swashbuckle.AspNetCore.Filters;
using InclusiON.DTOs.Requests.Auth;
using InclusiON.DTOs.Requests.Persons;
using InclusiON.DTOs.Requests.Professionals;

namespace InclusiON.Api.Swagger
{
    // ═══════════════════════════════════════════════════════════════
    // AUTH
    // ═══════════════════════════════════════════════════════════════

    public class LoginRequestExample : IMultipleExamplesProvider<LoginRequest>
    {
        public IEnumerable<SwaggerExample<LoginRequest>> GetExamples()
        {
            yield return SwaggerExample.Create("Admin", new LoginRequest
            {
                Email = "admin@inclusion.com",
                Password = "Admin123!",
                RememberMe = false
            });
            yield return SwaggerExample.Create("Profesional (Pedro)", new LoginRequest
            {
                Email = "profesional@test.com",
                Password = "Prof123!",
                RememberMe = false
            });
            yield return SwaggerExample.Create("Profesional (Laura)", new LoginRequest
            {
                Email = "docente@test.com",
                Password = "Doc123!",
                RememberMe = false
            });
            yield return SwaggerExample.Create("Familiar (Rosa)", new LoginRequest
            {
                Email = "familia@test.com",
                Password = "Fam123!",
                RememberMe = false
            });
            yield return SwaggerExample.Create("Familiar (Miguel)", new LoginRequest
            {
                Email = "tutor@test.com",
                Password = "Tutor123!",
                RememberMe = false
            });
        }
    }

    public class RegisterRequestExample : IExamplesProvider<RegisterRequest>
    {
        public RegisterRequest GetExamples() => new()
        {
            Name = "Nuevo",
            Surname = "Usuario",
            Email = "nuevo@inclusion.com",
            Password = "Nuevo123!",
            ConfirmPassword = "Nuevo123!",
            PhoneNumber = "1122334455"
        };
    }

    public class IdentifyUserRequestExample : IMultipleExamplesProvider<IdentifyUserRequest>
    {
        public IEnumerable<SwaggerExample<IdentifyUserRequest>> GetExamples()
        {
            yield return SwaggerExample.Create("Maria (PIN)", new IdentifyUserRequest
            {
                Identifier = "Maria",
                UserType = "Person"
            });
            yield return SwaggerExample.Create("Juan (Standard)", new IdentifyUserRequest
            {
                Identifier = "Juan",
                UserType = "Person"
            });
            yield return SwaggerExample.Create("Ana (Assisted)", new IdentifyUserRequest
            {
                Identifier = "Ana",
                UserType = "Person"
            });
        }
    }

    public class PinLoginRequestExample : IExamplesProvider<PinLoginRequest>
    {
        public PinLoginRequest GetExamples() => new()
        {
            UserId = Guid.Parse("00000000-0000-0000-0000-000000000010"),
            Pin = "1234",
            RememberDevice = false
        };
    }

    public class VisualStandardLoginRequestExample : IExamplesProvider<VisualStandardLoginRequest>
    {
        public VisualStandardLoginRequest GetExamples() => new()
        {
            UserId = Guid.Parse("00000000-0000-0000-0000-000000000011"),
            Password = "Juan123!",
            RememberDevice = false
        };
    }

    public class FamilyLoginRequestExample : IMultipleExamplesProvider<FamilyLoginRequest>
    {
        public IEnumerable<SwaggerExample<FamilyLoginRequest>> GetExamples()
        {
            yield return SwaggerExample.Create("Rosa (Madre)", new FamilyLoginRequest
            {
                UserId = Guid.Parse("00000000-0000-0000-0000-000000000030"),
                Password = "Fam123!",
                RememberDevice = false
            });
            yield return SwaggerExample.Create("Miguel (Tutor)", new FamilyLoginRequest
            {
                UserId = Guid.Parse("00000000-0000-0000-0000-000000000031"),
                Password = "Tutor123!",
                RememberDevice = false
            });
        }
    }

    public class AssistedLoginRequestExample : IExamplesProvider<AssistedLoginRequest>
    {
        public AssistedLoginRequest GetExamples() => new()
        {
            UserId = Guid.Parse("00000000-0000-0000-0000-000000000012"),
            SupervisorEmail = "profesional@test.com",
            SupervisorPassword = "Prof123!"
        };
    }

    public class RefreshTokenRequestExample : IExamplesProvider<RefreshTokenRequest>
    {
        public RefreshTokenRequest GetExamples() => new()
        {
            RefreshToken = "<pegar refresh token obtenido del login>"
        };
    }

    // ═══════════════════════════════════════════════════════════════
    // PERSONS
    // ═══════════════════════════════════════════════════════════════

    public class CreatePersonRequestExample : IExamplesProvider<CreatePersonRequest>
    {
        public CreatePersonRequest GetExamples() => new()
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
    }

    public class UpdatePersonRequestExample : IExamplesProvider<UpdatePersonRequest>
    {
        public UpdatePersonRequest GetExamples() => new()
        {
            FirstName = "Sofia",
            AttentionLevel = 4,
            CommunicationLevel = 3,
            LearningStyle = "Auditivo"
        };
    }

    public class UpdateLoginMethodRequestExample : IExamplesProvider<UpdateLoginMethodRequest>
    {
        public UpdateLoginMethodRequest GetExamples() => new()
        {
            LoginMethodId = 2,
            Pin = "5678"
        };
    }

    // ═══════════════════════════════════════════════════════════════
    // PROFESSIONALS
    // ═══════════════════════════════════════════════════════════════

    public class CreateProfessionalRequestExample : IExamplesProvider<CreateProfessionalRequest>
    {
        public CreateProfessionalRequest GetExamples() => new()
        {
            FirstName = "Carolina",
            LastName = "Mendez",
            DocumentNumber = "30456789",
            Phone = "1199887766",
            Specialty = "Psicopedagogia",
            LicenseNumber = "MP-12345",
            BirthDate = new DateTime(1985, 8, 15),
            Address = "Av. Corrientes 1234, CABA",
            Email = "carolina.mendez@inclusion.com"
        };
    }

    public class UpdateProfessionalRequestExample : IExamplesProvider<UpdateProfessionalRequest>
    {
        public UpdateProfessionalRequest GetExamples() => new()
        {
            Phone = "1188776655",
            Specialty = "Terapia Ocupacional",
            Address = "Av. Santa Fe 5678, CABA"
        };
    }
}
