using System.Text.Json;
using System.Text.Json.Serialization;
using System.Net.Http.Json;
using Order.Service.DTOs;
using Order.Service.Entities;

namespace Order.Service.Services;

public interface ICatalogLookupService
{
    Task<ApiResponse<OrderItem>> BuildOrderItemAsync(CreateOrderItemRequest request);
    Task<ApiResponse<bool>> CommitInventoryAsync(IEnumerable<OrderItem> items);
    Task<ApiResponse<bool>> ReleaseInventoryAsync(IEnumerable<OrderItem> items);
}

public class CatalogLookupService : ICatalogLookupService
{
    private readonly HttpClient _http;
    private readonly IConfiguration _config;
    private readonly ILogger<CatalogLookupService> _logger;

    private static readonly JsonSerializerOptions _jsonOptions = new()
    {
        PropertyNameCaseInsensitive = true,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        Converters = { new JsonStringEnumConverter() }
    };

    public CatalogLookupService(HttpClient http, IConfiguration config, ILogger<CatalogLookupService> logger)
    {
        _http = http;
        _config = config;
        _logger = logger;
    }

    public async Task<ApiResponse<OrderItem>> BuildOrderItemAsync(CreateOrderItemRequest request)
    {
        if (request.ProductId.HasValue == request.PetId.HasValue)
            return Fail("Mỗi dòng đơn hàng phải chọn đúng một sản phẩm hoặc một thú cưng.");

        var quantity = Math.Max(1, request.Quantity);

        if (request.ProductId.HasValue)
        {
            var (product, error) = await GetAsync<CatalogProductResponse>($"api/products/{request.ProductId.Value}");
            if (error != null)
                return Fail(error);
            if (product == null)
                return Fail($"Sản phẩm #{request.ProductId.Value} không tồn tại.");
            if (product.Stock < quantity)
                return Fail($"Sản phẩm '{product.Name}' chỉ còn {product.Stock} trong kho.");

            return Ok(new OrderItem
            {
                ProductId = product.Id,
                ProductName = product.Name,
                Quantity = quantity,
                UnitPrice = product.Price
            });
        }

        var (pet, petError) = await GetAsync<CatalogPetResponse>($"api/pets/{request.PetId!.Value}");
        if (petError != null)
            return Fail(petError);
        if (pet == null)
            return Fail($"Thú cưng #{request.PetId.Value} không tồn tại.");
        if (!string.Equals(pet.Status, "Available", StringComparison.OrdinalIgnoreCase))
            return Fail($"Thú cưng '{pet.Name}' hiện không còn sẵn sàng để bán.");

        return Ok(new OrderItem
        {
            PetId = pet.Id,
            PetName = pet.Name,
            Quantity = 1,
            UnitPrice = pet.Price
        });
    }

    /// <summary>
    /// Gọi GET API của Catalog.Service và parse response.
    /// Trả về (data, errorMessage). errorMessage != null nghĩa là lỗi kết nối/HTTP.
    /// data == null && errorMessage == null nghĩa là API trả về success=false (không tìm thấy).
    /// </summary>
    private async Task<(T? Data, string? Error)> GetAsync<T>(string url)
    {
        try
        {
            using var response = await _http.GetAsync(url);

            if (!response.IsSuccessStatusCode)
            {
                var body = await response.Content.ReadAsStringAsync();
                _logger.LogWarning(
                    "CatalogLookup GET {Url} failed: HTTP {Status}, Body: {Body}",
                    url, (int)response.StatusCode, Truncate(body, 300));

                return (default, $"Không thể kết nối đến Catalog.Service (HTTP {(int)response.StatusCode}). "
                    + "Vui lòng đảm bảo Catalog.Service đang chạy tại http://localhost:5002.");
            }

            await using var stream = await response.Content.ReadAsStreamAsync();
            var apiResponse = await JsonSerializer.DeserializeAsync<ApiResponse<T>>(stream, _jsonOptions);

            if (apiResponse == null)
                return (default, "Không thể đọc phản hồi từ Catalog.Service (response rỗng).");

            return apiResponse.Success ? (apiResponse.Data, null) : (default, null);
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "CatalogLookup GET {Url} connection failed: {Message}", url, ex.Message);
            return (default, "Không thể kết nối đến Catalog.Service (http://localhost:5002). "
                + "Vui lòng đảm bảo Catalog.Service đang chạy.");
        }
        catch (TaskCanceledException ex) when (!ex.CancellationToken.IsCancellationRequested)
        {
            _logger.LogError(ex, "CatalogLookup GET {Url} timeout", url);
            return (default, "Kết nối đến Catalog.Service bị timeout. Vui lòng thử lại.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "CatalogLookup GET {Url} unexpected error: {Message}", url, ex.Message);
            return (default, $"Lỗi khi gọi Catalog.Service: {ex.Message}");
        }
    }

