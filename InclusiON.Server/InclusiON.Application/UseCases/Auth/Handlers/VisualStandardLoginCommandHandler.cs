using InclusiON.Application.Interfaces.Common;
using InclusiON.Application.Interfaces.Infrastructure;
using InclusiON.Application.Interfaces.Repositories;
using InclusiON.Application.UseCases.Auth.Commands;
using InclusiON.DTOs.Common;
using InclusiON.DTOs.Responses;
using InclusiON.DTOs.Responses.Auth;
using InclusiON.Shared.Resources;

namespace InclusiON.Application.UseCases.Auth.Handlers
{
    public class VisualStandardLoginCommandHandler : ICommandHandler<VisualStandardLoginCommand, ApiResponse<VisualLoginResponse>>
    {
        private readonly IVisualLoginRepository _repository;
        private readonly IIdentityService _identityService;
        private readonly ILoginSessionService _loginSessionService;

        private const int MaxFailedAttempts = 5;

        public VisualStandardLoginCommandHandler(
            IVisualLoginRepository repository,
            IIdentityService identityService,
            ILoginSessionService loginSessionService)
        {
            _repository = repository;
            _identityService = identityService;
            _loginSessionService = loginSessionService;
        }

        public async Task<ApiResponse<VisualLoginResponse>> HandleAsync(
            VisualStandardLoginCommand command,
            CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();

            return await Task.FromResult(ApiResponse<VisualLoginResponse>.SuccessResult(
                new VisualLoginResponse
                {
                    Success = false,
                    ErrorMessage = "El inicio de sesión por contraseña ya no está disponible para alumnos. Utilizá PIN o inicio de sesión Asistido."
                }));
        }
    }
}
