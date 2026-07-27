# 🐾 PETSHOP - BÁO CÁO PHÂN TÍCH KỸ THUẬT CHI TIẾT

**Ngày phân tích:** 2026-07-21
**Phiên bản:** 1.0
**Công nghệ:** .NET 8.0, ASP.NET Core Web API, Razor Pages

---

## 1. TỔNG QUAN DỰ ÁN

### 1.1. Mô tả tổng quan

PetShop là một hệ thống thương mại điện tử (e-commerce) dành cho cửa hàng thú cưng, cho phép:
- **Khách hàng:** Duyệt sản phẩm/thú cưng, thêm vào giỏ hàng, đặt hàng, thanh toán COD/PayOS, theo dõi đơn hàng, đánh giá sản phẩm
- **Nhân viên (Staff):** Quản lý sản phẩm, thú cưng, danh mục, xử lý đơn hàng, tạo vận đơn
- **Admin:** Quản lý toàn bộ hệ thống, thống kê, xem audit log, xuất Excel

### 1.2. Kiến trúc tổng thể

Hệ thống áp dụng kiến trúc **Microservices** với **API Gateway Pattern**:

```
┌─────────────────────────────────────────────────────────────┐
│                    PetShop.Client (Razor Pages)              │
│                    http://localhost:50140                     │
│              Session Storage / JWT Token / SignalR           │
└────────────────────────┬────────────────────────────────────┘
                         │ HTTP REST + SignalR WebSocket
                         ▼
┌─────────────────────────────────────────────────────────────┐
│                  ApiGateway (Ocelot)                         │
│                  http://localhost:5000                        │
│         JWT Validation / CORS / Routing / Rate Limiting      │
└───┬──────────┬──────────┬──────────┬────────────────────────┘
    │          │          │          │
    ▼          ▼          ▼          ▼
┌────────┐ ┌────────┐ ┌────────┐ ┌────────┐
│  Auth  │ │Catalog │ │ Order  │ │  User  │
│Service │ │Service │ │Service │ │Service │
│ :5001  │ │ :5002  │ │ :5003  │ │ :5004  │
└───┬────┘ └───┬────┘ └───┬────┘ └───┬────┘
    │          │          │          │
    ▼          ▼          ▼          ▼
┌────────┐ ┌────────┐ ┌────────┐ ┌────────┐
│PetShop │ │PetShop │ │PetShop │ │PetShop │
│  Auth  │ │Catalog │ │ Orders │ │ Users  │
│  DB    │ │   DB   │ │   DB   │ │   DB   │
└────────┘ └────────┘ └────────┘ └────────┘
```

### 1.3. Tech Stack

| Thành phần | Công nghệ |
|------------|-----------|
| **Framework** | .NET 8.0 |
| **API Gateway** | Ocelot 23.x |
| **Database** | SQL Server (LocalDB) |
| **ORM** | Entity Framework Core 8.0 |
| **Authentication** | JWT Bearer Token |
| **Password Hashing** | BCrypt.Net-Next 4.0.3 |
| **Real-time** | SignalR (WebSocket) |
| **API Documentation** | Swashbuckle / Swagger |
| **Validation** | FluentValidation 11.9.0 |
| **OData** | Microsoft.AspNetCore.OData 8.2.5 |
| **Excel Export** | ClosedXML |
| **Payment Gateway** | PayOS (tích hợp mock) |
| **Shipping** | GHN Mock (giả lập) |
| **Frontend** | Razor Pages (.cshtml) + Bootstrap |

### 1.4. Cấu trúc thư mục

```
PetShopSolution/
├── ApiGateway/                    # API Gateway (Ocelot)
│   ├── Program.cs                 # Cấu hình Ocelot + JWT validation
│   ├── ocelot.json                # Routing rules
│   └── appsettings.json
│
├── Services/                      # Microservices
│   ├── Auth.Service/              # Xác thực & phân quyền
│   │   ├── Controllers/           # AuthController.cs
│   │   ├── Services/              # AuthService.cs, TokenService.cs
│   │   ├── Entities/              # User.cs
│   │   ├── Data/                  # AuthDbContext.cs
│   │   ├── DTOs/                  # AuthDTOs.cs
│   │   ├── Middleware/            # ExceptionMiddleware.cs
│   │   └── Migrations/
│   │
│   ├── Catalog.Service/           # Sản phẩm & thú cưng
│   │   ├── Controllers/           # Products, Pets, Reviews, Categories
│   │   ├── Services/              # Product/Pet/Review/Category/Inventory
│   │   ├── Entities/              # Product, Pet, Review, Category
│   │   ├── Data/                  # CatalogDbContext.cs
│   │   ├── DTOs/                  # CatalogDTOs.cs
│   │   ├── Hubs/                  # CatalogHub.cs (SignalR)
│   │   └── Migrations/
│   │
│   ├── Order.Service/             # Đơn hàng & thanh toán
│   │   ├── Controllers/           # OrdersController.cs
│   │   ├── Services/              # Order, Shipping, Payment, Audit
│   │   ├── Entities/              # Order, OrderItem, StaffAuditLog
│   │   ├── Data/                  # OrderDbContext.cs
│   │   ├── DTOs/                  # OrderDTOs.cs
│   │   ├── Hubs/                  # OrderHub.cs (SignalR)
│   │   └── Migrations/
│   │
│   └── User.Service/              # Hồ sơ người dùng
│       ├── Controllers/           # UsersController.cs
│       ├── Services/              # UserService.cs
│       ├── Entities/              # User.cs
│       ├── Data/                  # UserDbContext.cs
│       └── DTOs/                  # UserDTOs.cs
│
├── PetShop.Client/                # Frontend (Razor Pages)
│   ├── Pages/
│   │   ├── Auth/                  # Login, Register, Logout
│   │   ├── Admin/                 # Dashboard, Products, Pets, Orders, Users
│   │   ├── Staff/                 # Dashboard, Products, Pets, Orders, Audit
│   │   ├── User/                  # Dashboard, Orders, Profile, PayOS
│   │   ├── Products/              # Detail
│   │   └── Pets/                  # Detail
│   ├── Services/                  # HTTP Client services
│   ├── Middleware/                # AuthMiddleware.cs
│   └── wwwroot/                   # Static files (CSS, JS)
│
├── PetShop.Shared/                # Thư viện dùng chung
│   ├── Enums/                     # UserRole, OrderStatus, PetStatus...
│   ├── DTOs/Request/              # Request DTOs
│   ├── DTOs/Response/             # Response DTOs
│   ├── Constants/                 # Roles, ApiRoutes
│   └── Validators/                # FluentValidation validators
│
├── PetShop.Repository/            # Pattern Repository (legacy)
│   ├── Entities/                  # Entities (monolithic)
│   ├── Data/                      # PetShopDbContext
│   ├── Repositories/              # Generic IRepository<T>
│   ├── UnitOfWork/                # Unit of Work pattern
│   └── Migrations/
│
└── PetShop.API/                   # Monolithic API (legacy)
    ├── Controllers/               # Controllers.cs
    ├── Services/                  # AuthService, BusinessServices
    ├── Hubs/                      # OrderHub.cs
    ├── Middleware/                 # ExceptionMiddleware.cs
    └── Profiles/                  # AutoMapper MappingProfile.cs
```

---

## 2. PHÂN TÍCH CHI TIẾT TỪNG PROJECT/SERVICE

### 2.1. ApiGateway

#### Vai trò và chức năng

ApiGateway là **điểm vào duy nhất** cho toàn bộ hệ thống. Mọi request từ Client đều đi qua Gateway trước khi được forward đến microservice tương ứng.

#### Công nghệ

- **Ocelot 23.x**: Thư viện API Gateway mã nguồn mở cho .NET
- **JWT Bearer Authentication**: Gateway xác thực token trước khi forward request
- **CORS**: Cho phép cross-origin requests từ Client

#### Cấu hình Program.cs

```csharp
// Load Ocelot config từ ocelot.json
builder.Configuration.AddJsonFile("ocelot.json", optional: false, reloadOnChange: true);

// JWT validation tại Gateway
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer("Bearer", opt => opt.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true, ValidateAudience = true,
        ValidateLifetime = true, ValidateIssuerSigningKey = true,
        ValidIssuer = builder.Configuration["Jwt:Issuer"],
        ValidAudience = builder.Configuration["Jwt:Audience"],
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey))
    });

builder.Services.AddOcelot();
```

#### Bảng Routing (ocelot.json)

| Upstream Path Template | Method | Downstream | Port | Auth Required |
|------------------------|--------|------------|------|---------------|
| `/api/auth/{everything}` | ALL | Auth.Service | 5001 | No |
| `/odata/{everything}` | GET | Catalog.Service | 5002 | No |
| `/api/products` | GET | Catalog.Service | 5002 | No |
| `/api/products` | POST | Catalog.Service | 5002 | **Yes** |
| `/api/products/search` | GET | Catalog.Service | 5002 | No |
| `/api/products/{id}` | GET | Catalog.Service | 5002 | No |
| `/api/products/{everything}` | POST,PUT,DELETE | Catalog.Service | 5002 | **Yes** |
| `/api/pets` | GET | Catalog.Service | 5002 | No |
| `/api/pets` | POST | Catalog.Service | 5002 | **Yes** |
| `/api/pets/search` | GET | Catalog.Service | 5002 | No |
| `/api/pets/{id}` | GET | Catalog.Service | 5002 | No |
| `/api/pets/{everything}` | POST,PUT,DELETE | Catalog.Service | 5002 | **Yes** |
| `/api/categories` | GET,POST | Catalog.Service | 5002 | No |
| `/api/categories/{everything}` | ALL | Catalog.Service | 5002 | **Yes** |
| `/api/reviews` | POST | Catalog.Service | 5002 | **Yes** |
| `/api/reviews/{everything}` | GET,DELETE | Catalog.Service | 5002 | No |
| `/api/orders/payments/payos/webhook` | POST | Order.Service | 5003 | No |
| `/api/orders` | GET,POST | Order.Service | 5003 | **Yes** |
| `/api/orders/{everything}` | ALL | Order.Service | 5003 | **Yes** |
| `/api/users` | GET | User.Service | 5004 | No |
| `/api/users/{everything}` | ALL | User.Service | 5004 | **Yes** |
| `/hubs/orders/{everything}` | GET,POST | Order.Service | 5003 | No (WS) |
| `/hubs/catalog/{everything}` | GET,POST | Catalog.Service | 5002 | No (WS) |

#### Cách routing hoạt động

1. Client gửi request tới `http://localhost:5000/api/products`
2. Gateway nhận request, match với route `/api/products` (GET)
3. Gateway forward request tới `http://localhost:5002/api/products`
4. Catalog.Service xử lý và trả về response
5. Gateway forward response về Client

Đối với các route yêu cầu authentication (`AuthenticationOptions.AuthenticationProviderKey = "Bearer"`), Gateway sẽ kiểm tra JWT token **trước khi** forward. Nếu token không hợp lệ, Gateway trả về 401 ngay lập tức.

---

### 2.2. Auth.Service (Port 5001)

#### Mục đích và trách nhiệm

Auth.Service là **issuer duy nhất của JWT token** trong hệ thống. Nó chịu trách nhiệm:
- Đăng ký tài khoản mới (role mặc định: Customer)
- Đăng nhập và phát hành JWT token
- Refresh token (làm mới access token)
- Đổi mật khẩu
- Lưu trữ thông tin xác thực (password hash, refresh token)

