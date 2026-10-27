using Catalog.Service.Data;
using Catalog.Service.DTOs;
using Catalog.Service.Entities;
using Catalog.Service.Hubs;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using System.Net.Http.Json;

namespace Catalog.Service.Services;

// ── PRODUCT SERVICE ───────────────────────────────────────────────

public interface IProductService
{
    Task<ApiResponse<List<ProductResponse>>>         GetAllAsync();
    Task<ApiResponse<PagedResponse<ProductResponse>>> SearchAsync(ProductSearchParams p);
    Task<ApiResponse<ProductResponse>>               GetByIdAsync(int id);
    Task<ApiResponse<ProductResponse>>               CreateAsync(ProductCreateRequest request);
    Task<ApiResponse<ProductResponse>>               UpdateAsync(int id, ProductUpdateRequest request);
    Task<ApiResponse<bool>>                          DeleteAsync(int id);
}

public class ProductService : IProductService
{
    private readonly CatalogDbContext         _db;
    private readonly IHubContext<CatalogHub>  _hub;

    public ProductService(CatalogDbContext db, IHubContext<CatalogHub> hub)
    {
        _db  = db;
        _hub = hub;
    }

    public async Task<ApiResponse<List<ProductResponse>>> GetAllAsync()
    {
        var items = await _db.Products.Where(p => p.IsActive).ToListAsync();
        return Ok(items.Select(ToResponse).ToList());
    }

    /// <summary>Tìm kiếm + lọc + phân trang sản phẩm (#4 #13)</summary>
    public async Task<ApiResponse<PagedResponse<ProductResponse>>> SearchAsync(ProductSearchParams p)
    {
        var query = _db.Products.Where(x => x.IsActive).AsQueryable();

        if (!string.IsNullOrWhiteSpace(p.Search))
            query = query.Where(x => x.Name.Contains(p.Search) || x.Description.Contains(p.Search));

        if (!string.IsNullOrWhiteSpace(p.Category))
            query = query.Where(x => x.Category == p.Category);

        if (p.MinPrice.HasValue)
            query = query.Where(x => x.Price >= p.MinPrice.Value);

        if (p.MaxPrice.HasValue)
            query = query.Where(x => x.Price <= p.MaxPrice.Value);

        var total = await query.CountAsync();
        var items = await query
            .OrderByDescending(x => x.CreatedAt)
            .Skip((p.Page - 1) * p.PageSize)
            .Take(p.PageSize)
            .ToListAsync();

        return Ok(new PagedResponse<ProductResponse>(items.Select(ToResponse).ToList(), total, p.Page, p.PageSize));
    }

    public async Task<ApiResponse<ProductResponse>> GetByIdAsync(int id)
    {
        var item = await _db.Products.FindAsync(id);
        if (item == null || !item.IsActive)
            return Fail<ProductResponse>("Sản phẩm không tồn tại hoặc đã bị xóa.");
        return Ok(ToResponse(item));
    }

    public async Task<ApiResponse<ProductResponse>> CreateAsync(ProductCreateRequest request)
    {
        var product = new Product
        {
            Name        = request.Name,
            Description = request.Description,
            Price       = request.Price,
            Stock       = request.Stock,
            Category    = request.Category,
            ImageUrl    = request.ImageUrl
        };
        _db.Products.Add(product);
        await _db.SaveChangesAsync();

        var resp = ToResponse(product);
        await _hub.Clients.Group("Customers").SendAsync("NewProductAdded", resp);

        return new ApiResponse<ProductResponse>(true, "Tạo sản phẩm thành công.", resp);
    }

    public async Task<ApiResponse<ProductResponse>> UpdateAsync(int id, ProductUpdateRequest request)
    {
        var product = await _db.Products.FindAsync(id);
        if (product == null) return Fail<ProductResponse>("Sản phẩm không tồn tại.");

        var oldStock = product.Stock;
        product.Name        = request.Name;
        product.Description = request.Description;
        product.Price       = request.Price;
        product.Stock       = request.Stock;
        product.Category    = request.Category;
        product.ImageUrl    = request.ImageUrl;
        await _db.SaveChangesAsync();

        if (oldStock != request.Stock)
        {
            await _hub.Clients.Group("Customers").SendAsync("StockUpdated", new
            {
                ProductId = product.Id,
                NewStock  = product.Stock,
                Name      = product.Name
            });
        }

        return new ApiResponse<ProductResponse>(true, "Cập nhật thành công.", ToResponse(product));
    }

