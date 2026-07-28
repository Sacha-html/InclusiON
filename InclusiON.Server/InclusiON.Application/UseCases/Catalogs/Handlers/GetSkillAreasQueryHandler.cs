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
    public class GetSkillAreasQueryHandler
        : IQueryHandler<GetSkillAreasQuery, ApiResponse<List<SkillAreaResponse>>>
    {
        private readonly IReadOnlyRepository<SkillArea> _repository;
        private readonly IMemoryCache _cache;
        private readonly IEncryptionService _encryption;

        private static readonly TimeSpan CacheDuration = TimeSpan.FromHours(1);

        public GetSkillAreasQueryHandler(IReadOnlyRepository<SkillArea> repository, IMemoryCache cache, IEncryptionService encryption)
        {
            _repository = repository;
            _cache = cache;
            _encryption = encryption;
        }

        public async Task<ApiResponse<List<SkillAreaResponse>>> HandleAsync(
            GetSkillAreasQuery query, CancellationToken cancellationToken)
        {
            if (_cache.TryGetValue(CatalogCacheKeys.SkillAreas, out List<SkillAreaResponse>? cached) && cached is not null)
                return ApiResponse<List<SkillAreaResponse>>.SuccessResult(cached);

            var items = await _repository.GetAllActiveAsync(cancellationToken);
            var response = items.Select(x =>
            {
                var item = SkillAreaResponse.MapToResponse(x);
                item.EncryptedId = ToUrlSafeBase64(_encryption.Encrypt(x.Id.ToString()));
                return item;
            }).ToList();

            _cache.Set(CatalogCacheKeys.SkillAreas, response, new MemoryCacheEntryOptions()
                .SetAbsoluteExpiration(CacheDuration)
                .SetPriority(CacheItemPriority.Normal));

            return ApiResponse<List<SkillAreaResponse>>.SuccessResult(response);
        }

        private static string ToUrlSafeBase64(string s) => s.Replace('+', '-').Replace('/', '_').TrimEnd('=');
    }
}