#### Database Schema

**Bảng: Users**
| Field | Type | Description |
|-------|------|-------------|
| Id | int (PK) | Identity auto-increment |
| FullName | nvarchar(max) | Tên đầy đủ |
| Email | nvarchar(450) | Email, unique index |
| PasswordHash | nvarchar(max) | BCrypt hash của mật khẩu |
| Phone | nvarchar(max) | Số điện thoại |
| Address | nvarchar(max), nullable | Địa chỉ giao hàng |
| Role | nvarchar(max) | "Admin" / "Staff" / "Customer" |
| RefreshToken | nvarchar(max), nullable | 64-byte random (Base64) |
| RefreshTokenExpiry | datetime2, nullable | Hết hạn refresh token |
| CreatedAt | datetime2 | Thời gian tạo, default GETUTCDATE() |
| IsActive | bit | true=active, false=disabled |

#### Seed Data (3 users mặc định)

| Id | Email | Password (plain) | Role |
|----|-------|-------------------|------|
| 1 | admin@petshop.com | Admin@123 | Admin |
| 2 | staff@petshop.com | Staff@123 | Staff |
| 3 | customer@petshop.com | Customer@123 | Customer |

#### Controllers & Endpoints

| Endpoint | Method | Auth | Description |
|----------|--------|------|-------------|
| `/api/auth/login` | POST | No | Đăng nhập, trả về JWT |
| `/api/auth/register` | POST | No | Đăng ký tài khoản Customer |
| `/api/auth/refresh` | POST | No | Refresh access token |
| `/api/auth/change-password` | POST | **Yes** | Đổi mật khẩu |

#### Business Logic

**Đăng nhập (`LoginAsync`):**
```csharp
// 1. Tìm user theo email (chỉ user IsActive)
var user = await _db.Users.FirstOrDefaultAsync(u => u.Email == request.Email && u.IsActive);

// 2. Verify password hash bằng BCrypt
if (!BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash))
    return Fail("Email hoặc mật khẩu không đúng.");

// 3. Phát hành cặp token (access + refresh)
return await IssueTokens(user, "Đăng nhập thành công.");
```

**Phát hành token (`IssueTokens`):**
```csharp
// Access token claims:
//   - NameIdentifier: user.Id
//   - Email: user.Email
//   - Name: user.FullName
//   - Role: user.Role
//   - MobilePhone: user.Phone

// Refresh token: 64 bytes random (Base64), lưu vào DB với thời hạn 7 ngày
```

**JWT Token structure:**
```
Header: { "alg": "HS256", "typ": "JWT" }
Payload: {
  "nameid": "1",
  "email": "admin@petshop.com",
  "unique_name": "Admin PetShop",
  "role": "Admin",
  "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/mobilephone": "0901234567",
  "iss": "PetShopAuth",
  "aud": "PetShopClients",
  "exp": <60 minutes from now>
}
```

#### Middleware

- **ExceptionMiddleware**: Bắt tất cả exceptions chưa xử lý, trả về JSON error response với status code 500 (hoặc 400 cho JSON parsing errors).

#### NuGet Packages

- `BCrypt.Net-Next` 4.0.3: Password hashing
- `FluentValidation.AspNetCore` 11.3.0: Request validation
- `Microsoft.AspNetCore.Authentication.JwtBearer` 8.0.0: JWT auth
- `Microsoft.EntityFrameworkCore.SqlServer` 8.0.0: ORM
- `Swashbuckle.AspNetCore` 6.5.0: Swagger UI
- `System.IdentityModel.Tokens.Jwt` 7.3.1: JWT generation

---

### 2.3. Catalog.Service (Port 5002)

#### Mục đích và trách nhiệm

Catalog.Service quản lý toàn bộ danh mục sản phẩm và thú cưng:
- CRUD sản phẩm (thức ăn, phụ kiện, thuốc, đồ chơi, grooming)
- CRUD thú cưng (chó, mèo, chim, cá, thỏ)
- Quản lý tồn kho (inventory)
- Đánh giá sản phẩm/thú cưng (reviews)
- Quản lý danh mục (categories)
- Real-time updates qua SignalR (CatalogHub)

#### Database Schema (PetShopCatalog)

**Bảng: Products**
| Field | Type | Description |
|-------|------|-------------|
| Id | int (PK) | Identity |
| Name | nvarchar(max) | Tên sản phẩm |
| Description | nvarchar(max) | Mô tả chi tiết |
| Price | decimal(18,2) | Giá bán |
| Stock | int | Số lượng tồn kho |
| Category | nvarchar(max) | "Food"/"Accessory"/"Medicine"/"Toy"/"Grooming" |
| ImageUrl | nvarchar(max), nullable | URL ảnh sản phẩm |
| CreatedAt | datetime2 | Ngày tạo |
| IsActive | bit | true=đang bán, false=đã xóa mềm |

**Bảng: Pets**
| Field | Type | Description |
|-------|------|-------------|
| Id | int (PK) | Identity |
| Name | nvarchar(max) | Tên thú cưng |
| Breed | nvarchar(max) | Giống |
| Category | nvarchar(max) | "Dog"/"Cat"/"Bird"/"Fish"/"Rabbit"/"Other" |
| AgeMonths | int | Tuổi (tháng) |
| Price | decimal(18,2) | Giá bán |
| Description | nvarchar(max) | Mô tả |
| ImageUrl | nvarchar(max), nullable | URL ảnh |
| Status | nvarchar(max) | "Available"/"Sold"/"Reserved" |
| CreatedAt | datetime2 | Ngày tạo |

**Bảng: Reviews**
| Field | Type | Description |
|-------|------|-------------|
| Id | int (PK) | Identity |
| UserId | int | FK → User |
| UserName | nvarchar(max) | Tên người đánh giá |
| ProductId | int, nullable | FK → Product (XOR với PetId) |
| PetId | int, nullable | FK → Pet (XOR với ProductId) |
| Rating | int | 1-5 sao |
| Comment | nvarchar(max) | Nội dung đánh giá |
| CreatedAt | datetime2 | Ngày đánh giá |

**Bảng: Categories**
| Field | Type | Description |
|-------|------|-------------|
| Id | int (PK) | Identity |
| Name | nvarchar(max) | Tên danh mục |
| Type | nvarchar(max) | "Product" hoặc "Pet" |
| Description | nvarchar(max), nullable | Mô tả |
| Icon | nvarchar(max), nullable | Emoji hoặc icon class |

#### Seed Data

- **15 sản phẩm**: 4 thức ăn (Royal Canin, Whiskas, Pedigree, Me-O), 4 phụ kiện (vòng cổ, dây dắt, lồng, bát ăn), 2 thuốc (Drontal, Frontline), 3 đồ chơi (bóng, cần câu, IQ), 2 chải/ làm đẹp (FURminator, Bio-groom)
- **8 thú cưng**: 4 chó (Golden, Husky, Corgi, Poodle), 2 mèo (Anh lông ngắn, Scottish Fold), 1 vẹt (Cockatiel), 1 thỏ (Holland Lop)

#### Controllers

| Controller | Route | Methods |
|------------|-------|---------|
| ProductsController | `/api/products` | GET, GET/search, GET/{id}, POST, PUT/{id}, DELETE/{id} |
| PetsController | `/api/pets` | GET, GET/search, GET/{id}, POST, PUT/{id}, DELETE/{id} |
| ReviewsController | `/api/reviews` | GET/product/{id}, GET/pet/{id}, POST, DELETE/{id} |
| CategoriesController | `/api/categories` | GET, GET/{id}, POST, PUT/{id}, DELETE/{id} |
| InternalCatalogController | `/api/internal/catalog` | POST inventory/commit, POST inventory/release |
| ProductsODataController | `/odata/Products` | GET (OData query) |
| PetsODataController | `/odata/Pets` | GET (OData query) |

#### Authorization Rules

| Action | Required Role |
|--------|---------------|
| GET products/pets (all, search, by id) | Public (no auth) |
| POST/PUT/DELETE products/pets | **Staff** |
| GET reviews | Public |
| POST review | Any authenticated user |
| DELETE review | Admin hoặc chủ review |
| GET categories | Public |
| POST/PUT categories | Admin, Staff |
| DELETE categories | **Admin only** |
| Internal inventory API | X-Internal-Key header |

#### SignalR - CatalogHub

**Events gửi tới group "Customers":**

| Event | Trigger | Payload |
|-------|---------|---------|
| `NewProductAdded` | Tạo sản phẩm mới | ProductResponse |
| `NewPetAdded` | Thêm thú cưng mới | PetResponse |
| `StockUpdated` | Thay đổi tồn kho | { ProductId, NewStock, Name } |
| `PetStatusChanged` | Thay đổi trạng thái thú cưng | { PetId, NewStatus, Name } |
| `ProductDeleted` | Xóa mềm sản phẩm | { ProductId } |
| `PetDeleted` | Xóa thú cưng | { PetId } |

#### Service-to-Service Communication

Catalog.Service gọi sang Order.Service để xác thực việc mua hàng trước khi cho phép đánh giá:

```csharp
// Catalog → Order: Kiểm tra user đã mua & nhận hàng chưa
// Route: GET api/orders/internal/purchases/validate?userId=X&productId=Y
// Header: X-Internal-Key: dev-internal-key
```

---

### 2.4. Order.Service (Port 5003)

#### Mục đích và trách nhiệm

Order.Service là service phức tạp nhất, quản lý toàn bộ vòng đời đơn hàng:
- Tạo đơn hàng (Customer)
- Thanh toán COD / PayOS
- Cập nhật trạng thái đơn hàng (Staff)
- Tạo vận đơn với hãng ship ngoài (GHN Mock)
- Xác nhận đã nhận hàng (Customer)
- Hủy đơn hàng
- Thống kê & xuất Excel (Admin)
- Audit log (lịch sử thao tác của Staff)
- Real-time notifications qua SignalR (OrderHub)

#### Database Schema (PetShopOrders)

**Bảng: Orders**
| Field | Type | Description |
|-------|------|-------------|
| Id | int (PK) | Identity |
| UserId | int | FK → User |
| UserName | nvarchar(max) | Tên khách hàng |
| CustomerPhone | nvarchar(max) | SĐT giao hàng |
| ShippingAddress | nvarchar(max) | Địa chỉ giao hàng |
| Note | nvarchar(max), nullable | Ghi chú |
| Status | nvarchar(max) | Pending/Confirmed/Shipping/AwaitingReceipt/Delivered/Cancelled |
| TotalAmount | decimal(18,2) | Tổng tiền |
| IsCod | bit | true=COD, false=PayOS |
| TrackingCode | nvarchar(max), nullable | Mã vận đơn |
| ShippingProvider | nvarchar(max), nullable | Hãng vận chuyển |
| TrackingUrl | nvarchar(max), nullable | Link tra cứu |
| FailureReason | nvarchar(max), nullable | Lý do giao thất bại |
| DeliveryProofUrl | nvarchar(max), nullable | Ảnh xác nhận giao hàng |
| CreatedAt | datetime2 | Ngày tạo đơn |
| UpdatedAt | datetime2, nullable | Cập nhật gần nhất |
| ShippedAt | datetime2, nullable | Ngày gửi hàng |
| DeliveredAt | datetime2, nullable | Ngày giao thành công |

