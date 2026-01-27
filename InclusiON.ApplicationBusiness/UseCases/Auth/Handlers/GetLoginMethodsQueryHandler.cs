using Microsoft.Extensions.Logging;
using InclusiON.ApplicationBusiness.Interfaces.Common;
using InclusiON.ApplicationBusiness.Interfaces.Repositories;
using InclusiON.ApplicationBusiness.UseCases.Auth.Queries;
using InclusiON.DTOs.Responses;
using InclusiON.DTOs.Responses.Auth;

namespace InclusiON.ApplicationBusiness.UseCases.Auth.Handlers
{
    /// <summary>
    /// Handler para obtener los metodos de login disponibles.
    /// </summary>
    public class GetLoginMethodsQueryHandler : IQueryHandler<GetLoginMethodsQuery, ApiResponse<List<LoginMethodResponse>>>
    {
        private readonly IVisualLoginRepository _repository;
        private readonly ILogger<GetLoginMethodsQueryHandler> _logger;

        public GetLoginMethodsQueryHandler(
            IVisualLoginRepository repository,
            ILogger<GetLoginMethodsQueryHandler> logger)
        {
            _repository = repository;
            _logger = logger;
        }

        public async Task<ApiResponse<List<LoginMethodResponse>>> HandleAsync(
            GetLoginMethodsQuery query,
            CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();

            try
            {
                var loginMethods = await _repository.GetActiveLoginMethodsAsync(cancellationToken);

                var response = loginMethods.Select(lm => new LoginMethodResponse
                {
                    Id = lm.Id,
                    Code = lm.Code,
                    Name = lm.Name,
                    Description = lm.Description,
                    RequiresPassword = lm.RequiresPassword,
                    RequiresPin = lm.RequiresPin,
                    RequiresSupervisor = lm.RequiresSupervisor,
                    DisplayOrder = lm.DisplayOrder
                }).ToList();

                return ApiResponse<List<LoginMethodResponse>>.SuccessResult(
                    response,
                    "Metodos de login obtenidos correctamente");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener metodos de login");
                return ApiResponse<List<LoginMethodResponse>>.ErrorResult(
                    $"Error al obtener metodos de login: {ex.Message}");
            }
        }
    }
}
