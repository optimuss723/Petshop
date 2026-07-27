using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using PetShop.Client.Services;
using PetShop.Shared.DTOs.Request;

namespace PetShop.Client.Pages.User;

public class ProfileModel : PageModel
{
    private readonly IUserClientService  _userSvc;
    private readonly IAuthClientService  _authSvc;

    public string? Message      { get; set; }
    public bool    IsSuccess    { get; set; }

    [BindProperty] public string  FullName        { get; set; } = "";
    [BindProperty] public string  Phone           { get; set; } = "";
    [BindProperty] public string? Address         { get; set; }
    [BindProperty] public string  CurrentPassword { get; set; } = "";
    [BindProperty] public string  NewPassword     { get; set; } = "";
    [BindProperty] public string  ConfirmPassword { get; set; } = "";

    public ProfileModel(IUserClientService userSvc, IAuthClientService authSvc)
    {
        _userSvc = userSvc;
        _authSvc = authSvc;
    }

    public async Task OnGetAsync()
    {
        Message = TempData["Message"]?.ToString();
        IsSuccess = TempData["IsSuccess"]?.ToString() == "true";

        // Load từ session trước, sau đó load từ API
        FullName = HttpContext.Session.GetString("UserName") ?? "";
        Phone    = HttpContext.Session.GetString("UserPhone") ?? "";
        Address  = HttpContext.Session.GetString("UserAddress");

        // Gọi API lấy thông tin mới nhất
        var result = await _userSvc.GetMeAsync();
        if (result?.Data != null)
        {
            FullName = result.Data.FullName;
            Phone    = result.Data.Phone;
            Address  = result.Data.Address;
        }
    }

    public async Task<IActionResult> OnPostUpdateProfileAsync()
    {
        var userId = HttpContext.Session.GetString("UserId");
        if (string.IsNullOrEmpty(userId)) return RedirectToPage("/Auth/Login");

        var req    = new UserUpdateRequest(FullName, Phone, Address);
        var result = await _userSvc.UpdateAsync(int.Parse(userId), req);

        if (result?.Success == true)
        {
            // Cập nhật session
            HttpContext.Session.SetString("UserName",    FullName);
            HttpContext.Session.SetString("UserPhone",   Phone);
            HttpContext.Session.SetString("UserAddress", Address ?? "");
            TempData["IsSuccess"] = "true";
            TempData["Message"]   = "Cập nhật thông tin thành công.";
        }
        else
        {
            TempData["IsSuccess"] = "false";
            TempData["Message"]   = result?.Message ?? "Cập nhật thất bại.";
        }
        return RedirectToPage();
    }

    public async Task<IActionResult> OnPostChangePasswordAsync()
    {
        if (NewPassword != ConfirmPassword)
        {
            TempData["Message"]   = "Mật khẩu mới không khớp với xác nhận.";
            TempData["IsSuccess"] = "false";
            return RedirectToPage();
        }

        var req    = new ChangePasswordRequest(CurrentPassword, NewPassword);
        var result = await _authSvc.ChangePasswordAsync(req);

        TempData["IsSuccess"] = result?.Success == true ? "true" : "false";
        TempData["Message"]   = result?.Message ?? (result?.Success == true ? "Đổi mật khẩu thành công." : "Đổi mật khẩu thất bại.");
        return RedirectToPage();
    }
}