    public async Task<ApiResponse<bool>> CommitInventoryAsync(IEnumerable<OrderItem> items)
        => await SendInventoryRequestAsync("api/internal/catalog/inventory/commit", items);

    public async Task<ApiResponse<bool>> ReleaseInventoryAsync(IEnumerable<OrderItem> items)
        => await SendInventoryRequestAsync("api/internal/catalog/inventory/release", items);

    private async Task<ApiResponse<bool>> SendInventoryRequestAsync(string url, IEnumerable<OrderItem> items)
    {
        var requestBody = new InventoryCommitRequest(
            items.Select(i => new InventoryCommitItemRequest(i.ProductId, i.PetId, i.Quantity)).ToList());

        try
        {
            using var request = new HttpRequestMessage(HttpMethod.Post, url)
            {
                Content = JsonContent.Create(requestBody, options: _jsonOptions)
            };
            request.Headers.Add("X-Internal-Key", _config["InternalApiKey"] ?? "dev-internal-key");

            using var response = await _http.SendAsync(request);

            if (!response.IsSuccessStatusCode)
            {
                var body = await response.Content.ReadAsStringAsync();
                _logger.LogWarning(
                    "CatalogLookup POST {Url} failed: HTTP {Status}, Body: {Body}",
                    url, (int)response.StatusCode, Truncate(body, 500));

                // 401 = sai InternalApiKey
                if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
                    return new ApiResponse<bool>(false,
                        "Lỗi xác thực nội bộ giữa Order.Service và Catalog.Service. "
                        + "Kiểm tra InternalApiKey trong appsettings.json của cả 2 service phải giống nhau.", false);

                return new ApiResponse<bool>(false,
                    $"Không thể cập nhật tồn kho (HTTP {(int)response.StatusCode}). "
                    + "Vui lòng đảm bảo Catalog.Service đang chạy tại http://localhost:5002.", false);
            }

            var result = await response.Content.ReadFromJsonAsync<ApiResponse<bool>>(_jsonOptions);
            if (result == null)
                return new ApiResponse<bool>(false, "Không thể đọc phản hồi tồn kho từ Catalog.Service.", false);

            // Trả về kết quả từ Catalog.Service (có thể success=true hoặc success=false với message chi tiết)
            return result;
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "CatalogLookup POST {Url} connection failed: {Message}", url, ex.Message);
            return new ApiResponse<bool>(false,
                "Không thể kết nối đến Catalog.Service (http://localhost:5002). "
                + "Vui lòng đảm bảo Catalog.Service đang chạy trước khi đặt hàng.", false);
        }
        catch (TaskCanceledException ex) when (!ex.CancellationToken.IsCancellationRequested)
        {
            _logger.LogError(ex, "CatalogLookup POST {Url} timeout", url);
            return new ApiResponse<bool>(false, "Kết nối đến Catalog.Service bị timeout. Vui lòng thử lại.", false);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "CatalogLookup POST {Url} unexpected error: {Message}", url, ex.Message);
            return new ApiResponse<bool>(false, $"Lỗi khi cập nhật tồn kho: {ex.Message}", false);
        }
    }

    private static string Truncate(string value, int maxLength)
        => value.Length <= maxLength ? value : value[..maxLength] + "...";

    private static ApiResponse<OrderItem> Ok(OrderItem item) => new(true, null, item);
    private static ApiResponse<OrderItem> Fail(string message) => new(false, message, null);

    private record CatalogProductResponse(
        int Id,
        string Name,
        string Description,
        decimal Price,
        int Stock,
        string Category,
        string? ImageUrl,
        bool IsActive,
        DateTime CreatedAt);

    private record CatalogPetResponse(
        int Id,
        string Name,
        string Breed,
        string Category,
        int AgeMonths,
        decimal Price,
        string Description,
        string? ImageUrl,
        string Status,
        DateTime CreatedAt);

    private record InventoryCommitRequest(List<InventoryCommitItemRequest> Items);
    private record InventoryCommitItemRequest(int? ProductId, int? PetId, int Quantity);
}
