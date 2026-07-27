using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using PetShop.Client.Services;
using PetShop.Shared.DTOs.Request;
using System.Text.Json;

namespace PetShop.Client.Pages;

public class CartModel : PageModel
{
    private readonly IOrderClientService _orderSvc;
    private readonly IUserClientService _userSvc;

    [BindProperty] public string CartJson        { get; set; } = "";
    [BindProperty] public string ShippingAddress { get; set; } = "";
    [BindProperty] public string? Note          { get; set; }

    public string CustomerName  { get; private set; } = "";
    public string CustomerPhone { get; private set; } = "";
    public string? ErrorMessage   { get; set; }
    public string? SuccessMessage { get; set; }

    public CartModel(IOrderClientService orderSvc, IUserClientService userSvc)
    {
        _orderSvc = orderSvc;
        _userSvc = userSvc;
    }

    public async Task<IActionResult> OnGetAsync()
    {
        if (string.IsNullOrEmpty(HttpContext.Session.GetString("AccessToken")))
            return Page(); // Vẫn cho xem giỏ hàng, chỉ block khi đặt

        await LoadCustomerInfoAsync();
        return Page();
    }

    public async Task<IActionResult> OnPostAsync()
    {
        // Kiểm tra đăng nhập
        var userId = HttpContext.Session.GetString("UserId");
        if (string.IsNullOrEmpty(userId))
            return RedirectToPage("/Auth/Login");

        // Parse giỏ hàng từ JSON
        if (string.IsNullOrWhiteSpace(CartJson))
        {
            await LoadCustomerInfoAsync();
            ErrorMessage = "Giỏ hàng trống.";
            return Page();
        }

        if (string.IsNullOrWhiteSpace(ShippingAddress))
        {
            await LoadCustomerInfoAsync();
            ErrorMessage = "Vui lòng nhập địa chỉ giao hàng.";
            return Page();
        }

        try
        {
            var cartItems = JsonSerializer.Deserialize<List<CartItem>>(CartJson,
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

            if (cartItems == null || cartItems.Count == 0)
            {
                await LoadCustomerInfoAsync();
                ErrorMessage = "Giỏ hàng trống.";
                return Page();
            }

            var orderItems = cartItems.Select(i => new OrderItemRequest(
                ProductId: i.Type == "product" ? (int?)i.Id : null,
                PetId:     i.Type == "pet"     ? (int?)i.Id : null,
                Quantity:  i.Qty,
                UnitPrice: i.Price
            )).ToList();

            var request = new OrderRequest(orderItems, ShippingAddress, Note);
            var result  = await _orderSvc.CreateAsync(request);

            if (result?.Success == true)
            {
                TempData["OrderSuccess"] = "Đặt hàng thành công! Chúng tôi sẽ liên hệ với bạn sớm.";
                TempData["ClearCart"]    = "true";   // Báo hiệu Dashboard xóa cart
                return RedirectToPage("/User/Dashboard");
            }

            ErrorMessage = result?.Message ?? "Đặt hàng thất bại. Vui lòng thử lại.";
        }
        catch (Exception ex)
        {
            ErrorMessage = $"Lỗi: {ex.Message}";
        }

        await LoadCustomerInfoAsync();
        return Page();
    }

    private async Task LoadCustomerInfoAsync()
    {
        CustomerName = HttpContext.Session.GetString("UserName") ?? "";
        CustomerPhone = HttpContext.Session.GetString("UserPhone") ?? "";

        if (string.IsNullOrWhiteSpace(ShippingAddress))
            ShippingAddress = HttpContext.Session.GetString("UserAddress") ?? "";

        var result = await _userSvc.GetMeAsync();
        if (result?.Data == null) return;

        CustomerName = result.Data.FullName;
        CustomerPhone = result.Data.Phone;

        HttpContext.Session.SetString("UserName", result.Data.FullName ?? "");
        HttpContext.Session.SetString("UserPhone", result.Data.Phone ?? "");
        HttpContext.Session.SetString("UserAddress", result.Data.Address ?? "");

        if (string.IsNullOrWhiteSpace(ShippingAddress))
            ShippingAddress = result.Data.Address ?? "";
    }

    private class CartItem
    {
        public string  Type  { get; set; } = "";
        public int     Id    { get; set; }
        public string  Name  { get; set; } = "";
        public decimal Price { get; set; }
        public int     Qty   { get; set; }
    }
}
