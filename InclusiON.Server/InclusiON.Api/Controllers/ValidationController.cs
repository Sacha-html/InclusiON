using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using InclusiON.Application.Interfaces.Infrastructure;

namespace InclusiON.Api.Controllers
{
    public sealed class FieldAvailabilityResponse
    {
        public bool IsAvailable { get; set; }
        public string? Message { get; set; }
        public bool IsInactiveStudent { get; set; }
    }

    [Route("api/validation")]
    [ApiController]
    [Authorize]
    [Produces("application/json")]
    public class ValidationController : ControllerBase
    {
        private readonly IUniquenessValidationService _uniquenessValidation;

        public ValidationController(IUniquenessValidationService uniquenessValidation)
        {
            _uniquenessValidation = uniquenessValidation;
        }

        /// <summary>
        /// Verifica unicidad global de DNI entre Alumnos, Familiares y Profesionales.
        /// </summary>
        [HttpGet("document")]
        [ProducesResponseType(typeof(FieldAvailabilityResponse), StatusCodes.Status200OK)]
        public async Task<ActionResult<FieldAvailabilityResponse>> CheckDocument(
            [FromQuery] string? documentNumber,
            [FromQuery] Guid? excludeEntityId = null,
            CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(documentNumber))
            {
                return Ok(new FieldAvailabilityResponse { IsAvailable = true });
            }

            var result = await _uniquenessValidation.CheckDocumentAsync(documentNumber, excludeEntityId, cancellationToken);
            return Ok(new FieldAvailabilityResponse
            {
                IsAvailable = result.IsAvailable,
                Message = result.Message,
                IsInactiveStudent = result.IsInactiveStudent
            });
        }

        /// <summary>
        /// Verifica unicidad global de Email (case-insensitive) entre Usuarios, Profesionales y opcionalmente Invitaciones activas.
        /// </summary>
        [HttpGet("email")]
        [ProducesResponseType(typeof(FieldAvailabilityResponse), StatusCodes.Status200OK)]
        public async Task<ActionResult<FieldAvailabilityResponse>> CheckEmail(
            [FromQuery] string? email,
            [FromQuery] Guid? excludeUserId = null,
            [FromQuery] Guid? excludeEntityId = null,
            [FromQuery] bool checkInvitations = false,
            CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(email))
            {
                return BadRequest(new FieldAvailabilityResponse { IsAvailable = false, Message = "Email requerido" });
            }

            var result = await _uniquenessValidation.CheckEmailAsync(
                email, excludeUserId, excludeEntityId, checkInvitations, cancellationToken);

            return Ok(new FieldAvailabilityResponse
            {
                IsAvailable = result.IsAvailable,
                Message = result.Message
            });
        }

        /// <summary>
        /// Verifica unicidad de Matrícula profesional.
        /// </summary>
        [HttpGet("license-number")]
        [ProducesResponseType(typeof(FieldAvailabilityResponse), StatusCodes.Status200OK)]
        public async Task<ActionResult<FieldAvailabilityResponse>> CheckLicenseNumber(
            [FromQuery] string? licenseNumber,
            [FromQuery] Guid? excludeProfessionalId = null,
            CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(licenseNumber))
            {
                return Ok(new FieldAvailabilityResponse { IsAvailable = true });
            }

            var result = await _uniquenessValidation.CheckLicenseNumberAsync(
                licenseNumber, excludeProfessionalId, cancellationToken);

            return Ok(new FieldAvailabilityResponse
            {
                IsAvailable = result.IsAvailable,
                Message = result.Message
            });
        }
    }
}
