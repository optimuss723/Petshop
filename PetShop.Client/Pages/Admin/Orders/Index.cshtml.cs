using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using PetShop.Client.Services;
using PetShop.Shared.DTOs.Response;

namespace PetShop.Client.Pages.Admin.Orders;

public class IndexModel : PageModel
{
    private readonly IOrderClientService _svc;

    public List<OrderResponse> Orders       { get; set; } = [];
    public string?             StatusFilter { get; set; }
    public string?             Message      { get; set; }

    public IndexModel(IOrderClientService svc) => _svc = svc;

    public async Task OnGetAsync(string? status)
    {
        StatusFilter = status;
        Message      = TempData["Message"]?.ToString();

        var result = await _svc.GetAllAsync();
        var list   = result?.Data ?? [];

        if (!string.IsNullOrEmpty(status))
            list = list.Where(o => o.Status == status).ToList();

        Orders = list.OrderByDescending(o => o.CreatedAt).ToList();
    }

}