    public async Task<ApiResponse<bool>> DeleteAsync(int id)
    {
        var product = await _db.Products.FindAsync(id);
        if (product == null) return Fail<bool>("Sản phẩm không tồn tại.");
        product.IsActive = false;
        await _db.SaveChangesAsync();

        await _hub.Clients.Group("Customers").SendAsync("ProductDeleted", new { ProductId = id });

        return new ApiResponse<bool>(true, "Xóa sản phẩm thành công.", true);
    }

    private static ProductResponse ToResponse(Product p) =>
        new(p.Id, p.Name, p.Description, p.Price, p.Stock, p.Category, p.ImageUrl, p.IsActive, p.CreatedAt);

    private static ApiResponse<T> Ok<T>(T data) => new(true, null, data);
    private static ApiResponse<T> Fail<T>(string msg) => new(false, msg, default);
}

// ── PET SERVICE ───────────────────────────────────────────────

public interface IPetService
{
    Task<ApiResponse<List<PetResponse>>>         GetAllAsync();
    Task<ApiResponse<PagedResponse<PetResponse>>> SearchAsync(PetSearchParams p);
    Task<ApiResponse<PetResponse>>               GetByIdAsync(int id);
    Task<ApiResponse<PetResponse>>               CreateAsync(PetCreateRequest request);
    Task<ApiResponse<PetResponse>>               UpdateAsync(int id, PetUpdateRequest request);
    Task<ApiResponse<bool>>                      DeleteAsync(int id);
}

public class PetService : IPetService
{
    private readonly CatalogDbContext        _db;
    private readonly IHubContext<CatalogHub> _hub;

    public PetService(CatalogDbContext db, IHubContext<CatalogHub> hub)
    {
        _db  = db;
        _hub = hub;
    }

    public async Task<ApiResponse<List<PetResponse>>> GetAllAsync()
    {
        var items = await _db.Pets.ToListAsync();
        return Ok(items.Select(ToResponse).ToList());
    }

    /// <summary>Tìm kiếm + lọc + phân trang thú cưng (#4 #13)</summary>
    public async Task<ApiResponse<PagedResponse<PetResponse>>> SearchAsync(PetSearchParams p)
    {
        var query = _db.Pets.AsQueryable();

        if (!string.IsNullOrWhiteSpace(p.Search))
            query = query.Where(x => x.Name.Contains(p.Search) || x.Breed.Contains(p.Search));

        if (!string.IsNullOrWhiteSpace(p.Category))
            query = query.Where(x => x.Category == p.Category);

        if (!string.IsNullOrWhiteSpace(p.Status))
            query = query.Where(x => x.Status == p.Status);

        if (p.MinPrice.HasValue)
            query = query.Where(x => x.Price >= p.MinPrice.Value);

        if (p.MaxPrice.HasValue)
            query = query.Where(x => x.Price <= p.MaxPrice.Value);

        var total = await query.CountAsync();
        var items = await query
            .OrderByDescending(x => x.CreatedAt)
            .Skip((p.Page - 1) * p.PageSize)
            .Take(p.PageSize)
            .ToListAsync();

        return Ok(new PagedResponse<PetResponse>(items.Select(ToResponse).ToList(), total, p.Page, p.PageSize));
    }

    public async Task<ApiResponse<PetResponse>> GetByIdAsync(int id)
    {
        var item = await _db.Pets.FindAsync(id);
        if (item == null) return Fail<PetResponse>("Thú cưng không tồn tại.");
        return Ok(ToResponse(item));
    }

    public async Task<ApiResponse<PetResponse>> CreateAsync(PetCreateRequest request)
    {
        var pet = new Pet
        {
            Name        = request.Name,
            Breed       = request.Breed,
            Category    = request.Category,
            AgeMonths   = request.AgeMonths,
            Price       = request.Price,
            Description = request.Description,
            ImageUrl    = request.ImageUrl,
            Status      = request.Status
        };
        _db.Pets.Add(pet);
        await _db.SaveChangesAsync();

        var resp = ToResponse(pet);
        await _hub.Clients.Group("Customers").SendAsync("NewPetAdded", resp);

        return new ApiResponse<PetResponse>(true, "Thêm thú cưng thành công.", resp);
    }

