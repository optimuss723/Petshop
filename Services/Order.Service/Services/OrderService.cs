using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using ClosedXML.Excel;
using Order.Service.Data;
using Order.Service.DTOs;
using Order.Service.Entities;
using Order.Service.Hubs;

namespace Order.Service.Services;

// ── INTERFACE ────────────────────────────────────────────────────

public interface IOrderService
{
    // Customer
    Task<ApiResponse<OrderResponse>>        CreateAsync(CreateOrderRequest req, int userId, string userName, string customerPhone);
    Task<ApiResponse<OrderResponse>>        PayAsync(int orderId, int userId, string? paymentCode);
    Task<ApiResponse<PayOsPaymentLinkResponse>> CreatePayOsPaymentLinkAsync(int orderId, int userId);
    Task<ApiResponse<OrderResponse>>        ConfirmPaidAsync(int orderId, string? paymentCode, int? paidAmount = null, string? paymentLinkId = null);
    Task<ApiResponse<OrderResponse>>        GetByIdAsync(int id, int? requestingUserId);
    Task<ApiResponse<List<OrderResponse>>>  GetMyOrdersAsync(int userId);
    Task<ApiResponse<bool>>                 CancelAsync(int orderId, int userId);
    Task<ApiResponse<OrderResponse>>        ConfirmReceivedAsync(int orderId, int userId);

    // Admin / Staff
    Task<ApiResponse<List<OrderResponse>>>  GetAllAsync(string? status);
    Task<ApiResponse<OrderResponse>>        UpdateStatusAsync(int orderId, string newStatus, int actorId, string actorName);
    Task<ApiResponse<ShipmentCreatedResponse>> CreateShipmentAsync(int orderId, int actorId, string actorName, CreateShipmentRequest req);
    Task<ApiResponse<OrderStatsResponse>>   GetStatsAsync();
    Task<byte[]?>                           ExportExcelAsync();
    Task<ApiResponse<List<AuditLogResponse>>> GetAuditLogsAsync(int? staffId, int page);
    Task<ApiResponse<bool>>                 HasDeliveredItemAsync(int userId, int? productId, int? petId);
}

// ── IMPLEMENTATION ───────────────────────────────────────────────

public class OrderService : IOrderService
{
    private readonly OrderDbContext        _db;
    private readonly IHubContext<OrderHub> _hub;
    private readonly IMockShippingService  _shipping;
    private readonly IAuditLogService      _audit;
    private readonly ICatalogLookupService _catalog;
    private readonly IPayOsPaymentService  _payOs;
    private readonly IConfiguration        _config;

    public OrderService(
        OrderDbContext db,
        IHubContext<OrderHub> hub,
        IMockShippingService shipping,
        IAuditLogService audit,
        ICatalogLookupService catalog,
        IPayOsPaymentService payOs,
        IConfiguration config)
    {
        _db       = db;
        _hub      = hub;
        _shipping = shipping;
        _audit    = audit;
        _catalog  = catalog;
        _payOs    = payOs;
        _config   = config;
    }

    // ── CUSTOMER: TẠO ĐƠN ────────────────────────────────────────
    public async Task<ApiResponse<OrderResponse>> CreateAsync(CreateOrderRequest req, int userId, string userName, string customerPhone)
    {
        if (!req.Items.Any())
            return Fail<OrderResponse>("Đơn hàng phải có ít nhất 1 sản phẩm.");
        if (string.IsNullOrWhiteSpace(req.ShippingAddress))
            return Fail<OrderResponse>("Địa chỉ giao hàng không được để trống.");
        if (string.IsNullOrWhiteSpace(customerPhone))
            return Fail<OrderResponse>("Không tìm thấy số điện thoại khách hàng. Vui lòng đăng nhập lại.");

        var orderItems = new List<OrderItem>();
        foreach (var item in req.Items)
        {
            var built = await _catalog.BuildOrderItemAsync(item);
            if (built.Success != true || built.Data == null)
                return Fail<OrderResponse>(built.Message ?? "Không thể kiểm tra thông tin sản phẩm.");
            orderItems.Add(built.Data);
        }

        var inventoryResult = await _catalog.CommitInventoryAsync(orderItems);
        if (inventoryResult.Success != true)
            return Fail<OrderResponse>(inventoryResult.Message ?? "Không thể cập nhật tồn kho.");

        var order = new Order.Service.Entities.Order
        {
            UserId          = userId,
            UserName        = userName,
            CustomerPhone   = customerPhone,
            ShippingAddress = req.ShippingAddress,
            Note            = req.Note,
            Status          = "Pending",
            IsCod           = req.IsCod,
            Items           = orderItems
        };
        order.TotalAmount = order.Items.Sum(i => i.Quantity * i.UnitPrice);

        _db.Orders.Add(order);
        await _db.SaveChangesAsync();

        // Notify Admins và Staff
        await _hub.Clients.Group("Admins")
            .SendAsync("NewOrder", new { order.Id, order.UserName, order.TotalAmount });

        return Ok(ToResponse(order));
    }

