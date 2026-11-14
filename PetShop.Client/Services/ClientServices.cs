using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using PetShop.Shared.DTOs.Request;
using PetShop.Shared.DTOs.Response;

namespace PetShop.Client.Services;

public class ODataListResponse<T>
{
    [JsonPropertyName("@odata.count")]
    public int? Count { get; set; }

    public List<T> Value { get; set; } = [];
}

// ── BASE API CLIENT ───────────────────────────────────────────────
public interface IApiClient
{
    Task<T?> GetAsync<T>(string url);
    Task<T?> PostAsync<T>(string url, object body);
    Task<T?> PutAsync<T>(string url, object body);
    Task<T?> PatchAsync<T>(string url, object body);
    Task<T?> DeleteAsync<T>(string url);
    Task<byte[]?> GetBytesAsync(string url);
}

public class ApiClient : IApiClient
{
    private readonly IHttpClientFactory _factory;
    private readonly IHttpContextAccessor _ctx;
    private static readonly JsonSerializerOptions _opts = new()
    {
        PropertyNameCaseInsensitive = true,
        Converters = { new JsonStringEnumConverter() }
    };

    public ApiClient(IHttpClientFactory factory, IHttpContextAccessor ctx)
    {
        _factory = factory;
        _ctx     = ctx;
    }

    private HttpClient CreateClient()
    {
        var client = _factory.CreateClient("PetShopAPI");
        var token  = _ctx.HttpContext?.Session.GetString("AccessToken");
        if (!string.IsNullOrEmpty(token))
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
        return client;
    }

    public async Task<T?> GetAsync<T>(string url)
    {
        var res = await CreateClient().GetAsync(url);
        return await Deserialize<T>(res);
    }

    public async Task<T?> PostAsync<T>(string url, object body)
    {
        var res = await CreateClient().PostAsync(url, Json(body));
        return await Deserialize<T>(res);
    }

    public async Task<T?> PutAsync<T>(string url, object body)
    {
        var res = await CreateClient().PutAsync(url, Json(body));
        return await Deserialize<T>(res);
    }

    public async Task<T?> PatchAsync<T>(string url, object body)
    {
        var res = await CreateClient().PatchAsync(url, Json(body));
        return await Deserialize<T>(res);
    }

    public async Task<T?> DeleteAsync<T>(string url)
    {
        var res = await CreateClient().DeleteAsync(url);
        return await Deserialize<T>(res);
    }

    public async Task<byte[]?> GetBytesAsync(string url)
    {
        var res = await CreateClient().GetAsync(url);
        if (!res.IsSuccessStatusCode) return null;
        return await res.Content.ReadAsByteArrayAsync();
    }

    private static StringContent Json(object body)
        => new(JsonSerializer.Serialize(body, _opts), Encoding.UTF8, "application/json");

    private static async Task<T?> Deserialize<T>(HttpResponseMessage res)
    {
        var json = await res.Content.ReadAsStringAsync();
        if (!string.IsNullOrWhiteSpace(json))
        {
            try
            {
                var parsed = JsonSerializer.Deserialize<T>(json, _opts);
                if (parsed is not null) return parsed;
            }
            catch (JsonException)
            {
                if (res.IsSuccessStatusCode) return default;
            }
        }

        return res.IsSuccessStatusCode ? default : FailedResponse<T>(res, json);
    }

    private static T? FailedResponse<T>(HttpResponseMessage res, string body)
    {
        var type = typeof(T);
        if (!type.IsGenericType || type.GetGenericTypeDefinition() != typeof(ApiResponse<>))
            return default;

        var dataType = type.GetGenericArguments()[0];
        var data = dataType.IsValueType ? Activator.CreateInstance(dataType) : null;
        var message = ExtractErrorMessage(body) ?? $"{(int)res.StatusCode} {res.ReasonPhrase}".Trim();
        return (T?)Activator.CreateInstance(type, false, message, data);
    }

