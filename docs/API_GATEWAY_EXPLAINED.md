    # API GATEWAY - GIẢI THÍCH CHI TIẾT

## 📋 MỤC LỤC
1. [Tổng quan về API Gateway](#1-tổng-quan)
2. [Kiến trúc tổng thể](#2-kiến-trúc-tổng-thể)
3. [Luồng request chi tiết](#3-luồng-request-chi-tiết)
4. [Ocelot Configuration](#4-ocelot-configuration)
5. [JWT Authentication](#5-jwt-authentication)
6. [Ví dụ thực tế](#6-ví-dụ-thực-tế)

---

## 1. TỔNG QUAN

### API Gateway là gì?

**API Gateway** là một pattern trong kiến trúc microservices, đóng vai trò là:
- **Single Entry Point**: Điểm vào duy nhất cho tất cả requests từ client
- **Request Router**: Định tuyến requests tới microservice phù hợp
- **Authentication Gateway**: Xác thực JWT token trước khi forward
- **Load Balancer**: Có thể cân bằng tải (nếu có nhiều instances)

### Tại sao cần API Gateway?

**Không có Gateway:**
```
Client → Auth.Service:5001
Client → Catalog.Service:5002
Client → Order.Service:5003
Client → User.Service:5004
```

❌ **Vấn đề:**
- Client phải biết địa chỉ của TẤT CẢ services
- CORS phải config cho mỗi service
- Security phải implement ở mọi service
- Khó thay đổi khi scale/refactor

**Có Gateway:**
```
Client → ApiGateway:5000 → (route) → Auth.Service:5001
                         → (route) → Catalog.Service:5002
                         → (route) → Order.Service:5003
                         → (route) → User.Service:5004
```

✅ **Lợi ích:**
- Client chỉ cần biết 1 địa chỉ: `http://localhost:5000`
- CORS chỉ config ở Gateway
- Authentication tập trung
- Dễ thêm/xóa/thay đổi services
- Rate limiting, caching có thể thêm ở Gateway

---

## 2. KIẾN TRÚC TỔNG THỂ

### Sơ đồ chi tiết:

```
┌─────────────────────────────────────────────────────────────┐
│                     PetShop.Client                          │
│                  (Razor Pages - Port 50140)                 │
│                                                             │
│  - Index.cshtml (trang chủ)                                │
│  - Login.cshtml                                            │
│  - Admin pages                                             │
└──────────────────────┬──────────────────────────────────────┘
                       │
                       │ HTTP Requests
                       │ BaseUrl: http://localhost:5000
                       │
                       ▼
┌─────────────────────────────────────────────────────────────┐
│                      API GATEWAY                            │
│                   (Ocelot - Port 5000)                      │
│                                                             │
│  [CORS Policy] → [JWT Authentication] → [Ocelot Router]    │
│                                                             │
│  Routes Configuration (ocelot.json):                       │
│  ┌───────────────────────────────────────────────────────┐ │
│  │ /api/auth/*      → localhost:5001 (Auth.Service)     │ │
│  │ /api/products/*  → localhost:5002 (Catalog.Service)  │ │
│  │ /api/pets/*      → localhost:5002 (Catalog.Service)  │ │
│  │ /api/orders/*    → localhost:5003 (Order.Service)    │ │
│  │ /api/users/*     → localhost:5004 (User.Service)     │ │
│  └───────────────────────────────────────────────────────┘ │
└─────┬──────┬──────┬──────┬──────────────────────────────────┘
      │      │      │      │
      │      │      │      │ HTTP Forward
      ▼      ▼      ▼      ▼
   ┌─────┐┌─────┐┌─────┐┌─────┐
   │Auth ││Cata ││Order││User │
   │5001 ││5002 ││5003 ││5004 │
   └──┬──┘└──┬──┘└──┬──┘└──┬──┘
      │      │      │      │
      ▼      ▼      ▼      ▼
   ┌─────┐┌─────┐┌─────┐┌─────┐
   │ DB  ││ DB  ││ DB  ││ DB  │
   │Auth ││Cata ││Order││User │
   └─────┘└─────┘└─────┘└─────┘
```


---

## 3. LUỒNG REQUEST CHI TIẾT

### 3.1. Request KHÔNG CẦN AUTHENTICATION (Xem sản phẩm)

**Ví dụ: User vào trang chủ, xem danh sách sản phẩm**

#### Bước 1: Client tạo request

**File:** `PetShop.Client/Pages/Index.cshtml.cs`

```csharp
public async Task OnGetAsync()
{
    // Gọi ProductClientService
    var prodResult = await _productSvc.SearchAsync(
        search: null, 
        category: null, 
        page: 1, 
        pageSize: 9
    );
    Products = prodResult?.Data?.Items ?? [];
}
```

#### Bước 2: ProductClientService gọi API

**File:** `PetShop.Client/Services/ClientServices.cs`

```csharp
public class ProductClientService : IProductClientService
{
    private readonly IApiClient _api;
    
    public Task<ApiResponse<PagedResponse<ProductResponse>>?> SearchAsync(...)
    {
        // Build query string
        var qs = $"search={search}&category={category}&page={page}&pageSize={pageSize}";
        
        // Gọi qua ApiClient
        return _api.GetAsync<ApiResponse<PagedResponse<ProductResponse>>>(
            $"api/products/search?{qs}"  // ← Relative URL
        );
    }
}
```


#### Bước 3: ApiClient tạo HTTP request

```csharp
public class ApiClient : IApiClient
{
    private readonly IHttpClientFactory _factory;
    
    private HttpClient CreateClient()
    {
        // HttpClient được config với BaseAddress = http://localhost:5000
        var client = _factory.CreateClient("PetShopAPI");
        
        // Lấy JWT token từ Session (nếu có)
        var token = _ctx.HttpContext?.Session.GetString("AccessToken");
        if (!string.IsNullOrEmpty(token))
            client.DefaultRequestHeaders.Authorization = 
                new AuthenticationHeaderValue("Bearer", token);
        
        return client;
    }
    
    public async Task<T?> GetAsync<T>(string url)
    {
        // url = "api/products/search?..."
        // BaseAddress = "http://localhost:5000"
        // → Full URL = "http://localhost:5000/api/products/search?..."
        
        var res = await CreateClient().GetAsync(url);
        return await Deserialize<T>(res);
    }
}
```

**HttpClient configuration** trong `Program.cs`:

```csharp
var apiBase = builder.Configuration["ApiSettings:BaseUrl"] 
    ?? "http://localhost:5000";  // ← API Gateway address

builder.Services.AddHttpClient("PetShopAPI", client =>
{
    client.BaseAddress = new Uri(apiBase);  // ← Tất cả requests đi tới đây
    client.DefaultRequestHeaders.Add("Accept", "application/json");
});
```


#### Bước 4: Request đến API Gateway

**Request HTTP:**
```http
GET http://localhost:5000/api/products/search?page=1&pageSize=9 HTTP/1.1
Host: localhost:5000
Accept: application/json
```

**Gateway nhận request** tại `ApiGateway/Program.cs`:

```csharp
var app = builder.Build();

app.UseCors("GatewayPolicy");  // ← Xử lý CORS
app.UseAuthentication();       // ← Kiểm tra JWT (nếu có)
app.UseAuthorization();        // ← Kiểm tra quyền

await app.UseOcelot();         // ← Ocelot xử lý routing
```

#### Bước 5: Ocelot routing

Ocelot đọc `ocelot.json` và tìm route phù hợp:

```json
{
  "UpstreamPathTemplate": "/api/products/search",
  "UpstreamHttpMethod": ["GET"],
  "DownstreamPathTemplate": "/api/products/search",
  "DownstreamScheme": "http",
  "DownstreamHostAndPorts": [
    { "Host": "localhost", "Port": 5002 }
  ],
  "Key": "products-search"
}
```

**Ocelot matching logic:**
- ✅ Request path: `/api/products/search` → **MATCH** với `UpstreamPathTemplate`
- ✅ Request method: `GET` → **MATCH** với `UpstreamHttpMethod`
- ✅ Không có `AuthenticationOptions` → Không cần token


#### Bước 6: Gateway forward request tới Catalog.Service

**Gateway tạo request mới:**

```http
GET http://localhost:5002/api/products/search?page=1&pageSize=9 HTTP/1.1
Host: localhost:5002
Accept: application/json
```

**Chú ý:**
- Gateway giữ nguyên path: `/api/products/search`
- Gateway giữ nguyên query string: `?page=1&pageSize=9`
- Gateway giữ nguyên headers (trừ Host)
- Gateway thay đổi destination: `localhost:5002`

#### Bước 7: Catalog.Service xử lý

**File:** `Services/Catalog.Service/Controllers/ProductsController.cs`

```csharp
[ApiController]
[Route("api/products")]
public class ProductsController : ControllerBase
{
    [HttpGet("search")]
    public async Task<ActionResult<ApiResponse<PagedResponse<ProductResponse>>>> Search(
        [FromQuery] string? search,
        [FromQuery] string? category,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10)
    {
        var result = await _productService.SearchAsync(
            search, category, null, null, page, pageSize
        );
        return Ok(new ApiResponse<PagedResponse<ProductResponse>>
        {
            Success = true,
            Data = result
        });
    }
}
```

ProductService query database → trả về data


#### Bước 8: Response quay về Gateway

**Catalog.Service response:**
```http
HTTP/1.1 200 OK
Content-Type: application/json

{
  "success": true,
  "data": {
    "items": [
      { "id": 1, "name": "Thức ăn Royal Canin cho chó", "price": 350000, ... },
      { "id": 2, "name": "Thức ăn Whiskas cho mèo", "price": 45000, ... },
      ...
    ],
    "totalCount": 8,
    "currentPage": 1,
    "totalPages": 1
  },
  "message": null
}
```

Gateway nhận response và forward nguyên xi về Client

#### Bước 9: Client nhận response

```csharp
// ApiClient deserialize JSON
var prodResult = await _productSvc.SearchAsync(...);

// IndexModel nhận data
Products = prodResult?.Data?.Items ?? [];

// Razor view render HTML
@foreach (var p in Model.Products) {
    <div class="card">
        <h6>@p.Name</h6>
        <span>@p.Price.ToString("N0")đ</span>
    </div>
}
```

**User thấy sản phẩm trên trang chủ!** 🎉


---

### 3.2. Request CẦN AUTHENTICATION (Thêm sản phẩm)

**Ví dụ: Admin thêm sản phẩm mới**

#### Bước 1-3: Tương tự case trên

Client → ProductClientService → ApiClient → HTTP POST

```http
POST http://localhost:5000/api/products HTTP/1.1
Host: localhost:5000
Authorization: Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...
Content-Type: application/json

{
  "name": "Thức ăn mới",
  "description": "...",
  "price": 100000,
  "stock": 50,
  "category": "Food"
}
```

**Chú ý:** Header `Authorization: Bearer <token>` được tự động thêm bởi `ApiClient.CreateClient()`

#### Bước 4: Gateway kiểm tra authentication

Ocelot tìm route:

```json
{
  "UpstreamPathTemplate": "/api/products",
  "UpstreamHttpMethod": ["POST"],
  "DownstreamPathTemplate": "/api/products",
  "DownstreamScheme": "http",
  "DownstreamHostAndPorts": [
    { "Host": "localhost", "Port": 5002 }
  ],
  "AuthenticationOptions": {
    "AuthenticationProviderKey": "Bearer",  // ← YÊU CẦU JWT!
    "AllowedScopes": []
  },
  "Key": "products-post"
}
```


**Gateway validation JWT** trong `Program.cs`:

```csharp
var jwtKey = builder.Configuration["Jwt:Key"]!;

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer("Bearer", opt => opt.TokenValidationParameters = new()
    {
        ValidateIssuer           = true,
        ValidateAudience         = true,
        ValidateLifetime         = true,  // Kiểm tra expired
        ValidateIssuerSigningKey = true,
        ValidIssuer              = builder.Configuration["Jwt:Issuer"],
        ValidAudience            = builder.Configuration["Jwt:Audience"],
        IssuerSigningKey         = new SymmetricSecurityKey(
            Encoding.UTF8.GetBytes(jwtKey)
        )
    });
```

**Validation process:**

1. ✅ **Token format hợp lệ?** (JWT structure)
2. ✅ **Signature đúng?** (verify với secret key)
3. ✅ **Issuer đúng?** (`iss` claim = "PetShopAPI")
4. ✅ **Audience đúng?** (`aud` claim = "PetShopClient")
5. ✅ **Chưa hết hạn?** (`exp` claim > current time)

**Nếu token INVALID:**

```http
HTTP/1.1 401 Unauthorized
WWW-Authenticate: Bearer error="invalid_token"
```

Client nhận 401 → Redirect tới `/Auth/Login`

**Nếu token VALID:** Gateway tiếp tục forward


#### Bước 5-9: Forward → Service xử lý → Response

Gateway forward request (GIỮ NGUYÊN JWT TOKEN trong header):

```http
POST http://localhost:5002/api/products HTTP/1.1
Authorization: Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...
Content-Type: application/json

{ ... product data ... }
```

**Catalog.Service** nhận request và kiểm tra authorization:

```csharp
[HttpPost]
[Authorize(Roles = "Admin,Staff")]  // ← CHỈ Admin/Staff được thêm
public async Task<ActionResult> Create([FromBody] ProductRequest req)
{
    // Lấy user info từ JWT claims
    var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
    
    var product = await _productService.CreateAsync(req);
    return CreatedAtAction(nameof(GetById), 
        new { id = product.Id }, 
        new ApiResponse<ProductResponse> { Success = true, Data = product }
    );
}
```

**Service kiểm tra Role:**
- JWT token có claim: `"role": "Admin"`
- ✅ Match với `[Authorize(Roles = "Admin,Staff")]`
- → Cho phép tạo product

**Nếu user không có quyền (Customer):**

```http
HTTP/1.1 403 Forbidden
```

---


## 4. OCELOT CONFIGURATION CHI TIẾT

### 4.1. Cấu trúc Route Configuration

Mỗi route trong `ocelot.json` có cấu trúc:

```json
{
  "UpstreamPathTemplate": "/api/products/{id}",    // Path Client gọi
  "UpstreamHttpMethod": ["GET", "PUT", "DELETE"],  // Methods được phép
  
  "DownstreamPathTemplate": "/api/products/{id}",  // Path forward tới service
  "DownstreamScheme": "http",                      // http hoặc https
  "DownstreamHostAndPorts": [
    { "Host": "localhost", "Port": 5002 }          // Service address
  ],
  
  "AuthenticationOptions": {                       // Optional - nếu cần auth
    "AuthenticationProviderKey": "Bearer",
    "AllowedScopes": []
  },
  
  "Key": "products-get-by-id"                      // Unique identifier
}
```

### 4.2. Path Template Patterns

#### Simple Path
```json
"UpstreamPathTemplate": "/api/products"
// Chỉ match chính xác: /api/products
```

#### Path với Parameter
```json
"UpstreamPathTemplate": "/api/products/{id}"
// Match: /api/products/1, /api/products/123, etc.
// {id} được forward tới downstream
```

#### Catch-all Pattern
```json
"UpstreamPathTemplate": "/api/auth/{everything}"
// Match: /api/auth/login, /api/auth/register, /api/auth/change-password, etc.
// {everything} = toàn bộ phần còn lại của path
```


### 4.3. Route Priority và Matching Order

Ocelot match routes theo thứ tự **CỤ THỂ → CHUNG**:

```json
{
  "Routes": [
    // 1. Cụ thể nhất - được check trước
    {
      "UpstreamPathTemplate": "/api/products/search",
      "Key": "products-search"
    },
    
    // 2. Có parameter - được check tiếp
    {
      "UpstreamPathTemplate": "/api/products/{id}",
      "Key": "products-get-by-id"
    },
    
    // 3. Catch-all - được check cuối
    {
      "UpstreamPathTemplate": "/api/products/{everything}",
      "Key": "products-catchall"
    }
  ]
}
```

**Ví dụ matching:**
- Request: `GET /api/products/search?q=dog`
  - ✅ Match route #1 (exact match)
  
- Request: `GET /api/products/123`
  - ❌ Không match route #1 (không phải `/search`)
  - ✅ Match route #2 (`{id}` = 123)
  
- Request: `PUT /api/products/123/stock`
  - ❌ Không match route #1
  - ❌ Không match route #2 (method PUT không có trong route #2)
  - ✅ Match route #3 (`{everything}` = 123/stock)

### 4.4. HTTP Method Filtering

```json
{
  "UpstreamPathTemplate": "/api/products",
  "UpstreamHttpMethod": ["GET"],  // Chỉ cho phép GET
  // POST/PUT/DELETE sẽ bị reject với 405 Method Not Allowed
}
```


### 4.5. Authentication Configuration

#### Public Endpoint (không cần auth)
```json
{
  "UpstreamPathTemplate": "/api/products",
  "UpstreamHttpMethod": ["GET"],
  // Không có AuthenticationOptions → Public endpoint
}
```

#### Protected Endpoint (cần JWT)
```json
{
  "UpstreamPathTemplate": "/api/products",
  "UpstreamHttpMethod": ["POST"],
  "AuthenticationOptions": {
    "AuthenticationProviderKey": "Bearer"  // Phải có JWT Bearer token
  }
}
```

**Flow:**
1. Gateway check header `Authorization: Bearer <token>`
2. Nếu không có → `401 Unauthorized`
3. Nếu có → Validate token
4. Nếu invalid → `401 Unauthorized`
5. Nếu valid → Forward request (giữ nguyên token)

### 4.6. WebSocket Support (SignalR)

```json
{
  "UpstreamPathTemplate": "/hubs/orders/{everything}",
  "UpstreamHttpMethod": ["GET", "POST"],
  "DownstreamPathTemplate": "/hubs/orders/{everything}",
  "DownstreamScheme": "ws",  // ← WebSocket scheme
  "DownstreamHostAndPorts": [
    { "Host": "localhost", "Port": 5003 }
  ]
}
```

Gateway support WebSocket để forward SignalR connections.

---


## 5. JWT AUTHENTICATION TRONG GATEWAY

### 5.1. JWT Token Structure

Khi user login thành công, Auth.Service tạo JWT token:

```
Header.Payload.Signature
```

**Decoded:**

```json
// HEADER
{
  "alg": "HS256",
  "typ": "JWT"
}

// PAYLOAD (Claims)
{
  "sub": "3",                          // User ID
  "email": "customer@petshop.com",
  "role": "Customer",                  // Role cho authorization
  "name": "Nguyễn Văn A",
  "iss": "PetShopAPI",                 // Issuer
  "aud": "PetShopClient",              // Audience
  "exp": 1735689600,                   // Expiration timestamp
  "iat": 1735603200                    // Issued at timestamp
}

// SIGNATURE
HMACSHA256(
  base64UrlEncode(header) + "." + base64UrlEncode(payload),
  secret_key
)
```

### 5.2. Token Validation trong Gateway

**File:** `ApiGateway/Program.cs`

```csharp
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer("Bearer", opt => 
    {
        opt.TokenValidationParameters = new TokenValidationParameters
        {
            // Kiểm tra issuer (người tạo token)
            ValidateIssuer = true,
            ValidIssuer = "PetShopAPI",
            
            // Kiểm tra audience (người nhận token)
            ValidateAudience = true,
            ValidAudience = "PetShopClient",
            
            // Kiểm tra token chưa hết hạn
            ValidateLifetime = true,
            
            // Kiểm tra signature
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(jwtKey)
            )
        };
    });
```


### 5.3. JWT Configuration

**File:** `ApiGateway/appsettings.json`

```json
{
  "Jwt": {
    "Key": "your-super-secret-key-minimum-32-characters",
    "Issuer": "PetShopAPI",
    "Audience": "PetShopClient",
    "ExpiresInMinutes": 1440
  }
}
```

**Chú ý:** 
- `Key` phải GIỐNG NHAU ở Auth.Service và Gateway
- Nếu khác → Gateway không validate được token → 401

### 5.4. Token Flow Chi Tiết

```
1. User login
   ↓
2. Auth.Service tạo JWT với secret key
   ↓
3. Client lưu token vào Session
   ↓
4. Client gửi request tới Gateway với header:
   Authorization: Bearer <token>
   ↓
5. Gateway validate token:
   - Parse JWT
   - Verify signature với cùng secret key
   - Check issuer, audience, expiration
   ↓
6. Nếu valid: Forward request tới microservice
   (GIỮ NGUYÊN token trong header)
   ↓
7. Microservice có thể đọc claims từ token
   để biết user ID, role, etc.
```

---


## 6. VÍ DỤ THỰC TẾ

### 6.1. Ví dụ 1: Xem trang chủ (không đăng nhập)

**URL browser:** `http://localhost:50140/`

**Request sequence:**

```
1. Browser → Client (Razor Pages)
   GET http://localhost:50140/

2. IndexModel.OnGetAsync() calls:
   await _productSvc.SearchAsync()
   await _petSvc.SearchAsync()

3. ProductClientService generates:
   GET http://localhost:5000/api/products/search?page=1&pageSize=9
      ^                           ^
      Gateway                     Endpoint

4. Gateway (Ocelot) matches route:
   ✅ Path: /api/products/search
   ✅ Method: GET
   ✅ No auth required
   → Forward to: http://localhost:5002/api/products/search

5. Catalog.Service processes:
   ProductsController.Search() → ProductService → Database
   
6. Response: 200 OK with product list

7. Gateway forwards response to Client

8. Client renders HTML with products

9. Browser displays products
```

**Timeline:**
```
0ms    Client request → Gateway
5ms    Gateway → Catalog.Service
50ms   Catalog query database
55ms   Catalog → Gateway
60ms   Gateway → Client
65ms   Client render HTML
70ms   Browser display
```
