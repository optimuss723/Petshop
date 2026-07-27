using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using PetShop.Client.Services;
using PetShop.Shared.DTOs.Response;

namespace PetShop.Client.Pages.Admin.Products;

public class IndexModel : PageModel
{
    private readonly IProductClientService _svc;

    public List<ProductResponse> Products { get; set; } = [];
    public string? Message   { get; set; }
    public string? Search    { get; set; }
    public string? Category  { get; set; }
    public string? Sort      { get; set; }
    public int PageNumber    { get; set; } = 1;
    public int PageSize      { get; set; } = 10;
    public int TotalCount    { get; set; }
    public int TotalPages    => (int)Math.Ceiling((double)TotalCount / PageSize);
    public bool UsedOData    { get; set; }

    public IndexModel(IProductClientService svc) => _svc = svc;

    public IActionResult OnGet(string? search, string? category, string? sort = "CreatedAt desc", int page = 1, int pageSize = 10)
        => RedirectToPage("/Admin/Dashboard");

    public async Task<IActionResult> OnPostDeleteAsync(int id)
    {
        var result = await _svc.DeleteAsync(id);
        TempData["Message"] = result?.Success == true
            ? "Xoá sản phẩm thành công."
            : result?.Message ?? "Xoá thất bại.";
        return RedirectToPage();
    }
}
