namespace Order.Service.Services;

/// <summary>
/// Mock shipping service — giả lập dịch vụ vận chuyển ngoài (GHN, GHTK...).
/// Trong production, thay thế bằng HttpClient gọi API thật.
/// </summary>
public interface IMockShippingService
{
    Task<ShipmentResult> CreateShipmentAsync(
        string recipientName,
        string recipientPhone,
        string deliveryAddress,
        decimal codAmount,
        string? note);
}

public record ShipmentResult(
    bool   Success,
    string TrackingCode,
    string Provider,
    string TrackingUrl,
    string? ErrorMessage = null);

public class MockShippingService : IMockShippingService
{
    private static readonly Random _rng = new();
    private const string Provider   = "GHN Mock";
    private const string TrackingBase = "https://tracking.ghn.dev/?order_code=";

    public Task<ShipmentResult> CreateShipmentAsync(
        string recipientName,
        string recipientPhone,
        string deliveryAddress,
        decimal codAmount,
        string? note)
    {
        // Giả lập độ trễ API ~200ms
        // Tạo mã vận đơn giả theo định dạng GHN thực
        var date      = DateTime.Now.ToString("MMdd");
        var seq       = _rng.Next(10000, 99999);
        var tracking  = $"GHN{date}{seq}";
        var trackingUrl = $"{TrackingBase}{tracking}";

        var result = new ShipmentResult(
            Success:      true,
            TrackingCode: tracking,
            Provider:     Provider,
            TrackingUrl:  trackingUrl);

        return Task.FromResult(result);
    }
}
