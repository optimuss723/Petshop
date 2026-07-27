namespace Catalog.Service.DTOs;

// ═══════════════════════════════════════════════════════════════
//  PRODUCT DTOs
// ═══════════════════════════════════════════════════════════════

/// <summary>Dữ liệu tạo/cập nhật sản phẩm</summary>
public record ProductCreateRequest(
    string  Name,
    string  Description,
    decimal Price,
    int     Stock,
    string  Category,   // Food | Accessory | Medicine | Toy | Other
    string? ImageUrl);

/// <summary>Alias cho update (có thể mở rộng sau)</summary>
public record ProductUpdateRequest(
    string  Name,
    string  Description,
    decimal Price,
    int     Stock,
    string  Category,
    string? ImageUrl);

/// <summary>Dữ liệu sản phẩm trả về client</summary>
public record ProductResponse(
    int      Id,
    string   Name,
    string   Description,
    decimal  Price,
    int      Stock,
    string   Category,
    string?  ImageUrl,
    bool     IsActive,
    DateTime CreatedAt);

// ═══════════════════════════════════════════════════════════════
//  PET DTOs
// ═══════════════════════════════════════════════════════════════

/// <summary>Dữ liệu tạo/cập nhật thú cưng</summary>
public record PetCreateRequest(
    string  Name,
    string  Breed,
    string  Category,   // Dog | Cat | Bird | Fish | Other
    int     AgeMonths,
    decimal Price,
    string  Description,
    string? ImageUrl,
    string  Status);    // Available | Sold | Reserved

/// <summary>Alias cho update</summary>
public record PetUpdateRequest(
    string  Name,
    string  Breed,
    string  Category,
    int     AgeMonths,
    decimal Price,
    string  Description,
    string? ImageUrl,
    string  Status);

/// <summary>Dữ liệu thú cưng trả về client</summary>
public record PetResponse(
    int      Id,
    string   Name,
    string   Breed,
    string   Category,
    int      AgeMonths,
    decimal  Price,
    string   Description,
    string?  ImageUrl,
    string   Status,
    DateTime CreatedAt);

// ═══════════════════════════════════════════════════════════════
//  REVIEW DTOs  (#14 - Review/Rating)
// ═══════════════════════════════════════════════════════════════

/// <summary>Tạo đánh giá mới cho sản phẩm hoặc thú cưng</summary>
public record ReviewCreateRequest(
    int?   ProductId,   // null nếu review thú cưng
    int?   PetId,       // null nếu review sản phẩm
    int    Rating,      // 1-5
    string Comment);

/// <summary>Dữ liệu review trả về client</summary>
public record ReviewResponse(
    int      Id,
    int      UserId,
    string   UserName,
    int?     ProductId,
    int?     PetId,
    int      Rating,
    string   Comment,
    DateTime CreatedAt);

public record InventoryCommitRequest(
    List<InventoryCommitItemRequest> Items);

public record InventoryCommitItemRequest(
    int? ProductId,
    int? PetId,
    int Quantity);

// ═══════════════════════════════════════════════════════════════
//  CATEGORY DTOs  (#10 - Category Management)
// ═══════════════════════════════════════════════════════════════

/// <summary>Tạo danh mục mới</summary>
public record CategoryCreateRequest(
    string  Name,
    string  Type,         // Product | Pet
    string? Description,
    string? Icon);

/// <summary>Dữ liệu danh mục trả về client</summary>
public record CategoryResponse(
    int     Id,
    string  Name,
    string  Type,
    string? Description,
    string? Icon);

// ═══════════════════════════════════════════════════════════════
//  SEARCH / PAGINATION PARAMS  (#4 #13)
// ═══════════════════════════════════════════════════════════════

/// <summary>Query params cho tìm kiếm sản phẩm có phân trang</summary>
public record ProductSearchParams(
    string?  Search    = null,
    string?  Category  = null,
    decimal? MinPrice  = null,
    decimal? MaxPrice  = null,
    int      Page      = 1,
    int      PageSize  = 9);

/// <summary>Query params cho tìm kiếm thú cưng có phân trang</summary>
public record PetSearchParams(
    string?  Search    = null,
    string?  Category  = null,
    decimal? MinPrice  = null,
    decimal? MaxPrice  = null,
    string?  Status    = "Available",
    int      Page      = 1,
    int      PageSize  = 9);

// ═══════════════════════════════════════════════════════════════
//  COMMON
// ═══════════════════════════════════════════════════════════════

/// <summary>Wrapper chuẩn cho tất cả API responses của Catalog</summary>
public record ApiResponse<T>(bool Success, string? Message, T? Data);

/// <summary>Response phân trang</summary>
public record PagedResponse<T>(List<T> Items, int TotalCount, int Page, int PageSize)
{
    public int TotalPages => (int)Math.Ceiling((double)TotalCount / PageSize);
    public bool HasPrev   => Page > 1;
    public bool HasNext   => Page < TotalPages;
}