    public async Task<ApiResponse<OrderResponse>> PayAsync(int orderId, int userId, string? paymentCode)
    {
        if (!_config.GetValue<bool>("PayOS:AllowManualConfirmation"))
            return Fail<OrderResponse>("PayOS chi duoc xac nhan bang webhook hop le. Vui long thanh toan qua link PayOS.");

        var order = await _db.Orders.Include(o => o.Items).FirstOrDefaultAsync(o => o.Id == orderId);
        if (order == null) return Fail<OrderResponse>("Đơn hàng không tồn tại.");
        if (order.UserId != userId) return Fail<OrderResponse>("Bạn không có quyền thanh toán đơn hàng này.");

        return await ConfirmPaidAsync(orderId, paymentCode);
    }

    public async Task<ApiResponse<PayOsPaymentLinkResponse>> CreatePayOsPaymentLinkAsync(int orderId, int userId)
    {
        var order = await _db.Orders.Include(o => o.Items).FirstOrDefaultAsync(o => o.Id == orderId);
        if (order == null) return Fail<PayOsPaymentLinkResponse>("Đơn hàng không tồn tại.");
        if (order.UserId != userId) return Fail<PayOsPaymentLinkResponse>("Bạn không có quyền thanh toán đơn hàng này.");
        if (order.Status != "Pending") return Fail<PayOsPaymentLinkResponse>("Chỉ có thể tạo link thanh toán cho đơn hàng đang chờ xử lý.");

        var result = await _payOs.CreatePaymentLinkAsync(order);
        if (result.Success != true || result.Data == null)
            return result;

        order.IsCod = false;
        order.UpdatedAt = DateTime.UtcNow;
        await _db.SaveChangesAsync();

        return result;
    }

    public async Task<ApiResponse<OrderResponse>> ConfirmPaidAsync(int orderId, string? paymentCode, int? paidAmount = null, string? paymentLinkId = null)
    {
        var order = await _db.Orders.Include(o => o.Items).FirstOrDefaultAsync(o => o.Id == orderId);
        if (order == null) return Fail<OrderResponse>("Đơn hàng không tồn tại.");
        if (order.Status != "Pending")
        {
            if (!order.IsCod && order.Status == "Confirmed")
                return Ok(ToResponse(order));

            return Fail<OrderResponse>("Chỉ có thể thanh toán đơn hàng đang chờ xử lý.");
        }

        if (paidAmount.HasValue && paidAmount.Value != ToPayOsAmount(order.TotalAmount))
            return Fail<OrderResponse>("So tien PayOS khong khop voi tong tien don hang.");

        var payOsNote = string.IsNullOrWhiteSpace(paymentCode)
            ? "PayOS paid"
            : $"PayOS paid - Code: {paymentCode}";
        if (!string.IsNullOrWhiteSpace(paymentLinkId))
            payOsNote = $"{payOsNote} - Link: {paymentLinkId}";

        order.IsCod = false;
        order.Status = "Confirmed";
        order.Note = string.IsNullOrWhiteSpace(order.Note)
            ? payOsNote
            : $"{order.Note} | {payOsNote}";
        order.UpdatedAt = DateTime.UtcNow;

        await _db.SaveChangesAsync();
        await NotifyStatusChange(order);

        await _hub.Clients.Group("Admins")
            .SendAsync("OrderPaid", new { order.Id, order.UserName, order.TotalAmount });

        return Ok(ToResponse(order));
    }