**Bảng: OrderItems**
| Field | Type | Description |
|-------|------|-------------|
| Id | int (PK) | Identity |
| OrderId | int (FK) | FK → Order (Cascade delete) |
| ProductId | int, nullable | FK → Product (XOR với PetId) |
| ProductName | nvarchar(max), nullable | Tên sản phẩm (denormalized) |
| PetId | int, nullable | FK → Pet (XOR với ProductId) |
| PetName | nvarchar(max), nullable | Tên thú cưng (denormalized) |
| Quantity | int | Số lượng |
| UnitPrice | decimal(18,2) | Đơn giá |
| SubTotal | (computed) | Quantity × UnitPrice |

**Bảng: StaffAuditLogs**
| Field | Type | Description |
|-------|------|-------------|
| Id | int (PK) | Identity |
| StaffId | int | Staff thực hiện |
| StaffName | nvarchar(max) | Tên Staff |
| Action | nvarchar(max) | "UpdateStatus", "CreateShipment" |
| OrderId | int, nullable | Đơn hàng liên quan |
| Details | nvarchar(max), nullable | Chi tiết thao tác |
| CreatedAt | datetime2 | Thời gian |

#### Order Status Transitions

```
                         ┌──────────┐
                         │ Pending  │ (Khách vừa đặt)
                         └────┬─────┘
                              │
              ┌───────────────┼───────────────┐
              │ PayOS pay     │ COD confirm   │
              ▼               ▼               ▼
         ┌──────────┐   ┌───────────┐   ┌───────────┐
         │ Confirmed│   │ Confirmed │   │ Cancelled │
         └────┬─────┘   └─────┬─────┘   └───────────┘
              │               │
              │ Tạo vận đơn   │
              ▼               │
         ┌──────────┐        │
         │ Shipping │        │
         └────┬─────┘        │
              │              │
              ▼              │
      ┌───────────────┐     │
      │AwaitingReceipt│     │
      └───────┬───────┘     │
              │             │
   Khách xác  │             │
   nhận đã    ▼             │
   nhận  ┌──────────┐      │
         │ Delivered│      │
         └──────────┘      │
                           │
              (hủy) ───────┘
        Chỉ từ Pending hoặc Confirmed
```

**Quy tắc chuyển trạng thái:**
- `Pending → Confirmed`: COD tự động, PayOS chỉ khi webhook báo thành công
- `Pending → Cancelled`: Khách hủy hoặc Staff hủy
- `Confirmed → Cancelled`: Staff hủy (hoàn tồn kho)
- `Confirmed → Shipping`: Staff tạo vận đơn
- `Shipping → AwaitingReceipt`: Tự động hoặc Staff cập nhật
- `AwaitingReceipt → Shipping`: Rollback nếu cần giao lại
- `AwaitingReceipt → Delivered`: Khách xác nhận đã nhận

#### Controllers

| Endpoint | Method | Auth | Role |
|----------|--------|------|------|
| `/api/orders/my` | GET | Yes | Customer (xem đơn của mình) |
| `/api/orders` | POST | Yes | Customer (đặt hàng) |
| `/api/orders/{id}/pay` | POST | Yes | Customer (thanh toán) |
| `/api/orders/{id}/payos/link` | POST | Yes | Customer (tạo link PayOS) |
| `/api/orders/{id}` | GET | Yes | Customer/Staff/Admin |
| `/api/orders/{id}` | DELETE | Yes | Customer (hủy đơn) |
| `/api/orders/{id}/confirm-received` | POST | Yes | Customer (xác nhận nhận hàng) |
| `/api/orders` | GET | Yes | Admin, Staff (xem tất cả) |
| `/api/orders/{id}/status` | PATCH | Yes | **Staff only** |
| `/api/orders/{id}/ship` | POST | Yes | **Staff only** |
| `/api/orders/audit-logs` | GET | Yes | Admin, Staff |
| `/api/orders/stats` | GET | Yes | **Admin only** |
| `/api/orders/export` | GET | Yes | **Admin only** |
| `/api/orders/internal/purchases/validate` | GET | No | Internal (X-Internal-Key) |
| `/api/orders/payments/payos/webhook` | POST | No | PayOS callback |

#### Service-to-Service Communication

**Order.Service → Catalog.Service** (qua CatalogLookupService):
1. Khi tạo đơn: Gọi `GET /api/products/{id}` và `GET /api/pets/{id}` để lấy thông tin sản phẩm
2. Commit inventory: Gọi `POST /api/internal/catalog/inventory/commit` (có X-Internal-Key)
3. Release inventory (khi hủy): Gọi `POST /api/internal/catalog/inventory/release`

#### SignalR - OrderHub

**Events:**

| Event | Target Group | Trigger |
|-------|-------------|---------|
| `NewOrder` | Admins | Khi có đơn hàng mới |
| `OrderStatusChanged` | User_{userId} | Mọi thay đổi trạng thái |
| `OrderPaid` | Admins | Khi đơn được thanh toán |
| `OrderReceived` | Admins | Khi khách xác nhận nhận hàng |
| `ShipmentCreated` | Admins | Khi tạo vận đơn |

**Join groups tự động khi connect:**
- Admin/Staff → group "Admins" + "Shipper_{id}"
- Customer → group "User_{id}"

---

### 2.5. User.Service (Port 5004)

#### Mục đích và trách nhiệm

User.Service quản lý hồ sơ người dùng (đọc/ghi), tách biệt với Auth.Service:
- Xem danh sách users (Admin)
- Xem hồ sơ cá nhân
- Cập nhật thông tin cá nhân
- Vô hiệu hóa / Kích hoạt tài khoản (Admin)

#### Database Schema (PetShopUsers)

**Bảng: Users** (read-only schema, Auth.Service sở hữu migrations)
| Field | Type |
|-------|------|
| Id | int (PK) |
| FullName | nvarchar(max) |
| Email | nvarchar(450), unique |
| PasswordHash | nvarchar(max) |
| Phone | nvarchar(max) |
| Address | nvarchar(max), nullable |
| Role | nvarchar(max) |
| IsActive | bit |
| CreatedAt | datetime2 |

#### Controllers

| Endpoint | Method | Auth | Role |
|----------|--------|------|------|
| `/api/users` | GET | Yes | Admin |
| `/api/users/{id}` | GET | Yes | Admin |
| `/api/users/me` | GET | Yes | Any authenticated |
| `/api/users/{id}` | PUT | Yes | Admin hoặc chính user |
| `/api/users/{id}` | DELETE | Yes | Admin |
| `/api/users/{id}/activate` | PATCH | Yes | Admin |

---

### 2.6. PetShop.Repository (Legacy - Monolithic)

Đây là project **legacy**, đại diện cho kiến trúc monolithic trước khi chuyển sang microservices.

#### Cấu trúc

```
PetShop.Repository/
├── Entities/Entities.cs     # User, Product, Pet, Order, OrderItem (dùng enum)
├── Data/PetShopDbContext.cs  # Single DbContext cho toàn bộ database
├── Repositories/IRepository.cs # Generic Repository<T>
└── UnitOfWork/UnitOfWork.cs  # Unit of Work pattern
```

#### Repository Pattern

```csharp
public interface IRepository<T> where T : class
{
    Task<IEnumerable<T>> GetAllAsync();
    Task<T?> GetByIdAsync(int id);
    Task<IEnumerable<T>> FindAsync(Expression<Func<T, bool>> predicate);
    Task<T?> FirstOrDefaultAsync(Expression<Func<T, bool>> predicate);
    IQueryable<T> Query();
    Task AddAsync(T entity);
    void Update(T entity);
    void Remove(T entity);
    Task<bool> AnyAsync(Expression<Func<T, bool>> predicate);
    Task<int> CountAsync(Expression<Func<T, bool>>? predicate = null);
}
```

#### Unit of Work

```csharp
public interface IUnitOfWork : IDisposable
{
    IRepository<User> Users { get; }
    IRepository<Product> Products { get; }
    IRepository<Pet> Pets { get; }
    IRepository<Order> Orders { get; }
    IRepository<OrderItem> OrderItems { get; }
    Task<int> SaveChangesAsync();
}
```

**Ưu điểm:** Tập trung transaction management, dễ test với mock
**Lưu ý:** Project này KHÔNG được sử dụng trong phiên bản microservices hiện tại. Các service mới sử dụng DbContext riêng và gọi trực tiếp không qua Repository pattern.

---

### 2.7. PetShop.Shared

#### Mục đích

Thư viện dùng chung chứa các types được chia sẻ giữa các project.

#### Enums (Enums.cs)

```csharp
public enum UserRole { Admin, Staff, Customer }
public enum OrderStatus { Pending, Confirmed, Shipping, AwaitingReceipt, Delivered, Cancelled }
public enum PetStatus { Available, Sold, Reserved }
public enum ProductCategory { Food, Accessory, Medicine, Toy, Grooming }
public enum PetCategory { Dog, Cat, Bird, Fish, Rabbit, Other }
```

#### DTOs

**Request DTOs:**
- `LoginRequest`, `RegisterRequest`, `ChangePasswordRequest`
- `ProductRequest`, `PetRequest`
- `OrderRequest`, `OrderItemRequest`, `UpdateOrderStatusRequest`
- `UserUpdateRequest`
- `ReviewCreateRequest`, `CategoryCreateRequest`

**Response DTOs:**
- `AuthResponse`, `UserResponse`
- `ProductResponse`, `PetResponse`
- `OrderResponse`, `OrderItemResponse`
- `PayOsPaymentLinkResponse`, `ShipmentCreatedResponse`
- `ReviewResponse`, `CategoryResponse`
- `OrderStatsResponse`, `MonthlyRevenueItem`
- `ApiResponse<T>`, `PagedResponse<T>`
- `AuditLogResponse`

#### Validators (FluentValidation)

| Validator | Rules |
|-----------|-------|
| `LoginValidator` | Email required + valid format, Password min 6 chars |
| `RegisterValidator` | FullName required max 100, Email valid, Password min 6 + uppercase + digit required, Phone 10-11 digits |
| `ProductValidator` | Name max 200, Price > 0, Stock >= 0, Description max 2000 |
| `PetValidator` | Name max 100, Breed max 100, Price > 0, AgeMonths >= 0, Description max 2000 |
| `OrderValidator` | ShippingAddress max 500, Items not empty, each item Quantity > 0, UnitPrice > 0 |

**Lưu ý:** FluentValidation validators được định nghĩa trong Shared nhưng **không được sử dụng** trực tiếp bởi các service (các service tự validate trong service layer). Client sử dụng Data Annotations trên Razor Pages.

---

### 2.8. PetShop.Client (Port 50140)

#### Kiến trúc

PetShop.Client là ứng dụng **Razor Pages** (.NET 8.0), đóng vai trò là frontend cho toàn bộ hệ thống.

#### Cấu trúc Pages

