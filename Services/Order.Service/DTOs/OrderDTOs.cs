namespace Order.Service.DTOs;

// ═══════════════════════════════════════════════════════════════
//  ORDER REQUEST DTOs
// ═══════════════════════════════════════════════════════════════

public class CreateOrderRequest
{
    public List<CreateOrderItemRequest> Items { get; init; } = [];
    public string ShippingAddress { get; init; } = "";
    public string? Note { get; init; }
    public bool IsCod { get; init; } = true;
}

public record CreateOrderItemRequest(
    int?   ProductId,
    string? ProductName,
    int?   PetId,
    string? PetName,
    int    Quantity,
    decimal UnitPrice);

public record UpdateOrderStatusRequest(string Status);

public record ConfirmPaymentRequest(string? PaymentCode);

public record PayOsPaymentLinkResponse(
    long OrderCode,
    int Amount,
    string Description,
    string PaymentLinkId,
    string CheckoutUrl,
    string QrCode,
    string Status);

public class PayOsWebhookRequest
{
    public string? Code { get; init; }
    public string? Desc { get; init; }
    public bool Success { get; init; }
    public PayOsWebhookData? Data { get; init; }
    public string? Signature { get; init; }
}

public class PayOsWebhookData
{
    public long OrderCode { get; init; }
    public int Amount { get; init; }
    public string? Description { get; init; }
    public string? AccountNumber { get; init; }
    public string? Reference { get; init; }
    public string? TransactionDateTime { get; init; }
    public string? Currency { get; init; }
    public string? PaymentLinkId { get; init; }
    public string? Code { get; init; }
    public string? Desc { get; init; }
    public string? CounterAccountBankId { get; init; }
    public string? CounterAccountBankName { get; init; }
    public string? CounterAccountName { get; init; }
    public string? CounterAccountNumber { get; init; }
    public string? VirtualAccountName { get; init; }
    public string? VirtualAccountNumber { get; init; }
}

/// <summary>Admin/Staff tạo vận đơn với hãng ship ngoài</summary>
public record CreateShipmentRequest(
    string? Note); // Ghi chú thêm khi giao

// ═══════════════════════════════════════════════════════════════
//  ORDER RESPONSE DTOs
// ═══════════════════════════════════════════════════════════════

public record OrderResponse(
    int    Id,
    int    UserId,
    string UserName,
    string CustomerPhone,
    string ShippingAddress,
    string? Note,
    string Status,
    decimal TotalAmount,
    bool   IsCod,
    string PaymentMethod,
    string PaymentStatus,
    string? TrackingCode,
    string? ShippingProvider,
    string? TrackingUrl,
    string? FailureReason,
    List<OrderItemResponse> Items,
    DateTime  CreatedAt,
    DateTime? UpdatedAt,
    DateTime? ShippedAt,
    DateTime? DeliveredAt);

public record OrderItemResponse(
    int    Id,
    int?   ProductId,
    string? ProductName,
    int?   PetId,
    string? PetName,
    int    Quantity,
    decimal UnitPrice,
    decimal SubTotal);

/// <summary>Kết quả tạo vận đơn</summary>
public record ShipmentCreatedResponse(
    string TrackingCode,
    string Provider,
    string TrackingUrl,
    string Message);

/// <summary>Thống kê đơn hàng cho Admin Dashboard</summary>
public record OrderStatsResponse(
    int TotalOrders,
    decimal TotalRevenue,
    int PendingCount,
    int ConfirmedCount,
    int ShippingCount,
    int AwaitingReceiptCount,
    int DeliveredCount,
    int CancelledCount,
    List<MonthlyRevenueItem> MonthlyRevenue);

public record MonthlyRevenueItem(int Year, int Month, decimal Revenue, int OrderCount);

/// <summary>SignalR notification khi trạng thái đơn thay đổi</summary>
public record OrderStatusChangedNotification(int OrderId, string NewStatus, DateTime UpdatedAt);

public record ApiResponse<T>(bool Success, string? Message, T? Data);

/// <summary>Ghi lịch sử thao tác của Staff</summary>
public record AuditLogResponse(
    int      Id,
    int      StaffId,
    string   StaffName,
    string   Action,
    int?     OrderId,
    string?  Details,
    DateTime CreatedAt);