    private static string? ExtractErrorMessage(string body)
    {
        if (string.IsNullOrWhiteSpace(body)) return null;

        try
        {
            using var doc = JsonDocument.Parse(body);
            var root = doc.RootElement;

            // 1. Kiểm tra validation errors (ProblemDetails: { "errors": { "Field": ["Error"] } })
            if (root.TryGetProperty("errors", out var errorsProp) && errorsProp.ValueKind == JsonValueKind.Object)
            {
                var errList = new List<string>();
                foreach (var prop in errorsProp.EnumerateObject())
                {
                    if (prop.Value.ValueKind == JsonValueKind.Array)
                    {
                        foreach (var errItem in prop.Value.EnumerateArray())
                        {
                            if (errItem.ValueKind == JsonValueKind.String)
                                errList.Add(errItem.GetString()!);
                        }
                    }
                    else if (prop.Value.ValueKind == JsonValueKind.String)
                    {
                        errList.Add(prop.Value.GetString()!);
                    }
                }
                if (errList.Count > 0)
                    return string.Join(" ", errList);
            }

            // 2. Kiểm tra message / error thông thường
            foreach (var name in new[] { "message", "Message", "error", "title" })
            {
                if (root.TryGetProperty(name, out var prop) && prop.ValueKind == JsonValueKind.String)
                {
                    var str = prop.GetString();
                    if (!string.IsNullOrWhiteSpace(str) && str != "One or more validation errors occurred.")
                        return str;
                }
            }
        }
        catch (JsonException)
        {
            return body.Length <= 300 ? body : body[..300];
        }

        return body.Length <= 300 ? body : body[..300];
    }
}

// ── AUTH CLIENT ───────────────────────────────────────────────────
public interface IAuthClientService
{
    Task<ApiResponse<AuthResponse>?> LoginAsync(LoginRequest request);
    Task<ApiResponse<AuthResponse>?> RegisterAsync(RegisterRequest request);
    Task<ApiResponse<bool>?>         ChangePasswordAsync(ChangePasswordRequest request);
}

public class AuthClientService : IAuthClientService
{
    private readonly IApiClient _api;
    public AuthClientService(IApiClient api) => _api = api;

    public Task<ApiResponse<AuthResponse>?> LoginAsync(LoginRequest req)
        => _api.PostAsync<ApiResponse<AuthResponse>>("api/auth/login", req);

    public Task<ApiResponse<AuthResponse>?> RegisterAsync(RegisterRequest req)
        => _api.PostAsync<ApiResponse<AuthResponse>>("api/auth/register", req);

    public Task<ApiResponse<bool>?> ChangePasswordAsync(ChangePasswordRequest req)
        => _api.PostAsync<ApiResponse<bool>>("api/auth/change-password", req);
}

// ── PRODUCT CLIENT ────────────────────────────────────────────────
public interface IProductClientService
{
    Task<ApiResponse<List<ProductResponse>>?>        GetAllAsync();
    Task<ApiResponse<PagedResponse<ProductResponse>>?> SearchAsync(string? search = null, string? category = null, decimal? minPrice = null, decimal? maxPrice = null, int page = 1, int pageSize = 8);
    Task<ODataListResponse<ProductResponse>?>        GetODataAsync(string? search = null, string? category = null, string? orderBy = null, int skip = 0, int top = 8);
    Task<ApiResponse<ProductResponse>?>              GetByIdAsync(int id);
    Task<ApiResponse<ProductResponse>?>              CreateAsync(ProductRequest req);
    Task<ApiResponse<ProductResponse>?>              UpdateAsync(int id, ProductRequest req);
    Task<ApiResponse<bool>?>                         DeleteAsync(int id);
}

public class ProductClientService : IProductClientService
{
    private readonly IApiClient _api;
    public ProductClientService(IApiClient api) => _api = api;

    public Task<ApiResponse<List<ProductResponse>>?> GetAllAsync()
        => _api.GetAsync<ApiResponse<List<ProductResponse>>>("api/products");

    public Task<ApiResponse<PagedResponse<ProductResponse>>?> SearchAsync(string? search = null, string? category = null, decimal? minPrice = null, decimal? maxPrice = null, int page = 1, int pageSize = 8)
    {
        var qs = new List<string>();
        if (!string.IsNullOrEmpty(search))    qs.Add($"search={Uri.EscapeDataString(search)}");
        if (!string.IsNullOrEmpty(category))  qs.Add($"category={Uri.EscapeDataString(category)}");
        if (minPrice.HasValue)                qs.Add($"minPrice={minPrice}");
        if (maxPrice.HasValue)                qs.Add($"maxPrice={maxPrice}");
        qs.Add($"page={page}");
        qs.Add($"pageSize={pageSize}");
        return _api.GetAsync<ApiResponse<PagedResponse<ProductResponse>>>($"api/products/search?{string.Join("&", qs)}");
    }

