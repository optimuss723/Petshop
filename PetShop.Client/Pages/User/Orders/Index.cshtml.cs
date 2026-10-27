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
    private readonly IProductClientService _productSvc;
    private readonly IPetClientService _petSvc;

    public IndexModel(
        IOrderClientService orders,
        IProductClientService productSvc,
        IPetClientService petSvc)
    {
        _orders = orders;
        _productSvc = productSvc;
        _petSvc = petSvc;
    }

    // ── Dữ liệu hiển thị ──────────────────────────────────────────
    public List<OrderResponse> Orders                  { get; private set; } = [];
    public Dictionary<int, ProductResponse> ProductMap { get; set; } = new();
    public Dictionary<int, PetResponse> PetMap         { get; set; } = new();

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

        var ordersTask = _orders.GetMyOrdersAsync();
        var prodsTask  = _productSvc.GetAllAsync();
        var petsTask   = _petSvc.GetAllAsync();

        await Task.WhenAll(ordersTask, prodsTask, petsTask);

        var ordersRes = await ordersTask;
        var prodsRes  = await prodsTask;
        var petsRes   = await petsTask;

        var all = ordersRes?.Data ?? [];
        if (prodsRes?.Data != null)
        {
            ProductMap = prodsRes.Data.GroupBy(p => p.Id).ToDictionary(g => g.Key, g => g.First());
        }
        if (petsRes?.Data != null)
        {
            PetMap = petsRes.Data.GroupBy(p => p.Id).ToDictionary(g => g.Key, g => g.First());
        }

        // Stats
        AllCount             = all.Count;
        PendingCount         = all.Count(o => o.Status == "Pending");
        ShippingCount        = all.Count(o => o.Status == "Shipping" || o.Status == "Confirmed");
        AwaitingReceiptCount = all.Count(o => o.Status == "AwaitingReceipt");
        DeliveredCount       = all.Count(o => o.Status == "Delivered");
        TotalSpent           = all.Where(o => o.Status == "Delivered").Sum(o => o.TotalAmount);

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

    public string GetItemImageUrl(OrderItemResponse item)
    {
        if (item.ProductId.HasValue && ProductMap.TryGetValue(item.ProductId.Value, out var prod) && !string.IsNullOrWhiteSpace(prod.ImageUrl))
            return prod.ImageUrl;
        if (item.PetId.HasValue && PetMap.TryGetValue(item.PetId.Value, out var pet) && !string.IsNullOrWhiteSpace(pet.ImageUrl))
            return pet.ImageUrl;
        return item.ProductId.HasValue
            ? "https://images.unsplash.com/photo-1583337130417-3346a1be7dee?auto=format&fit=crop&w=150&q=80"
            : "https://images.unsplash.com/photo-1543466835-00a7907e9de1?auto=format&fit=crop&w=150&q=80";
    }

    public string GetItemCategory(OrderItemResponse item)
    {
        string raw = "";
        if (item.ProductId.HasValue && ProductMap.TryGetValue(item.ProductId.Value, out var prod) && !string.IsNullOrWhiteSpace(prod.Category))
            raw = prod.Category;
        else if (item.PetId.HasValue && PetMap.TryGetValue(item.PetId.Value, out var pet))
            raw = !string.IsNullOrWhiteSpace(pet.Breed) ? pet.Breed : (!string.IsNullOrWhiteSpace(pet.Category) ? pet.Category : "Thú cưng");
        else
            raw = item.ProductId.HasValue ? "Phụ kiện" : "Thú cưng";

        return raw switch
        {
            "Food"      => "Thức ăn & Dinh dưỡng",
            "Toy"       => "Đồ chơi & Phụ kiện",
            "Accessory" => "Chuồng & Phụ kiện",
            "Medicine"  => "Thuốc & Y tế",
            "Care"      => "Vệ sinh & Chăm sóc",
            "Dog"       => "Chó cảnh thuần chủng",
            "Cat"       => "Mèo cảnh thuần chủng",
            "Bird"      => "Chim cảnh",
            "Fish"      => "Cá cảnh thủy sinh",
            _           => raw
        };
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