    // ── CUSTOMER: XEM ĐƠN ────────────────────────────────────────
    public async Task<ApiResponse<OrderResponse>> GetByIdAsync(int id, int? requestingUserId)
    {
        var order = await _db.Orders.Include(o => o.Items).FirstOrDefaultAsync(o => o.Id == id);
        if (order == null) return Fail<OrderResponse>("Đơn hàng không tồn tại.");

        // Customer chỉ xem đơn của mình
        if (requestingUserId.HasValue && order.UserId != requestingUserId.Value)
            return Fail<OrderResponse>("Bạn không có quyền xem đơn hàng này.");

        return Ok(ToResponse(order));
    }

    public async Task<ApiResponse<List<OrderResponse>>> GetMyOrdersAsync(int userId)
    {
        var orders = await _db.Orders.Include(o => o.Items)
            .Where(o => o.UserId == userId)
            .OrderByDescending(o => o.CreatedAt)
            .ToListAsync();
        return Ok(orders.Select(ToResponse).ToList());
    }

    public async Task<ApiResponse<bool>> CancelAsync(int orderId, int userId)
    {
        var order = await _db.Orders.Include(o => o.Items).FirstOrDefaultAsync(o => o.Id == orderId);
        if (order == null) return Fail<bool>("Đơn hàng không tồn tại.");
        if (order.UserId != userId) return Fail<bool>("Không có quyền hủy đơn này.");
        if (order.Status == "Cancelled")
            return Fail<bool>("Đơn hàng đã được hủy trước đó.");
        if (order.Status != "Pending")
            return Fail<bool>("Chỉ có thể hủy đơn hàng đang chờ xử lý.");

        var releaseResult = await _catalog.ReleaseInventoryAsync(order.Items);
        if (releaseResult.Success != true)
            return Fail<bool>(releaseResult.Message ?? "Không thể hoàn tồn kho cho đơn hàng.");

        order.Status    = "Cancelled";
        order.UpdatedAt = DateTime.UtcNow;
        await _db.SaveChangesAsync();

        await NotifyStatusChange(order);
        return Ok(true);
    }

    public async Task<ApiResponse<OrderResponse>> ConfirmReceivedAsync(int orderId, int userId)
    {
        var order = await _db.Orders.Include(o => o.Items).FirstOrDefaultAsync(o => o.Id == orderId);
        if (order == null) return Fail<OrderResponse>("Đơn hàng không tồn tại.");
        if (order.UserId != userId) return Fail<OrderResponse>("Bạn không có quyền xác nhận đơn này.");
        if (order.Status == "Delivered") return Ok(ToResponse(order));
        if (order.Status != "AwaitingReceipt")
            return Fail<OrderResponse>("Chỉ có thể xác nhận đã nhận hàng khi đơn đang chờ khách xác nhận.");

        order.Status      = "Delivered";
        order.DeliveredAt = DateTime.UtcNow;
        order.UpdatedAt   = DateTime.UtcNow;

        await _db.SaveChangesAsync();
        await NotifyStatusChange(order);
        await _hub.Clients.Group("Admins")
            .SendAsync("OrderReceived", new { order.Id, order.UserName, order.TotalAmount });

        return Ok(ToResponse(order));
    }

    // ── ADMIN/STAFF: XEM TẤT CẢ ──────────────────────────────────
    public async Task<ApiResponse<List<OrderResponse>>> GetAllAsync(string? status)
    {
        var q = _db.Orders.Include(o => o.Items).AsQueryable();
        if (!string.IsNullOrEmpty(status))
            q = q.Where(o => o.Status == status);
        var orders = await q.OrderByDescending(o => o.CreatedAt).ToListAsync();
        return Ok(orders.Select(ToResponse).ToList());
    }

