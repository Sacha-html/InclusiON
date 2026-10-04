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

public class GetInstitutionalRolesQueryHandler : IQueryHandler<GetInstitutionalRolesQuery, ApiResponse<List<CatalogItemResponse>>>
{
    private readonly IReadOnlyRepository<InstitutionalRole> _repository;
    private readonly IMemoryCache _cache;
    private readonly IEncryptionService _encryption;

    public GetInstitutionalRolesQueryHandler(IReadOnlyRepository<InstitutionalRole> repository, IMemoryCache cache, IEncryptionService encryption)
    {
        _repository = repository;
        _cache = cache;
        _encryption = encryption;
    }

    public async Task<ApiResponse<List<CatalogItemResponse>>> HandleAsync(GetInstitutionalRolesQuery query, CancellationToken ct)
    {
        if (_cache.TryGetValue(CatalogCacheKeys.InstitutionalRoles, out List<CatalogItemResponse>? cached) && cached is not null && cached.Count > 0)
            return ApiResponse<List<CatalogItemResponse>>.SuccessResult(cached);

        var result = (await _repository.GetAllActiveAsync(ct)).Select(x => new CatalogItemResponse
        {
            Id = x.Id,
            EncryptedId = ToUrlSafeBase64(_encryption.Encrypt(x.Id.ToString())),
            Name = x.Name,
            Description = x.Description,
            IsActive = x.IsActive,
        }).ToList();

        if (result.Count > 0)
            _cache.Set(CatalogCacheKeys.InstitutionalRoles, result, TimeSpan.FromHours(1));

        return ApiResponse<List<CatalogItemResponse>>.SuccessResult(result);
    }

    private static string ToUrlSafeBase64(string base64) =>
        base64.Replace('+', '-').Replace('/', '_').TrimEnd('=');
}
