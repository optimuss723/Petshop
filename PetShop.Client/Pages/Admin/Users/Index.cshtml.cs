using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using PetShop.Client.Services;
using PetShop.Shared.DTOs.Response;

namespace PetShop.Client.Pages.Admin.Users;

public class IndexModel : PageModel
{
    private readonly IUserClientService _userSvc;
    public List<UserResponse> Users { get; set; } = [];
    public string? Message { get; set; }

    public IndexModel(IUserClientService userSvc) => _userSvc = userSvc;

    public async Task OnGetAsync()
    {
        Message = TempData["Message"]?.ToString();
        var result = await _userSvc.GetAllAsync();
        Users = result?.Data?.OrderByDescending(u => u.CreatedAt).ToList() ?? [];
    }

    public async Task<IActionResult> OnPostDeactivateAsync(int id)
    {
        var result = await _userSvc.DeactivateAsync(id);
        TempData["Message"] = result?.Success == true ? $"Đã vô hiệu hoá tài khoản #{id}." : result?.Message;
        return RedirectToPage();
    }

    public async Task<IActionResult> OnPostActivateAsync(int id)
    {
        var result = await _userSvc.ActivateAsync(id);
        TempData["Message"] = result?.Success == true ? $"Đã kích hoạt tài khoản #{id}." : result?.Message;
        return RedirectToPage();
    }
}