```
Pages/
├── Auth/                   # Xác thực
│   ├── Login.cshtml        # Form đăng nhập
│   ├── Register.cshtml     # Form đăng ký
│   ├── Logout.cshtml       # Đăng xuất (POST)
│   └── AccessDenied.cshtml # Trang từ chối truy cập
│
├── Admin/                  # Admin dashboard
│   ├── Dashboard.cshtml    # Thống kê + biểu đồ doanh thu
│   ├── Orders/Index.cshtml # Quản lý tất cả đơn hàng
│   ├── Orders/Detail.cshtml# Chi tiết đơn hàng
│   ├── Products/           # CRUD sản phẩm
│   ├── Pets/               # CRUD thú cưng
│   ├── Users/Index.cshtml  # Quản lý users
│   ├── Reports/Index.cshtml# Báo cáo thống kê
│   └── Categories/         # CRUD danh mục
│
├── Staff/                  # Staff dashboard
│   ├── Dashboard.cshtml    # Thống kê đơn hàng
│   ├── Orders/             # Xử lý đơn hàng
│   ├── Products/           # Quản lý sản phẩm
│   ├── Pets/               # Quản lý thú cưng
│   ├── AuditLog.cshtml     # Xem lịch sử thao tác
│   └── Categories/         # Quản lý danh mục
│
├── User/                   # Customer dashboard
│   ├── Dashboard.cshtml    # Trang cá nhân
│   ├── Orders/Index.cshtml # Đơn hàng của tôi
│   ├── Orders/Detail.cshtml# Chi tiết đơn
│   ├── Profile.cshtml      # Hồ sơ cá nhân
│   └── PayOS.cshtml        # Thanh toán PayOS
│
├── Products/Detail.cshtml  # Chi tiết sản phẩm
├── Pets/Detail.cshtml      # Chi tiết thú cưng
├── Cart.cshtml             # Giỏ hàng & checkout
├── Index.cshtml            # Trang chủ
└── Shared/_Layout.cshtml   # Layout chính
```

#### Middleware - AuthMiddleware

Custom middleware kiểm soát truy cập dựa trên path prefix và session role:

```csharp
// Public paths (không cần đăng nhập)
["/Auth/Login", "/Auth/Register", "/", "/Index", "/Products/Index", "/Pets/Index"]

// Role-based access
{ "/Admin", ["Admin"] }                // Admin only
{ "/Staff", ["Admin", "Staff"] }       // Staff + Admin
{ "/User",  ["Admin","Staff","Customer"] }
```

Nếu không có token và path không public → redirect `/Auth/Login`
Nếu role không khớp → redirect `/Auth/AccessDenied`

#### Session Management

Client lưu trữ session state trong `HttpContext.Session`:
- `AccessToken`: JWT token
- `RefreshToken`: Refresh token
- `UserId`: ID người dùng
- `UserName`: Tên đầy đủ
- `UserRole`: Vai trò
- `UserPhone`: Số điện thoại
- `UserAddress`: Địa chỉ

Session timeout: 2 giờ, HttpOnly cookie.

#### Client Services (HTTP Communication)

Tất cả client services đều gọi API thông qua **ApiClient** base class, nơi:
1. Tự động inject JWT token vào `Authorization: Bearer {token}` header
2. Tự động parse JSON response
3. Tự động tạo error response object khi API trả lỗi

```
IApiClient ← Base HTTP client (IHttpClientFactory)
├── IAuthClientService    → /api/auth/*
├── IProductClientService → /api/products/* + /odata/Products
├── IPetClientService     → /api/pets/*
├── IOrderClientService   → /api/orders/*
├── IUserClientService    → /api/users/*
├── IReviewClientService  → /api/reviews/*
└── ICategoryClientService→ /api/categories/*
```

**Lưu ý:** Client gọi **ApiGateway** (`http://localhost:5000`), không gọi trực tiếp microservices. Đường dẫn API bắt đầu bằng `/api/...` (tương đối với base URL của Gateway).

---

### 2.9. PetShop.API (Legacy Monolithic)

#### Vai trò

`PetShop.API` là phiên bản **monolithic** ban đầu trước khi refactor sang microservices. Project này:
- Kết hợp tất cả chức năng vào một API duy nhất
- Sử dụng `PetShop.Repository` (Repository + Unit of Work)
- Sử dụng AutoMapper cho Entity ↔ DTO mapping
- Có SignalR OrderHub tích hợp sẵn
- Kết nối đến **một database duy nhất** (PetShop)

**So sánh với microservices:**

| Khía cạnh | PetShop.API (Monolith) | Microservices |
|-----------|------------------------|---------------|
| Database | 1 database chung | 4 databases riêng |
| Repository | Repository + Unit of Work | DbContext trực tiếp |
| Mapping | AutoMapper | Manual mapping |
| Auth | Tích hợp trong cùng service | Auth.Service riêng |
| Deployment | 1 service | 4 services + Gateway |
| SignalR | 1 Hub | 2 Hubs (Order + Catalog) |
| DTOs | Dùng PetShop.Shared | DTOs riêng mỗi service |

---

## 3. LUỒNG HOẠT ĐỘNG (WORKFLOWS)

### 3.1. Luồng đăng ký (Registration Flow)

```
Bước 1: User nhập form tại /Auth/Register
  → FullName, Email, Password, Phone, Address

Bước 2: Client gửi POST /api/auth/register
  → Headers: Content-Type: application/json
  → Body: { "fullName": "...", "email": "...", "password": "...", "phone": "...", "address": "..." }

Bước 3: ApiGateway match route → forward đến Auth.Service:5001

Bước 4: AuthController.Register() nhận request
  → Validate ModelState (Data Annotations)
  → Gọi AuthService.RegisterAsync()

Bước 5: AuthService.RegisterAsync()
  a. Validate từng field (server-side double check)
  b. Kiểm tra email đã tồn tại chưa
     SELECT * FROM Users WHERE Email = @email
  c. Tạo User mới:
     - PasswordHash = BCrypt.HashPassword(password)
     - Role = "Customer" (mặc định)
  d. INSERT INTO Users ...
  e. Reload user từ DB
  f. Phát hành JWT tokens:
     - GenerateAccessToken(user) → HS256, 60 phút
       Claims: NameIdentifier, Email, Name, Role, MobilePhone
     - GenerateRefreshToken() → 64 bytes random (Base64)
     - Lưu RefreshToken + RefreshTokenExpiry vào DB

Bước 6: Response trả về
  {
    "success": true,
    "message": "Đăng ký thành công.",
    "data": {
      "token": "eyJhbGciOi...",
      "refreshToken": "abc123...",
      "user": { "id": 4, "fullName": "...", "email": "...", "role": "Customer", ... }
    }
  }

Bước 7: Client lưu session
  → HttpContext.Session.SetString("AccessToken", token)
  → HttpContext.Session.SetString("RefreshToken", refreshToken)
  → HttpContext.Session.SetString("UserId", user.Id)
  → HttpContext.Session.SetString("UserRole", "Customer")
  → Redirect đến /User/Dashboard
```

### 3.2. Luồng đăng nhập (Login Flow)

```
Bước 1: User nhập Email + Password tại /Auth/Login

Bước 2: Client gửi POST /api/auth/login
  → Body: { "email": "...", "password": "..." }

Bước 3: ApiGateway forward → Auth.Service

Bước 4: AuthService.LoginAsync()
  a. Tìm user: SELECT * FROM Users WHERE Email = @email AND IsActive = 1
  b. Verify password: BCrypt.Verify(password, user.PasswordHash)
  c. Nếu sai → return Fail("Email hoặc mật khẩu không đúng.")
  d. Nếu đúng → IssueTokens(user) → trả về token pair

Bước 5: Client nhận response
  → Lưu vào session: AccessToken, RefreshToken, UserId, UserName, UserRole, UserPhone
  → Redirect dựa trên role:
    - Admin → /Admin/Dashboard
    - Staff → /Staff/Dashboard
    - Customer → /User/Dashboard
```

### 3.3. Luồng xem trang chủ (Browse Homepage)

```
Bước 1: User truy cập http://localhost:50140/ (trang chủ)
  → AuthMiddleware: Path "/" nằm trong _publicPaths → cho qua
  → Index.cshtml.cs.OnGetAsync() được gọi

Bước 2: IndexModel.OnGetAsync()
  → Nếu KHÔNG có filter (search, category, price):
    a. Gọi GET /api/products → Catalog.Service
       → Lấy tất cả sản phẩm IsActive = true
    b. Gọi GET /api/pets/search?status=Available&page=1&pageSize=9
       → Lấy thú cưng Available, phân trang
    c. Phân nhóm sản phẩm:
       - FoodProducts = all.Where(cat == "Food").Take(4)
       - ToyProducts = all.Where(cat == "Toy").Take(4)
       - AccessoryProducts = all.Where(cat == "Accessory").Take(4)

  → Nếu CÓ filter (search, category, minPrice, maxPrice):
    a. Gọi GET /api/products/search?search=...&category=...&minPrice=...&maxPrice=...&page=...&pageSize=9
    b. Trả về PagedResponse<ProductResponse>

Bước 3: Render Index.cshtml
  → Hiển thị sản phẩm theo từng section
  → Hiển thị phân trang thú cưng
  → Form tìm kiếm + filter
```

### 3.4. Luồng thêm vào giỏ hàng (Add to Cart)

```
Cơ chế: Client-side ONLY — sử dụng sessionStorage của browser

Cấu trúc giỏ hàng (JSON trong sessionStorage):
[
  { "type": "product", "id": 1, "name": "Royal Canin", "price": 350000, "qty": 2 },
  { "type": "pet", "id": 5, "name": "Mimi", "price": 4500000, "qty": 1 }
]

Bước 1: User bấm "Thêm vào giỏ hàng" trên trang chi tiết sản phẩm/thú cưng
  → JavaScript đọc dữ liệu từ DOM (data attributes)
  → Đọc giỏ hàng hiện tại từ sessionStorage
  → Nếu item đã tồn tại → tăng qty
  → Nếu chưa → thêm mới
  → Ghi lại vào sessionStorage
  → Hiển thị toast "Đã thêm vào giỏ hàng"

Bước 2: User vào /Cart để xem giỏ hàng
  → JavaScript đọc sessionStorage và render bảng
  → Tính tổng tiền
  → User có thể thay đổi số lượng hoặc xóa item
```

### 3.5. Luồng đặt hàng (Checkout Flow)

```
Bước 1: User tại trang /Cart
  → Nhập địa chỉ giao hàng (ShippingAddress)
  → Ghi chú (Note) - optional
  → Bấm "Đặt hàng"

Bước 2: Cart.OnPostAsync()
  a. Kiểm tra đăng nhập:
     if (string.IsNullOrEmpty(HttpContext.Session.GetString("UserId")))
         return Redirect("/Auth/Login")
  b. Parse CartJson từ form data
  c. Validate: CartJson không rỗng, ShippingAddress không rỗng
  d. Map CartItem → List<OrderItemRequest>
  e. Gọi POST /api/orders
     Body: {
       "items": [
         { "productId": 1, "petId": null, "quantity": 2, "unitPrice": 350000 },
         { "productId": null, "petId": 5, "quantity": 1, "unitPrice": 4500000 }
       ],
       "shippingAddress": "123 Đường ABC, Quận XYZ",
       "note": "Giao giờ hành chính",
       "isCod": true
     }

Bước 3: ApiGateway → Order.Service:5003

Bước 4: OrderService.CreateAsync()
  a. Validate items (không rỗng), shipping address (không rỗng)
  b. Với mỗi item → gọi CatalogLookupService.BuildOrderItemAsync():
     - GET /api/products/{id} (từ Catalog.Service)
     - Kiểm tra stock ≥ quantity
     - Hoặc GET /api/pets/{id} (từ Catalog.Service)
     - Kiểm tra Status == "Available"
     - Trả về OrderItem với ProductName/PetName (denormalized)
  c. Commit inventory: POST /api/internal/catalog/inventory/commit
     → Catalog.Service giảm stock / đổi pet status → "Sold"
  d. Tạo Order entity:
     - Status = "Pending"
     - IsCod = true (COD mặc định)
     - TotalAmount = sum(Quantity × UnitPrice)
  e. INSERT vào database
  f. SignalR: gửi "NewOrder" tới group "Admins"

Bước 5: Client nhận response success
  → TempData["ClearCart"] = "true"
  → Redirect /User/Dashboard
  → JavaScript xóa sessionStorage cart
```

