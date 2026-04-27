using Microsoft.AspNetCore.OpenApi;
using Microsoft.OpenApi;
using System.Diagnostics.CodeAnalysis;
using System.Text.Json;
using System.Text.Json.Nodes;

namespace InclusiON.Api.Scalar;

/// <summary>
/// Inyecta ejemplos de request en la documentación OpenAPI sin tocar los DTOs.
/// Los ejemplos viven en <see cref="RequestExamples"/> y se asignan por controller + action.
/// </summary>
// Excluida de cobertura: es infraestructura de documentación (Scalar/OpenAPI), no lógica de negocio.
// GetExamples es un switch exhaustivo de mapeos controller→ejemplos que no aporta valor testearse.
[ExcludeFromCodeCoverage]
public class OpenApiExamplesTransformer : IOpenApiOperationTransformer
{
    public Task TransformAsync(
        OpenApiOperation operation,
        OpenApiOperationTransformerContext context,
        CancellationToken cancellationToken)
    {
        var controller = context.Description.ActionDescriptor.RouteValues["controller"];
        var action     = context.Description.ActionDescriptor.RouteValues["action"];

        var examples = GetExamples(controller, action);
        if (examples is null) return Task.CompletedTask;

        if (operation.RequestBody?.Content?.TryGetValue("application/json", out var mediaType) != true
            || mediaType is null)
            return Task.CompletedTask;

        mediaType.Examples ??= new Dictionary<string, IOpenApiExample>();

        foreach (var (label, value) in examples)
        {
            mediaType.Examples[label] = new OpenApiExample
            {
                Summary = label,
                Value   = JsonSerializer.SerializeToNode(value)
            };
        }

        return Task.CompletedTask;
    }

