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

    public async Task OnGetAsync(string? search, string? category, string? sort = "CreatedAt desc", int page = 1, int pageSize = 10)
    {
        Search     = search;
        Category   = category;
        Sort       = string.IsNullOrWhiteSpace(sort) ? "CreatedAt desc" : sort;
        PageNumber = Math.Max(1, page);
        PageSize   = Math.Clamp(pageSize, 5, 50);
        Message    = TempData["Message"]?.ToString();

        var skip  = (PageNumber - 1) * PageSize;
        var odata = await _svc.GetODataAsync(Search, Category, Sort, skip, PageSize);
        if (odata != null)
        {
            UsedOData  = true;
            Products   = odata.Value;
            TotalCount = odata.Count ?? Products.Count;
            return;
        }

        var all  = await _svc.GetAllAsync();
        var list = all?.Data ?? [];

        if (!string.IsNullOrEmpty(Category))
            list = list.Where(p => p.Category.Equals(Category, StringComparison.OrdinalIgnoreCase)).ToList();

        if (!string.IsNullOrEmpty(Search))
            list = list.Where(p => p.Name.Contains(Search, StringComparison.OrdinalIgnoreCase)).ToList();

        list = Sort switch
        {
            "Name asc"   => list.OrderBy(p => p.Name).ToList(),
            "Name desc"  => list.OrderByDescending(p => p.Name).ToList(),
            "Price asc"  => list.OrderBy(p => p.Price).ToList(),
            "Price desc" => list.OrderByDescending(p => p.Price).ToList(),
            "Stock asc"  => list.OrderBy(p => p.Stock).ToList(),
            _             => list.OrderByDescending(p => p.CreatedAt).ToList()
        };

        TotalCount = list.Count;
        Products   = list.Skip(skip).Take(PageSize).ToList();
    }

    public async Task<IActionResult> OnPostDeleteAsync(int id)
    {
        var result = await _svc.DeleteAsync(id);
        TempData["Message"] = result?.Success == true
            ? "Xoá sản phẩm thành công."
            : result?.Message ?? "Xoá thất bại.";
        return RedirectToPage();
    }
}
