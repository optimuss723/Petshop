using System.Text.Json;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using PetShop.Client.Services;
using PetShop.Shared.DTOs.Request;
using PetShop.Shared.DTOs.Response;

namespace PetShop.Client.Pages.User;

public class PayOSModel : PageModel
{
    private readonly IOrderClientService _orderSvc;

    [BindProperty] public string CartJson { get; set; } = "";
    [BindProperty] public string ShippingAddress { get; set; } = "";
    [BindProperty] public string? Note { get; set; }
    [BindProperty] public int? OrderId { get; set; }

    public List<CartItem> Items { get; private set; } = [];
    public OrderResponse? ExistingOrder { get; private set; }
    public decimal TotalAmount => Items.Sum(i => i.Price * i.Qty);
    public string? ErrorMessage { get; private set; }

    public PayOSModel(IOrderClientService orderSvc) => _orderSvc = orderSvc;

    public async Task<IActionResult> OnGetAsync(int? orderId)
    {
        if (orderId.HasValue)
        {
            var token = HttpContext.Session.GetString("AccessToken");
            if (string.IsNullOrEmpty(token))
                return RedirectToPage("/Auth/Login");

            OrderId = orderId.Value;
            if (!await LoadOrderAsync(orderId.Value))
                return Page();

            return await RedirectToPayOsCheckoutAsync(orderId.Value);
        }

        return RedirectToPage("/Cart");
    }

    public async Task<IActionResult> OnPostAsync()
    {
        var userId = HttpContext.Session.GetString("UserId");
        if (string.IsNullOrEmpty(userId))
            return RedirectToPage("/Auth/Login");

        if (!LoadCart())
            return Page();

        var orderItems = Items.Select(i => new OrderItemRequest(
            ProductId: i.Type == "product" ? i.Id : null,
            PetId: i.Type == "pet" ? i.Id : null,
            Quantity: i.Qty,
            UnitPrice: i.Price
        )).ToList();

        var finalNote = string.IsNullOrWhiteSpace(Note)
            ? "PayOS pending"
            : $"{Note} | PayOS pending";

        var result = await _orderSvc.CreateAsync(new OrderRequest(orderItems, ShippingAddress, finalNote, IsCod: false));

        if (result?.Success == true)
        {
            OrderId = result.Data?.Id;
            TempData["ClearCart"] = "true";

            if (OrderId.HasValue)
                return await RedirectToPayOsCheckoutAsync(OrderId.Value);
        }

        ErrorMessage = result?.Message ?? "Khong tao duoc don hang PayOS.";
        return Page();
    }

    private async Task<IActionResult> RedirectToPayOsCheckoutAsync(int orderId)
    {
        var paymentLink = await _orderSvc.CreatePayOsPaymentLinkAsync(orderId);
        if (paymentLink?.Success == true && !string.IsNullOrWhiteSpace(paymentLink.Data?.CheckoutUrl))
            return Redirect(paymentLink.Data.CheckoutUrl);

        ErrorMessage = paymentLink?.Message ?? "Khong tao duoc link thanh toan PayOS.";
        await LoadOrderAsync(orderId);
        return Page();
    }

    private async Task<bool> LoadOrderAsync(int orderId)
    {
        var result = await _orderSvc.GetByIdAsync(orderId);
        if (result?.Success != true || result.Data == null)
        {
            ErrorMessage = result?.Message ?? "Khong tim thay don hang.";
            return false;
        }

        ExistingOrder = result.Data;
        ShippingAddress = ExistingOrder.ShippingAddress;
        Note = ExistingOrder.Note;

        if (ExistingOrder.Status != "Pending")
        {
            ErrorMessage = "Chi co the thanh toan don hang dang cho xu ly.";
            return false;
        }

        Items = ExistingOrder.Items?.Select(i => new CartItem
        {
            Type = i.ProductId.HasValue ? "product" : "pet",
            Id = i.ProductId ?? i.PetId ?? 0,
            Name = i.ProductName ?? i.PetName ?? $"Mat hang #{i.Id}",
            Price = i.UnitPrice,
            Qty = i.Quantity
        }).ToList() ?? [];

        return true;
    }

    private bool LoadCart()
    {
        if (string.IsNullOrWhiteSpace(CartJson))
        {
            ErrorMessage = "Gio hang trong.";
            return false;
        }

        if (string.IsNullOrWhiteSpace(ShippingAddress))
        {
            ErrorMessage = "Vui long nhap dia chi giao hang.";
            return false;
        }

        try
        {
            Items = JsonSerializer.Deserialize<List<CartItem>>(CartJson,
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true }) ?? [];
        }
        catch (JsonException)
        {
            ErrorMessage = "Du lieu gio hang khong hop le.";
            return false;
        }

        if (Items.Count == 0)
        {
            ErrorMessage = "Gio hang trong.";
            return false;
        }

        foreach (var item in Items)
        {
            item.Qty = Math.Max(1, item.Qty);
            if (item.Type == "pet")
                item.Qty = 1;
        }

        return true;
    }

    public class CartItem
    {
        public string Type { get; set; } = "";
        public int Id { get; set; }
        public string Name { get; set; } = "";
        public decimal Price { get; set; }
        public int Qty { get; set; }
    }
}