    public Task<ODataListResponse<ProductResponse>?> GetODataAsync(
        string? search = null,
        string? category = null,
        string? orderBy = null,
        int skip = 0,
        int top = 8)
    {
        var qs = new List<string>
        {
            "$count=true",
            $"$skip={Math.Max(0, skip)}",
            $"$top={Math.Clamp(top, 1, 100)}",
            $"$orderby={Uri.EscapeDataString(string.IsNullOrWhiteSpace(orderBy) ? "CreatedAt desc" : orderBy)}"
        };

        var filters = new List<string>();
        if (!string.IsNullOrWhiteSpace(search))
            filters.Add($"contains(Name,'{EscapeODataString(search)}')");
        if (!string.IsNullOrWhiteSpace(category))
            filters.Add($"Category eq '{EscapeODataString(category)}'");
        if (filters.Count > 0)
            qs.Add($"$filter={Uri.EscapeDataString(string.Join(" and ", filters))}");

        return _api.GetAsync<ODataListResponse<ProductResponse>>($"odata/Products?{string.Join("&", qs)}");
    }

    private static string EscapeODataString(string value)
        => value.Replace("'", "''");

    public Task<ApiResponse<ProductResponse>?> GetByIdAsync(int id)
        => _api.GetAsync<ApiResponse<ProductResponse>>($"api/products/{id}");

    public Task<ApiResponse<ProductResponse>?> CreateAsync(ProductRequest req)
        => _api.PostAsync<ApiResponse<ProductResponse>>("api/products", req);

    public Task<ApiResponse<ProductResponse>?> UpdateAsync(int id, ProductRequest req)
        => _api.PutAsync<ApiResponse<ProductResponse>>($"api/products/{id}", req);

    public Task<ApiResponse<bool>?> DeleteAsync(int id)
        => _api.DeleteAsync<ApiResponse<bool>>($"api/products/{id}");

}

// ── PET CLIENT ────────────────────────────────────────────────────
public interface IPetClientService
{
    Task<ApiResponse<List<PetResponse>>?>        GetAllAsync();
    Task<ApiResponse<PagedResponse<PetResponse>>?> SearchAsync(string? search = null, string? category = null, decimal? minPrice = null, decimal? maxPrice = null, string? status = "Available", int page = 1, int pageSize = 8);
    Task<ApiResponse<PetResponse>?>              GetByIdAsync(int id);
    Task<ApiResponse<PetResponse>?>              CreateAsync(PetRequest req);
    Task<ApiResponse<PetResponse>?>              UpdateAsync(int id, PetRequest req);
    Task<ApiResponse<bool>?>                     DeleteAsync(int id);
}

public class PetClientService : IPetClientService
{
    private readonly IApiClient _api;
    public PetClientService(IApiClient api) => _api = api;

    public Task<ApiResponse<List<PetResponse>>?> GetAllAsync()
        => _api.GetAsync<ApiResponse<List<PetResponse>>>("api/pets");

    public Task<ApiResponse<PagedResponse<PetResponse>>?> SearchAsync(string? search = null, string? category = null, decimal? minPrice = null, decimal? maxPrice = null, string? status = "Available", int page = 1, int pageSize = 8)
    {
        var qs = new List<string>();
        if (!string.IsNullOrEmpty(search))   qs.Add($"search={Uri.EscapeDataString(search)}");
        if (!string.IsNullOrEmpty(category)) qs.Add($"category={Uri.EscapeDataString(category)}");
        if (minPrice.HasValue)               qs.Add($"minPrice={minPrice}");
        if (maxPrice.HasValue)               qs.Add($"maxPrice={maxPrice}");
        if (!string.IsNullOrEmpty(status))   qs.Add($"status={status}");
        qs.Add($"page={page}");
        qs.Add($"pageSize={pageSize}");
        return _api.GetAsync<ApiResponse<PagedResponse<PetResponse>>>($"api/pets/search?{string.Join("&", qs)}");
    }

    public Task<ApiResponse<PetResponse>?> GetByIdAsync(int id)
        => _api.GetAsync<ApiResponse<PetResponse>>($"api/pets/{id}");

    public Task<ApiResponse<PetResponse>?> CreateAsync(PetRequest req)
        => _api.PostAsync<ApiResponse<PetResponse>>("api/pets", req);

