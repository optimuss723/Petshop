namespace Catalog.Service.Entities;

/// <summary>Sản phẩm trong cửa hàng (thức ăn, phụ kiện, thuốc...)</summary>
public class Product
{
    public int      Id          { get; set; }
    public string   Name        { get; set; } = null!;
    public string   Description { get; set; } = null!;
    public decimal  Price       { get; set; }
    public int      Stock       { get; set; }
    public string   Category    { get; set; } = "Other"; // stored as string
    public string?  ImageUrl    { get; set; }
    public DateTime CreatedAt   { get; set; } = DateTime.UtcNow;
    public bool     IsActive    { get; set; } = true;
}

/// <summary>Thú cưng bán trong cửa hàng</summary>
public class Pet
{
    public int      Id          { get; set; }
    public string   Name        { get; set; } = null!;
    public string   Breed       { get; set; } = null!;
    public string   Category    { get; set; } = "Other"; // Dog | Cat | Bird | Fish | Other
    public int      AgeMonths   { get; set; }
    public decimal  Price       { get; set; }
    public string   Description { get; set; } = null!;
    public string?  ImageUrl    { get; set; }
    public string   Status      { get; set; } = "Available"; // Available | Sold | Reserved
    public DateTime CreatedAt   { get; set; } = DateTime.UtcNow;
}

/// <summary>
/// Đánh giá sản phẩm hoặc thú cưng của khách hàng.
/// Một review thuộc về ProductId XOR PetId (không đồng thời cả hai).
/// </summary>
public class Review
{
    public int      Id        { get; set; }
    public int      UserId    { get; set; }   // Reference → Auth/User Service
    public string   UserName  { get; set; } = null!;
    public int?     ProductId { get; set; }   // null nếu là review thú cưng
    public int?     PetId     { get; set; }   // null nếu là review sản phẩm
    public int      Rating    { get; set; }   // 1 → 5 sao
    public string   Comment   { get; set; } = null!;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}

/// <summary>
/// Danh mục sản phẩm / thú cưng dùng cho CRUD quản lý danh mục.
/// Type = "Product" | "Pet"
/// </summary>
public class Category
{
    public int     Id          { get; set; }
    public string  Name        { get; set; } = null!;
    public string  Type        { get; set; } = "Product"; // Product | Pet
    public string? Description { get; set; }
    public string? Icon        { get; set; } // emoji hoặc icon class
}