    public async Task<ApiResponse<PetResponse>> UpdateAsync(int id, PetUpdateRequest request)
    {
        var pet = await _db.Pets.FindAsync(id);
        if (pet == null) return Fail<PetResponse>("Thú cưng không tồn tại.");

        var oldStatus = pet.Status;
        pet.Name        = request.Name;
        pet.Breed       = request.Breed;
        pet.Category    = request.Category;
        pet.AgeMonths   = request.AgeMonths;
        pet.Price       = request.Price;
        pet.Description = request.Description;
        pet.ImageUrl    = request.ImageUrl;
        pet.Status      = request.Status;
        await _db.SaveChangesAsync();

        if (oldStatus != request.Status)
        {
            await _hub.Clients.Group("Customers").SendAsync("PetStatusChanged", new
            {
                PetId     = pet.Id,
                NewStatus = pet.Status,
                Name      = pet.Name
            });
        }

        return new ApiResponse<PetResponse>(true, "Cập nhật thành công.", ToResponse(pet));
    }

    public async Task<ApiResponse<bool>> DeleteAsync(int id)
    {
        var pet = await _db.Pets.FindAsync(id);
        if (pet == null) return Fail<bool>("Thú cưng không tồn tại.");
        _db.Pets.Remove(pet);
        await _db.SaveChangesAsync();

        await _hub.Clients.Group("Customers").SendAsync("PetDeleted", new { PetId = id });

        return new ApiResponse<bool>(true, "Xóa thành công.", true);
    }

    private static PetResponse ToResponse(Pet p) =>
        new(p.Id, p.Name, p.Breed, p.Category, p.AgeMonths, p.Price, p.Description, p.ImageUrl, p.Status, p.CreatedAt);

    private static ApiResponse<T> Ok<T>(T data) => new(true, null, data);
    private static ApiResponse<T> Fail<T>(string msg) => new(false, msg, default);
}

public interface IInventoryService
{
    Task<ApiResponse<bool>> CommitOrderAsync(InventoryCommitRequest request);
    Task<ApiResponse<bool>> ReleaseOrderAsync(InventoryCommitRequest request);
}

public class InventoryService : IInventoryService
{
    private readonly CatalogDbContext _db;
    private readonly IHubContext<CatalogHub> _hub;

    public InventoryService(CatalogDbContext db, IHubContext<CatalogHub> hub)
    {
        _db = db;
        _hub = hub;
    }

    public async Task<ApiResponse<bool>> CommitOrderAsync(InventoryCommitRequest request)
    {
        if (request.Items.Count == 0)
            return Fail<bool>("Don hang khong co mat hang.");

        var strategy = _db.Database.CreateExecutionStrategy();
        var success = false;
        string? errorMessage = null;

        await strategy.ExecuteAsync(async () =>
        {
            await using var tx = await _db.Database.BeginTransactionAsync();
            try
            {
                foreach (var item in request.Items)
                {
                    if (item.ProductId.HasValue == item.PetId.HasValue)
                    {
                        errorMessage = "Moi dong don hang phai chon dung mot san pham hoac mot thu cung.";
                        return;
                    }

                    if (item.ProductId.HasValue)
                    {
                        var quantity = Math.Max(1, item.Quantity);
                        var product = await _db.Products.FirstOrDefaultAsync(p => p.Id == item.ProductId.Value && p.IsActive);
                        if (product == null)
                        {
                            errorMessage = $"San pham #{item.ProductId.Value} khong ton tai.";
                            return;
                        }
                        if (product.Stock < quantity)
                        {
                            errorMessage = $"San pham '{product.Name}' chi con {product.Stock} trong kho.";
                            return;
                        }

                        product.Stock -= quantity;
                    }
                    else
                    {
                        var pet = await _db.Pets.FindAsync(item.PetId!.Value);
                        if (pet == null)
                        {
                            errorMessage = $"Thu cung #{item.PetId.Value} khong ton tai.";
                            return;
                        }
                        if (!string.Equals(pet.Status, "Available", StringComparison.OrdinalIgnoreCase))
                        {
                            errorMessage = $"Thu cung '{pet.Name}' hien khong san sang de ban.";
                            return;
                        }

                        pet.Status = "Sold";
                    }
                }

                await _db.SaveChangesAsync();
                await tx.CommitAsync();
                success = true;
            }
            catch (Exception ex)
            {
                await tx.RollbackAsync();
                errorMessage = ex.Message;
            }
        });

        if (!success)
            return Fail<bool>(errorMessage ?? "Khong the cap nhat ton kho.");

        foreach (var item in request.Items)
        {
            if (item.ProductId.HasValue)
            {
                var product = await _db.Products.FindAsync(item.ProductId.Value);
                if (product != null)
                {
                    await _hub.Clients.Group("Customers").SendAsync("StockUpdated", new
                    {
                        ProductId = product.Id,
                        NewStock = product.Stock,
                        Name = product.Name
                    });
                }
            }
            else
            {
                var pet = await _db.Pets.FindAsync(item.PetId!.Value);
                if (pet != null)
                {
                    await _hub.Clients.Group("Customers").SendAsync("PetStatusChanged", new
                    {
                        PetId = pet.Id,
                        NewStatus = pet.Status,
                        Name = pet.Name
                    });
                }
            }
        }

        return new ApiResponse<bool>(true, "Da cap nhat ton kho.", true);
    }