    public Task<ApiResponse<PetResponse>?> UpdateAsync(int id, PetRequest req)
        => _api.PutAsync<ApiResponse<PetResponse>>($"api/pets/{id}", req);

    public Task<ApiResponse<bool>?> DeleteAsync(int id)
        => _api.DeleteAsync<ApiResponse<bool>>($"api/pets/{id}");
}

// ── ORDER CLIENT ──────────────────────────────────────────────────
public interface IOrderClientService
{
    Task<ApiResponse<List<OrderResponse>>?> GetAllAsync(string? status = null);
    Task<ApiResponse<List<OrderResponse>>?> GetMyOrdersAsync();
    Task<ApiResponse<OrderResponse>?>       GetByIdAsync(int id);
    Task<ApiResponse<OrderResponse>?>       CreateAsync(OrderRequest req);
    Task<ApiResponse<PayOsPaymentLinkResponse>?> CreatePayOsPaymentLinkAsync(int id);
    Task<ApiResponse<OrderResponse>?>       UpdateStatusAsync(int id, UpdateOrderStatusRequest req);
    Task<ApiResponse<bool>?>                CancelAsync(int id);
    Task<ApiResponse<OrderResponse>?>       ConfirmReceivedAsync(int id);
    Task<ApiResponse<ShipmentCreatedResponse>?> CreateShipmentAsync(int orderId, string? note = null);
    Task<ApiResponse<OrderStatsResponse>?>  GetStatsAsync();
    Task<ApiResponse<List<AuditLogResponse>>?> GetAuditLogsAsync(int page = 1);
    Task<byte[]?>                           ExportExcelAsync();
}

public class OrderClientService : IOrderClientService
{
    private readonly IApiClient _api;
    public OrderClientService(IApiClient api) => _api = api;

    public Task<ApiResponse<List<OrderResponse>>?> GetAllAsync(string? status = null)
    {
        var url = string.IsNullOrWhiteSpace(status)
            ? "api/orders"
            : $"api/orders?status={Uri.EscapeDataString(status)}";
        return _api.GetAsync<ApiResponse<List<OrderResponse>>>(url);
    }

    public Task<ApiResponse<List<OrderResponse>>?> GetMyOrdersAsync()
        => _api.GetAsync<ApiResponse<List<OrderResponse>>>("api/orders/my");

    public Task<ApiResponse<OrderResponse>?> GetByIdAsync(int id)
        => _api.GetAsync<ApiResponse<OrderResponse>>($"api/orders/{id}");

    public Task<ApiResponse<OrderResponse>?> CreateAsync(OrderRequest req)
        => _api.PostAsync<ApiResponse<OrderResponse>>("api/orders", req);

    public Task<ApiResponse<PayOsPaymentLinkResponse>?> CreatePayOsPaymentLinkAsync(int id)
        => _api.PostAsync<ApiResponse<PayOsPaymentLinkResponse>>($"api/orders/{id}/payos/link", new { });

    public Task<ApiResponse<OrderResponse>?> UpdateStatusAsync(int id, UpdateOrderStatusRequest req)
        => _api.PatchAsync<ApiResponse<OrderResponse>>($"api/orders/{id}/status", req);

    public Task<ApiResponse<bool>?> CancelAsync(int id)
        => _api.DeleteAsync<ApiResponse<bool>>($"api/orders/{id}");

    public Task<ApiResponse<OrderResponse>?> ConfirmReceivedAsync(int id)
        => _api.PostAsync<ApiResponse<OrderResponse>>($"api/orders/{id}/confirm-received", new { });

    public Task<ApiResponse<ShipmentCreatedResponse>?> CreateShipmentAsync(int orderId, string? note = null)
        => _api.PostAsync<ApiResponse<ShipmentCreatedResponse>>($"api/orders/{orderId}/ship", new { Note = note });

    public Task<ApiResponse<OrderStatsResponse>?> GetStatsAsync()
        => _api.GetAsync<ApiResponse<OrderStatsResponse>>("api/orders/stats");

    public Task<ApiResponse<List<AuditLogResponse>>?> GetAuditLogsAsync(int page = 1)
        => _api.GetAsync<ApiResponse<List<AuditLogResponse>>>($"api/orders/audit-logs?page={page}");

