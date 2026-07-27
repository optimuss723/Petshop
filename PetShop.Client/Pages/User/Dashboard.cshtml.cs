using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using PetShop.Client.Services;
using PetShop.Shared.DTOs.Request;
using PetShop.Shared.DTOs.Response;

namespace PetShop.Client.Pages.User;

public class ProfileInput
{
    public string  FullName { get; set; } = "";
    public string  Phone    { get; set; } = "";
    public string? Address  { get; set; }
}

public class DashboardModel : PageModel
{
    private readonly IOrderClientService _orderSvc;

    public List<OrderResponse> Orders              { get; set; } = [];
    public string  FullName          => HttpContext.Session.GetString("UserName") ?? "Khách";
    public string? ProfileMessage    { get; set; }
    public string? OrderSuccessMessage { get; set; }  // Thông báo đặt hàng thành công
    public bool    ShouldClearCart   { get; set; }    // Trang này sẽ xóa cart trong JS

    [BindProperty] public ProfileInput ProfileInput { get; set; } = new();

    // Stats
    public int     PendingCount   => Orders.Count(o => o.Status == "Pending");
    public int     ShippingCount  => Orders.Count(o => o.Status == "Shipping" || o.Status == "Confirmed" || o.Status == "AwaitingReceipt");
    public int     DeliveredCount => Orders.Count(o => o.Status == "Delivered");
    public decimal TotalSpent     => Orders.Where(o => o.Status == "Delivered").Sum(o => o.TotalAmount);

    public DashboardModel(IOrderClientService orderSvc) => _orderSvc = orderSvc;

    public async Task OnGetAsync()
    {
        ProfileMessage     = TempData["ProfileMessage"]?.ToString();
        OrderSuccessMessage = TempData["OrderSuccess"]?.ToString();
        ShouldClearCart     = TempData["ClearCart"]?.ToString() == "true";

        // Pre-fill profile inputs from session
        ProfileInput.FullName = HttpContext.Session.GetString("UserName") ?? "";
        ProfileInput.Phone    = HttpContext.Session.GetString("UserPhone") ?? "";
        ProfileInput.Address  = HttpContext.Session.GetString("UserAddress");

        var result = await _orderSvc.GetMyOrdersAsync();
        Orders = result?.Data?.OrderByDescending(o => o.CreatedAt).ToList() ?? [];
    }

    public async Task<IActionResult> OnPostUpdateProfileAsync()
    {
        // Lưu vào session (thực tế nên gọi User.Service API)
        HttpContext.Session.SetString("UserName",    ProfileInput.FullName);
        HttpContext.Session.SetString("UserPhone",   ProfileInput.Phone);
        HttpContext.Session.SetString("UserAddress", ProfileInput.Address ?? "");

        TempData["ProfileMessage"] = "Cập nhật thông tin thành công.";
        return RedirectToPage();
    }

    public async Task<IActionResult> OnPostCancelAsync(int id)
    {
        var result = await _orderSvc.CancelAsync(id);
        TempData["ProfileMessage"] = result?.Success == true
            ? $"Đã huỷ đơn hàng #{id}."
            : result?.Message ?? "Huỷ đơn thất bại.";
        return RedirectToPage();
    }

    public async Task<IActionResult> OnPostConfirmReceivedAsync(int id)
    {
        var result = await _orderSvc.ConfirmReceivedAsync(id);
        TempData["ProfileMessage"] = result?.Success == true
            ? $"Cảm ơn bạn đã xác nhận đã nhận đơn hàng #{id}."
            : result?.Message ?? "Xác nhận nhận hàng thất bại.";
        return RedirectToPage();
    }
}