    public async Task<ApiResponse<bool>> ReleaseOrderAsync(InventoryCommitRequest request)
    {
        if (request.Items.Count == 0)
            return Fail<bool>("Don hang khong co mat hang.");

        var strategy = _db.Database.CreateExecutionStrategy();
        var success = false;
        string? errorMessage = null;

        await strategy.ExecuteAsync(async () =>
        {
            await using var tx = await _db.Database.BeginTransactionAsync();
            try
            {
                foreach (var item in request.Items)
                {
                    if (item.ProductId.HasValue == item.PetId.HasValue)
                    {
                        errorMessage = "Moi dong don hang phai chon dung mot san pham hoac mot thu cung.";
                        return;
                    }

                    if (item.ProductId.HasValue)
                    {
                        var quantity = Math.Max(1, item.Quantity);
                        var product = await _db.Products.FindAsync(item.ProductId.Value);
                        if (product == null)
                        {
                            errorMessage = $"San pham #{item.ProductId.Value} khong ton tai.";
                            return;
                        }

                        product.Stock += quantity;
                    }
                    else
                    {
                        var pet = await _db.Pets.FindAsync(item.PetId!.Value);
                        if (pet == null)
                        {
                            errorMessage = $"Thu cung #{item.PetId.Value} khong ton tai.";
                            return;
                        }

                        pet.Status = "Available";
                    }
                }

                await _db.SaveChangesAsync();
                await tx.CommitAsync();
                success = true;
            }
            catch (Exception ex)
            {
                await tx.RollbackAsync();
                errorMessage = ex.Message;
            }
        });

        if (!success)
            return Fail<bool>(errorMessage ?? "Khong the hoan tra ton kho.");

        foreach (var item in request.Items)
        {
            if (item.ProductId.HasValue)
            {
                var product = await _db.Products.FindAsync(item.ProductId.Value);
                if (product != null)
                {
                    await _hub.Clients.Group("Customers").SendAsync("StockUpdated", new
                    {
                        ProductId = product.Id,
                        NewStock = product.Stock,
                        Name = product.Name
                    });
                }
            }
            else
            {
                var pet = await _db.Pets.FindAsync(item.PetId!.Value);
                if (pet != null)
                {
                    await _hub.Clients.Group("Customers").SendAsync("PetStatusChanged", new
                    {
                        PetId = pet.Id,
                        NewStatus = pet.Status,
                        Name = pet.Name
                    });
                }
            }
        }

        return new ApiResponse<bool>(true, "Da hoan ton kho.", true);
    }

    private static ApiResponse<T> Fail<T>(string msg) => new(false, msg, default);
}

public interface IOrderPurchaseValidator
{
    Task<bool> HasDeliveredPurchaseAsync(int userId, int? productId, int? petId);
}

public class OrderPurchaseValidator : IOrderPurchaseValidator
{
    private readonly HttpClient _http;
    private readonly IConfiguration _config;

    public OrderPurchaseValidator(HttpClient http, IConfiguration config)
    {
        _http = http;
        _config = config;
    }

