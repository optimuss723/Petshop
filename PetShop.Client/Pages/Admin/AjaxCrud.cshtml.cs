using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace PetShop.Client.Pages.Admin;

public class AjaxCrudModel : PageModel
{
    private readonly IConfiguration _config;

    public string ApiBaseUrl { get; set; } = "http://localhost:5000";
    public string? AccessToken { get; set; }
    public string? UserRole { get; set; }

    public AjaxCrudModel(IConfiguration config)
    {
        _config = config;
    }

    public IActionResult OnGet()
    {
        UserRole = HttpContext.Session.GetString("UserRole");
        AccessToken = HttpContext.Session.GetString("AccessToken");

        // Nếu chưa đăng nhập hoặc không phải Admin/Staff, chuyển hướng đến trang Login
        if (string.IsNullOrEmpty(AccessToken) || (UserRole != "Admin" && UserRole != "Staff"))
        {
            return RedirectToPage("/Auth/Login");
        }

        ApiBaseUrl = _config["ApiSettings:BaseUrl"] ?? "http://localhost:5000";
        return Page();
    }
}

