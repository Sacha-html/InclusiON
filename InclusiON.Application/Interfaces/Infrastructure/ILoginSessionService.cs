using InclusiON.Domain.Models;
using InclusiON.DTOs.Responses;
using InclusiON.DTOs.Responses.Auth;

namespace InclusiON.Application.Interfaces.Infrastructure
{
    /// <summary>
    /// Servicio para crear sesiones de login: genera tokens, revoca sesiones anteriores,
    /// actualiza metadata del usuario y persiste todo en una transaccion.
    /// </summary>
    public interface ILoginSessionService
    {
        /// <summary>
        /// Crea una sesion de login estandar (email+password, refresh token).
        /// Genera access/refresh tokens, revoca tokens anteriores y actualiza metadata del usuario.
        /// </summary>
        Task<ApiResponse<LoginResponse>> CreateLoginSessionAsync(
            User user,
            int refreshTokenExpiryDays,
            string revokeReason,
            string successMessage,
            CancellationToken cancellationToken);

        /// <summary>
        /// Crea una sesion de login visual (PIN, password, asistido).
        /// Genera tokens, revoca sesiones anteriores, actualiza metadata y registra dispositivo opcional.
        /// </summary>
        Task<ApiResponse<VisualLoginResponse>> CreateVisualLoginSessionAsync(
            User user,
            PersonWithDisability person,
            int refreshTokenExpiryDays,
            string? deviceId,
            bool rememberDevice,
            string revokeReason,
            string successMessage,
            CancellationToken cancellationToken);
    }
}
