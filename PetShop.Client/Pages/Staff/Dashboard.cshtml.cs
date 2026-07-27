using Microsoft.AspNetCore.Mvc.RazorPages;
using PetShop.Client.Services;
using PetShop.Shared.DTOs.Response;

namespace PetShop.Client.Pages.Staff;

public class DashboardModel : PageModel
{
    private readonly IOrderClientService _orders;

    public int PendingCount   { get; private set; }
    public int ConfirmedCount { get; private set; }
    public int ShippingCount  { get; private set; }
    public int DeliveredCount { get; private set; }
    public List<OrderResponse> RecentOrders { get; private set; } = [];
    public string UserName => HttpContext.Session.GetString("UserName") ?? "Staff";

    public DashboardModel(IOrderClientService orders) => _orders = orders;

    public async Task OnGetAsync()
    {
        var all = await _orders.GetAllAsync();
        var list = all?.Data ?? [];

        PendingCount   = list.Count(o => o.Status == "Pending");
        ConfirmedCount = list.Count(o => o.Status == "Confirmed");
        ShippingCount  = list.Count(o => o.Status == "Shipping" || o.Status == "AwaitingReceipt");
        DeliveredCount = list.Count(o => o.Status == "Delivered");

        // 5 đơn mới nhất cần xử lý (Pending + Confirmed + Shipping/AwaitingReceipt)
        RecentOrders = list
            .Where(o => o.Status is "Pending" or "Confirmed" or "Shipping" or "AwaitingReceipt")
            .OrderByDescending(o => o.CreatedAt)
            .Take(5)
            .ToList();
    }
}
