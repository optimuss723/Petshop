using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using PetShop.Client.Services;
using PetShop.Shared.DTOs.Response;

namespace PetShop.Client.Pages.Admin.Orders;

public class DetailModel : PageModel
{
    private readonly IOrderClientService _svc;

    public OrderResponse? Order   { get; set; }
    public string?        Message { get; set; }

    public DetailModel(IOrderClientService svc) => _svc = svc;

    public async Task<IActionResult> OnGetAsync(int id)
    {
        Message = TempData["Message"]?.ToString();
        var result = await _svc.GetByIdAsync(id);
        if (result?.Data == null) return RedirectToPage("Index");
        Order = result.Data;
        return Page();
    }

}
