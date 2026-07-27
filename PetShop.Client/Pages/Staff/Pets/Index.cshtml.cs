using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using PetShop.Client.Services;
using PetShop.Shared.DTOs.Response;

namespace PetShop.Client.Pages.Staff.Pets;

public class IndexModel : PageModel
{
    private readonly IPetClientService _svc;

    public List<PetResponse> Pets     { get; set; } = [];
    public string? Message  { get; set; }
    public string? Category { get; set; }
    public string? Status   { get; set; }

    public IndexModel(IPetClientService svc) => _svc = svc;

    public async Task OnGetAsync(string? category, string? status)
    {
        Category = category;
        Status   = status;
        Message  = TempData["Message"]?.ToString();

        var result = await _svc.GetAllAsync();
        var list   = result?.Data ?? [];

        if (!string.IsNullOrEmpty(category))
            list = list.Where(p => p.Category == category).ToList();
        if (!string.IsNullOrEmpty(status))
            list = list.Where(p => p.Status == status).ToList();

        Pets = list;
    }

    public async Task<IActionResult> OnPostDeleteAsync(int id)
    {
        var result = await _svc.DeleteAsync(id);
        TempData["Message"] = result?.Success == true ? "Xoá thú cưng thành công." : result?.Message;
        return RedirectToPage();
    }
}