    // ── ADMIN/STAFF: CẬP NHẬT TRẠNG THÁI ────────────────────────
    public async Task<ApiResponse<OrderResponse>> UpdateStatusAsync(int orderId, string newStatus, int actorId, string actorName)
    {
        var allowed = new[] { "Pending", "Confirmed", "Shipping", "AwaitingReceipt", "Delivered", "Cancelled" };
        if (!allowed.Contains(newStatus))
            return Fail<OrderResponse>($"Trạng thái '{newStatus}' không hợp lệ.");

        var order = await _db.Orders.Include(o => o.Items).FirstOrDefaultAsync(o => o.Id == orderId);
        if (order == null) return Fail<OrderResponse>("Đơn hàng không tồn tại.");

        var oldStatus = order.Status;
        if (oldStatus == newStatus)
            return Ok(ToResponse(order));
        if (newStatus == "Shipping" && oldStatus == "Confirmed")
            return Fail<OrderResponse>("Vui lòng dùng chức năng Tạo vận đơn để chuyển đơn sang Đang giao.");
        if (newStatus == "Delivered")
            return Fail<OrderResponse>("Khách hàng phải bấm Đã nhận được hàng để hoàn tất đơn.");
        if (!CanMoveStatus(oldStatus, newStatus))
            return Fail<OrderResponse>($"Không thể chuyển trạng thái đơn hàng từ '{oldStatus}' sang '{newStatus}'. Vui lòng tải lại danh sách nếu trạng thái đã thay đổi.");

        if (!order.IsCod && oldStatus == "Pending" && newStatus == "Confirmed")
            return Fail<OrderResponse>("Don PayOS chi duoc xac nhan khi PayOS webhook bao thanh toan thanh cong.");

        if (newStatus == "Cancelled")
        {
            var releaseResult = await _catalog.ReleaseInventoryAsync(order.Items);
            if (releaseResult.Success != true)
                return Fail<OrderResponse>(releaseResult.Message ?? "Không thể hoàn tồn kho cho đơn hàng.");
        }

        order.Status    = newStatus;
        order.UpdatedAt = DateTime.UtcNow;

        await _db.SaveChangesAsync();
        await NotifyStatusChange(order);

        // 📝 Audit log
        await _audit.LogAsync(actorId, actorName, "UpdateStatus", orderId,
            $"{oldStatus} → {newStatus}");

        return Ok(ToResponse(order));
    }

    private static bool CanMoveStatus(string currentStatus, string newStatus)
    {
        var transitions = new Dictionary<string, string[]>
        {
            ["Pending"] = ["Confirmed", "Cancelled"],
            ["Confirmed"] = ["Cancelled"],
            ["Shipping"] = ["AwaitingReceipt"],
            ["AwaitingReceipt"] = ["Shipping"],
            ["Delivered"] = [],
            ["Cancelled"] = []
        };

        return transitions.TryGetValue(currentStatus, out var allowed) && allowed.Contains(newStatus);
    }

    // ── ADMIN/STAFF: TẠO VẬN ĐƠN VỚI HÃNG SHIP NGOÀI ────────────
    public async Task<ApiResponse<ShipmentCreatedResponse>> CreateShipmentAsync(
        int orderId, int actorId, string actorName, CreateShipmentRequest req)
    {
        var order = await _db.Orders.Include(o => o.Items).FirstOrDefaultAsync(o => o.Id == orderId);
        if (order == null)
            return Fail<ShipmentCreatedResponse>("Đơn hàng không tồn tại.");
        if (order.Status != "Confirmed")
            return Fail<ShipmentCreatedResponse>("Chỉ có thể tạo vận đơn khi đơn ở trạng thái 'Đã xác nhận'.");
        if (!string.IsNullOrEmpty(order.TrackingCode))
            return Fail<ShipmentCreatedResponse>($"Đơn hàng đã có mã vận đơn: {order.TrackingCode}");

        var result = await _shipping.CreateShipmentAsync(
            order.UserName, order.CustomerPhone, order.ShippingAddress,
            order.IsCod ? order.TotalAmount : 0, req.Note ?? order.Note);

        if (!result.Success)
            return Fail<ShipmentCreatedResponse>(result.ErrorMessage ?? "Tạo vận đơn thất bại.");

        order.TrackingCode     = result.TrackingCode;
        order.ShippingProvider = result.Provider;
        order.TrackingUrl      = result.TrackingUrl;
        order.Status           = "Shipping";
        order.ShippedAt        = DateTime.UtcNow;
        order.UpdatedAt        = DateTime.UtcNow;
        await _db.SaveChangesAsync();

        await NotifyStatusChange(order);
        await _hub.Clients.Group("Admins")
            .SendAsync("ShipmentCreated", new { orderId, trackingCode = result.TrackingCode, provider = result.Provider });

        // 📝 Audit log
        await _audit.LogAsync(actorId, actorName, "CreateShipment", orderId,
            $"Tracking: {result.TrackingCode} ({result.Provider})");

        return Ok(new ShipmentCreatedResponse(result.TrackingCode, result.Provider, result.TrackingUrl,
            $"Vận đơn đã được tạo thành công. Mã: {result.TrackingCode}"));
    }

