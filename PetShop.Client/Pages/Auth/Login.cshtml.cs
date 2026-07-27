using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using PetShop.Client.Services;
using PetShop.Shared.DTOs.Request;

namespace PetShop.Client.Pages.Auth;

public class LoginModel : PageModel
{
    private readonly IAuthClientService _auth;

    [BindProperty] public LoginRequest Input { get; set; } = null!;
    public string? ErrorMessage { get; set; }

    public LoginModel(IAuthClientService auth) => _auth = auth;

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
            ModelState.AddModelError(string.Empty, "Du lieu dang nhap khong hop le.");
            return Page();
        }

        // Clear whitespace
        input = input with
        {
            Email = input.Email?.Trim() ?? string.Empty,
            Password = input.Password ?? string.Empty
        };
        Input = input;

        // Manual validation
        if (string.IsNullOrWhiteSpace(Input?.Email) || !Input.Email.Contains("@"))
        {
            ModelState.AddModelError("Input.Email", "Email không hợp lệ");
        }

        if (string.IsNullOrWhiteSpace(Input?.Password) || Input.Password.Length < 6)
        {
            ModelState.AddModelError("Input.Password", "Mật khẩu phải có ít nhất 6 ký tự");
        }

        if (!ModelState.IsValid)
        {
            return Page();
        }

        var result = await _auth.LoginAsync(input);
        if (result?.Success == true && result.Data != null)
        {
            HttpContext.Session.SetString("AccessToken",  result.Data.Token);
            HttpContext.Session.SetString("RefreshToken", result.Data.RefreshToken);
            HttpContext.Session.SetString("UserRole",     result.Data.User.Role);
            HttpContext.Session.SetString("UserName",     result.Data.User.FullName);
            HttpContext.Session.SetString("UserId",       result.Data.User.Id.ToString());

            return result.Data.User.Role switch
            {
                "Admin" => RedirectToPage("/Admin/Dashboard"),
                "Staff" => RedirectToPage("/Staff/Dashboard"),
                _       => RedirectToPage("/User/Dashboard")
            };
        }

        ErrorMessage = result?.Message ?? "Email hoặc mật khẩu không đúng. Vui lòng thử lại.";
        return Page();
    }
}
