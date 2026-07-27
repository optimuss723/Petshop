using System.Security.Claims;
using Catalog.Service.DTOs;
using Catalog.Service.Data;
using Catalog.Service.Entities;
using Catalog.Service.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OData.Query;
using Microsoft.AspNetCore.OData.Routing.Controllers;

namespace Catalog.Service.Controllers;

// ── PRODUCTS CONTROLLER ───────────────────────────────────────────────

/// <summary>
/// Quản lý danh mục sản phẩm (thức ăn, phụ kiện, thuốc thú cưng).
/// Public GET — cần xác thực cho POST/PUT/DELETE.
/// </summary>
[ApiController]
[Route("api/products")]
public class ProductsController : ControllerBase
{
    private readonly IProductService _svc;
    public ProductsController(IProductService svc) => _svc = svc;

    /// <summary>Lấy tất cả sản phẩm đang hoạt động</summary>
    [HttpGet]
    public async Task<IActionResult> GetAll() => Ok(await _svc.GetAllAsync());

    /// <summary>Tìm kiếm + lọc + phân trang sản phẩm (#4 #13)</summary>
    [HttpGet("search")]
    public async Task<IActionResult> Search(
        [FromQuery] string?  search   = null,
        [FromQuery] string?  category = null,
        [FromQuery] decimal? minPrice = null,
        [FromQuery] decimal? maxPrice = null,
        [FromQuery] int      page     = 1,
        [FromQuery] int      pageSize = 9)
    {
        var p = new ProductSearchParams(search, category, minPrice, maxPrice, page, pageSize);
        return Ok(await _svc.SearchAsync(p));
    }

    /// <summary>Lấy sản phẩm theo ID</summary>
    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetById(int id) => Ok(await _svc.GetByIdAsync(id));

    /// <summary>Tạo sản phẩm mới — yêu cầu Staff</summary>
    [HttpPost]
    [Authorize(Roles = "Staff")]
    public async Task<IActionResult> Create([FromBody] ProductCreateRequest request)
        => Ok(await _svc.CreateAsync(request));

    /// <summary>Cập nhật sản phẩm — yêu cầu Staff</summary>
    [HttpPut("{id:int}")]
    [Authorize(Roles = "Staff")]
    public async Task<IActionResult> Update(int id, [FromBody] ProductUpdateRequest request)
        => Ok(await _svc.UpdateAsync(id, request));

    /// <summary>Xóa mềm sản phẩm — yêu cầu Staff</summary>
    [HttpDelete("{id:int}")]
    [Authorize(Roles = "Staff")]
    public async Task<IActionResult> Delete(int id) => Ok(await _svc.DeleteAsync(id));
}

// ── PETS CONTROLLER ───────────────────────────────────────────────

/// <summary>
/// Quản lý danh mục thú cưng bán trong cửa hàng.
/// Public GET — cần xác thực cho POST/PUT/DELETE.
/// </summary>
[ApiController]
[Route("api/pets")]
public class PetsController : ControllerBase
{
    private readonly IPetService _svc;
    public PetsController(IPetService svc) => _svc = svc;

    /// <summary>Lấy tất cả thú cưng</summary>
    [HttpGet]
    public async Task<IActionResult> GetAll() => Ok(await _svc.GetAllAsync());

    /// <summary>Tìm kiếm + lọc + phân trang thú cưng (#4 #13)</summary>
    [HttpGet("search")]
    public async Task<IActionResult> Search(
        [FromQuery] string?  search   = null,
        [FromQuery] string?  category = null,
        [FromQuery] decimal? minPrice = null,
        [FromQuery] decimal? maxPrice = null,
        [FromQuery] string?  status   = "Available",
        [FromQuery] int      page     = 1,
        [FromQuery] int      pageSize = 9)
    {
        var p = new PetSearchParams(search, category, minPrice, maxPrice, status, page, pageSize);
        return Ok(await _svc.SearchAsync(p));
    }

    /// <summary>Lấy thú cưng theo ID</summary>
    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetById(int id) => Ok(await _svc.GetByIdAsync(id));

    /// <summary>Thêm thú cưng mới — yêu cầu Staff</summary>
    [HttpPost]
    [Authorize(Roles = "Staff")]
    public async Task<IActionResult> Create([FromBody] PetCreateRequest request)
        => Ok(await _svc.CreateAsync(request));

    /// <summary>Cập nhật thú cưng — yêu cầu Staff</summary>
    [HttpPut("{id:int}")]
    [Authorize(Roles = "Staff")]
    public async Task<IActionResult> Update(int id, [FromBody] PetUpdateRequest request)
        => Ok(await _svc.UpdateAsync(id, request));

    /// <summary>Xóa thú cưng — yêu cầu Staff</summary>
    [HttpDelete("{id:int}")]
    [Authorize(Roles = "Staff")]
    public async Task<IActionResult> Delete(int id) => Ok(await _svc.DeleteAsync(id));
}

// ── REVIEWS CONTROLLER  (#14) ─────────────────────────────────────

