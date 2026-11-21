using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using InclusiON.Application.Interfaces.Infrastructure;
using InclusiON.DTOs.Responses.Professionals;

namespace InclusiON.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]   // Solo usuarios autenticados (admin/profesional) pueden verificar unicidad.
    public class ProfessionalValidationController : ControllerBase
    {
        private readonly IUniquenessValidationService _uniquenessValidation;

        public ProfessionalValidationController(IUniquenessValidationService uniquenessValidation)
        {
            _uniquenessValidation = uniquenessValidation;
        }

        [HttpGet("email")]
        [ProducesResponseType(typeof(ProfessionalValidationResponse), StatusCodes.Status200OK)]
        public async Task<ActionResult<ProfessionalValidationResponse>> CheckEmail(
            [FromQuery] string email,
            [FromQuery] Guid? excludeUserId = null,
            [FromQuery] Guid? excludeEntityId = null,
            CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(email))
            {
                return BadRequest(new ProfessionalValidationResponse { IsAvailable = false, Message = "Email requerido" });
            }

            var result = await _uniquenessValidation.CheckEmailAsync(email, excludeUserId, excludeEntityId, false, cancellationToken);
            return Ok(new ProfessionalValidationResponse { IsAvailable = result.IsAvailable, Message = result.Message });
        }

        [HttpGet("license-number")]
        [ProducesResponseType(typeof(ProfessionalValidationResponse), StatusCodes.Status200OK)]
        public async Task<ActionResult<ProfessionalValidationResponse>> CheckLicenseNumber(
            [FromQuery] string licenseNumber,
            [FromQuery] Guid? excludeProfessionalId = null,
            CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(licenseNumber))
            {
                return Ok(new ProfessionalValidationResponse { IsAvailable = true });
            }

            var result = await _uniquenessValidation.CheckLicenseNumberAsync(licenseNumber, excludeProfessionalId, cancellationToken);
            return Ok(new ProfessionalValidationResponse { IsAvailable = result.IsAvailable, Message = result.Message });
        }
    }
}