### 3.6. Luồng xử lý đơn hàng (Staff)

```
Bước 1: Staff truy cập /Staff/Orders
  → Gọi GET /api/orders → trả về tất cả đơn hàng
  → Có thể filter theo status: Pending, Confirmed, Shipping...

Bước 2: Staff xem chi tiết đơn tại /Staff/Orders/Detail/{id}
  → Gọi GET /api/orders/{id}
  → Hiển thị: thông tin khách, items, tổng tiền, trạng thái

Bước 3: Staff cập nhật trạng thái
  → Gọi PATCH /api/orders/{id}/status
  → Body: { "status": "Confirmed" }

  OrderService.UpdateStatusAsync():
  a. Validate trạng thái mới (nằm trong danh sách cho phép)
  b. Validate transition: CanMoveStatus(old, new)
     - Pending → Confirmed/Cancelled
     - Confirmed → Cancelled (không thể → Shipping, phải qua CreateShipment)
     - Shipping → AwaitingReceipt
     - AwaitingReceipt → Shipping (rollback)
     - Delivered, Cancelled → không đổi được
  c. Nếu PayOS order và chưa thanh toán → từ chối Confirm
  d. Nếu hủy → Release inventory
  e. Cập nhật Status + UpdatedAt
  f. SignalR: gửi "OrderStatusChanged" tới group "User_{userId}"
  g. Audit log: lưu thao tác

Bước 4: Staff tạo vận đơn (khi đơn đã Confirmed)
  → Gọi POST /api/orders/{id}/ship
  → Body: { "note": "..." }

  OrderService.CreateShipmentAsync():
  a. Kiểm tra Status == "Confirmed"
  b. Gọi MockShippingService.CreateShipmentAsync()
     → Giả lập tạo vận đơn với GHN Mock
     → Trả về: TrackingCode, Provider ("GHN Mock"), TrackingUrl
  c. Cập nhật Order:
     - TrackingCode, ShippingProvider, TrackingUrl
     - Status = "Shipping"
     - ShippedAt = DateTime.UtcNow
  d. SignalR: "OrderStatusChanged" + "ShipmentCreated"
  e. Audit log
```

### 3.7. Luồng thanh toán PayOS

```
Bước 1: Customer chọn thanh toán PayOS thay vì COD
  → Từ /User/Orders/Detail/{id}
  → Bấm "Tạo link thanh toán PayOS"

Bước 2: Gọi POST /api/orders/{id}/payos/link
  → OrderService.CreatePayOsPaymentLinkAsync():
  a. Kiểm tra order.Status == "Pending"
  b. Gọi PayOsPaymentService.CreatePaymentLinkAsync(order)
     → Gửi request đến PayOS API (mock)
     → Tạo payment link với thông tin đơn hàng
  c. Cập nhật order.IsCod = false

Bước 3: Trả về PayOsPaymentLinkResponse
  {
    "orderCode": 123456,
    "amount": 5200000,
    "checkoutUrl": "https://pay.payos.vn/...",
    "qrCode": "data:image/png;base64,...",
    "status": "PENDING"
  }

Bước 4: Client redirect đến trang PayOS hoặc hiển thị QR code

Bước 5: Sau khi thanh toán, PayOS gửi webhook
  → POST /api/orders/payments/payos/webhook
  → Xác thực signature (PayOsWebhookVerifier)
  → OrderService.ConfirmPaidAsync():
     - Cập nhật Status = "Confirmed"
     - Lưu payment info vào Note
     - SignalR: "OrderPaid" + "OrderStatusChanged"
```

### 3.8. Luồng shipper nhận đơn và giao hàng

```
Bước 1: Sau khi Staff tạo vận đơn (Status = "Shipping")
  → MockShippingService tự động mô phỏng quá trình giao hàng
  → Khi đến nơi: Status tự động chuyển thành "AwaitingReceipt"

Bước 2: Customer nhận thông báo real-time
  → SignalR: "OrderStatusChanged" → Status = "AwaitingReceipt"
  → UI tự động cập nhật

Bước 3: Customer xác nhận đã nhận hàng
  → Gọi POST /api/orders/{id}/confirm-received
  → OrderService.ConfirmReceivedAsync():
     - Status = "Delivered"
     - DeliveredAt = DateTime.UtcNow
     - SignalR: "OrderReceived" + "OrderStatusChanged"

Bước 4: Với đơn COD:
  → Staff đánh dấu đã thu tiền (trong thực tế)
```

---

## 4. KIẾN TRÚC VÀ PATTERNS

### 4.1. Architectural Patterns

| Pattern | Implementation |
|---------|---------------|
| **Microservices** | 4 services độc lập (Auth, Catalog, Order, User) + API Gateway |
| **API Gateway** | Ocelot — single entry point, JWT validation, routing |
| **Database per Service** | Mỗi service có database riêng (PetShopAuth, PetShopCatalog, PetShopOrders, PetShopUsers) |
| **Repository Pattern** | PetShop.Repository (legacy) — Generic IRepository<T> |
| **Unit of Work** | PetShop.Repository (legacy) — IUnitOfWork với single SaveChangesAsync |
| **Service Layer** | Mỗi service có service classes riêng (business logic tách khỏi controllers) |
| **DTO Pattern** | Request/Response DTOs riêng cho từng service, manual mapping |

### 4.2. Communication Patterns

| Pattern | Chi tiết |
|---------|----------|
| **Synchronous REST** | Client → Gateway → Microservice (HTTP/JSON) |
| **Real-time** | SignalR WebSocket: Client ↔ Service (qua Gateway) |
| **Service-to-Service (sync)** | Order.Service gọi Catalog.Service qua HTTP (CatalogLookupService) |
| **Service-to-Service (sync)** | Catalog.Service gọi Order.Service qua HTTP (OrderPurchaseValidator) |
| **Internal API protection** | `X-Internal-Key` header cho service-to-service calls |
| **Webhook** | PayOS → Order.Service (HTTP POST callback) |

### 4.3. Data Management

| Khía cạnh | Implementation |
|-----------|---------------|
| **Database per Service** | 4 LocalDB databases |
| **Data Consistency** | Không có distributed transactions. Inventory commit/release theo best-effort |
| **Cross-service data** | Denormalized: OrderItems lưu ProductName/PetName để tránh dependency |
| **Migration** | EF Core Migrations, auto-migrate khi khởi động |
| **Soft Delete** | Product.IsActive (xóa mềm), Pet xóa cứng, Order không xóa |

---

## 5. AUTHENTICATION & AUTHORIZATION

### 5.1. JWT Implementation

**Token Generation (Auth.Service/TokenService):**

```csharp
// Claims trong JWT:
new Claim(ClaimTypes.NameIdentifier, user.Id.ToString())  // "1"
new Claim(ClaimTypes.Email, user.Email)                    // "admin@petshop.com"
new Claim(ClaimTypes.Name, user.FullName)                  // "Admin PetShop"
new Claim(ClaimTypes.Role, user.Role)                      // "Admin"
new Claim(ClaimTypes.MobilePhone, user.Phone)              // "0901234567"

// Token config:
Issuer: "PetShopAuth"
Audience: "PetShopClients"
Expiry: 60 minutes
Algorithm: HS256 (HMAC-SHA256)
Secret: "PetShop-SuperSecretKey-2024-MustBe32CharsMin!!"
```

**Token Validation (tại Gateway + mỗi Service):**
- ValidateIssuer = true
- ValidateAudience = true
- ValidateLifetime = true
- ValidateIssuerSigningKey = true
- Cùng key, issuer, audience trên toàn hệ thống

**Refresh Token:**
- 64 bytes random, lưu trong DB
- Thời hạn: 7 ngày
- Khi refresh: validate access token (cho phép expired) + refresh token match

### 5.2. Role-Based Access Control

| Role | Quyền |
|------|-------|
| **Admin** | Tất cả: CRUD sản phẩm/pets, xem/sửa tất cả đơn, quản lý users, thống kê, export Excel, xem audit log, quản lý categories |
| **Staff** | CRUD sản phẩm/pets, xem tất cả đơn, cập nhật trạng thái đơn, tạo vận đơn, xem audit log của mình, quản lý categories |
| **Customer** | Đăng ký/đăng nhập, duyệt sản phẩm, đặt hàng, thanh toán, xem đơn của mình, hủy đơn (Pending), xác nhận nhận hàng, đánh giá, cập nhật hồ sơ |

**Permission Matrix (Controllers):**

| Endpoint | Admin | Staff | Customer | Public |
|----------|-------|-------|----------|--------|
| POST /api/auth/login | ✓ | ✓ | ✓ | ✓ |
| POST /api/auth/register | ✓ | ✓ | ✓ | ✓ |
| GET /api/products | ✓ | ✓ | ✓ | ✓ |
| POST /api/products | ✗ | ✓ | ✗ | ✗ |
| PUT /api/products/{id} | ✗ | ✓ | ✗ | ✗ |
| DELETE /api/products/{id} | ✗ | ✓ | ✗ | ✗ |
| POST /api/orders | ✓ | ✓ | ✓ | ✗ |
| GET /api/orders (all) | ✓ | ✓ | ✗ | ✗ |
| GET /api/orders/my | ✓ | ✓ | ✓ | ✗ |
| PATCH /api/orders/{id}/status | ✗ | ✓ | ✗ | ✗ |
| POST /api/orders/{id}/ship | ✗ | ✓ | ✗ | ✗ |
| GET /api/orders/stats | ✓ | ✗ | ✗ | ✗ |
| GET /api/orders/export | ✓ | ✗ | ✗ | ✗ |
| GET /api/users | ✓ | ✗ | ✗ | ✗ |
| DELETE /api/users/{id} | ✓ | ✗ | ✗ | ✗ |
| PATCH /api/users/{id}/activate | ✓ | ✗ | ✗ | ✗ |
| DELETE /api/categories/{id} | ✓ | ✗ | ✗ | ✗ |

### 5.3. Session Management

Client lưu session state trong `HttpContext.Session`:
- Cấu hình: Idle timeout 2 giờ, HttpOnly cookie
- Lưu trữ: AccessToken, RefreshToken, UserId, UserName, UserRole
- Data Protection: keys lưu trong thư mục `.keys/`

---

## 6. DATABASE SCHEMA

### 6.1. PetShopAuth Database

```
┌──────────────────────────────────────────────┐
│                    Users                      │
├──────────────────────────────────────────────┤
│ Id              INT (PK, IDENTITY)            │
│ FullName        NVARCHAR(MAX)                 │
│ Email           NVARCHAR(450) UNIQUE          │
│ PasswordHash    NVARCHAR(MAX)                 │
│ Phone           NVARCHAR(MAX)                 │
│ Address         NVARCHAR(MAX) NULL            │
│ Role            NVARCHAR(MAX)                 │
│ RefreshToken    NVARCHAR(MAX) NULL            │
│ RefreshTokenExp DATETIME2 NULL                │
│ CreatedAt       DATETIME2 (GETUTCDATE)        │
│ IsActive        BIT                           │
└──────────────────────────────────────────────┘
```

### 6.2. PetShopCatalog Database

