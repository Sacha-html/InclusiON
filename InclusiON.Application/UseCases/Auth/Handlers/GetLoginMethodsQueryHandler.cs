using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using InclusiON.Application.Interfaces.Common;
using InclusiON.Application.Interfaces.Repositories;
using InclusiON.Application.UseCases.Auth.Queries;
using InclusiON.DTOs.Common;
using InclusiON.DTOs.Responses;
using InclusiON.DTOs.Responses.Auth;

namespace InclusiON.Application.UseCases.Auth.Handlers
{
    /// <summary>
    /// Handler para obtener los metodos de login disponibles.
    /// Implementa cache en memoria para evitar consultas repetidas a la BD.
    /// </summary>
    public class GetLoginMethodsQueryHandler : IQueryHandler<GetLoginMethodsQuery, ApiResponse<List<LoginMethodResponse>>>
    {
        private readonly IVisualLoginRepository _repository;
        private readonly IMemoryCache _cache;
        private readonly ILogger<GetLoginMethodsQueryHandler> _logger;

        private const string CacheKey = "LoginMethods_Active";
        private static readonly TimeSpan CacheDuration = TimeSpan.FromHours(1);

        public GetLoginMethodsQueryHandler(
            IVisualLoginRepository repository,
            IMemoryCache cache,
            ILogger<GetLoginMethodsQueryHandler> logger)
        {
            _repository = repository;
            _cache = cache;
            _logger = logger;
        }

        public async Task<ApiResponse<List<LoginMethodResponse>>> HandleAsync(
            GetLoginMethodsQuery query,
            CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();

            try
            {
                // Intentar obtener del cache primero
                if (_cache.TryGetValue(CacheKey, out List<LoginMethodResponse>? cachedResponse) && cachedResponse != null)
                {
                    _logger.LogDebug("LoginMethods obtenidos desde cache");
                    return ApiResponse<List<LoginMethodResponse>>.SuccessResult(
                        cachedResponse,
                        "Metodos de login obtenidos correctamente");
                }

                // Si no está en cache, consultar BD
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

                // Guardar en cache
                var cacheOptions = new MemoryCacheEntryOptions()
                    .SetAbsoluteExpiration(CacheDuration)
                    .SetPriority(CacheItemPriority.High);

                _cache.Set(CacheKey, response, cacheOptions);
                _logger.LogDebug("LoginMethods guardados en cache por {Duration}", CacheDuration);

                return ApiResponse<List<LoginMethodResponse>>.SuccessResult(
                    response,
                    "Metodos de login obtenidos correctamente");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener metodos de login");
                return ApiResponse<List<LoginMethodResponse>>.ErrorResult(
                    ErrorCode.InternalError,
                    "Error interno al obtener metodos de login");
            }
        }
    }
}
