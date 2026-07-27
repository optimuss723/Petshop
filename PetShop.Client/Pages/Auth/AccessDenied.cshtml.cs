using Microsoft.AspNetCore.Mvc.RazorPages;

namespace PetShop.Client.Pages.Auth;

public class AccessDeniedModel : PageModel
{
    public string ReturnUrl { get; private set; } = "/";

    public void OnGet()
    {
        ReturnUrl = HttpContext.Session.GetString("UserRole") switch
        {
            "Admin" => "/Admin/Dashboard",
            "Staff" => "/Staff/Dashboard",
            "Customer" => "/User/Dashboard",
            _ => "/"
        };
    }
}