```
┌──────────────────┐  ┌──────────────────┐
│    Products      │  │      Pets        │
├──────────────────┤  ├──────────────────┤
│ Id       INT PK  │  │ Id       INT PK  │
│ Name     NVARCHAR│  │ Name     NVARCHAR│
│ Descr    NVARCHAR│  │ Breed    NVARCHAR│
│ Price    DECIMAL │  │ Category NVARCHAR│
│ Stock    INT     │  │ AgeMths  INT     │
│ Category NVARCHAR│  │ Price    DECIMAL │
│ ImageUrl NVARCHAR│  │ Descr    NVARCHAR│
│ CreatedAt DATETIME│ │ ImageUrl NVARCHAR│
│ IsActive BIT     │  │ Status   NVARCHAR│
└──────────────────┘  │ CreatedAt DATETIME│
                      └──────────────────┘

┌──────────────────┐  ┌──────────────────┐
│    Reviews       │  │   Categories     │
├──────────────────┤  ├──────────────────┤
│ Id       INT PK  │  │ Id       INT PK  │
│ UserId   INT     │  │ Name     NVARCHAR│
│ UserName NVARCHAR│  │ Type     NVARCHAR│
│ ProductId INT NULL│ │ Descr    NVARCHAR│
│ PetId    INT NULL│  │ Icon     NVARCHAR│
│ Rating   INT     │  └──────────────────┘
│ Comment  NVARCHAR│
│ CreatedAt DATETIME│
└──────────────────┘
```

### 6.3. PetShopOrders Database

```
┌────────────────────────────────────────────┐
│                  Orders                     │
├────────────────────────────────────────────┤
│ Id               INT (PK, IDENTITY)         │
│ UserId           INT                        │
│ UserName         NVARCHAR(MAX)              │
│ CustomerPhone    NVARCHAR(MAX)              │
│ ShippingAddress  NVARCHAR(MAX)              │
│ Note             NVARCHAR(MAX) NULL         │
│ Status           NVARCHAR(MAX)              │
│ TotalAmount      DECIMAL(18,2)              │
│ IsCod            BIT                        │
│ TrackingCode     NVARCHAR(MAX) NULL         │
│ ShippingProvider NVARCHAR(MAX) NULL         │
│ TrackingUrl      NVARCHAR(MAX) NULL         │
│ FailureReason    NVARCHAR(MAX) NULL         │
│ DeliveryProofUrl NVARCHAR(MAX) NULL         │
│ CreatedAt        DATETIME2                  │
│ UpdatedAt        DATETIME2 NULL             │
│ ShippedAt        DATETIME2 NULL             │
│ DeliveredAt      DATETIME2 NULL             │
└────────────────────────────────────────────┘

┌──────────────────────────────┐  ┌──────────────────────────────┐
│         OrderItems            │  │      StaffAuditLogs          │
├──────────────────────────────┤  ├──────────────────────────────┤
│ Id         INT PK             │  │ Id         INT PK             │
│ OrderId    INT FK (CASCADE)   │  │ StaffId    INT               │
│ ProductId  INT NULL           │  │ StaffName  NVARCHAR(MAX)     │
│ ProductName NVARCHAR NULL     │  │ Action     NVARCHAR(MAX)     │
│ PetId      INT NULL           │  │ OrderId    INT NULL          │
│ PetName    NVARCHAR NULL      │  │ Details    NVARCHAR(MAX) NULL│
│ Quantity   INT                │  │ CreatedAt  DATETIME2         │
│ UnitPrice  DECIMAL(18,2)      │  └──────────────────────────────┘
│ [SubTotal] COMPUTED           │
└──────────────────────────────┘
```

### 6.4. PetShopUsers Database

```
Cấu trúc giống PetShopAuth (Users table)
→ User.Service chỉ READ/UPDATE, không sở hữu migrations
```

---

## 7. API ENDPOINTS DOCUMENTATION

### 7.1. Auth.Service (`http://localhost:5001`)

```
POST /api/auth/login
  Request: { email: string, password: string }
  Response: ApiResponse<AuthResponse> { success, message, data: { token, refreshToken, user } }
  Auth: None
  Errors: 400 (validation), 200 với success=false (sai email/password)

POST /api/auth/register
  Request: { fullName, email, password, phone, address? }
  Response: ApiResponse<AuthResponse>
  Auth: None
  Errors: 400 (validation), 200 với success=false (email đã tồn tại)

POST /api/auth/refresh
  Request: { accessToken, refreshToken }
  Response: ApiResponse<AuthResponse>
  Auth: None
  Errors: 200 với success=false (token không hợp lệ/hết hạn)

POST /api/auth/change-password
  Request: { currentPassword, newPassword }
  Response: ApiResponse<bool>
  Auth: Required (Bearer)
  Errors: 200 với success=false (sai mật khẩu cũ)
```

### 7.2. Catalog.Service (`http://localhost:5002`)

```
GET /api/products
  Response: ApiResponse<List<ProductResponse>>
  Auth: None

GET /api/products/search?search=&category=&minPrice=&maxPrice=&page=&pageSize=
  Response: ApiResponse<PagedResponse<ProductResponse>>
  Auth: None

GET /api/products/{id}
  Response: ApiResponse<ProductResponse>
  Auth: None

POST /api/products
  Request: { name, description, price, stock, category, imageUrl? }
  Response: ApiResponse<ProductResponse>
  Auth: Required (Staff)

PUT /api/products/{id}
  Request: { name, description, price, stock, category, imageUrl? }
  Response: ApiResponse<ProductResponse>
  Auth: Required (Staff)

DELETE /api/products/{id}
  Response: ApiResponse<bool>
  Auth: Required (Staff)
  Note: Soft delete (IsActive = false)

GET /api/pets
  Response: ApiResponse<List<PetResponse>>
  Auth: None

GET /api/pets/search?search=&category=&minPrice=&maxPrice=&status=Available&page=&pageSize=
  Response: ApiResponse<PagedResponse<PetResponse>>
  Auth: None

GET /api/pets/{id}
  Response: ApiResponse<PetResponse>
  Auth: None

POST /api/pets
  Request: { name, breed, category, ageMonths, price, description, imageUrl?, status }
  Response: ApiResponse<PetResponse>
  Auth: Required (Staff)

PUT /api/pets/{id}
  Request: { name, breed, category, ageMonths, price, description, imageUrl?, status }
  Response: ApiResponse<PetResponse>
  Auth: Required (Staff)

DELETE /api/pets/{id}
  Response: ApiResponse<bool>
  Auth: Required (Staff)
  Note: Hard delete

GET /api/reviews/product/{productId}
  Response: ApiResponse<List<ReviewResponse>>
  Auth: None

GET /api/reviews/pet/{petId}
  Response: ApiResponse<List<ReviewResponse>>
  Auth: None

POST /api/reviews
  Request: { productId?, petId?, rating (1-5), comment }
  Response: ApiResponse<ReviewResponse>
  Auth: Required (Any)
  Business rule: Chỉ review khi đã mua & nhận hàng

DELETE /api/reviews/{id}
  Response: ApiResponse<bool>
  Auth: Required (Admin hoặc chủ review)

GET /api/categories?type=Product|Pet
  Response: ApiResponse<List<CategoryResponse>>
  Auth: None

POST /api/categories
  Request: { name, type, description?, icon? }
  Response: ApiResponse<CategoryResponse>
  Auth: Required (Admin,Staff)

PUT /api/categories/{id}
  Response: ApiResponse<CategoryResponse>
  Auth: Required (Admin,Staff)

DELETE /api/categories/{id}
  Response: ApiResponse<bool>
  Auth: Required (Admin only)

# OData Endpoints
GET /odata/Products?$filter=Category eq 'Food'&$orderby=Price desc&$top=10&$skip=0&$count=true
GET /odata/Pets?$filter=Status eq 'Available'&$orderby=CreatedAt desc

# Internal API (service-to-service)
POST /api/internal/catalog/inventory/commit
  Header: X-Internal-Key: dev-internal-key
  Request: { items: [{ productId?, petId?, quantity }] }

POST /api/internal/catalog/inventory/release
  Header: X-Internal-Key: dev-internal-key
  Request: { items: [{ productId?, petId?, quantity }] }
```

### 7.3. Order.Service (`http://localhost:5003`)

```
# Customer endpoints
GET    /api/orders/my                                              → List<OrderResponse>
POST   /api/orders                { items, shippingAddress, note?, isCod } → OrderResponse
GET    /api/orders/{id}                                           → OrderResponse
DELETE /api/orders/{id}                                           → bool (hủy đơn)
POST   /api/orders/{id}/pay              { paymentCode? }         → OrderResponse
POST   /api/orders/{id}/payos/link       {}                       → PayOsPaymentLinkResponse
POST   /api/orders/{id}/confirm-received {}                       → OrderResponse

# Staff endpoints
GET    /api/orders?status=Pending                                  → List<OrderResponse>
PATCH  /api/orders/{id}/status           { status }               → OrderResponse
POST   /api/orders/{id}/ship             { note? }                → ShipmentCreatedResponse

# Admin endpoints
GET    /api/orders/stats                                           → OrderStatsResponse
GET    /api/orders/export                                          → File (Excel)
GET    /api/orders/audit-logs?staffId=&page=                       → List<AuditLogResponse>

# Internal
GET    /api/orders/internal/purchases/validate?userId=&productId=&petId=
  Header: X-Internal-Key: dev-internal-key
  Response: ApiResponse<bool>

# Webhook
POST   /api/orders/payments/payos/webhook   PayOsWebhookRequest   → { success: bool }
```

### 7.4. User.Service (`http://localhost:5004`)

```
GET    /api/users                           → List<UserResponse>      [Admin]
GET    /api/users/{id}                      → UserResponse            [Admin]
GET    /api/users/me                        → UserResponse            [Auth]
PUT    /api/users/{id}   { fullName, phone, address? } → UserResponse [Admin hoặc chính user]
DELETE /api/users/{id}                      → bool                    [Admin]
PATCH  /api/users/{id}/activate  {}         → bool                    [Admin]
```

---

## 8. SIGNALR REAL-TIME FEATURES

### 8.1. OrderHub (`ws://localhost:5003/hubs/orders`)

**Connection URL:** `/hubs/orders` (qua Gateway: `/hubs/orders/{everything}` → WebSocket)

**Hub Methods (Client → Server):**

| Method | Params | Description |
|--------|--------|-------------|
| `JoinUserGroup` | userId: string | Customer join group `User_{userId}` |
| `JoinAdminGroup` | - | Admin/Staff join group `Admins` |
| `JoinShipperGroup` | shipperId: string | Shipper join group `Shipper_{id}` |
| `LeaveGroup` | groupName: string | Rời group |

**Auto-join khi connect:**
- Role = "Admin" hoặc "Staff" → group `Admins` + `Shipper_{id}`
- Role khác → group `User_{id}`

**Events (Server → Client):**

| Event | Group | Payload |
|-------|-------|---------|
| `NewOrder` | Admins | `{ id, userName, totalAmount }` |
| `OrderStatusChanged` | User_{userId} | `{ orderId, newStatus, updatedAt }` |
| `OrderPaid` | Admins | `{ id, userName, totalAmount }` |
| `OrderReceived` | Admins | `{ id, userName, totalAmount }` |
| `ShipmentCreated` | Admins | `{ orderId, trackingCode, provider }` |

### 8.2. CatalogHub (`ws://localhost:5002/hubs/catalog`)

**Connection URL:** `/hubs/catalog`

**Auto-join khi connect:** Group `Customers`

**Hub Methods:** `JoinAdminGroup` → thêm vào group `Admins`

**Events (Server → Client group "Customers"):**

