using Microsoft.Extensions.Caching.Memory;
using InclusiON.Application.Constants;
using InclusiON.Application.Interfaces.Common;
using InclusiON.Application.Interfaces.Infrastructure;
using InclusiON.Application.Interfaces.Repositories.Base;
using InclusiON.Application.UseCases.Catalogs.Queries;
using InclusiON.Domain.Models;
using InclusiON.DTOs.Responses;
using InclusiON.DTOs.Responses.Catalogs;

namespace InclusiON.Application.UseCases.Catalogs.Handlers;

public class GetSpecialtiesQueryHandler : IQueryHandler<GetSpecialtiesQuery, ApiResponse<List<CatalogItemResponse>>>
{
    private readonly IReadOnlyRepository<Specialty> _repository;
    private readonly IMemoryCache _cache;
    private readonly IEncryptionService _encryption;
    public GetSpecialtiesQueryHandler(IReadOnlyRepository<Specialty> repository, IMemoryCache cache, IEncryptionService encryption)
    {
        _repository = repository;
        _cache = cache;
        _encryption = encryption;
    }
    public async Task<ApiResponse<List<CatalogItemResponse>>> HandleAsync(GetSpecialtiesQuery query, CancellationToken ct)
    {
        if (_cache.TryGetValue(CatalogCacheKeys.Specialties, out List<CatalogItemResponse>? cached) && cached is not null && cached.Count > 0)
            return ApiResponse<List<CatalogItemResponse>>.SuccessResult(cached);
        var result = (await _repository.GetAllActiveAsync(ct)).Select(x => new CatalogItemResponse
        {
            Id = x.Id,
            EncryptedId = ToUrlSafeBase64(_encryption.Encrypt(x.Id.ToString())),
            Name = x.Name,
            IsActive = x.IsActive,
        }).ToList();
        // Never cache an empty catalog: a startup/query failure must not hide newly migrated data for an hour.
        if (result.Count > 0)
            _cache.Set(CatalogCacheKeys.Specialties, result, TimeSpan.FromHours(1));
        return ApiResponse<List<CatalogItemResponse>>.SuccessResult(result);
    }

    private static string ToUrlSafeBase64(string s) => s.Replace('+', '-').Replace('/', '_').TrimEnd('=');
}
