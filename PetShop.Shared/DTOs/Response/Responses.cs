using PetShop.Shared.Enums;

namespace PetShop.Shared.DTOs.Response;

public record AuthResponse(string Token, string RefreshToken, UserResponse User);

public record UserResponse(
    int      Id,
    string   FullName,
    string   Email,
    string   Phone,
    string?  Address,
    string   Role,
    bool     IsActive,
    DateTime CreatedAt);

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
    DateTime?                DeliveredAt      = null);

public record OrderItemResponse(
    int     Id,
    int?    ProductId,
    string? ProductName,
    int?    PetId,
    string? PetName,
    int     Quantity,
    decimal UnitPrice,
    decimal SubTotal = 0);

public record PayOsPaymentLinkResponse(
    long OrderCode,
    int Amount,
    string Description,
    string PaymentLinkId,
    string CheckoutUrl,
    string QrCode,
    string Status);

/// <summary>Kết quả tạo vận đơn với hãng ship ngoài</summary>
public record ShipmentCreatedResponse(
    string TrackingCode,
    string Provider,
    string TrackingUrl,
    string Message);

/// <summary>Đánh giá sản phẩm / thú cưng (#14)</summary>
public record ReviewResponse(
    int      Id,
    int      UserId,
    string   UserName,
    int?     ProductId,
    int?     PetId,
    int      Rating,
    string   Comment,
    DateTime CreatedAt);

/// <summary>Danh mục sản phẩm / thú cưng (#10)</summary>
public record CategoryResponse(
    int     Id,
    string  Name,
    string  Type,
    string? Description,
    string? Icon);

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
    List<MonthlyRevenueItem> MonthlyRevenue);

/// <summary>Doanh thu theo tháng cho biểu đồ Chart.js</summary>
public record MonthlyRevenueItem(int Year, int Month, decimal Revenue, int OrderCount);

public record ApiResponse<T>(bool Success, string? Message, T? Data);

public record PagedResponse<T>(List<T> Items, int TotalCount, int Page, int PageSize)
{
    public int  TotalPages => (int)Math.Ceiling((double)TotalCount / PageSize);
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
    DateTime CreatedAt);
