using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using PetShop.Client.Services;
using PetShop.Shared.DTOs.Request;

namespace PetShop.Client.Pages.Auth;

public class RegisterModel : PageModel
{
    private readonly IAuthClientService _auth;

    [BindProperty] public RegisterRequest Input { get; set; } = null!;
    public string? ErrorMessage { get; set; }

    public RegisterModel(IAuthClientService auth) => _auth = auth;

    public IActionResult OnGet()
    {
        if (!string.IsNullOrEmpty(HttpContext.Session.GetString("AccessToken")))
            return RedirectToPage("/Index");
        return Page();
    }

    public async Task<IActionResult> OnPostAsync()
    {
        var input = Input;
        if (input is null)
        {
            ModelState.AddModelError(string.Empty, "Du lieu dang ky khong hop le.");
            return Page();
        }

        // Clear whitespace
        input = input with
        {
            FullName = input.FullName?.Trim() ?? string.Empty,
            Email = input.Email?.Trim() ?? string.Empty,
            Password = input.Password ?? string.Empty,
            Phone = input.Phone?.Trim() ?? string.Empty,
            Address = input.Address?.Trim()
        };
        Input = input;

        // Manual validation
        if (string.IsNullOrWhiteSpace(Input?.FullName) || Input.FullName.Length < 2)
        {
            ModelState.AddModelError("Input.FullName", "Họ tên phải có ít nhất 2 ký tự");
        }

        if (string.IsNullOrWhiteSpace(Input?.Email) || !Input.Email.Contains("@"))
        {
            ModelState.AddModelError("Input.Email", "Email không hợp lệ");
        }

        if (string.IsNullOrWhiteSpace(Input?.Password) || Input.Password.Length < 6)
        {
            ModelState.AddModelError("Input.Password", "Mật khẩu phải có ít nhất 6 ký tự");
        }

        if (string.IsNullOrWhiteSpace(Input?.Phone))
        {
            ModelState.AddModelError("Input.Phone", "Số điện thoại không được để trống");
        }
        else if (!System.Text.RegularExpressions.Regex.IsMatch(Input.Phone, @"^(0|\+84)[0-9]{9,10}$"))
        {
            ModelState.AddModelError("Input.Phone", "Số điện thoại không hợp lệ (VD: 0987654321)");
        }

        if (!ModelState.IsValid)
        {
            return Page();
        }

        var result = await _auth.RegisterAsync(input);
        if (result?.Success == true && result.Data != null)
        {
            HttpContext.Session.SetString("AccessToken",  result.Data.Token);
            HttpContext.Session.SetString("RefreshToken", result.Data.RefreshToken);
            HttpContext.Session.SetString("UserRole",     result.Data.User.Role);
            HttpContext.Session.SetString("UserName",     result.Data.User.FullName);
            HttpContext.Session.SetString("UserId",       result.Data.User.Id.ToString());
            return RedirectToPage("/User/Dashboard");
        }

        ErrorMessage = result?.Message ?? "Đăng ký thất bại. Vui lòng thử lại.";
        return Page();
    }
}
