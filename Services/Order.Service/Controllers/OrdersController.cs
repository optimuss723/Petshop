using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Order.Service.DTOs;
using Order.Service.Services;

namespace Order.Service.Controllers;

[ApiController]
[Route("api/orders")]
[Authorize]
public class OrdersController : ControllerBase
{
    private readonly IOrderService _svc;
    private readonly IConfiguration _config;
    private readonly IPayOsWebhookVerifier _payOsVerifier;
    private int    UserId    => int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);
    private string UserName  => User.FindFirst(ClaimTypes.Name)?.Value
                             ?? User.FindFirst("name")?.Value
                             ?? "Unknown";
    private string CustomerPhone => User.FindFirst(ClaimTypes.MobilePhone)?.Value
                                 ?? User.FindFirst("phone")?.Value
                                 ?? "";
    private string UserRole  => User.FindFirst(ClaimTypes.Role)?.Value ?? "";
    private bool   IsAdminOrStaff => UserRole == "Admin" || UserRole == "Staff";

    public OrdersController(IOrderService svc, IConfiguration config, IPayOsWebhookVerifier payOsVerifier)
    {
        _svc = svc;
        _config = config;
        _payOsVerifier = payOsVerifier;
    }

    // ── CUSTOMER ─────────────────────────────────────────────────

    /// <summary>Customer xem đơn của mình</summary>
    [HttpGet("my")]
    public async Task<IActionResult> GetMy()
        => Ok(await _svc.GetMyOrdersAsync(UserId));

    /// <summary>Customer đặt hàng</summary>
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateOrderRequest req)
        => Ok(await _svc.CreateAsync(req, UserId, UserName, CustomerPhone));

    /// <summary>Customer thanh toán đơn hàng đang chờ</summary>
    [HttpPost("{id:int}/pay")]
    public async Task<IActionResult> Pay(int id, [FromBody] ConfirmPaymentRequest req)
        => Ok(await _svc.PayAsync(id, UserId, req.PaymentCode));

    /// <summary>Customer tao link thanh toan PayOS that cho don hang dang cho</summary>
    [HttpPost("{id:int}/payos/link")]
    public async Task<IActionResult> CreatePayOsPaymentLink(int id)
        => Ok(await _svc.CreatePayOsPaymentLinkAsync(id, UserId));

    /// <summary>Xem chi tiết đơn — Customer chỉ xem đơn của mình, Admin/Staff xem tất cả</summary>
    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetById(int id)
        => Ok(await _svc.GetByIdAsync(id, IsAdminOrStaff ? null : UserId));

    /// <summary>Customer hủy đơn</summary>
    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Cancel(int id)
        => Ok(await _svc.CancelAsync(id, UserId));

    /// <summary>Customer xác nhận đã nhận được hàng</summary>
    [HttpPost("{id:int}/confirm-received")]
    public async Task<IActionResult> ConfirmReceived(int id)
        => Ok(await _svc.ConfirmReceivedAsync(id, UserId));

    // ── ADMIN / STAFF ─────────────────────────────────────────────

    /// <summary>Admin/Staff xem tất cả đơn, có thể lọc theo status</summary>
    [HttpGet]
    [Authorize(Roles = "Admin,Staff")]
    public async Task<IActionResult> GetAll([FromQuery] string? status)
        => Ok(await _svc.GetAllAsync(status));

    /// <summary>Staff cập nhật trạng thái đơn hàng</summary>
    [HttpPatch("{id:int}/status")]
    [Authorize(Roles = "Admin,Staff")]
    public async Task<IActionResult> UpdateStatus(int id, [FromBody] UpdateOrderStatusRequest req)
        => Ok(await _svc.UpdateStatusAsync(id, req.Status, UserId, UserName));

    /// <summary>
    /// Staff tạo vận đơn — gọi hãng ship ngoài (GHN Mock).
    /// </summary>
    [HttpPost("{id:int}/ship")]
    [Authorize(Roles = "Admin,Staff")]
    public async Task<IActionResult> CreateShipment(int id, [FromBody] CreateShipmentRequest req)
        => Ok(await _svc.CreateShipmentAsync(id, UserId, UserName, req));

    /// <summary>Xem lịch sử thao tác — Staff xem của mình, Admin xem tất cả</summary>
    [HttpGet("audit-logs")]
    [Authorize(Roles = "Admin,Staff")]
    public async Task<IActionResult> AuditLogs([FromQuery] int? staffId, [FromQuery] int page = 1)
    {
        // Staff chỉ xem log của chính mình
        var queryStaffId = UserRole == "Staff" ? (int?)UserId : staffId;
        return Ok(await _svc.GetAuditLogsAsync(queryStaffId, page));
    }

    /// <summary>Admin thống kê đơn hàng</summary>
    [HttpGet("stats")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Stats()
        => Ok(await _svc.GetStatsAsync());

    /// <summary>Admin export Excel</summary>
    [HttpGet("export")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Export()
    {
        var bytes = await _svc.ExportExcelAsync();
        if (bytes == null) return NoContent();
        return File(bytes, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "orders.xlsx");
    }

    [HttpGet("internal/purchases/validate")]
    [AllowAnonymous]
    public async Task<IActionResult> ValidateDeliveredPurchase(
        [FromQuery] int userId,
        [FromQuery] int? productId,
        [FromQuery] int? petId)
    {
        if (!IsInternalRequest())
            return Unauthorized();

        return Ok(await _svc.HasDeliveredItemAsync(userId, productId, petId));
    }

    [HttpPost("payments/payos/webhook")]
    [AllowAnonymous]
    public async Task<IActionResult> PayOsWebhook([FromBody] PayOsWebhookRequest request)
    {
        if (request.Success == true
            && request.Data != null
            && request.Data.Code == "00"
            && IsPayOsVerificationSample(request.Data))
        {
            return Ok(new { success = true });
        }

        if (!_payOsVerifier.Verify(request))
            return Unauthorized(new { success = false, message = "Invalid PayOS signature." });

        if (request.Success != true || request.Data == null || request.Data.Code != "00")
            return Ok(new { success = true });

        if (request.Data.OrderCode > int.MaxValue)
            return BadRequest(new { success = false, message = "Invalid order code." });

        var result = await _svc.ConfirmPaidAsync(
            (int)request.Data.OrderCode,
            request.Data.Reference,
            request.Data.Amount,
            request.Data.PaymentLinkId);
        if (result.Success != true)
            return BadRequest(new { success = false, message = result.Message });

        return Ok(new { success = true });
    }

    private static bool IsPayOsVerificationSample(PayOsWebhookData data)
        => data.OrderCode == 123
        && data.Amount == 3000
        && string.Equals(data.Description, "VQRIO123", StringComparison.OrdinalIgnoreCase)
        && string.Equals(data.Reference, "TF230204212323", StringComparison.OrdinalIgnoreCase);

    private bool IsInternalRequest()
    {
        var configuredKey = _config["InternalApiKey"] ?? "dev-internal-key";
        return Request.Headers.TryGetValue("X-Internal-Key", out var key)
            && key.ToString() == configuredKey;
    }
}
