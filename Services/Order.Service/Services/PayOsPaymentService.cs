using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using Order.Service.DTOs;
using OrderEntity = Order.Service.Entities.Order;

namespace Order.Service.Services;

public interface IPayOsPaymentService
{
    Task<ApiResponse<PayOsPaymentLinkResponse>> CreatePaymentLinkAsync(OrderEntity order);
}

public class PayOsPaymentService : IPayOsPaymentService
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        PropertyNameCaseInsensitive = true,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
    };

    private readonly HttpClient _http;
    private readonly IConfiguration _config;

    public PayOsPaymentService(HttpClient http, IConfiguration config)
    {
        _http = http;
        _config = config;
    }

    public async Task<ApiResponse<PayOsPaymentLinkResponse>> CreatePaymentLinkAsync(OrderEntity order)
    {
        var clientId = _config["PayOS:ClientId"];
        var apiKey = _config["PayOS:ApiKey"];
        var checksumKey = _config["PayOS:ChecksumKey"];

        if (string.IsNullOrWhiteSpace(clientId) || string.IsNullOrWhiteSpace(apiKey) || string.IsNullOrWhiteSpace(checksumKey)
            || clientId == "CHANGE_ME"
            || apiKey == "CHANGE_ME"
            || checksumKey == "CHANGE_ME")
        {
            return Fail("Chua cau hinh PayOS ClientId/ApiKey/ChecksumKey.");
        }

        var amount = ToPayOsAmount(order.TotalAmount);
        if (amount <= 0)
            return Fail("So tien thanh toan PayOS khong hop le.");

        var orderCode = order.Id;
        var description = BuildDescription(order.Id);
        var cancelUrl = BuildUrl("CancelUrl", order.Id);
        var returnUrl = BuildUrl("ReturnUrl", order.Id);
        var signature = CreateSignature(amount, cancelUrl, description, orderCode, returnUrl, checksumKey);

        var payload = new PayOsCreatePaymentLinkRequest(
            OrderCode: orderCode,
            Amount: amount,
            Description: description,
            BuyerName: order.UserName,
            BuyerPhone: order.CustomerPhone,
            Items: order.Items.Select(i => new PayOsPaymentItemRequest(
                Name: i.ProductName ?? i.PetName ?? $"Item {i.Id}",
                Quantity: Math.Max(1, i.Quantity),
                Price: ToPayOsAmount(i.UnitPrice))).ToList(),
            CancelUrl: cancelUrl,
            ReturnUrl: returnUrl,
            Signature: signature);

        using var request = new HttpRequestMessage(HttpMethod.Post, "v2/payment-requests");
        request.Headers.TryAddWithoutValidation("x-client-id", clientId);
        request.Headers.TryAddWithoutValidation("x-api-key", apiKey);

        var partnerCode = _config["PayOS:PartnerCode"];
        if (!string.IsNullOrWhiteSpace(partnerCode))
            request.Headers.TryAddWithoutValidation("x-partner-code", partnerCode);

        var json = JsonSerializer.Serialize(payload, JsonOptions);
        request.Content = new StringContent(json, Encoding.UTF8, "application/json");

        using var response = await _http.SendAsync(request);
        var body = await response.Content.ReadAsStringAsync();

        PayOsApiResponse<PayOsCreatePaymentLinkData>? result;
        try
        {
            result = string.IsNullOrWhiteSpace(body)
                ? null
                : JsonSerializer.Deserialize<PayOsApiResponse<PayOsCreatePaymentLinkData>>(body, JsonOptions);
        }
        catch (JsonException)
        {
            return Fail($"PayOS tra ve du lieu khong hop le: {body}");
        }

        if (!response.IsSuccessStatusCode || result?.Code != "00" || result.Data == null)
        {
            var message = result?.Desc ?? $"PayOS error {(int)response.StatusCode}";
            return Fail(message);
        }

        if (string.IsNullOrWhiteSpace(result.Data.CheckoutUrl) || string.IsNullOrWhiteSpace(result.Data.PaymentLinkId))
            return Fail("PayOS khong tra ve checkoutUrl hop le.");

        return Ok(new PayOsPaymentLinkResponse(
            OrderCode: result.Data.OrderCode,
            Amount: result.Data.Amount,
            Description: result.Data.Description ?? description,
            PaymentLinkId: result.Data.PaymentLinkId,
            CheckoutUrl: result.Data.CheckoutUrl,
            QrCode: result.Data.QrCode ?? "",
            Status: result.Data.Status ?? "PENDING"));
    }

    private string BuildUrl(string key, int orderId)
    {
        var template = _config[$"PayOS:{key}"];
        if (string.IsNullOrWhiteSpace(template))
            template = "http://localhost:50140/User/Orders?expand={orderId}";

        return template
            .Replace("{orderId}", orderId.ToString(), StringComparison.OrdinalIgnoreCase)
            .Replace("{orderCode}", orderId.ToString(), StringComparison.OrdinalIgnoreCase);
    }

    private static string CreateSignature(int amount, string cancelUrl, string description, long orderCode, string returnUrl, string checksumKey)
    {
        var signedData = $"amount={amount}&cancelUrl={cancelUrl}&description={description}&orderCode={orderCode}&returnUrl={returnUrl}";
        using var hmac = new HMACSHA256(Encoding.UTF8.GetBytes(checksumKey));
        var hash = hmac.ComputeHash(Encoding.UTF8.GetBytes(signedData));
        return Convert.ToHexString(hash).ToLowerInvariant();
    }

    private static string BuildDescription(int orderId)
    {
        var value = $"PS{orderId}";
        return value.Length <= 9 ? value : $"PS{orderId % 10_000_000:0000000}";
    }

    private static int ToPayOsAmount(decimal amount)
        => amount <= 0 || amount > int.MaxValue ? 0 : (int)decimal.Round(amount, 0, MidpointRounding.AwayFromZero);

    private static ApiResponse<PayOsPaymentLinkResponse> Ok(PayOsPaymentLinkResponse data)
        => new(true, null, data);

    private static ApiResponse<PayOsPaymentLinkResponse> Fail(string message)
        => new(false, message, null);

    private sealed record PayOsCreatePaymentLinkRequest(
        long OrderCode,
        int Amount,
        string Description,
        string? BuyerName,
        string? BuyerPhone,
        List<PayOsPaymentItemRequest> Items,
        string CancelUrl,
        string ReturnUrl,
        string Signature);

    private sealed record PayOsPaymentItemRequest(
        string Name,
        int Quantity,
        int Price);

    private sealed class PayOsApiResponse<T>
    {
        public string? Code { get; init; }
        public string? Desc { get; init; }
        public T? Data { get; init; }
        public string? Signature { get; init; }
    }

    private sealed class PayOsCreatePaymentLinkData
    {
        public int Amount { get; init; }
        public string? Description { get; init; }
        public long OrderCode { get; init; }
        public string? Currency { get; init; }
        public string? PaymentLinkId { get; init; }
        public string? Status { get; init; }
        public string? CheckoutUrl { get; init; }
        public string? QrCode { get; init; }
    }
}
