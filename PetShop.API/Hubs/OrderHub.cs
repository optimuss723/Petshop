using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using System.Security.Claims;

namespace PetShop.API.Hubs;

[Authorize]
public class OrderHub : Hub
{
    public override async Task OnConnectedAsync()
    {
        var role = Context.User?.FindFirst(ClaimTypes.Role)?.Value;
        if (role is "Admin" or "Staff")
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, "Admins");
        }

        var userId = Context.UserIdentifier 
                     ?? Context.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (!string.IsNullOrEmpty(userId))
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, $"User_{userId}");
        }

        await base.OnConnectedAsync();
    }

    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        var role = Context.User?.FindFirst(ClaimTypes.Role)?.Value;
        if (role is "Admin" or "Staff")
        {
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, "Admins");
        }

        var userId = Context.UserIdentifier 
                     ?? Context.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (!string.IsNullOrEmpty(userId))
        {
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, $"User_{userId}");
        }

        await base.OnDisconnectedAsync(exception);
    }

    public async Task JoinAdminGroup()
    {
        var role = Context.User?.FindFirst(ClaimTypes.Role)?.Value;
        if (role is "Admin" or "Staff")
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, "Admins");
        }
    }

    public async Task JoinUserGroup(string userId)
    {
        if (!string.IsNullOrEmpty(userId))
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, $"User_{userId}");
        }
    }
}
