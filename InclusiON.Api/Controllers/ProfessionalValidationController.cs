using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using InclusiON.Application.Interfaces.Repositories;
using InclusiON.Application.Interfaces.Infrastructure;
using InclusiON.DTOs.Responses.Professionals;

namespace InclusiON.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]   // Solo usuarios autenticados (admin/profesional) pueden verificar unicidad.
    public class ProfessionalValidationController : ControllerBase
    {
        private readonly IProfessionalsRepository _repository;
        private readonly IIdentityService _identityService;

        public ProfessionalValidationController(
            IProfessionalsRepository repository,
            IIdentityService identityService)
        {
            _repository = repository;
            _identityService = identityService;
        }

        [HttpGet("email")]
        [ProducesResponseType(typeof(ProfessionalValidationResponse), StatusCodes.Status200OK)]
        public async Task<ActionResult<ProfessionalValidationResponse>> CheckEmail([FromQuery] string email)
        {
            if (string.IsNullOrWhiteSpace(email))
            {
                return BadRequest(new ProfessionalValidationResponse { IsAvailable = false, Message = "Email requerido" });
            }

            var existsInProfessionals = await _repository.ExistsProfessionalEmailAsync(email);
            var existsInUsers = await _identityService.FindByEmailAsync(email) != null;

            if (existsInProfessionals || existsInUsers)
            {
                return Ok(new ProfessionalValidationResponse { IsAvailable = false, Message = "Este email ya está registrado" });
            }

            return Ok(new ProfessionalValidationResponse { IsAvailable = true });
        }

        [HttpGet("license-number")]
        [ProducesResponseType(typeof(ProfessionalValidationResponse), StatusCodes.Status200OK)]
        public async Task<ActionResult<ProfessionalValidationResponse>> CheckLicenseNumber([FromQuery] string licenseNumber)
        {
            if (string.IsNullOrWhiteSpace(licenseNumber))
            {
                return Ok(new ProfessionalValidationResponse { IsAvailable = true });
            }

            var exists = await _repository.ExistsLicenseNumberAsync(licenseNumber);

            if (exists)
            {
                return Ok(new ProfessionalValidationResponse { IsAvailable = false, Message = "Esta matrícula ya está registrada" });
            }

            return Ok(new ProfessionalValidationResponse { IsAvailable = true });
        }
    }
}