/// <summary>
/// Đánh giá sản phẩm/thú cưng. GET là public, POST cần xác thực Customer.
/// </summary>
[ApiController]
[Route("api/reviews")]
public class ReviewsController : ControllerBase
{
    private readonly IReviewService _svc;
    public ReviewsController(IReviewService svc) => _svc = svc;

    private int    UserId   => int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);
    private string UserName => User.FindFirst(ClaimTypes.Name)?.Value ?? "Khách";
    private string Role     => User.FindFirst(ClaimTypes.Role)?.Value ?? "";

    /// <summary>Lấy tất cả review của sản phẩm</summary>
    [HttpGet("product/{productId:int}")]
    public async Task<IActionResult> GetByProduct(int productId)
        => Ok(await _svc.GetByProductAsync(productId));

    /// <summary>Lấy tất cả review của thú cưng</summary>
    [HttpGet("pet/{petId:int}")]
    public async Task<IActionResult> GetByPet(int petId)
        => Ok(await _svc.GetByPetAsync(petId));

    /// <summary>Gửi đánh giá mới — cần đăng nhập</summary>
    [HttpPost]
    [Authorize]
    public async Task<IActionResult> Create([FromBody] ReviewCreateRequest request)
        => Ok(await _svc.CreateAsync(UserId, UserName, request));

    /// <summary>Xóa đánh giá — Admin hoặc chủ review</summary>
    [HttpDelete("{id:int}")]
    [Authorize]
    public async Task<IActionResult> Delete(int id)
        => Ok(await _svc.DeleteAsync(id, UserId, Role));
}

// ── CATEGORIES CONTROLLER  (#10) ──────────────────────────────────

/// <summary>
/// CRUD danh mục sản phẩm/thú cưng. GET là public, write cần Admin/Shipper.
/// </summary>
[ApiController]
[Route("api/categories")]
public class CategoriesController : ControllerBase
{
    private readonly ICategoryService _svc;
    public CategoriesController(ICategoryService svc) => _svc = svc;

    /// <summary>Lấy tất cả danh mục (có thể lọc theo type=Product|Pet)</summary>
    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] string? type = null)
        => Ok(await _svc.GetAllAsync(type));

    /// <summary>Lấy danh mục theo ID</summary>
    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetById(int id)
        => Ok(await _svc.GetByIdAsync(id));

    /// <summary>Tạo danh mục mới — Admin/Staff</summary>
    [HttpPost]
    [Authorize(Roles = "Admin,Staff")]
    public async Task<IActionResult> Create([FromBody] CategoryCreateRequest request)
        => Ok(await _svc.CreateAsync(request));

    /// <summary>Cập nhật danh mục — Admin/Staff</summary>
    [HttpPut("{id:int}")]
    [Authorize(Roles = "Admin,Staff")]
    public async Task<IActionResult> Update(int id, [FromBody] CategoryCreateRequest request)
        => Ok(await _svc.UpdateAsync(id, request));

    /// <summary>Xóa danh mục — chỉ Admin</summary>
    [HttpDelete("{id:int}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Delete(int id)
        => Ok(await _svc.DeleteAsync(id));
}

[ApiController]
[Route("api/internal/catalog")]
public class InternalCatalogController : ControllerBase
{
    private readonly IInventoryService _inventory;
    private readonly IConfiguration _config;

    public InternalCatalogController(IInventoryService inventory, IConfiguration config)
    {
        _inventory = inventory;
        _config = config;
    }

    [HttpPost("inventory/commit")]
    public async Task<IActionResult> CommitInventory([FromBody] InventoryCommitRequest request)
    {
        if (!IsInternalRequest())
            return Unauthorized();

        return Ok(await _inventory.CommitOrderAsync(request));
    }

    [HttpPost("inventory/release")]
    public async Task<IActionResult> ReleaseInventory([FromBody] InventoryCommitRequest request)
    {
        if (!IsInternalRequest())
            return Unauthorized();

        return Ok(await _inventory.ReleaseOrderAsync(request));
    }

    private bool IsInternalRequest()
    {
        var configuredKey = _config["InternalApiKey"] ?? "dev-internal-key";
        return Request.Headers.TryGetValue("X-Internal-Key", out var key)
            && key.ToString() == configuredKey;
    }
}

// ── ODATA CONTROLLERS ─────────────────────────────────────────────

public class ProductsODataController : ODataController
{
    private readonly CatalogDbContext _db;
    public ProductsODataController(CatalogDbContext db) => _db = db;

    [EnableQuery(MaxTop = 100, AllowedQueryOptions = AllowedQueryOptions.All)]
    public IQueryable<Product> Get()
        => _db.Products.Where(p => p.IsActive);
}

public class PetsODataController : ODataController
{
    private readonly CatalogDbContext _db;
    public PetsODataController(CatalogDbContext db) => _db = db;

    [EnableQuery(MaxTop = 100, AllowedQueryOptions = AllowedQueryOptions.All)]
    public IQueryable<Pet> Get()
        => _db.Pets;
}
