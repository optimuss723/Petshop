using PetShop.Shared.Enums;

namespace PetShop.Shared.DTOs.Response;

public record AuthResponse(string Token, string RefreshToken, UserResponse User)
{
    public AuthResponse() : this(string.Empty, string.Empty, new UserResponse()) { }
}

public record UserResponse(
    int      Id,
    string   FullName,
    string   Email,
    string   Phone,
    string?  Address,
    string   Role,
    bool     IsActive,
    DateTime CreatedAt)
{
    public UserResponse() : this(0, string.Empty, string.Empty, string.Empty, null, string.Empty, true, DateTime.UtcNow) { }
}

public record ProductResponse(
    int      Id,
    string   Name,
    string   Description,
    decimal  Price,
    int      Stock,
    string   Category,
    string?  ImageUrl,
    bool     IsActive,
    DateTime CreatedAt)
{
    public ProductResponse() : this(0, string.Empty, string.Empty, 0, 0, string.Empty, null, true, DateTime.UtcNow) { }
}

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
    DateTime CreatedAt)
{
    public PetResponse() : this(0, string.Empty, string.Empty, string.Empty, 0, 0, string.Empty, null, string.Empty, DateTime.UtcNow) { }
}

public record OrderResponse(
    int                      Id,
    int                      UserId,
    string                   UserName,
    string                   CustomerPhone,
    string                   ShippingAddress,
    string?                  Note,
    string                   Status,
    decimal                  TotalAmount,
    bool                     IsCod            = true,
    string                   PaymentMethod    = "COD",
    string                   PaymentStatus    = "CODPending",
    string?                  TrackingCode     = null,
    string?                  ShippingProvider = null,
    string?                  TrackingUrl      = null,
    string?                  FailureReason    = null,
    List<OrderItemResponse>? Items            = null,
    DateTime                 CreatedAt        = default,
    DateTime?                UpdatedAt        = null,
    DateTime?                ShippedAt        = null,
    DateTime?                DeliveredAt      = null)
{
    public OrderResponse() : this(0, 0, string.Empty, string.Empty, string.Empty, null, string.Empty, 0) { }
}

public record OrderItemResponse(
    int     Id,
    int?    ProductId,
    string? ProductName,
    int?    PetId,
    string? PetName,
    int     Quantity,
    decimal UnitPrice,
    decimal SubTotal = 0)
{
    public OrderItemResponse() : this(0, null, null, null, null, 0, 0, 0) { }
}

public record PayOsPaymentLinkResponse(
    long OrderCode,
    int Amount,
    string Description,
    string PaymentLinkId,
    string CheckoutUrl,
    string QrCode,
    string Status)
{
    public PayOsPaymentLinkResponse() : this(0, 0, string.Empty, string.Empty, string.Empty, string.Empty, string.Empty) { }
}

/// <summary>Kết quả tạo vận đơn với hãng ship ngoài</summary>
public record ShipmentCreatedResponse(
    string TrackingCode,
    string Provider,
    string TrackingUrl,
    string Message)
{
    public ShipmentCreatedResponse() : this(string.Empty, string.Empty, string.Empty, string.Empty) { }
}

/// <summary>Đánh giá sản phẩm / thú cưng (#14)</summary>
public record ReviewResponse(
    int      Id,
    int      UserId,
    string   UserName,
    int?     ProductId,
    int?     PetId,
    int      Rating,
    string   Comment,
    DateTime CreatedAt)
{
    public ReviewResponse() : this(0, 0, string.Empty, null, null, 5, string.Empty, DateTime.UtcNow) { }
}

/// <summary>Danh mục sản phẩm / thú cưng (#10)</summary>
public record CategoryResponse(
    int     Id,
    string  Name,
    string  Type,
    string? Description,
    string? Icon)
{
    public CategoryResponse() : this(0, string.Empty, string.Empty, null, null) { }
}

/// <summary>Thống kê tổng hợp cho Admin Dashboard (#8)</summary>
public record OrderStatsResponse(
    int                      TotalOrders,
    decimal                  TotalRevenue,
    int                      PendingCount,
    int                      ConfirmedCount,
    int                      ShippingCount,
    int                      AwaitingReceiptCount,
    int                      DeliveredCount,
    int                      CancelledCount,
    List<MonthlyRevenueItem> MonthlyRevenue)
{
    public OrderStatsResponse() : this(0, 0, 0, 0, 0, 0, 0, 0, new List<MonthlyRevenueItem>()) { }
}

/// <summary>Doanh thu theo tháng cho biểu đồ Chart.js</summary>
public record MonthlyRevenueItem(int Year, int Month, decimal Revenue, int OrderCount)
{
    public MonthlyRevenueItem() : this(0, 0, 0, 0) { }
}

public record ApiResponse<T>(bool Success, string? Message, T? Data)
{
    public ApiResponse() : this(true, null, default) { }
}

public record PagedResponse<T>(List<T> Items, int TotalCount, int Page, int PageSize)
{
    public PagedResponse() : this(new List<T>(), 0, 1, 10) { }
    public int  TotalPages => PageSize > 0 ? (int)Math.Ceiling((double)TotalCount / PageSize) : 0;
    public bool HasPrev    => Page > 1;
    public bool HasNext    => Page < TotalPages;
}

/// <summary>Lịch sử thao tác của Staff (audit log)</summary>
public record AuditLogResponse(
    int      Id,
    int      StaffId,
    string   StaffName,
    string   Action,
    int?     OrderId,
    string?  Details,
    DateTime CreatedAt)
{
    public AuditLogResponse() : this(0, 0, string.Empty, string.Empty, null, null, DateTime.UtcNow) { }
}
