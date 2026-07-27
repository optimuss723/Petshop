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
    private static readonly JsonSerializerOptions _jsonOptions = new()
    {
        PropertyNameCaseInsensitive = true,
        Converters = { new JsonStringEnumConverter() }
    };

    public CatalogLookupService(HttpClient http, IConfiguration config)
    {
        _http = http;
        _config = config;
    }

    public async Task<ApiResponse<OrderItem>> BuildOrderItemAsync(CreateOrderItemRequest request)
    {
        if (request.ProductId.HasValue == request.PetId.HasValue)
            return Fail("Mỗi dòng đơn hàng phải chọn đúng một sản phẩm hoặc một thú cưng.");

        var quantity = Math.Max(1, request.Quantity);

        if (request.ProductId.HasValue)
        {
            var product = await GetAsync<CatalogProductResponse>($"api/products/{request.ProductId.Value}");
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

        var pet = await GetAsync<CatalogPetResponse>($"api/pets/{request.PetId!.Value}");
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

    private async Task<T?> GetAsync<T>(string url)
    {
        using var response = await _http.GetAsync(url);
        if (!response.IsSuccessStatusCode)
            return default;

        await using var stream = await response.Content.ReadAsStreamAsync();
        var apiResponse = await JsonSerializer.DeserializeAsync<ApiResponse<T>>(stream, _jsonOptions);
        return apiResponse?.Success == true ? apiResponse.Data : default;
    }

    public async Task<ApiResponse<bool>> CommitInventoryAsync(IEnumerable<OrderItem> items)
        => await SendInventoryRequestAsync("api/internal/catalog/inventory/commit", items);

    public async Task<ApiResponse<bool>> ReleaseInventoryAsync(IEnumerable<OrderItem> items)
        => await SendInventoryRequestAsync("api/internal/catalog/inventory/release", items);

    private async Task<ApiResponse<bool>> SendInventoryRequestAsync(string url, IEnumerable<OrderItem> items)
    {
        var requestBody = new InventoryCommitRequest(
            items.Select(i => new InventoryCommitItemRequest(i.ProductId, i.PetId, i.Quantity)).ToList());

        using var request = new HttpRequestMessage(HttpMethod.Post, url)
        {
            Content = JsonContent.Create(requestBody, options: _jsonOptions)
        };
        request.Headers.Add("X-Internal-Key", _config["InternalApiKey"] ?? "dev-internal-key");

        using var response = await _http.SendAsync(request);
        if (!response.IsSuccessStatusCode)
            return new ApiResponse<bool>(false, "Khong the cap nhat ton kho.", false);

        var result = await response.Content.ReadFromJsonAsync<ApiResponse<bool>>(_jsonOptions);
        return result ?? new ApiResponse<bool>(false, "Khong doc duoc phan hoi ton kho.", false);
    }

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
