namespace Order.Service.Entities;

/// <summary>
/// Entity đơn hàng.
/// Tích hợp với dịch vụ vận chuyển ngoài qua TrackingCode/TrackingUrl.
/// </summary>
public class Order
{
    public int    Id             { get; set; }
    public int    UserId         { get; set; }
    public string UserName       { get; set; } = null!;
    public string CustomerPhone  { get; set; } = null!;  // SĐT giao hàng
    public string ShippingAddress{ get; set; } = null!;
    public string? Note          { get; set; }
    public string Status         { get; set; } = "Pending";
    // Pending → Confirmed → Shipping → AwaitingReceipt → Delivered | Cancelled
    public decimal TotalAmount   { get; set; }
    public bool   IsCod          { get; set; } = true;   // Thanh toán khi nhận

    // ── Thông tin vận chuyển ngoài ────────────────────────────────
    public string? TrackingCode      { get; set; }   // Mã vận đơn từ hãng ship
    public string? ShippingProvider  { get; set; }   // "GHN Mock", "GHTK Mock"...
    public string? TrackingUrl       { get; set; }   // Link tra cứu vận đơn
    public string? FailureReason     { get; set; }   // Lý do giao thất bại
    public string? DeliveryProofUrl  { get; set; }   // Ảnh xác nhận giao hàng

    // ── Timestamps ────────────────────────────────────────────────
    public DateTime  CreatedAt   { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt   { get; set; }
    public DateTime? ShippedAt   { get; set; }       // Khi tạo vận đơn thành công
    public DateTime? DeliveredAt { get; set; }       // Khi khách xác nhận đã nhận hàng

    public List<OrderItem> Items { get; set; } = [];
}

public class OrderItem
{
    public int    Id        { get; set; }
    public int    OrderId   { get; set; }
    public int?   ProductId { get; set; }
    public string? ProductName { get; set; }
    public int?   PetId     { get; set; }
    public string? PetName  { get; set; }
    public int    Quantity  { get; set; }
    public decimal UnitPrice{ get; set; }
    public decimal SubTotal => Quantity * UnitPrice;

    public Order Order { get; set; } = null!;
}
