using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using PetShop.Client.Services;
using PetShop.Shared.DTOs.Response;

namespace PetShop.Client.Pages.Staff.Pets;

public class IndexModel : PageModel
{
    private readonly IPetClientService _svc;

    public List<PetResponse> Pets       { get; set; } = [];
    public string? Message   { get; set; }
    public string? Search    { get; set; }
    public string? Category  { get; set; }
    public string? Status    { get; set; }
    public int     PageNumber{ get; set; } = 1;
    public int     PageSize  { get; set; } = 8;
    public int     TotalCount{ get; set; }
    public int     TotalPages => (int)Math.Ceiling((double)TotalCount / Math.Max(1, PageSize));

    public IndexModel(IPetClientService svc) => _svc = svc;

    public async Task OnGetAsync(string? search, string? category, string? status, int pageNumber = 1, int pageSize = 8)
    {
        Search     = search;
        Category   = category;
        Status     = status;
        PageNumber = Math.Max(1, pageNumber);
        PageSize   = Math.Clamp(pageSize, 4, 50);
        Message    = TempData["Message"]?.ToString();

        var result = await _svc.SearchAsync(search, category, null, null, status, PageNumber, PageSize);
        if (result?.Data != null)
        {
            Pets       = result.Data.Items;
            TotalCount = result.Data.TotalCount;
        }
        else
        {
            var allResult = await _svc.GetAllAsync();
            var list      = allResult?.Data ?? [];

            if (!string.IsNullOrEmpty(search))
                list = list.Where(p => p.Name.Contains(search, StringComparison.OrdinalIgnoreCase) || p.Breed.Contains(search, StringComparison.OrdinalIgnoreCase)).ToList();
            if (!string.IsNullOrEmpty(category))
                list = list.Where(p => p.Category == category).ToList();
            if (!string.IsNullOrEmpty(status))
                list = list.Where(p => p.Status == status).ToList();

            TotalCount = list.Count;
            Pets       = list.Skip((PageNumber - 1) * PageSize).Take(PageSize).ToList();
        }
    }

    public async Task<IActionResult> OnPostDeleteAsync(int id)
    {
        var result = await _svc.DeleteAsync(id);
        TempData["Message"] = result?.Success == true ? "Xoá thú cưng thành công." : result?.Message;
        return RedirectToPage();
    }
}
