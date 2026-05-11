using Microsoft.Extensions.Caching.Memory;
using InclusiON.Application.Interfaces.Common;
using InclusiON.Application.Interfaces.Infrastructure;
using InclusiON.Application.Interfaces.Repositories.Base;
using InclusiON.Application.UseCases.Catalogs.Queries;
using InclusiON.Domain.Models;
using InclusiON.DTOs.Responses;
using InclusiON.DTOs.Responses.Catalogs;

namespace InclusiON.Application.UseCases.Catalogs.Handlers
{
    public class GetAutonomyLevelsQueryHandler
        : IQueryHandler<GetAutonomyLevelsQuery, ApiResponse<List<AutonomyLevelResponse>>>
    {
        private readonly IReadOnlyRepository<AutonomyLevel> _repository;
        private readonly IMemoryCache _cache;
        private readonly IEncryptionService _encryption;

        private const string CacheKey = "Catalog_AutonomyLevels";
        private static readonly TimeSpan CacheDuration = TimeSpan.FromHours(1);

        public GetAutonomyLevelsQueryHandler(IReadOnlyRepository<AutonomyLevel> repository, IMemoryCache cache, IEncryptionService encryption)
        {
            _repository = repository;
            _cache = cache;
            _encryption = encryption;
        }

        public async Task<ApiResponse<List<AutonomyLevelResponse>>> HandleAsync(
            GetAutonomyLevelsQuery query, CancellationToken cancellationToken)
        {
            if (_cache.TryGetValue(CacheKey, out List<AutonomyLevelResponse>? cached) && cached is not null)
                return ApiResponse<List<AutonomyLevelResponse>>.SuccessResult(cached);

            var items = await _repository.GetAllActiveAsync(cancellationToken);
            var response = items.Select(x =>
            {
                var item = AutonomyLevelResponse.MapToResponse(x);
                item.EncryptedId = ToUrlSafeBase64(_encryption.Encrypt(x.Id.ToString()));
                return item;
            }).ToList();

            _cache.Set(CacheKey, response, new MemoryCacheEntryOptions()
                .SetAbsoluteExpiration(CacheDuration)
                .SetPriority(CacheItemPriority.Normal));

            return ApiResponse<List<AutonomyLevelResponse>>.SuccessResult(response);
        }

        private static string ToUrlSafeBase64(string s) => s.Replace('+', '-').Replace('/', '_').TrimEnd('=');
    }
}
