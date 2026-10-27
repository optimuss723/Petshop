namespace PetShop.Client.Middleware;

public class AuthMiddleware
{
    private readonly RequestDelegate _next;

    private static readonly string[] _publicPaths =
        ["/Auth/Login", "/Auth/Register", "/", "/Index",
         "/Cart", "/Products/Index", "/Pets/Index"];

    // Prefix → roles được phép truy cập
    private static readonly Dictionary<string, string[]> _roleRequired = new()
    {
        { "/Admin", ["Admin"] },                     // Admin only
        { "/Staff", ["Admin", "Staff"] },            // Staff + Admin
        { "/User",  ["Admin","Staff","Customer"] }
    };

    public AuthMiddleware(RequestDelegate next) => _next = next;

    public async Task InvokeAsync(HttpContext context)
    {
        var path  = context.Request.Path.Value ?? "/";
        var token = context.Session.GetString("AccessToken");
        var role  = context.Session.GetString("UserRole");

        var isPublic = _publicPaths.Any(p =>
            path.Equals(p, StringComparison.OrdinalIgnoreCase) ||
            path.StartsWith("/Auth",     StringComparison.OrdinalIgnoreCase) ||
            path.StartsWith("/Products", StringComparison.OrdinalIgnoreCase) ||
            path.StartsWith("/Pets",     StringComparison.OrdinalIgnoreCase));

        if (string.IsNullOrEmpty(token) && !isPublic)
        {
            context.Response.Redirect("/Auth/Login");
            return;
        }

        foreach (var (prefix, roles) in _roleRequired)
        {
            if (path.StartsWith(prefix, StringComparison.OrdinalIgnoreCase) &&
                !roles.Contains(role))
            {
                context.Response.Redirect("/Auth/AccessDenied");
                return;
            }
        }

        await _next(context);
    }
}