    // ─────────────────────────────────────────────────────────────────────
    // Mapeo controller + action → ejemplos
    // Agregar nuevas entradas acá cuando se agreguen endpoints con body.
    // ─────────────────────────────────────────────────────────────────────
    private static Dictionary<string, object>? GetExamples(string? controller, string? action) =>
        (controller, action) switch
        {
            // ── Auth ──────────────────────────────────────────────────────
            ("Auth", "Login") => new()
            {
                ["Admin"]             = RequestExamples.LoginAdmin,
                ["Profesional Pedro"] = RequestExamples.LoginProfesionalPedro,
                ["Profesional Laura"] = RequestExamples.LoginProfesionalLaura,
                ["Familiar Rosa"]     = RequestExamples.LoginFamiliarRosa,
                ["Familiar Miguel"]   = RequestExamples.LoginFamiliarMiguel,
            },
            ("Auth", "Register") => new()
            {
                ["Nuevo usuario"] = RequestExamples.Register,
            },
            ("Auth", "IdentifyUser") => new()
            {
                ["María — PIN"]     = RequestExamples.IdentifyUserMariaPin,
                ["Juan — Standard"] = RequestExamples.IdentifyUserJuanStandard,
                ["Ana — Asistido"]  = RequestExamples.IdentifyUserAnaAssisted,
            },
            ("Auth", "LoginWithPin") => new()
            {
                ["PIN"] = RequestExamples.PinLogin,
            },
            ("Auth", "LoginVisualStandard") => new()
            {
                ["Visual standard"] = RequestExamples.VisualStandardLogin,
            },
            ("Auth", "LoginFamily") => new()
            {
                ["Rosa"]   = RequestExamples.FamilyLoginRosa,
                ["Miguel"] = RequestExamples.FamilyLoginMiguel,
            },
            ("Auth", "LoginAssisted") => new()
            {
                ["Asistido"] = RequestExamples.AssistedLogin,
            },
            ("Auth", "RefreshToken") => new()
            {
                ["Refresh"] = RequestExamples.RefreshToken,
            },
            ("Auth", "ChangePassword") => new()
            {
                ["Cambiar contraseña"] = RequestExamples.ChangePassword,
            },

            // ── AdminInstitutions ─────────────────────────────────────────
            ("AdminInstitutions", "CreateAdminUser") => new()
            {
                ["Luciana Torres"] = RequestExamples.CreateAdminUser,
            },
            ("AdminInstitutions", "AssignInstitution") => new()
            {
                ["Asignar institución"] = RequestExamples.AssignInstitution,
            },

            // ── Assignments ───────────────────────────────────────────────
            ("Assignments", "AssignPerson") => new()
            {
                ["Asignar persona"] = RequestExamples.AssignPerson,
            },
            ("Assignments", "AssignInstitution") => new()
            {
                ["Asignar institución"] = RequestExamples.AssignInstitution,
            },

            // ── CatalogAdmin ──────────────────────────────────────────────
            ("CatalogAdmin", "CreateDisabilityType") => new()
            {
                ["TEA"] = RequestExamples.CreateDisabilityType,
            },
            ("CatalogAdmin", "UpdateDisabilityType") => new()
            {
                ["Actualizar"] = RequestExamples.UpdateDisabilityType,
            },
            ("CatalogAdmin", "CreateAutonomyLevel") => new()
            {
                ["Apoyo Total"] = RequestExamples.CreateAutonomyLevel,
            },
            ("CatalogAdmin", "UpdateAutonomyLevel") => new()
            {
                ["Actualizar"] = RequestExamples.UpdateAutonomyLevel,
            },
            ("CatalogAdmin", "CreateActivityCategory") => new()
            {
                ["Vida Cotidiana"] = RequestExamples.CreateActivityCategory,
            },
            ("CatalogAdmin", "UpdateActivityCategory") => new()
            {
                ["Actualizar"] = RequestExamples.UpdateActivityCategory,
            },
            ("CatalogAdmin", "CreateSkillArea") => new()
            {
                ["Comunicación"] = RequestExamples.CreateSkillArea,
            },
            ("CatalogAdmin", "UpdateSkillArea") => new()
            {
                ["Actualizar"] = RequestExamples.UpdateSkillArea,
            },
            ("CatalogAdmin", "CreateActivityTemplateType") => new()
            {
                ["Asociación de imágenes"] = RequestExamples.CreateActivityTemplateType,
            },
            ("CatalogAdmin", "UpdateActivityTemplateType") => new()
            {
                ["Actualizar"] = RequestExamples.UpdateActivityTemplateType,
            },
            ("CatalogAdmin", "UpdateLoginMethod") => new()
            {
                ["PIN"] = RequestExamples.UpdateLoginMethodCatalog,
            },

            // ── Diagnoses ─────────────────────────────────────────────────
            ("Diagnoses", "CreateDiagnosis") => new()
            {
                ["TEA nivel 2"] = RequestExamples.CreateDiagnosis,
            },
            ("Diagnoses", "UpdateDiagnosis") => new()
            {
                ["Actualizar"] = RequestExamples.UpdateDiagnosis,
            },

            // ── Family ────────────────────────────────────────────────────
            ("Family", "CreateFamily") => new()
            {
                ["Carmen Gomez"] = RequestExamples.CreateFamily,
            },
            ("Family", "UpdateFamily") => new()
            {
                ["Actualizar"] = RequestExamples.UpdateFamily,
            },
            ("Family", "LinkFamilyToPerson") => new()
            {
                ["Vincular"] = RequestExamples.LinkFamilyToPerson,
            },
            ("Family", "UnlinkFamilyFromPerson") => new()
            {
                ["Desvincular"] = RequestExamples.UnlinkFamilyFromPerson,
            },
            ("Family", "LinkFamilyToPersonAsProfessional") => new()
            {
                ["Vincular"] = RequestExamples.LinkFamilyToPerson,
            },
            ("Family", "UnlinkFamilyFromPersonAsProfessional") => new()
            {
                ["Desvincular"] = RequestExamples.UnlinkFamilyFromPerson,
            },

            // ── Institutions ──────────────────────────────────────────────
            ("Institutions", "CreateInstitution") => new()
            {
                ["Escuela N° 12"] = RequestExamples.CreateInstitution,
            },
            ("Institutions", "UpdateInstitution") => new()
            {
                ["Actualizar"] = RequestExamples.UpdateInstitution,
            },

            // ── Invitations ───────────────────────────────────────────────
            ("Invitations", "CreateInvitation") => new()
            {
                ["Invitar familiar"] = RequestExamples.CreateInvitation,
            },
            ("Invitations", "AcceptInvitation") => new()
            {
                ["Aceptar"] = RequestExamples.AcceptInvitation,
            },

            // ── Persons ───────────────────────────────────────────────────
            ("Persons", "CreatePerson") => new()
            {
                ["Sofia Ramirez"] = RequestExamples.CreatePerson,
            },
            ("Persons", "UpdatePerson") => new()
            {
                ["Actualizar"] = RequestExamples.UpdatePerson,
            },
            ("Persons", "UpdateLoginMethod") => new()
            {
                ["Cambiar a PIN"] = RequestExamples.UpdateLoginMethod,
            },
            ("Persons", "UpdateMyLoginMethod") => new()
            {
                ["Cambiar a PIN"] = RequestExamples.UpdateLoginMethod,
            },
            ("Persons", "AddSkillArea") => new()
            {
                ["Área de comunicación"] = RequestExamples.AddSkillArea,
            },

            // ── Professionals ─────────────────────────────────────────────
            ("Professionals", "RegisterProfessional") => new()
            {
                ["Diego Fernandez"] = RequestExamples.RegisterProfessional,
            },
            ("Professionals", "CreateProfessional") => new()
            {
                ["Carolina Mendez"] = RequestExamples.CreateProfessional,
            },
            ("Professionals", "UpdateProfessional") => new()
            {
                ["Actualizar"] = RequestExamples.UpdateProfessional,
            },
            ("Professionals", "DeactivateProfessional") => new()
            {
                ["Desactivar"] = RequestExamples.DeactivateProfessional,
            },
            ("Professionals", "ValidateProfessional") => new()
            {
                ["Aprobar"]  = RequestExamples.ValidateProfessional,
                ["Rechazar"] = new InclusiON.DTOs.Requests.Professionals.ValidateProfessionalRequest
                {
                    IsApproved  = false,
                    Observation = "Matrícula profesional no encontrada en el registro oficial"
                },
            },
            ("Professionals", "ReactivateProfessional") => new()
            {
                ["Reactivar"] = RequestExamples.ReactivateProfessional,
            },

            // ── Reports ───────────────────────────────────────────────────
            ("Reports", "CreateReport") => new()
            {
                ["Informe Q1 2025"] = RequestExamples.CreateReport,
            },

            // ── Roles ─────────────────────────────────────────────────────
            ("Roles", "UpdateRolePermissions") => new()
            {
                ["Profesional"] = RequestExamples.UpdateRolePermissions,
            },

            _ => null
        };
}
