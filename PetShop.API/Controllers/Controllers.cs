using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OData.Query;
using Microsoft.AspNetCore.OData.Routing.Controllers;
using PetShop.API.Services;
using PetShop.Repository.Data;
using PetShop.Shared.Constants;
using PetShop.Shared.DTOs.Request;
using PetShop.Shared.Enums;

namespace PetShop.API.Controllers;

// ── AUTH ─────────────────────────────────────────────────────────
[ApiController]
[Route("api/auth")]
public class AuthController : ControllerBase
{
    private readonly IAuthService _auth;
    public AuthController(IAuthService auth) => _auth = auth;

    [HttpPost("login")]
    public async Task<IActionResult> Login(LoginRequest request)
        => Ok(await _auth.LoginAsync(request));

    [HttpPost("register")]
    public async Task<IActionResult> Register(RegisterRequest request)
        => Ok(await _auth.RegisterAsync(request));

    [HttpPost("refresh")]
    public async Task<IActionResult> Refresh([FromBody] RefreshRequest req)
        => Ok(await _auth.RefreshTokenAsync(req.AccessToken, req.RefreshToken));
}

public record RefreshRequest(string AccessToken, string RefreshToken);

// ── PRODUCTS ─────────────────────────────────────────────────────
[ApiController]
[Route("api/products")]
public class ProductsController : ControllerBase
{
    private readonly IProductService _svc;
    public ProductsController(IProductService svc) => _svc = svc;

    [HttpGet]
    public async Task<IActionResult> GetAll() => Ok(await _svc.GetAllAsync());

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id) => Ok(await _svc.GetByIdAsync(id));

    [HttpPost]
    [Authorize(Roles = Roles.Staff)]
    public async Task<IActionResult> Create([FromBody] ProductRequest request) => Ok(await _svc.CreateAsync(request));

    [HttpPut("{id}")]
    [Authorize(Roles = Roles.Staff)]
    public async Task<IActionResult> Update(int id, [FromBody] ProductRequest request) => Ok(await _svc.UpdateAsync(id, request));

    [HttpDelete("{id}")]
    [Authorize(Roles = Roles.Staff)]
    public async Task<IActionResult> Delete(int id) => Ok(await _svc.DeleteAsync(id));
}

// ── PETS ─────────────────────────────────────────────────────────
[ApiController]
[Route("api/pets")]
public class PetsController : ControllerBase
{
    private readonly IPetService _svc;
    public PetsController(IPetService svc) => _svc = svc;

    [HttpGet]
    public async Task<IActionResult> GetAll() => Ok(await _svc.GetAllAsync());

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id) => Ok(await _svc.GetByIdAsync(id));

    [HttpPost]
    [Authorize(Roles = Roles.Staff)]
    public async Task<IActionResult> Create([FromBody] PetRequest request) => Ok(await _svc.CreateAsync(request));

    [HttpPut("{id}")]
    [Authorize(Roles = Roles.Staff)]
    public async Task<IActionResult> Update(int id, [FromBody] PetRequest request) => Ok(await _svc.UpdateAsync(id, request));

    [HttpDelete("{id}")]
    [Authorize(Roles = Roles.Staff)]
    public async Task<IActionResult> Delete(int id) => Ok(await _svc.DeleteAsync(id));
}

// ── ORDERS ───────────────────────────────────────────────────────
[ApiController]
[Route("api/orders")]
[Authorize]
public class OrdersController : ControllerBase
{
    private readonly IOrderService _svc;
    public OrdersController(IOrderService svc) => _svc = svc;

    private int UserId  => int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);
    private string Role => User.FindFirst(ClaimTypes.Role)!.Value;

    [HttpGet]
    [Authorize(Roles = Roles.AdminOrStaff)]
    public async Task<IActionResult> GetAll() => Ok(await _svc.GetAllAsync());

    [HttpGet("my")]
    public async Task<IActionResult> GetMine() => Ok(await _svc.GetByUserIdAsync(UserId));

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id) => Ok(await _svc.GetByIdAsync(id));

    [HttpPost]
    [Authorize(Roles = Roles.Customer)]
    public async Task<IActionResult> Create(OrderRequest request) => Ok(await _svc.CreateAsync(UserId, request));

    [HttpPatch("{id}/status")]
    [Authorize(Roles = Roles.AdminOrStaff)]
    public async Task<IActionResult> UpdateStatus(int id, [FromBody] UpdateOrderStatusRequest req)
    {
        if (!Enum.TryParse<OrderStatus>(req.Status, ignoreCase: true, out var parsedStatus))
            return BadRequest(new { success = false, message = $"Trạng thái '{req.Status}' không hợp lệ." });
        return Ok(await _svc.UpdateStatusAsync(id, parsedStatus, UserId, Role));
    }

    [HttpPost("{id}/cancel")]
    [Authorize(Roles = Roles.Customer)]
    public async Task<IActionResult> Cancel(int id) => Ok(await _svc.CancelAsync(id, UserId));
}

// ── USERS ────────────────────────────────────────────────────────
[ApiController]
[Route("api/users")]
[Authorize]
public class UsersController : ControllerBase
{
    private readonly IUserService _svc;
    public UsersController(IUserService svc) => _svc = svc;

    private int UserId => int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);

    [HttpGet]
    [Authorize(Roles = Roles.Admin)]
    public async Task<IActionResult> GetAll() => Ok(await _svc.GetAllAsync());

    [HttpGet("{id}")]
    [Authorize(Roles = Roles.Admin)]
    public async Task<IActionResult> GetById(int id) => Ok(await _svc.GetByIdAsync(id));

    [HttpGet("me")]
    public async Task<IActionResult> GetMe() => Ok(await _svc.GetByIdAsync(UserId));

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(int id, UserUpdateRequest request)
    {
        if (Role != Roles.Admin && UserId != id) return Forbid();
        return Ok(await _svc.UpdateAsync(id, request));
    }

    [HttpDelete("{id}")]
    [Authorize(Roles = Roles.Admin)]
    public async Task<IActionResult> Deactivate(int id) => Ok(await _svc.DeactivateAsync(id));

    private string Role => User.FindFirst(ClaimTypes.Role)!.Value;
}

// ── ODATA CONTROLLERS ────────────────────────────────────────────
public class ProductsODataController : ODataController
{
    private readonly PetShopDbContext _db;
    public ProductsODataController(PetShopDbContext db) => _db = db;

    [HttpGet("odata/Products")]
    [EnableQuery(MaxTop = 100, AllowedQueryOptions = AllowedQueryOptions.All)]
    public IQueryable<Repository.Entities.Product> Get() => _db.Products.Where(p => p.IsActive);
}

public class PetsODataController : ODataController
{
    private readonly PetShopDbContext _db;
    public PetsODataController(PetShopDbContext db) => _db = db;

    [HttpGet("odata/Pets")]
    [EnableQuery(MaxTop = 100, AllowedQueryOptions = AllowedQueryOptions.All)]
    public IQueryable<Repository.Entities.Pet> Get() => _db.Pets;
}

public class OrdersODataController : ODataController
{
    private readonly PetShopDbContext _db;
    public OrdersODataController(PetShopDbContext db) => _db = db;

    [HttpGet("odata/Orders")]
    [Authorize(Roles = Roles.AdminOrStaff)]
    [EnableQuery(MaxTop = 100, AllowedQueryOptions = AllowedQueryOptions.All)]
    public IQueryable<Repository.Entities.Order> Get() => _db.Orders;
}
