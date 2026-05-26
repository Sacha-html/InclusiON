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
    public class GetReportTypesQueryHandler
        : IQueryHandler<GetReportTypesQuery, ApiResponse<List<CatalogItemResponse>>>
    {
        private readonly IReadOnlyRepository<ReportType> _repository;
        private readonly IMemoryCache _cache;
        private readonly IEncryptionService _encryption;

        private static readonly TimeSpan CacheDuration = TimeSpan.FromHours(1);

        public GetReportTypesQueryHandler(IReadOnlyRepository<ReportType> repository, IMemoryCache cache, IEncryptionService encryption)
        {
            _repository = repository;
            _cache = cache;
            _encryption = encryption;
        }

        public async Task<ApiResponse<List<CatalogItemResponse>>> HandleAsync(
            GetReportTypesQuery query, CancellationToken cancellationToken)
        {
            if (_cache.TryGetValue(CatalogCacheKeys.ReportTypes, out List<CatalogItemResponse>? cached) && cached is not null)
                return ApiResponse<List<CatalogItemResponse>>.SuccessResult(cached);

            var items = await _repository.GetAllActiveAsync(cancellationToken);
            var response = items.Select(x =>
            {
                var item = CatalogItemResponse.MapToResponse(x);
                item.EncryptedId = ToUrlSafeBase64(_encryption.Encrypt(x.Id.ToString()));
                return item;
            }).ToList();

            _cache.Set(CatalogCacheKeys.ReportTypes, response, new MemoryCacheEntryOptions()
                .SetAbsoluteExpiration(CacheDuration)
                .SetPriority(CacheItemPriority.Normal));

            return ApiResponse<List<CatalogItemResponse>>.SuccessResult(response);
        }

        private static string ToUrlSafeBase64(string s) => s.Replace('+', '-').Replace('/', '_').TrimEnd('=');
    }
}