    // ── ADMIN: THỐNG KÊ ──────────────────────────────────────────
    public async Task<ApiResponse<OrderStatsResponse>> GetStatsAsync()
    {
        var orders  = await _db.Orders.ToListAsync();
        var monthly = orders
            .Where(o => o.Status == "Delivered")
            .GroupBy(o => new { o.CreatedAt.Year, o.CreatedAt.Month })
            .Select(g => new MonthlyRevenueItem(g.Key.Year, g.Key.Month, g.Sum(o => o.TotalAmount), g.Count()))
            .OrderBy(m => m.Year).ThenBy(m => m.Month)
            .ToList();

        return Ok(new OrderStatsResponse(
            TotalOrders:    orders.Count,
            TotalRevenue:   orders.Where(o => o.Status == "Delivered").Sum(o => o.TotalAmount),
            PendingCount:   orders.Count(o => o.Status == "Pending"),
            ConfirmedCount: orders.Count(o => o.Status == "Confirmed"),
            ShippingCount:  orders.Count(o => o.Status == "Shipping"),
            AwaitingReceiptCount: orders.Count(o => o.Status == "AwaitingReceipt"),
            DeliveredCount: orders.Count(o => o.Status == "Delivered"),
            CancelledCount: orders.Count(o => o.Status == "Cancelled"),
            MonthlyRevenue: monthly));
    }