    public async Task<bool> HasDeliveredPurchaseAsync(int userId, int? productId, int? petId)
    {
        var query = $"userId={userId}";
        if (productId.HasValue) query += $"&productId={productId.Value}";
        if (petId.HasValue) query += $"&petId={petId.Value}";

        using var request = new HttpRequestMessage(HttpMethod.Get, $"api/orders/internal/purchases/validate?{query}");
        request.Headers.Add("X-Internal-Key", _config["InternalApiKey"] ?? "dev-internal-key");

        using var response = await _http.SendAsync(request);
        if (!response.IsSuccessStatusCode)
            return false;

        var result = await response.Content.ReadFromJsonAsync<ApiResponse<bool>>();
        return result?.Success == true && result.Data;
    }
}

// ── REVIEW SERVICE  (#14) ─────────────────────────────────────────

public interface IReviewService
{
    Task<ApiResponse<List<ReviewResponse>>> GetByProductAsync(int productId);
    Task<ApiResponse<List<ReviewResponse>>> GetByPetAsync(int petId);
    Task<ApiResponse<ReviewResponse>>       CreateAsync(int userId, string userName, ReviewCreateRequest request);
    Task<ApiResponse<bool>>                 DeleteAsync(int id, int requestingUserId, string role);
}

public class ReviewService : IReviewService
{
    private readonly CatalogDbContext _db;
    private readonly IOrderPurchaseValidator _purchaseValidator;

    public ReviewService(CatalogDbContext db, IOrderPurchaseValidator purchaseValidator)
    {
        _db = db;
        _purchaseValidator = purchaseValidator;
    }

    public async Task<ApiResponse<List<ReviewResponse>>> GetByProductAsync(int productId)
    {
        var reviews = await _db.Reviews.Where(r => r.ProductId == productId)
            .OrderByDescending(r => r.CreatedAt).ToListAsync();
        return Ok(reviews.Select(ToResponse).ToList());
    }

    public async Task<ApiResponse<List<ReviewResponse>>> GetByPetAsync(int petId)
    {
        var reviews = await _db.Reviews.Where(r => r.PetId == petId)
            .OrderByDescending(r => r.CreatedAt).ToListAsync();
        return Ok(reviews.Select(ToResponse).ToList());
    }

    public async Task<ApiResponse<ReviewResponse>> CreateAsync(int userId, string userName, ReviewCreateRequest request)
    {
        if (request.ProductId.HasValue == request.PetId.HasValue)
            return Fail<ReviewResponse>("Vui long chon dung mot san pham hoac mot thu cung de danh gia.");
        if (request.Rating < 1 || request.Rating > 5)
            return Fail<ReviewResponse>("Rating phải từ 1 đến 5.");
        if (string.IsNullOrWhiteSpace(request.Comment))
            return Fail<ReviewResponse>("Nội dung đánh giá không được để trống.");

        if (request.ProductId.HasValue)
        {
            var exists = await _db.Products.AnyAsync(p => p.Id == request.ProductId.Value && p.IsActive);
            if (!exists) return Fail<ReviewResponse>("San pham khong ton tai.");

            var duplicated = await _db.Reviews.AnyAsync(r => r.UserId == userId && r.ProductId == request.ProductId.Value);
            if (duplicated) return Fail<ReviewResponse>("Ban da danh gia san pham nay.");
        }
        else if (request.PetId.HasValue)
        {
            var exists = await _db.Pets.AnyAsync(p => p.Id == request.PetId.Value);
            if (!exists) return Fail<ReviewResponse>("Thu cung khong ton tai.");

            var duplicated = await _db.Reviews.AnyAsync(r => r.UserId == userId && r.PetId == request.PetId.Value);
            if (duplicated) return Fail<ReviewResponse>("Ban da danh gia thu cung nay.");
        }

        var hasDeliveredPurchase = await _purchaseValidator.HasDeliveredPurchaseAsync(userId, request.ProductId, request.PetId);
        if (!hasDeliveredPurchase)
            return Fail<ReviewResponse>("Chi co the danh gia mat hang trong don hang da giao thanh cong.");

        var review = new Review
        {
            UserId    = userId,
            UserName  = userName,
            ProductId = request.ProductId,
            PetId     = request.PetId,
            Rating    = request.Rating,
            Comment   = request.Comment
        };
        _db.Reviews.Add(review);
        await _db.SaveChangesAsync();
        return new ApiResponse<ReviewResponse>(true, "Đánh giá thành công.", ToResponse(review));
    }

