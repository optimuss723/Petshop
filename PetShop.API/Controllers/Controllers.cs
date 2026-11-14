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
    public async Task<IActionResult> Login([FromBody] LoginRequest request)
        => Ok(await _auth.LoginAsync(request));

    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] RegisterRequest request)
        => Ok(await _auth.RegisterAsync(request));

    [HttpPost("refresh")]
    public async Task<IActionResult> Refresh([FromBody] RefreshRequest req)
        => Ok(await _auth.RefreshTokenAsync(req.AccessToken, req.RefreshToken));
}

public record RefreshRequest(string AccessToken, string RefreshToken);

// ── PRODUCTS (Thành viên 1 phụ trách) ────────────────────────────
[ApiController]
[Route("api/products")]
public class ProductsController : ControllerBase
{
    private readonly IProductService _svc;
    public ProductsController(IProductService svc) => _svc = svc;

    [HttpGet]
    [Produces("application/json", "application/xml", "text/csv")]
    public async Task<IActionResult> GetAll() => Ok(await _svc.GetAllAsync());

    [HttpGet("search")]
    [Produces("application/json", "application/xml", "text/csv")]
    public async Task<IActionResult> Search(
        [FromQuery] string? search = null,
        [FromQuery] string? category = null,
        [FromQuery] decimal? minPrice = null,
        [FromQuery] decimal? maxPrice = null,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 8)
    {
        var result = await _svc.SearchAsync(search, category, minPrice, maxPrice, page, pageSize);
        return Ok(result);
    }

    [HttpGet("{id:int}")]
    [Produces("application/json", "application/xml", "text/csv")]
    public async Task<IActionResult> GetById(int id)
    {
        var result = await _svc.GetByIdAsync(id);
        if (!result.Success || result.Data == null) return NotFound(result);
        return Ok(result);
    }

    [HttpPost]
    [Authorize(Roles = Roles.AdminOrStaff)]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Create([FromBody] ProductRequest request)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);
        var result = await _svc.CreateAsync(request);
        if (!result.Success || result.Data == null) return BadRequest(result);
        return CreatedAtAction(nameof(GetById), new { id = result.Data.Id }, result);
    }

    [HttpPut("{id:int}")]
    [Authorize(Roles = Roles.AdminOrStaff)]
    public async Task<IActionResult> Update(int id, [FromBody] ProductRequest request)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);
        var result = await _svc.UpdateAsync(id, request);
        if (!result.Success) return BadRequest(result);
        return Ok(result);
    }

    [HttpDelete("{id:int}")]
    [Authorize(Roles = Roles.AdminOrStaff)]
    public async Task<IActionResult> Delete(int id)
    {
        var result = await _svc.DeleteAsync(id);
        if (!result.Success) return BadRequest(result);
        return Ok(result);
    }
}

// ── PETS (Thành viên 2 phụ trách) ────────────────────────────────
[ApiController]
[Route("api/pets")]
public class PetsController : ControllerBase
{
    private readonly IPetService _svc;
    public PetsController(IPetService svc) => _svc = svc;

    [HttpGet]
    [Produces("application/json", "application/xml", "text/csv")]
    public async Task<IActionResult> GetAll() => Ok(await _svc.GetAllAsync());

    [HttpGet("search")]
    [Produces("application/json", "application/xml", "text/csv")]
    public async Task<IActionResult> Search(
        [FromQuery] string? search = null,
        [FromQuery] string? category = null,
        [FromQuery] decimal? minPrice = null,
        [FromQuery] decimal? maxPrice = null,
        [FromQuery] string? status = null,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 8)
    {
        var result = await _svc.SearchAsync(search, category, minPrice, maxPrice, status, page, pageSize);
        return Ok(result);
    }

    [HttpGet("{id:int}")]
    [Produces("application/json", "application/xml", "text/csv")]
    public async Task<IActionResult> GetById(int id)
    {
        var result = await _svc.GetByIdAsync(id);
        if (!result.Success || result.Data == null) return NotFound(result);
        return Ok(result);
    }

