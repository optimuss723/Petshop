using Microsoft.AspNetCore.SignalR;

namespace Catalog.Service.Hubs;

/// <summary>
/// SignalR Hub phát sự kiện catalog real-time đến tất cả Client.
/// Events:
///   NewPetAdded(pet)        — Admin thêm thú cưng mới
///   NewProductAdded(prod)   — Admin thêm sản phẩm mới
///   PetStatusChanged(data)  — Trạng thái thú cưng thay đổi (Sold/Reserved)
///   StockUpdated(data)      — Số lượng tồn kho sản phẩm thay đổi
///   ProductDeleted(id)      — Sản phẩm bị xóa
///   PetDeleted(id)          — Thú cưng bị xóa
/// </summary>
public class CatalogHub : Hub
{
    public override async Task OnConnectedAsync()
    {
        // Tất cả client đều join group "Customers" để nhận broadcast
        await Groups.AddToGroupAsync(Context.ConnectionId, "Customers");
        await base.OnConnectedAsync();
    }

    public async Task JoinAdminGroup()
    {
        await Groups.AddToGroupAsync(Context.ConnectionId, "Admins");
    }
}
