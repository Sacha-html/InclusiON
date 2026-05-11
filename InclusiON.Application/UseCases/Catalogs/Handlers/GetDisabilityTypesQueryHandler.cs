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
    public class GetDisabilityTypesQueryHandler
        : IQueryHandler<GetDisabilityTypesQuery, ApiResponse<List<CatalogItemResponse>>>
    {
        private readonly IReadOnlyRepository<DisabilityType> _repository;
        private readonly IMemoryCache _cache;
        private readonly IEncryptionService _encryption;

        private const string CacheKey = "Catalog_DisabilityTypes";
        private static readonly TimeSpan CacheDuration = TimeSpan.FromHours(1);

        public GetDisabilityTypesQueryHandler(IReadOnlyRepository<DisabilityType> repository, IMemoryCache cache, IEncryptionService encryption)
        {
            _repository = repository;
            _cache = cache;
            _encryption = encryption;
        }

        public async Task<ApiResponse<List<CatalogItemResponse>>> HandleAsync(
            GetDisabilityTypesQuery query, CancellationToken cancellationToken)
        {
            if (_cache.TryGetValue(CacheKey, out List<CatalogItemResponse>? cached) && cached is not null)
                return ApiResponse<List<CatalogItemResponse>>.SuccessResult(cached);

            var items = await _repository.GetAllActiveAsync(cancellationToken);
            var response = items.Select(x =>
            {
                var item = CatalogItemResponse.MapToResponse(x);
                item.EncryptedId = ToUrlSafeBase64(_encryption.Encrypt(x.Id.ToString()));
                return item;
            }).ToList();

            _cache.Set(CacheKey, response, new MemoryCacheEntryOptions()
                .SetAbsoluteExpiration(CacheDuration)
                .SetPriority(CacheItemPriority.Normal));

            return ApiResponse<List<CatalogItemResponse>>.SuccessResult(response);
        }

        private static string ToUrlSafeBase64(string s) => s.Replace('+', '-').Replace('/', '_').TrimEnd('=');
    }
}
