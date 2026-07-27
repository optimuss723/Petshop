namespace Order.Service.Entities;

/// <summary>
/// Ghi lại lịch sử thao tác của Staff trên hệ thống đơn hàng.
/// </summary>
public class StaffAuditLog
{
    public int      Id        { get; set; }
    public int      StaffId   { get; set; }
    public string   StaffName { get; set; } = null!;
    /// <summary>ConfirmOrder | CreateShipment | UpdateStatus | CancelOrder</summary>
    public string   Action    { get; set; } = null!;
    public int?     OrderId   { get; set; }
    /// <summary>Mô tả ngắn: "Pending → Confirmed", "Tracking: GHN0629xxxxx"...</summary>
    public string?  Details   { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