    [HttpPost]
    [Authorize(Roles = Roles.AdminOrStaff)]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Create([FromBody] PetRequest request)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);
        var result = await _svc.CreateAsync(request);
        if (!result.Success || result.Data == null) return BadRequest(result);
        return CreatedAtAction(nameof(GetById), new { id = result.Data.Id }, result);
    }

    [HttpPut("{id:int}")]
    [Authorize(Roles = Roles.AdminOrStaff)]
    public async Task<IActionResult> Update(int id, [FromBody] PetRequest request)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);
        var result = await _svc.UpdateAsync(id, request);
        if (!result.Success) return BadRequest(result);
        return Ok(result);
    }

    [HttpDelete("{id:int}")]
    [Authorize(Roles = Roles.AdminOrStaff)]
    public async Task<IActionResult> Delete(int id)
    {
        var result = await _svc.DeleteAsync(id);
        if (!result.Success) return BadRequest(result);
        return Ok(result);
    }
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
    [Produces("application/json", "application/xml", "text/csv")]
    public async Task<IActionResult> GetAll([FromQuery] string? status = null)
    {
        if (Role == Roles.Admin || Role == Roles.Staff)
            return Ok(await _svc.GetAllAsync(status));
        return Ok(await _svc.GetByUserIdAsync(UserId, status));
    }

    [HttpGet("my")]
    [Produces("application/json", "application/xml", "text/csv")]
    public async Task<IActionResult> GetMine([FromQuery] string? status = null) 
        => Ok(await _svc.GetByUserIdAsync(UserId, status));

    [HttpGet("{id}")]
    [Produces("application/json", "application/xml", "text/csv")]
    public async Task<IActionResult> GetById(int id) => Ok(await _svc.GetByIdAsync(id));

    [HttpPost]
    [Authorize(Roles = Roles.Customer)]
    public async Task<IActionResult> Create([FromBody] OrderRequest request) => Ok(await _svc.CreateAsync(UserId, request));

    [HttpPatch("{id}/status")]
    [Authorize(Roles = Roles.AdminOrStaff)]
    public async Task<IActionResult> UpdateStatus(int id, [FromBody] UpdateOrderStatusRequest req)
    {
        if (!Enum.TryParse<OrderStatus>(req.Status, ignoreCase: true, out var parsedStatus))
            return BadRequest(new { success = false, message = $"Trạng thái '{req.Status}' không hợp lệ." });
        return Ok(await _svc.UpdateStatusAsync(id, parsedStatus, UserId, Role));
    }

    [HttpPost("{id}/cancel")]
    public async Task<IActionResult> Cancel(int id) => Ok(await _svc.CancelAsync(id, UserId, Role));

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id) => Ok(await _svc.CancelAsync(id, UserId, Role));

    [HttpPost("{id}/ship")]
    [Authorize(Roles = Roles.AdminOrStaff)]
    public async Task<IActionResult> CreateShipment(int id, [FromBody] ShipmentRequest? req = null)
        => Ok(await _svc.CreateShipmentAsync(id, req?.Note));

    [HttpPost("{id}/confirm-received")]
    [Authorize(Roles = Roles.Customer)]
    public async Task<IActionResult> ConfirmReceived(int id)
        => Ok(await _svc.UpdateStatusAsync(id, OrderStatus.Delivered, UserId, Role));
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

// ── ODATA CONTROLLERS (Hỗ trợ truy vấn $filter, $select, $orderby, $top) ──────
[Route("odata/Products")]
public class ProductsODataController : ODataController
{
    private readonly PetShopDbContext _db;
    public ProductsODataController(PetShopDbContext db) => _db = db;

    [HttpGet]
    [EnableQuery(MaxTop = 100, AllowedQueryOptions = AllowedQueryOptions.All)]
    public IQueryable<Repository.Entities.Product> Get() => _db.Products.Where(p => p.IsActive);
}

[Route("odata/Pets")]
public class PetsODataController : ODataController
{
    private readonly PetShopDbContext _db;
    public PetsODataController(PetShopDbContext db) => _db = db;

    [HttpGet]
    [EnableQuery(MaxTop = 100, AllowedQueryOptions = AllowedQueryOptions.All)]
    public IQueryable<Repository.Entities.Pet> Get() => _db.Pets;
}

[Route("odata/Orders")]
public class OrdersODataController : ODataController
{
    private readonly PetShopDbContext _db;
    public OrdersODataController(PetShopDbContext db) => _db = db;

    [HttpGet]
    [Authorize(Roles = Roles.AdminOrStaff)]
    [EnableQuery(MaxTop = 100, AllowedQueryOptions = AllowedQueryOptions.All)]
    public IQueryable<Repository.Entities.Order> Get() => _db.Orders;
}