| Event | Payload | Trigger |
|-------|---------|---------|
| `NewProductAdded` | ProductResponse | POST /api/products |
| `NewPetAdded` | PetResponse | POST /api/pets |
| `StockUpdated` | `{ productId, newStock, name }` | Inventory change |
| `PetStatusChanged` | `{ petId, newStatus, name }` | Pet status change |
| `ProductDeleted` | `{ productId }` | DELETE product |
| `PetDeleted` | `{ petId }` | DELETE pet |

---

## 9. CONFIGURATION & ENVIRONMENT

### 9.1. Connection Strings

| Service | Database | Connection String |
|---------|----------|-------------------|
| Auth.Service | PetShopAuth | `Server=(localdb)\mssqllocaldb;Database=PetShopAuth;Trusted_Connection=True;` |
| Catalog.Service | PetShopCatalog | `Server=(localdb)\mssqllocaldb;Database=PetShopCatalog;Trusted_Connection=True;` |
| Order.Service | PetShopOrders | `Server=(localdb)\mssqllocaldb;Database=PetShopOrders;Trusted_Connection=True;` |
| User.Service | PetShopUsers | `Server=(localdb)\mssqllocaldb;Database=PetShopUsers;Trusted_Connection=True;` |

### 9.2. Ports

| Service | Port | URL |
|---------|------|-----|
| ApiGateway | 5000 | `http://localhost:5000` |
| Auth.Service | 5001 | `http://localhost:5001` (Swagger UI at root) |
| Catalog.Service | 5002 | `http://localhost:5002` (Swagger UI at root) |
| Order.Service | 5003 | `http://localhost:5003` (Swagger UI at root) |
| User.Service | 5004 | `http://localhost:5004` (Swagger UI at root) |
| PetShop.Client | 50140 | `http://localhost:50140` / `https://localhost:50139` |

### 9.3. JWT Configuration (all services)

```json
{
  "Jwt": {
    "Key": "PetShop-SuperSecretKey-2024-MustBe32CharsMin!!",
    "Issuer": "PetShopAuth",
    "Audience": "PetShopClients",
    "ExpiryMinutes": "60",
    "RefreshExpiryDays": "7"
  }
}
```

### 9.4. Internal API Key

```json
{
  "InternalApiKey": "dev-internal-key"
}
```
Dùng cho service-to-service communication (Order ↔ Catalog).

### 9.5. CORS Configuration

- **Auth.Service & User.Service**: `AllowAnyOrigin().AllowAnyHeader().AllowAnyMethod()`
- **Catalog.Service & Order.Service**: `WithOrigins("localhost:50140", "localhost:50139", "localhost:5000-5003").AllowCredentials()` (bắt buộc cho SignalR)

---

## 10. DEPENDENCIES & PACKAGES

### Auth.Service

| Package | Version | Purpose |
|---------|---------|---------|
| BCrypt.Net-Next | 4.0.3 | Password hashing |
| FluentValidation.AspNetCore | 11.3.0 | Request validation |
| Microsoft.AspNetCore.Authentication.JwtBearer | 8.0.0 | JWT authentication |
| Microsoft.EntityFrameworkCore.SqlServer | 8.0.0 | SQL Server EF Core |
| Microsoft.EntityFrameworkCore.Design | 8.0.0 | EF Core design-time tools |
| Swashbuckle.AspNetCore | 6.5.0 | Swagger/OpenAPI |
| System.IdentityModel.Tokens.Jwt | 7.3.1 | JWT token generation |

### Catalog.Service

| Package | Version | Purpose |
|---------|---------|---------|
| AutoMapper.Extensions.Microsoft.DependencyInjection | 12.0.1 | Object mapping |
| BCrypt.Net-Next | 4.0.3 | Password hashing (legacy) |
| FluentValidation.AspNetCore | 11.3.0 | Validation |
| Microsoft.AspNetCore.Authentication.JwtBearer | 8.0.0 | JWT auth |
| Microsoft.AspNetCore.OData | 8.2.5 | OData query support |
| Microsoft.AspNetCore.SignalR.Client | 10.0.9 | SignalR client |
| Microsoft.EntityFrameworkCore.SqlServer | 8.0.0 | SQL Server |
| Swashbuckle.AspNetCore | 6.5.0 | Swagger |

### Order.Service

| Package | Version | Purpose |
|---------|---------|---------|
| ClosedXML | (latest) | Excel export |
| Microsoft.AspNetCore.SignalR | (built-in) | Real-time |
| Microsoft.EntityFrameworkCore.SqlServer | 8.0.0 | SQL Server |
| Swashbuckle.AspNetCore | 6.5.0 | Swagger |

### PetShop.Client

| Package | Version | Purpose |
|---------|---------|---------|
| Microsoft.AspNetCore.DataProtection | (built-in) | Session encryption |
| Microsoft.AspNetCore.SignalR.Client | (built-in) | SignalR client |

### PetShop.Shared

| Package | Version | Purpose |
|---------|---------|---------|
| FluentValidation | 11.9.0 | Validation framework |

---

## 11. ERROR HANDLING & VALIDATION

### 11.1. Exception Middleware

Mỗi service đều có `ExceptionMiddleware` giống nhau:

```csharp
public async Task InvokeAsync(HttpContext context)
{
    try { await _next(context); }
    catch (JsonException ex) {
        // Log error, return 400 với message "Invalid JSON format"
    }
    catch (Exception ex) {
        // Log error, return 500 với generic message
    }
}
```

Response format khi lỗi:
```json
{
  "statusCode": 500,
  "message": "An error occurred while processing your request",
  "detail": "..."
}
```

### 11.2. Validation

**Data Annotations** (trên DTOs):
- `[Required]`, `[EmailAddress]`, `[MinLength]`, `[Phone]`, `[Range]`

**FluentValidation** (trong PetShop.Shared):
- LoginValidator, RegisterValidator, ProductValidator, PetValidator, OrderValidator

**Business Logic Validation** (trong service layer):
- Mỗi service method tự validate business rules và trả về `ApiResponse<T>` với `Success = false`

### 11.3. Status Codes

| Status | Usage |
|--------|-------|
| 200 OK | Thành công (success/failure trong response body) |
| 201 Created | (Không dùng, tất cả trả 200 với ApiResponse wrapper) |
| 400 Bad Request | Validation errors (ModelState), JSON parse error |
| 401 Unauthorized | Thiếu hoặc invalid JWT token |
| 403 Forbidden | Không đủ quyền (role không khớp) |
| 404 Not Found | (Không dùng, entities không tồn tại trả ApiResponse success=false) |
| 500 Internal Server Error | Exception chưa xử lý |

---

## 12. BUSINESS RULES & CONSTRAINTS

### 12.1. Order Status Transitions

```
                      ┌─────────┐
           ┌──────────│ Pending │──────────┐
           │ (PayOS)  └────┬─────┘  (COD)   │ (Customer/Staff hủy)
           ▼               │               ▼
     ┌──────────┐          │        ┌───────────┐
     │Confirmed │◄─────────┘        │ Cancelled  │
     └────┬─────┘                   └───────────┘
          │                              ▲
          │ (Staff tạo vận đơn)          │ (Staff hủy từ Confirmed)
          ▼                              │
     ┌──────────┐                   ┌────┴──────┐
     │ Shipping │──────────┐        │ Confirmed │
     └────┬─────┘          │        └───────────┘
          │                │
          ▼                │ (Rollback)
   ┌───────────────┐       │
   │AwaitingReceipt│◄──────┘
   └───────┬───────┘
           │ (Customer xác nhận)
           ▼
     ┌───────────┐
     │ Delivered │
     └───────────┘
```

### 12.2. Inventory Rules

- **Commit**: Khi đơn hàng được tạo → giảm stock / đánh dấu pet "Sold"
- **Release**: Khi đơn bị hủy → tăng stock / đánh dấu pet "Available"
- **Validation**: Kiểm tra stock đủ trước khi commit
- **Transaction**: Inventory commit/release dùng database transaction

### 12.3. Payment Rules

| Payment Method | Flow |
|----------------|------|
| **COD** | Pending → Staff confirm → Confirmed → Ship → AwaitingReceipt → Delivered |
| **PayOS** | Pending → Customer pays qua PayOS → Webhook báo thành công → Confirmed → ... |
| **PayOS Amount Mismatch** | Từ chối thanh toán nếu số tiền không khớp |

### 12.4. Review Rules

- Chỉ đánh giá sản phẩm/thú cưng **đã mua và đã nhận hàng thành công** (Delivered)
- Mỗi user chỉ được đánh giá **1 lần** cho mỗi sản phẩm/thú cưng
- Mỗi review phải chọn **đúng 1** target (Product XOR Pet, không cả hai)
- Rating: 1-5 sao
- Xóa review: Admin hoặc chính người viết

---

## 13. FRONTEND (CLIENT) DETAILS

### 13.1. Page Structure

**Layout chính (`_Layout.cshtml`):**
- Navbar với: Logo, menu sản phẩm/thú cưng, giỏ hàng, user menu
- Responsive (Bootstrap)
- Toast notifications
- SignalR connection script

**Staff Layout (`Staff/Shared/_Layout.cshtml`):**
- Riêng biệt cho Staff area
- Menu: Dashboard, Orders, Products, Pets, Categories, Audit Log

### 13.2. Client-Side Logic

**Shopping Cart (JavaScript + sessionStorage):**
```javascript
// Cart structure in sessionStorage
[
  { type: "product", id: 1, name: "Royal Canin", price: 350000, qty: 2 },
  { type: "pet", id: 5, name: "Mimi", price: 4500000, qty: 1 }
]

// Functions:
addToCart(type, id, name, price)  // Thêm item
removeFromCart(index)              // Xóa item
updateQuantity(index, qty)         // Cập nhật số lượng
clearCart()                        // Xóa toàn bộ
getCartTotal()                     // Tính tổng tiền
```

### 13.3. SignalR Client

```javascript
// Kết nối OrderHub
const orderConnection = new signalR.HubConnectionBuilder()
    .withUrl("http://localhost:5000/hubs/orders", {
        accessTokenFactory: () => sessionStorage.getItem("AccessToken")
    })
    .build();

// Events
orderConnection.on("OrderStatusChanged", (data) => {
    // Cập nhật UI (toast + refresh order list)
});
orderConnection.on("NewOrder", (data) => {
    // Admin/Staff: toast "Có đơn hàng mới!"
});

// Kết nối CatalogHub
const catalogConnection = new signalR.HubConnectionBuilder()
    .withUrl("http://localhost:5000/hubs/catalog")
    .build();

// Events
catalogConnection.on("StockUpdated", (data) => { ... });
catalogConnection.on("NewProductAdded", (data) => { ... });
```

---

## 14. SECURITY CONSIDERATIONS

### 14.1. Authentication Security

| Aspect | Implementation | Assessment |
|--------|---------------|------------|
| Password Storage | BCrypt hash (salt tự động) | ✅ Tốt |
| JWT Secret | Hardcoded trong appsettings.json | ⚠️ Nên dùng environment variable / secrets manager |
| Token Expiry | 60 phút access, 7 ngày refresh | ✅ Hợp lý |
| HTTPS | Không bắt buộc (HTTP localhost) | ⚠️ Production cần HTTPS |
| Token in Query String | SignalR dùng `access_token` query param | ✅ Được phép cho WebSocket |

### 14.2. Authorization Security

