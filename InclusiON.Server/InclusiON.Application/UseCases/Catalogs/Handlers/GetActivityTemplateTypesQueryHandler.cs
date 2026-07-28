using Microsoft.Extensions.Caching.Memory;
using InclusiON.Application.Constants;
using InclusiON.Application.Interfaces.Common;
using InclusiON.Application.Interfaces.Infrastructure;
using InclusiON.Application.Interfaces.Repositories.Base;
using InclusiON.Application.UseCases.Catalogs.Queries;
using InclusiON.Domain.Models;
using InclusiON.DTOs.Responses;
using InclusiON.DTOs.Responses.Catalogs;

namespace InclusiON.Application.UseCases.Catalogs.Handlers
{
    public class GetActivityTemplateTypesQueryHandler
        : IQueryHandler<GetActivityTemplateTypesQuery, ApiResponse<List<ActivityTemplateTypeResponse>>>
    {
        private readonly IReadOnlyRepository<ActivityTemplateType> _repository;
        private readonly IMemoryCache _cache;
        private readonly IEncryptionService _encryption;

        private static readonly TimeSpan CacheDuration = TimeSpan.FromHours(1);

        public GetActivityTemplateTypesQueryHandler(IReadOnlyRepository<ActivityTemplateType> repository, IMemoryCache cache, IEncryptionService encryption)
        {
            _repository = repository;
            _cache = cache;
            _encryption = encryption;
        }

        public async Task<ApiResponse<List<ActivityTemplateTypeResponse>>> HandleAsync(
            GetActivityTemplateTypesQuery query, CancellationToken cancellationToken)
        {
            if (_cache.TryGetValue(CatalogCacheKeys.ActivityTemplateTypes, out List<ActivityTemplateTypeResponse>? cached) && cached is not null)
                return ApiResponse<List<ActivityTemplateTypeResponse>>.SuccessResult(cached);

            var items = await _repository.GetAllActiveAsync(cancellationToken);
            var response = items.Select(x =>
            {
                var item = ActivityTemplateTypeResponse.MapToResponse(x);
                item.EncryptedId = ToUrlSafeBase64(_encryption.Encrypt(x.Id.ToString()));
                return item;
            }).ToList();

            _cache.Set(CatalogCacheKeys.ActivityTemplateTypes, response, new MemoryCacheEntryOptions()
                .SetAbsoluteExpiration(CacheDuration)
                .SetPriority(CacheItemPriority.Normal));

            return ApiResponse<List<ActivityTemplateTypeResponse>>.SuccessResult(response);
        }

        private static string ToUrlSafeBase64(string s) => s.Replace('+', '-').Replace('/', '_').TrimEnd('=');
    }
}
