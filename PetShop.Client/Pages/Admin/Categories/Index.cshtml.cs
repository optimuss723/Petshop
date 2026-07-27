using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using PetShop.Client.Services;
using PetShop.Shared.DTOs.Request;
using PetShop.Shared.DTOs.Response;

namespace PetShop.Client.Pages.Admin.Categories;

public class IndexModel : PageModel
{
    private readonly ICategoryClientService _catSvc;
    public List<CategoryResponse> Categories { get; set; } = [];
    public string? Message { get; set; }
    public string? ErrorMessage { get; set; }

    [BindProperty] public string  Name        { get; set; } = "";
    [BindProperty] public string  Type        { get; set; } = "Product";
    [BindProperty] public string? Description { get; set; }
    [BindProperty] public string? Icon        { get; set; }
    [BindProperty] public int?    EditId      { get; set; }

    public IndexModel(ICategoryClientService catSvc) => _catSvc = catSvc;

    public async Task OnGetAsync()
    {
        Message = TempData["Message"]?.ToString();
        var result = await _catSvc.GetAllAsync();
        Categories = result?.Data ?? [];
    }

    public async Task<IActionResult> OnPostSaveAsync()
    {
        var req = new CategoryCreateRequest(Name, Type, Description, Icon);
        if (EditId.HasValue && EditId > 0)
        {
            var r = await _catSvc.UpdateAsync(EditId.Value, req);
            TempData["Message"] = r?.Success == true ? "Cập nhật danh mục thành công." : r?.Message;
        }
        else
        {
            var r = await _catSvc.CreateAsync(req);
            TempData["Message"] = r?.Success == true ? "Tạo danh mục thành công." : r?.Message;
        }
        return RedirectToPage();
    }

    public async Task<IActionResult> OnPostDeleteAsync(int id)
    {
        var r = await _catSvc.DeleteAsync(id);
        TempData["Message"] = r?.Success == true ? "Đã xóa danh mục." : r?.Message;
        return RedirectToPage();
    }
}
