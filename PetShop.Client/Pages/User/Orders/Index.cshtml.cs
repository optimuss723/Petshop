using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using PetShop.Client.Services;
using PetShop.Shared.DTOs.Response;

namespace PetShop.Client.Pages.User.Orders;

/// <summary>
/// Lịch sử đơn hàng của Customer — có phân trang và chi tiết expandable (#2 #13).
/// </summary>
public class IndexModel : PageModel
{
    private readonly IOrderClientService _orders;

    public IndexModel(IOrderClientService orders) => _orders = orders;

    // ── Dữ liệu hiển thị ──────────────────────────────────────────
    public List<OrderResponse> Orders      { get; private set; } = [];
    public int                 CurrentPage { get; private set; } = 1;
    public int                 PageSize    { get; private set; } = 8;
    public int                 TotalCount  { get; private set; }
    public int                 TotalPages  => (int)Math.Ceiling((double)TotalCount / PageSize);
    public string?             StatusFilter { get; private set; }
    public string?             Message     { get; private set; }
    public bool                ShouldClearCart { get; private set; }

    // ── Stats ──────────────────────────────────────────────────────
    public int     PendingCount   { get; private set; }
    public int     ShippingCount  { get; private set; }
    public int     AwaitingReceiptCount { get; private set; }
    public int     DeliveredCount { get; private set; }
    public decimal TotalSpent     { get; private set; }
    public int     AllCount       { get; private set; }

    public async Task<IActionResult> OnGetAsync(int page = 1, string? status = null)
    {
        var token = HttpContext.Session.GetString("AccessToken");
        if (string.IsNullOrEmpty(token))
            return RedirectToPage("/Auth/Login");

        CurrentPage  = Math.Max(1, page);
        StatusFilter = status;
        Message = TempData["OrderSuccess"] as string;
        ShouldClearCart = TempData["ClearCart"]?.ToString() == "true";

        var result = await _orders.GetMyOrdersAsync();
        var all    = result?.Data ?? [];

        // Stats
        AllCount       = all.Count;
        PendingCount   = all.Count(o => o.Status == "Pending");
        ShippingCount  = all.Count(o => o.Status == "Shipping" || o.Status == "Confirmed");
        AwaitingReceiptCount = all.Count(o => o.Status == "AwaitingReceipt");
        DeliveredCount = all.Count(o => o.Status == "Delivered");
        TotalSpent     = all.Where(o => o.Status == "Delivered").Sum(o => o.TotalAmount);

        // Filter
        var filtered = string.IsNullOrEmpty(status)
            ? all
            : all.Where(o => o.Status == status).ToList();

        TotalCount = filtered.Count;
        Orders     = filtered
            .Skip((CurrentPage - 1) * PageSize)
            .Take(PageSize)
            .ToList();

        return Page();
    }

    public async Task<IActionResult> OnPostCancelAsync(int id, int page = 1, string? status = null)
    {
        var token = HttpContext.Session.GetString("AccessToken");
        if (string.IsNullOrEmpty(token))
            return RedirectToPage("/Auth/Login");

        var result = await _orders.CancelAsync(id);
        TempData["OrderSuccess"] = result?.Success == true
            ? $"✅ Đã hủy đơn hàng #{id}."
            : $"❌ {result?.Message ?? "Không thể hủy đơn hàng."}";

        return RedirectToPage(new { page, status });
    }

    public async Task<IActionResult> OnPostConfirmReceivedAsync(int id, int page = 1, string? status = null)
    {
        var token = HttpContext.Session.GetString("AccessToken");
        if (string.IsNullOrEmpty(token))
            return RedirectToPage("/Auth/Login");

        var result = await _orders.ConfirmReceivedAsync(id);
        TempData["OrderSuccess"] = result?.Success == true
            ? $"✅ Cảm ơn bạn đã xác nhận đã nhận đơn hàng #{id}."
            : $"❌ {result?.Message ?? "Không thể xác nhận đã nhận hàng."}";

        return RedirectToPage(new { page, status = result?.Success == true ? "Delivered" : status });
    }
}