    public async Task<ApiResponse<bool>> DeleteAsync(int id, int requestingUserId, string role)
    {
        var review = await _db.Reviews.FindAsync(id);
        if (review == null) return Fail<bool>("Đánh giá không tồn tại.");
        if (role != "Admin" && review.UserId != requestingUserId)
            return Fail<bool>("Bạn không có quyền xóa đánh giá này.");

        _db.Reviews.Remove(review);
        await _db.SaveChangesAsync();
        return new ApiResponse<bool>(true, "Xóa đánh giá thành công.", true);
    }

    private static ReviewResponse ToResponse(Review r) =>
        new(r.Id, r.UserId, r.UserName, r.ProductId, r.PetId, r.Rating, r.Comment, r.CreatedAt);

    private static ApiResponse<T> Ok<T>(T data) => new(true, null, data);
    private static ApiResponse<T> Fail<T>(string msg) => new(false, msg, default);
}

// ── CATEGORY SERVICE  (#10) ───────────────────────────────────────

public interface ICategoryService
{
    Task<ApiResponse<List<CategoryResponse>>> GetAllAsync(string? type = null);
    Task<ApiResponse<CategoryResponse>>       GetByIdAsync(int id);
    Task<ApiResponse<CategoryResponse>>       CreateAsync(CategoryCreateRequest request);
    Task<ApiResponse<CategoryResponse>>       UpdateAsync(int id, CategoryCreateRequest request);
    Task<ApiResponse<bool>>                   DeleteAsync(int id);
}

public class CategoryService : ICategoryService
{
    private readonly CatalogDbContext _db;
    public CategoryService(CatalogDbContext db) => _db = db;

    public async Task<ApiResponse<List<CategoryResponse>>> GetAllAsync(string? type = null)
    {
        var query = _db.Categories.AsQueryable();
        if (!string.IsNullOrWhiteSpace(type))
            query = query.Where(c => c.Type == type);
        var list = await query.OrderBy(c => c.Name).ToListAsync();
        return Ok(list.Select(ToResponse).ToList());
    }

    public async Task<ApiResponse<CategoryResponse>> GetByIdAsync(int id)
    {
        var cat = await _db.Categories.FindAsync(id);
        if (cat == null) return Fail<CategoryResponse>("Danh mục không tồn tại.");
        return Ok(ToResponse(cat));
    }

    public async Task<ApiResponse<CategoryResponse>> CreateAsync(CategoryCreateRequest request)
    {
        var cat = new Category
        {
            Name        = request.Name,
            Type        = request.Type,
            Description = request.Description,
            Icon        = request.Icon
        };
        _db.Categories.Add(cat);
        await _db.SaveChangesAsync();
        return new ApiResponse<CategoryResponse>(true, "Tạo danh mục thành công.", ToResponse(cat));
    }

    public async Task<ApiResponse<CategoryResponse>> UpdateAsync(int id, CategoryCreateRequest request)
    {
        var cat = await _db.Categories.FindAsync(id);
        if (cat == null) return Fail<CategoryResponse>("Danh mục không tồn tại.");
        cat.Name        = request.Name;
        cat.Type        = request.Type;
        cat.Description = request.Description;
        cat.Icon        = request.Icon;
        await _db.SaveChangesAsync();
        return new ApiResponse<CategoryResponse>(true, "Cập nhật danh mục thành công.", ToResponse(cat));
    }

    public async Task<ApiResponse<bool>> DeleteAsync(int id)
    {
        var cat = await _db.Categories.FindAsync(id);
        if (cat == null) return Fail<bool>("Danh mục không tồn tại.");
        _db.Categories.Remove(cat);
        await _db.SaveChangesAsync();
        return new ApiResponse<bool>(true, "Xóa danh mục thành công.", true);
    }

    private static CategoryResponse ToResponse(Category c) =>
        new(c.Id, c.Name, c.Type, c.Description, c.Icon);

    private static ApiResponse<T> Ok<T>(T data) => new(true, null, data);
    private static ApiResponse<T> Fail<T>(string msg) => new(false, msg, default);
}