| Aspect | Implementation | Assessment |
|--------|---------------|------------|
| Role-based access | `[Authorize(Roles = "Admin")]` attribute | ✅ Tốt |
| Gateway-level JWT check | Ocelot `AuthenticationProviderKey = "Bearer"` | ✅ Tốt |
| Internal API protection | `X-Internal-Key` header | ✅ Cơ bản (nên upgrade lên mTLS hoặc API key rotation) |
| PayOS webhook verification | Signature check (`PayOsWebhookVerifier`) | ✅ Có xác thực |

### 14.3. Input Validation

| Aspect | Implementation |
|--------|---------------|
| XSS Prevention | ASP.NET Core tự động encode HTML output |
| SQL Injection | EF Core parameterized queries |
| CSRF | Không có anti-forgery token trên Razor Pages |
| Request Size | Client giới hạn 10MB request body |

---

## 15. TESTING & DEBUGGING

### 15.1. Test Projects

**Tests/ thư mục tồn tại** nhưng không có file test nào được tìm thấy.

### 15.2. HTTP Test Files

Không có file `.http` nào trong project.

### 15.3. Swagger

Mỗi service có Swagger UI tại root URL:
- `http://localhost:5001` → Auth Service
- `http://localhost:5002` → Catalog Service
- `http://localhost:5003` → Order Service
- `http://localhost:5004` → User Service

---

## 16. DEPLOYMENT & RUNNING

### 16.1. How to Run Locally

```
1. Prerequisites:
   - .NET 8.0 SDK
   - SQL Server LocalDB (tự động cài với Visual Studio)

2. Start services (theo thứ tự):
   cd Services\Auth.Service      && dotnet run   # Port 5001
   cd Services\Catalog.Service   && dotnet run   # Port 5002
   cd Services\Order.Service     && dotnet run   # Port 5003
   cd Services\User.Service      && dotnet run   # Port 5004
   cd ApiGateway                 && dotnet run   # Port 5000
   cd PetShop.Client             && dotnet run   # Port 50140

3. Hoặc dùng Visual Studio:
   - Mở PetShop.sln
   - Set multiple startup projects
   - F5

4. Truy cập:
   - Client: http://localhost:50140
   - Auth Swagger: http://localhost:5001
   - Catalog Swagger: http://localhost:5002
   - Order Swagger: http://localhost:5003
   - User Swagger: http://localhost:5004

5. Test accounts:
   - Admin: admin@petshop.com / Admin@123
   - Staff: staff@petshop.com / Staff@123
   - Customer: customer@petshop.com / Customer@123
```

### 16.2. Database Migration

Migrations tự động chạy khi service khởi động:
```csharp
using var scope = app.Services.CreateScope();
var db = scope.ServiceProvider.GetRequiredService<XxxDbContext>();
db.Database.Migrate();
```

### 16.3. Gateway Startup Wait

Client tự động retry kết nối đến Gateway trong 60 giây khi khởi động:
```csharp
// PetShop.Client/Program.cs
while (DateTime.UtcNow < deadline && !ready) {
    try { await probe.GetAsync(gatewayUrl); ready = true; }
    catch { await Task.Delay(1000); }
}
```

---

## 17. CODE QUALITY & PATTERNS

### 17.1. Strengths

1. **Clean separation**: Microservices tách biệt rõ ràng theo domain
2. **Consistent API format**: Tất cả API dùng `ApiResponse<T>` wrapper
3. **Vietnamese error messages**: Thân thiện với người dùng Việt
4. **Auto-migration**: Không cần chạy migration thủ công
5. **Real-time updates**: SignalR cho order status + catalog changes
6. **Audit logging**: Ghi lại thao tác của Staff
7. **Internal API pattern**: Service-to-service communication có bảo vệ

### 17.2. Weaknesses

1. **Không có tests**: Thư mục Tests trống
2. **DTO duplication**: Cả Shared và mỗi service đều có DTOs riêng
3. **JWT secret hardcoded**: Trong appsettings.json
4. **Không có health checks**: Không có endpoint kiểm tra sức khỏe service
5. **Không có circuit breaker**: Service-to-service call không có retry/fallback
6. **Không có API versioning**: Tất cả endpoint dùng chung prefix /api/
7. **Không có distributed tracing**: Không có correlation ID giữa các service
8. **PayOS webhook test bypass**: Code có hardcoded check `IsPayOsVerificationSample` để bypass xác thực signature cho test data

---

## 18. EXTENSIBILITY & FUTURE IMPROVEMENTS

### 18.1. Missing Features (theo code)

| Feature | Status |
|---------|--------|
| Payment gateway (PayOS) | ✅ Đã tích hợp (mock) |
| Shipping integration (GHN) | ✅ Đã tích hợp (mock) |
| Image upload | ⚠️ Chỉ có ImageUrl field (URL string) |
| Email notifications | ❌ Chưa có |
| SMS notifications | ❌ Chưa có |
| Advanced search (Elasticsearch) | ❌ Chỉ dùng SQL LIKE |
| Reporting dashboard | ✅ Dashboard cơ bản (Chart.js) |
| Excel export | ✅ Có (Admin) |
| Review/Rating | ✅ Có |
| OData | ✅ Có (Catalog) |
| Rate limiting | ❌ Chưa có |

### 18.2. Suggested Improvements

1. **API Versioning**: Thêm `/api/v1/` prefix
2. **Health Checks**: Thêm `/health` endpoint cho mỗi service
3. **Service Discovery**: Dùng Consul/Eureka thay vì hardcoded URLs
4. **Circuit Breaker**: Polly cho service-to-service HTTP calls
5. **Centralized Logging**: Serilog + Elasticsearch / Seq
6. **Distributed Tracing**: OpenTelemetry / Jaeger
7. **Containerization**: Docker + Docker Compose
8. **CI/CD**: GitHub Actions / Azure DevOps
9. **Unit/Integration Tests**: xUnit + TestContainers
10. **Secret Management**: Azure Key Vault / HashiCorp Vault
11. **Event-Driven Architecture**: Dùng RabbitMQ/Kafka cho cross-service events thay vì HTTP calls trực tiếp
12. **Caching**: Redis cho product/pet listings
13. **Image Upload**: Tích hợp Azure Blob Storage / Cloudinary

---

## 19. DEPENDENCIES BETWEEN SERVICES

```
                         ┌─────────────────────────────────┐
                         │        PetShop.Client            │
                         │     (Razor Pages :50140)          │
                         │  Session + SignalR client         │
                         └──────────────┬──────────────────┘
                                        │ HTTP REST + WS
                                        ▼
                         ┌─────────────────────────────────┐
                         │         ApiGateway               │
                         │      (Ocelot :5000)              │
                         │   JWT Validation + Routing        │
                         └──┬───────┬───────┬───────┬──────┘
                            │       │       │       │
                   ┌────────▼──┐ ┌──▼──────┐│ ┌─────▼─────┐
                   │Auth.Service│ │Catalog  ││ │Order      │  ┌──────────┐
                   │   :5001    │ │ :5002   ││ │ :5003     │  │User      │
                   │            │ │         ││ │           │  │ :5004    │
                   └──────┬─────┘ └──┬──────┘│ └────┬──────┘  └────┬─────┘
                          │          │       │      │             │
                          ▼          ▼       ▼      ▼             ▼
                   ┌──────────┐┌─────────┐┌────────┐┌──────────────┐
                   │PetShop   ││PetShop  ││PetShop ││PetShopUsers  │
                   │Auth DB   ││Catalog  ││Orders  ││(shared Users)│
                   └──────────┘│DB       ││DB      │└──────────────┘
                               └─────────┘└────────┘

Service-to-Service Communication:
  ┌────────────────────────────────────────────────────────┐
  │ Catalog.Service ──HTTP──► Order.Service                │
  │   OrderPurchaseValidator.HasDeliveredPurchaseAsync()   │
  │   GET /api/orders/internal/purchases/validate          │
  │   Header: X-Internal-Key                               │
  ├────────────────────────────────────────────────────────┤
  │ Order.Service ──HTTP──► Catalog.Service                │
  │   CatalogLookupService: BuildOrderItem, Commit, Release│
  │   GET /api/products/{id}, GET /api/pets/{id}           │
  │   POST /api/internal/catalog/inventory/commit          │
  │   POST /api/internal/catalog/inventory/release         │
  │   Header: X-Internal-Key                               │
  └────────────────────────────────────────────────────────┘
```

**Coupling Assessment:**
- **Order.Service ↔ Catalog.Service**: Tight coupling qua HTTP. Việc tạo đơn hàng phụ thuộc vào Catalog.Service đang chạy. Nếu Catalog.Service chết, không thể tạo đơn hàng mới.
- **Auth.Service**: Độc lập hoàn toàn, không gọi service nào khác
- **User.Service**: Độc lập hoàn toàn
- **Client**: Chỉ gọi Gateway, không biết về microservices

---

## 20. SUMMARY & CONCLUSION

### 20.1. Strengths

1. **Kiến trúc microservices rõ ràng**: Tách biệt domain, database per service
2. **API Gateway**: Single entry point với JWT validation tập trung
3. **Code dễ đọc**: Comment tiếng Việt, naming rõ ràng
4. **Real-time**: SignalR cho order tracking + catalog updates
5. **Rich seed data**: 15 sản phẩm, 8 thú cưng, 3 users mặc định
6. **Business logic đầy đủ**: Order status transitions, inventory management, payment integration
7. **Audit logging**: Theo dõi thao tác của Staff
8. **OData support**: Query linh hoạt cho Products/Pets
9. **Consistent API response**: `ApiResponse<T>` wrapper thống nhất

### 20.2. Weaknesses

1. **Không có tests**: Rủi ro cao khi thay đổi code
2. **Tight coupling**: Service-to-service HTTP calls không có resilience
3. **No service discovery**: Hardcoded URLs trong config
4. **Security concerns**: JWT secret trong source code, thiếu HTTPS enforcement
5. **DTO duplication**: Mỗi service tự định nghĩa DTOs, khó maintain
6. **No caching**: Mọi request đều query database
7. **No monitoring**: Thiếu health checks, metrics, logging tập trung

### 20.3. Learning Points

Đây là một codebase học thuật tốt, thể hiện:
1. Cách thiết kế microservices cơ bản
2. API Gateway pattern với Ocelot
3. JWT-based authentication trong distributed system
4. Database per service pattern
5. Service-to-service communication patterns
6. SignalR real-time trong microservices
7. Repository + Unit of Work pattern (legacy)
8. FluentValidation cho input validation

### 20.4. Architecture Score

| Dimension | Rating (1-5) | Notes |
|-----------|-------------|-------|
| Code Organization | 4/5 | Cấu trúc rõ ràng, separation of concerns tốt |
| Security | 3/5 | JWT tốt nhưng secret hardcoded, không HTTPS |
| Scalability | 3/5 | Microservices sẵn sàng scale nhưng tight coupling |
| Maintainability | 3/5 | Code sạch nhưng DTO duplication, không tests |
| Documentation | 4/5 | XML comments, Swagger UI, docs folder |
| Real-time Features | 4/5 | SignalR hoạt động tốt qua Gateway |
| Business Logic | 4/5 | Order flow đầy đủ, audit logging |

**Đánh giá tổng thể:** 3.5/5 — Hệ thống có kiến trúc microservices tốt cho mục đích học tập và demo. Cần thêm tests, security hardening, và resilience patterns để sẵn sàng cho production.

---

*Báo cáo được tạo bởi phân tích toàn bộ codebase PetShop — 2026-07-21*
