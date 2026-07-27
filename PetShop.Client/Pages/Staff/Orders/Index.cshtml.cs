using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using PetShop.Client.Services;
using PetShop.Shared.DTOs.Request;
using PetShop.Shared.DTOs.Response;

namespace PetShop.Client.Pages.Staff.Orders;

public class IndexModel : PageModel
{
    private readonly IOrderClientService _orders;

    public List<OrderResponse> Orders      { get; private set; } = [];
    public string?             StatusFilter { get; private set; }
    public string?             Message      { get; private set; }

    public IndexModel(IOrderClientService orders) => _orders = orders;

    public async Task OnGetAsync(string? status)
    {
        StatusFilter = status;
        Message = TempData["Message"]?.ToString();
        var result   = await _orders.GetAllAsync(status);
        Orders       = result?.Data ?? [];
    }

    public async Task<IActionResult> OnPostUpdateStatusAsync(int id, string status, string? statusFilter)
    {
        var result = await _orders.UpdateStatusAsync(id, new UpdateOrderStatusRequest(status));
        TempData["Message"] = result?.Success == true
            ? $"✅ Đã cập nhật đơn #{id} → {status}"
            : $"❌ {result?.Message}";

        return RedirectToPage(new { status = statusFilter });
    }

    public async Task<IActionResult> OnPostCreateShipmentAsync(int id, string? statusFilter)
    {
        var result = await _orders.CreateShipmentAsync(id);
        TempData["Message"] = result?.Success == true
            ? $"✅ {result.Data?.Message ?? $"Đã tạo vận đơn cho đơn #{id}."}"
            : $"❌ {result?.Message ?? "Tạo vận đơn thất bại."}";

        return RedirectToPage(new { status = result?.Success == true ? "Shipping" : statusFilter });
    }
}