    public async Task<byte[]?> ExportExcelAsync()
    {
        var orders = await _db.Orders.Include(o => o.Items)
            .OrderByDescending(o => o.CreatedAt)
            .ToListAsync();

        using var workbook = new XLWorkbook();
        var sheet = workbook.Worksheets.Add("Orders");

        var headers = new[]
        {
            "Id", "Customer", "Phone", "Address", "Status", "PaymentMethod", "PaymentStatus",
            "Total", "Items", "TrackingCode", "ShippingProvider", "CreatedAt", "UpdatedAt"
        };

        for (var i = 0; i < headers.Length; i++)
            sheet.Cell(1, i + 1).Value = headers[i];

        var row = 2;
        foreach (var order in orders)
        {
            sheet.Cell(row, 1).Value = order.Id;
            sheet.Cell(row, 2).Value = order.UserName;
            sheet.Cell(row, 3).Value = order.CustomerPhone;
            sheet.Cell(row, 4).Value = order.ShippingAddress;
            sheet.Cell(row, 5).Value = order.Status;
            sheet.Cell(row, 6).Value = GetPaymentMethod(order);
            sheet.Cell(row, 7).Value = GetPaymentStatus(order);
            sheet.Cell(row, 8).Value = order.TotalAmount;
            sheet.Cell(row, 9).Value = string.Join("; ", order.Items.Select(i =>
                $"{(i.ProductName ?? i.PetName ?? $"Item #{i.Id}")} x{i.Quantity}"));
            sheet.Cell(row, 10).Value = order.TrackingCode;
            sheet.Cell(row, 11).Value = order.ShippingProvider;
            sheet.Cell(row, 12).Value = order.CreatedAt;
            sheet.Cell(row, 13).Value = order.UpdatedAt;
            row++;
        }

        var usedRange = sheet.RangeUsed();
        if (usedRange != null)
        {
            usedRange.Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
            usedRange.Style.Border.InsideBorder = XLBorderStyleValues.Thin;
        }

        sheet.Row(1).Style.Font.Bold = true;
        sheet.Row(1).Style.Fill.BackgroundColor = XLColor.LightGray;
        sheet.Column(8).Style.NumberFormat.Format = "#,##0";
        sheet.Columns().AdjustToContents();

        using var stream = new MemoryStream();
        workbook.SaveAs(stream);
        return stream.ToArray();
    }

    // ── AUDIT LOG ─────────────────────────────────────────────────
    public async Task<ApiResponse<List<AuditLogResponse>>> GetAuditLogsAsync(int? staffId, int page)
    {
        var logs = staffId.HasValue
            ? await _audit.GetByStaffAsync(staffId.Value, page)
            : await _audit.GetAllAsync(page);
        return Ok(logs);
    }

    public async Task<ApiResponse<bool>> HasDeliveredItemAsync(int userId, int? productId, int? petId)
    {
        if (productId.HasValue == petId.HasValue)
            return Fail<bool>("Vui long chon dung mot san pham hoac mot thu cung.");

        var exists = await _db.Orders
            .Where(o => o.UserId == userId && o.Status == "Delivered")
            .AnyAsync(o => o.Items.Any(i =>
                productId.HasValue
                    ? i.ProductId == productId.Value
                    : i.PetId == petId!.Value));

        return Ok(exists);
    }

    // ── HELPERS ──────────────────────────────────────────────────
    private async Task NotifyStatusChange(Order.Service.Entities.Order order)
    {
        var notification = new OrderStatusChangedNotification(
            order.Id, order.Status, order.UpdatedAt ?? order.CreatedAt);

        await _hub.Clients.Group($"User_{order.UserId}")
            .SendAsync("OrderStatusChanged", notification);
    }

    private static OrderResponse ToResponse(Order.Service.Entities.Order o) => new(
        Id:               o.Id,
        UserId:           o.UserId,
        UserName:         o.UserName,
        CustomerPhone:    o.CustomerPhone,
        ShippingAddress:  o.ShippingAddress,
        Note:             o.Note,
        Status:           o.Status,
        TotalAmount:      o.TotalAmount,
        IsCod:            o.IsCod,
        PaymentMethod:    GetPaymentMethod(o),
        PaymentStatus:    GetPaymentStatus(o),
        TrackingCode:     o.TrackingCode,
        ShippingProvider: o.ShippingProvider,
        TrackingUrl:      o.TrackingUrl,
        FailureReason:    o.FailureReason,
        Items:            o.Items?.Select(i => new OrderItemResponse(
            i.Id, i.ProductId, i.ProductName, i.PetId, i.PetName, i.Quantity, i.UnitPrice, i.SubTotal
        )).ToList() ?? [],
        CreatedAt:        o.CreatedAt,
        UpdatedAt:        o.UpdatedAt,
        ShippedAt:        o.ShippedAt,
        DeliveredAt:      o.DeliveredAt);

    private static int ToPayOsAmount(decimal amount)
        => amount <= 0 || amount > int.MaxValue ? 0 : (int)decimal.Round(amount, 0, MidpointRounding.AwayFromZero);

    private static string GetPaymentMethod(Order.Service.Entities.Order order)
        => order.IsCod ? "COD" : "PayOS";

    private static string GetPaymentStatus(Order.Service.Entities.Order order)
    {
        if (order.Status == "Cancelled") return "Cancelled";
        if (order.IsCod) return order.Status == "Delivered" ? "CODCollected" : "CODPending";
        return order.Status == "Pending" ? "PayOSPending" : "Paid";
    }

    private static ApiResponse<T> Ok<T>(T data)   => new(true, null, data);
    private static ApiResponse<T> Fail<T>(string msg) => new(false, msg, default);
}
