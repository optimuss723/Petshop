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
    private readonly IProductClientService _productSvc;
    private readonly IPetClientService _petSvc;

    public List<OrderResponse> Orders                  { get; set; } = [];
    public Dictionary<int, ProductResponse> ProductMap { get; set; } = new();
    public Dictionary<int, PetResponse> PetMap         { get; set; } = new();

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

    public DashboardModel(
        IOrderClientService orderSvc,
        IProductClientService productSvc,
        IPetClientService petSvc)
    {
        _orderSvc = orderSvc;
        _productSvc = productSvc;
        _petSvc = petSvc;
    }

    public async Task OnGetAsync()
    {
        ProfileMessage      = TempData["ProfileMessage"]?.ToString();
        OrderSuccessMessage = TempData["OrderSuccess"]?.ToString();
        ShouldClearCart     = TempData["ClearCart"]?.ToString() == "true";

        // Pre-fill profile inputs from session
        ProfileInput.FullName = HttpContext.Session.GetString("UserName") ?? "";
        ProfileInput.Phone    = HttpContext.Session.GetString("UserPhone") ?? "";
        ProfileInput.Address  = HttpContext.Session.GetString("UserAddress");

        var ordersTask = _orderSvc.GetMyOrdersAsync();
        var prodsTask  = _productSvc.GetAllAsync();
        var petsTask   = _petSvc.GetAllAsync();

        await Task.WhenAll(ordersTask, prodsTask, petsTask);

        var ordersRes = await ordersTask;
        var prodsRes  = await prodsTask;
        var petsRes   = await petsTask;

        Orders = ordersRes?.Data?.OrderByDescending(o => o.CreatedAt).ToList() ?? [];
        if (prodsRes?.Data != null)
        {
            ProductMap = prodsRes.Data.GroupBy(p => p.Id).ToDictionary(g => g.Key, g => g.First());
        }
        if (petsRes?.Data != null)
        {
            PetMap = petsRes.Data.GroupBy(p => p.Id).ToDictionary(g => g.Key, g => g.First());
        }
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
