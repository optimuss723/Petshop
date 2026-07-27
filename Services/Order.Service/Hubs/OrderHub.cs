using Microsoft.AspNetCore.SignalR;
using System.Security.Claims;

namespace Order.Service.Hubs;

public class OrderHub : Hub
{
    /// <summary>Customer join group cá nhân để nhận cập nhật đơn hàng của mình</summary>
    public async Task JoinUserGroup(string userId)
        => await Groups.AddToGroupAsync(Context.ConnectionId, $"User_{userId}");

    /// <summary>Admin join group Admins để nhận thông báo đơn hàng mới</summary>
    public async Task JoinAdminGroup()
        => await Groups.AddToGroupAsync(Context.ConnectionId, "Admins");

    /// <summary>Shipper join group riêng để nhận thông báo được phân công đơn</summary>
    public async Task JoinShipperGroup(string shipperId)
        => await Groups.AddToGroupAsync(Context.ConnectionId, $"Shipper_{shipperId}");

    public async Task LeaveGroup(string groupName)
        => await Groups.RemoveFromGroupAsync(Context.ConnectionId, groupName);

    public override async Task OnConnectedAsync()
    {
        var role      = Context.User?.FindFirst(ClaimTypes.Role)?.Value;
        var userId    = Context.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;

        if (role == "Admin" || role == "Staff")
            await Groups.AddToGroupAsync(Context.ConnectionId, "Admins");

        if ((role == "Shipper" || role == "Staff") && !string.IsNullOrEmpty(userId))
            await Groups.AddToGroupAsync(Context.ConnectionId, $"Shipper_{userId}");

        if (role != "Admin" && role != "Staff" && role != "Shipper" && !string.IsNullOrEmpty(userId))
            await Groups.AddToGroupAsync(Context.ConnectionId, $"User_{userId}");

        await base.OnConnectedAsync();
    }

    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        // Các group sẽ tự động được xóa khi disconnect — không cần xử lý thêm
        await base.OnDisconnectedAsync(exception);
    }
}
