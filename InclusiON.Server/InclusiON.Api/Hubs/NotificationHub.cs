using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using InclusiON.Application.Constants;
using InclusiON.Application.Interfaces.Infrastructure;
using InclusiON.Domain.Enums;

namespace InclusiON.Api.Hubs;

[Authorize]
public class NotificationHub(IEncryptionService encryptionService) : Hub
{
    public override async Task OnConnectedAsync()
    {
        if (!string.IsNullOrEmpty(Context.UserIdentifier))
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, Context.UserIdentifier);
        }

        var eidClaim = Context.User?.FindFirst(Permissions.EntityIdClaimType)?.Value;
        if (!string.IsNullOrEmpty(eidClaim))
        {
            try
            {
                var decryptedEntityId = encryptionService.Decrypt(eidClaim);
                if (!string.IsNullOrEmpty(decryptedEntityId))
                {
                    await Groups.AddToGroupAsync(Context.ConnectionId, decryptedEntityId);
                }
            }
            catch
            {
                // Ignorar si el claim no es desencriptable
            }
        }

        await base.OnConnectedAsync();
    }

    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        if (!string.IsNullOrEmpty(Context.UserIdentifier))
        {
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, Context.UserIdentifier);
        }

        var eidClaim = Context.User?.FindFirst(Permissions.EntityIdClaimType)?.Value;
        if (!string.IsNullOrEmpty(eidClaim))
        {
            try
            {
                var decryptedEntityId = encryptionService.Decrypt(eidClaim);
                if (!string.IsNullOrEmpty(decryptedEntityId))
                {
                    await Groups.RemoveFromGroupAsync(Context.ConnectionId, decryptedEntityId);
                }
            }
            catch
            {
                // Ignorar
            }
        }

        await base.OnDisconnectedAsync(exception);
    }
}