    public Task<byte[]?> ExportExcelAsync()
        => _api.GetBytesAsync("api/orders/export");
}

// ── USER CLIENT  (#5 #7) ──────────────────────────────────────────
public interface IUserClientService
{
    Task<ApiResponse<UserResponse>?>       GetMeAsync();
    Task<ApiResponse<UserResponse>?>       UpdateAsync(int id, UserUpdateRequest req);
    Task<ApiResponse<List<UserResponse>>?> GetAllAsync();
    Task<ApiResponse<bool>?>               DeactivateAsync(int id);
    Task<ApiResponse<bool>?>               ActivateAsync(int id);
}

public class UserClientService : IUserClientService
{
    private readonly IApiClient _api;
    public UserClientService(IApiClient api) => _api = api;

    public Task<ApiResponse<UserResponse>?> GetMeAsync()
        => _api.GetAsync<ApiResponse<UserResponse>>("api/users/me");

    public Task<ApiResponse<UserResponse>?> UpdateAsync(int id, UserUpdateRequest req)
        => _api.PutAsync<ApiResponse<UserResponse>>($"api/users/{id}", req);

    public Task<ApiResponse<List<UserResponse>>?> GetAllAsync()
        => _api.GetAsync<ApiResponse<List<UserResponse>>>("api/users");

    public Task<ApiResponse<bool>?> DeactivateAsync(int id)
        => _api.DeleteAsync<ApiResponse<bool>>($"api/users/{id}");

    public Task<ApiResponse<bool>?> ActivateAsync(int id)
        => _api.PatchAsync<ApiResponse<bool>>($"api/users/{id}/activate", new {});
}

// ── REVIEW CLIENT  (#14) ──────────────────────────────────────────
public interface IReviewClientService
{
    Task<ApiResponse<List<ReviewResponse>>?> GetByProductAsync(int productId);
    Task<ApiResponse<List<ReviewResponse>>?> GetByPetAsync(int petId);
    Task<ApiResponse<ReviewResponse>?>       CreateAsync(ReviewCreateRequest req);
}

public class ReviewClientService : IReviewClientService
{
    private readonly IApiClient _api;
    public ReviewClientService(IApiClient api) => _api = api;

    public Task<ApiResponse<List<ReviewResponse>>?> GetByProductAsync(int productId)
        => _api.GetAsync<ApiResponse<List<ReviewResponse>>>($"api/reviews/product/{productId}");

    public Task<ApiResponse<List<ReviewResponse>>?> GetByPetAsync(int petId)
        => _api.GetAsync<ApiResponse<List<ReviewResponse>>>($"api/reviews/pet/{petId}");

    public Task<ApiResponse<ReviewResponse>?> CreateAsync(ReviewCreateRequest req)
        => _api.PostAsync<ApiResponse<ReviewResponse>>("api/reviews", req);
}

// ── CATEGORY CLIENT  (#10) ────────────────────────────────────────
public interface ICategoryClientService
{
    Task<ApiResponse<List<CategoryResponse>>?> GetAllAsync(string? type = null);
    Task<ApiResponse<CategoryResponse>?>       CreateAsync(CategoryCreateRequest req);
    Task<ApiResponse<CategoryResponse>?>       UpdateAsync(int id, CategoryCreateRequest req);
    Task<ApiResponse<bool>?>                   DeleteAsync(int id);
}

public class CategoryClientService : ICategoryClientService
{
    private readonly IApiClient _api;
    public CategoryClientService(IApiClient api) => _api = api;

    public Task<ApiResponse<List<CategoryResponse>>?> GetAllAsync(string? type = null)
    {
        var url = string.IsNullOrEmpty(type) ? "api/categories" : $"api/categories?type={type}";
        return _api.GetAsync<ApiResponse<List<CategoryResponse>>>(url);
    }

    public Task<ApiResponse<CategoryResponse>?> CreateAsync(CategoryCreateRequest req)
        => _api.PostAsync<ApiResponse<CategoryResponse>>("api/categories", req);

    public Task<ApiResponse<CategoryResponse>?> UpdateAsync(int id, CategoryCreateRequest req)
        => _api.PutAsync<ApiResponse<CategoryResponse>>($"api/categories/{id}", req);

    public Task<ApiResponse<bool>?> DeleteAsync(int id)
        => _api.DeleteAsync<ApiResponse<bool>>($"api/categories/{id}");
}
