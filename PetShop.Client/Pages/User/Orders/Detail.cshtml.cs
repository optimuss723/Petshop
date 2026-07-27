using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using PetShop.Client.Services;
using PetShop.Shared.DTOs.Response;

namespace PetShop.Client.Pages.User.Orders;

public class DetailModel : PageModel
{
    private readonly IOrderClientService _orders;

    public OrderResponse? Order { get; private set; }
    public string? Message { get; private set; }

    public DetailModel(IOrderClientService orders) => _orders = orders;

    public async Task<IActionResult> OnGetAsync(int id)
    {
        if (string.IsNullOrEmpty(HttpContext.Session.GetString("AccessToken")))
            return RedirectToPage("/Auth/Login");

        Message = TempData["OrderSuccess"]?.ToString();
        var result = await _orders.GetByIdAsync(id);
        if (result?.Data == null)
            return RedirectToPage("Index");

        Order = result.Data;
        return Page();
    }

    public async Task<IActionResult> OnPostCancelAsync(int id)
    {
        if (string.IsNullOrEmpty(HttpContext.Session.GetString("AccessToken")))
            return RedirectToPage("/Auth/Login");

        var result = await _orders.CancelAsync(id);
        TempData["OrderSuccess"] = result?.Success == true
            ? $"✅ Đã hủy đơn hàng #{id}."
            : $"❌ {result?.Message ?? "Không thể hủy đơn hàng."}";

        return RedirectToPage(new { id });
    }

    public async Task<IActionResult> OnPostConfirmReceivedAsync(int id)
    {
        if (string.IsNullOrEmpty(HttpContext.Session.GetString("AccessToken")))
            return RedirectToPage("/Auth/Login");

        var result = await _orders.ConfirmReceivedAsync(id);
        TempData["OrderSuccess"] = result?.Success == true
            ? $"✅ Cảm ơn bạn đã xác nhận đã nhận đơn hàng #{id}."
            : $"❌ {result?.Message ?? "Không thể xác nhận đã nhận hàng."}";

        return RedirectToPage(new { id });
    }
}
