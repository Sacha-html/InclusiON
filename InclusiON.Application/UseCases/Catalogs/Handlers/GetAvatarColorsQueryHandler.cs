using InclusiON.Application.Interfaces.Common;
using InclusiON.Application.UseCases.Catalogs.Queries;
using InclusiON.DTOs.Responses;
using InclusiON.DTOs.Responses.Catalogs;
using InclusiON.Shared.Constants;

namespace InclusiON.Application.UseCases.Catalogs.Handlers
{
    public class GetAvatarColorsQueryHandler
        : IQueryHandler<GetAvatarColorsQuery, ApiResponse<List<AvatarColorResponse>>>
    {
        public Task<ApiResponse<List<AvatarColorResponse>>> HandleAsync(
            GetAvatarColorsQuery query, CancellationToken cancellationToken)
        {
            var response = AvatarColors.Items
                .Select(c => new AvatarColorResponse { Hex = c.Hex, Name = c.Name })
                .ToList();

            return Task.FromResult(ApiResponse<List<AvatarColorResponse>>.SuccessResult(response));
        }
    }
}
