using Microsoft.AspNetCore.Mvc.RazorPages;
using PetShop.Client.Services;
using PetShop.Shared.DTOs.Response;

namespace PetShop.Client.Pages.Admin;

public class DashboardModel : PageModel
{
    private readonly IOrderClientService   _orderSvc;
    private readonly IProductClientService _productSvc;
    private readonly IPetClientService     _petSvc;
    private readonly IUserClientService    _userSvc;

    public List<OrderResponse> RecentOrders   { get; set; } = [];
    public int     TotalProducts   { get; set; }
    public int     TotalPets       { get; set; }
    public int     PendingOrders   { get; set; }
    public decimal TodayRevenue    { get; set; }

    // Stats người dùng
    public int TotalUsers     { get; set; }
    public int TotalStaff     { get; set; }
    public int TotalCustomers { get; set; }

    public string UserName => HttpContext.Session.GetString("UserName") ?? "Admin";

    public DashboardModel(IOrderClientService o, IProductClientService p, IPetClientService pet, IUserClientService u)
    {
        _orderSvc = o; _productSvc = p; _petSvc = pet; _userSvc = u;
    }

    public async Task OnGetAsync()
    {
        var ordersTask   = _orderSvc.GetAllAsync();
        var productsTask = _productSvc.GetAllAsync();
        var petsTask     = _petSvc.GetAllAsync();
        var usersTask    = _userSvc.GetAllAsync();

        await Task.WhenAll(ordersTask, productsTask, petsTask, usersTask);

        var productList = productsTask.Result?.Data ?? [];
        var orderList   = ordersTask.Result?.Data   ?? [];
        var userList    = usersTask.Result?.Data     ?? [];

        RecentOrders  = orderList.OrderByDescending(o => o.CreatedAt).Take(10).ToList();
        TotalProducts = productList.Count;
        TotalPets     = petsTask.Result?.Data?.Count ?? 0;
        PendingOrders = orderList.Count(o => o.Status == "Pending");
        TodayRevenue  = orderList
            .Where(o => o.CreatedAt.Date == DateTime.Today && o.Status != "Cancelled")
            .Sum(o => o.TotalAmount);

        TotalUsers     = userList.Count;
        TotalStaff     = userList.Count(u => u.Role == "Staff");
        TotalCustomers = userList.Count(u => u.Role == "Customer");
    }
}
