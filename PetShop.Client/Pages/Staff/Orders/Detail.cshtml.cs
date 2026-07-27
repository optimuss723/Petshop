using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using PetShop.Client.Services;
using PetShop.Shared.DTOs.Request;
using PetShop.Shared.DTOs.Response;

namespace PetShop.Client.Pages.Staff.Orders;

public class DetailModel : PageModel
{
    private readonly IOrderClientService _orders;

    public OrderResponse? Order { get; private set; }
    public string? Message { get; private set; }

    public DetailModel(IOrderClientService orders) => _orders = orders;

    public async Task<IActionResult> OnGetAsync(int id)
    {
        Message = TempData["Message"]?.ToString();
        var result = await _orders.GetByIdAsync(id);
        if (result?.Data == null)
            return RedirectToPage("Index");

        Order = result.Data;
        return Page();
    }

    public async Task<IActionResult> OnPostUpdateStatusAsync(int id, string status)
    {
        var result = await _orders.UpdateStatusAsync(id, new UpdateOrderStatusRequest(status));
        TempData["Message"] = result?.Success == true
            ? $"✅ Đã cập nhật đơn #{id} → {status}"
            : $"❌ {result?.Message ?? "Cập nhật thất bại."}";

        return RedirectToPage(new { id });
    }

    public async Task<IActionResult> OnPostCreateShipmentAsync(int id)
    {
        var result = await _orders.CreateShipmentAsync(id);
        TempData["Message"] = result?.Success == true
            ? $"✅ {result.Data?.Message ?? $"Đã tạo vận đơn cho đơn #{id}."}"
            : $"❌ {result?.Message ?? "Tạo vận đơn thất bại."}";

        return RedirectToPage(new { id });
    }
}
