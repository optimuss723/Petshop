# 🐾 BÁO CÁO ĐỒ ÁN ASP.NET CORE WEB API (NHÓM 2 NGƯỜI)
## Đề tài: Hệ thống Quản lý & Bán hàng Thú cưng - PetShop

---

## 👥 1. BẢNG PHÂN CHIA CÔNG VIỆC NHÓM 2 THÀNH VIÊN

| Tiêu chí / Hạng mục | Thành viên 1 | Thành viên 2 |
|---|---|---|
| **Module phụ trách chính** | **Module Sản phẩm (Products)** & Đơn hàng | **Module Thú cưng (Pets)** & Tài khoản |
| **5 RESTful APIs CRUD** | `GET /api/products`<br>`GET /api/products/{id}`<br>`POST /api/products`<br>`PUT /api/products/{id}`<br>`DELETE /api/products/{id}` | `GET /api/pets`<br>`GET /api/pets/{id}`<br>`POST /api/pets`<br>`PUT /api/pets/{id}`<br>`DELETE /api/pets/{id}` |
| **Media Formatter & Content Negotiation** | Xây dựng **`CsvOutputFormatter`** (Custom Media Formatter cho `text/csv`) & cấu hình Content Negotiation (JSON, XML, CSV). | Cấu hình **`AddXmlDataContractSerializerFormatters()`** & kiểm thử Serialization cho Response DTOs. |
| **Thư viện OData & AutoMapper** | Cấu hình OData Endpoints (`/odata/Products`), hỗ trợ truy vấn `$filter`, `$select`, `$orderby`. | Cấu hình **AutoMapper Profile** (`MappingProfile.cs`), inject `IMapper` vào Business Services để chuyển đổi Entity ↔ DTO. |
| **Database & Entity Framework** | Thiết kế Entity `Product`, `Order`, `OrderItem` theo **EF Core Code First**, Data Seeding sản phẩm. | Thiết kế Entity `User`, `Pet` theo **EF Core Code First**, thực hiện Migration & Data Seeding tài khoản/thú cưng. |
| **Data Annotations** | Áp dụng Data Annotations trên `Product`, `Order`, `ProductRequest`, `OrderRequest`. | Áp dụng Data Annotations trên `User`, `Pet`, `LoginRequest`, `RegisterRequest`, `PetRequest`. |
| **Client gọi API bằng AJAX** | Xây dựng giao diện **AJAX CRUD cho Sản phẩm** (Single Page, Modal Thêm/Sửa, Xóa DOM không reload). | Xây dựng giao diện **AJAX CRUD cho Thú cưng** & Bảng điều khiển kiểm thử **Content Negotiation & OData**. |
| **JWT Authentication & Authorization** | Cấu hình kiểm tra phân quyền `[Authorize(Roles = Roles.Staff)]` trên các API nghiệp vụ. | Cấu hình `JwtBearer`, Token Service tạo Access Token/Refresh Token và cơ chế đính kèm token trong AJAX. |

---

## 📋 2. MA TRẬN ĐỐI CHIẾU 8 YÊU CẦU ĐỒ ÁN & MINH CHỨNG CODE

### 1️⃣ Chủ đề không vi phạm pháp luật & thuần phong mỹ tục
- **Minh chứng:** Cửa hàng thú cưng (PetShop), sản phẩm thức ăn, phụ kiện, y tế thú cưng. Dữ liệu hạt giống (Seed Data) lành mạnh trong `PetShop.Repository/Data/PetShopDbContext.cs`.

### 2️⃣ Tối thiểu 5 API: GET all, GET id, POST, PUT, DELETE
- **Minh chứng:**
  - `PetShop.API/Controllers/Controllers.cs`:
    - **Thành viên 1:** `ProductsController` (dòng 38-88):
      - `[HttpGet]` `GetAll()` -> Lấy tất cả sản phẩm
      - `[HttpGet("{id:int}")]` `GetById(id)` -> Lấy sản phẩm theo ID (trả về 404 nếu không tìm thấy)
      - `[HttpPost]` `Create(request)` -> Tạo mới (trả về `201 Created` kèm `CreatedAtAction`)
      - `[HttpPut("{id:int}")]` `Update(id, request)` -> Cập nhật sản phẩm
      - `[HttpDelete("{id:int}")]` `Delete(id)` -> Xóa sản phẩm
    - **Thành viên 2:** `PetsController` (dòng 91-141):
      - Đầy đủ 5 verbs tương tự cho Thú cưng (`/api/pets`).

### 3️⃣ Có Media Formatter, Content Negotiation
- **Minh chứng:**
  - **Custom Media Formatter:** File `PetShop.API/Formatters/CsvOutputFormatter.cs` kế thừa `TextOutputFormatter`, hỗ trợ media type `text/csv`, xuất dữ liệu động có tiêu đề cột và escape ký tự đặc biệt.
  - **Cấu hình Content Negotiation:** Trong `PetShop.API/Program.cs`:
    ```csharp
    builder.Services.AddControllers(options =>
    {
        options.RespectBrowserAcceptHeader = true; // Bật đàm phán nội dung theo Accept header
        options.ReturnHttpNotAcceptable = true;    // Trả về 406 nếu client yêu cầu format không hỗ trợ
        options.OutputFormatters.Add(new CsvOutputFormatter()); // Custom Media Formatter (CSV)
    })
    .AddXmlDataContractSerializerFormatters();     // XML Formatter
    ```
  - **3 định dạng hỗ trợ:**
    - `Accept: application/json` -> Trả về JSON
    - `Accept: application/xml` -> Trả về XML
    - `Accept: text/csv` -> Trả về CSV

### 4️⃣ Sử dụng thư viện OData, Auto Mapper
- **Minh chứng OData:**
  - Package: `Microsoft.AspNetCore.OData 8.2.5`
  - Cấu hình trong `PetShop.API/Program.cs`: `AddOData(...)` với `Select().Filter().OrderBy().Expand().Count().SetMaxTop(100)`
  - Controllers OData trong `PetShop.API/Controllers/Controllers.cs`:
    - `ProductsODataController` kế thừa `ODataController` với attribute `[EnableQuery]`
    - `PetsODataController` kế thừa `ODataController` với attribute `[EnableQuery]`
  - Hỗ trợ các câu query: `/odata/Products?$filter=Price gt 200000&$orderby=Price desc&$select=Name,Price`
- **Minh chứng AutoMapper:**
  - Profile ánh xạ: `PetShop.API/Profiles/MappingProfile.cs` kế thừa `Profile`. Ánh xạ hai chiều Entity ↔ DTO (`User`, `Product`, `Pet`, `Order`, `OrderItem`).
  - Sử dụng: Inject `IMapper _mapper` vào `AuthService`, `ProductService`, `PetService`, `OrderService`, `UserService` trong `PetShop.API/Services/`.

### 5️⃣ Dùng EF theo hướng Code First hoặc Database First
- **Minh chứng:** EF Core **Code First**
  - DbContext: `PetShop.Repository/Data/PetShopDbContext.cs`
  - Migrations: Thư mục `PetShop.Repository/Migrations/`
  - Tự động áp dụng Migration khi chạy: `db.Database.Migrate();` trong `Program.cs`.

### 6️⃣ Sử dụng các Data Annotation
- **Minh chứng trên Entity (`PetShop.Repository/Entities/Entities.cs`):**
  - `[Table("...")]`, `[Key]`, `[DatabaseGenerated]`
  - `[Required]`, `[StringLength]`, `[MinLength]`, `[MaxLength]`
  - `[Range]`, `[Column(TypeName = "decimal(18,2)")]`
  - `[ForeignKey]`, `[EmailAddress]`, `[Phone]`, `[Url]`
- **Minh chứng trên Request DTOs (`PetShop.Shared/DTOs/Request/Requests.cs`):**
  - `ProductRequest`: `[Required]`, `[StringLength(200, MinimumLength = 2)]`, `[Range(0.01, 1000000000)]`, `[Range(0, 100000)]`
  - `PetRequest`: `[Required]`, `[StringLength(100)]`, `[Range(0, 300)]`, `[Range(0.01, 1000000000)]`
  - `OrderRequest`: `[Required]`, `[MinLength(1)]`, `[StringLength(500)]`
  - `UserUpdateRequest`: `[Required]`, `[Phone]`, `[StringLength(100)]`

### 7️⃣ Gọi API bằng AJAX
- **Minh chứng:** Trang quản trị chuyên biệt **`PetShop.Client/Pages/Admin/AjaxCrud.cshtml`**
  - Sử dụng 100% JavaScript `fetch()`:
    - `loadProductsAjax()`: Gọi `GET /api/products` và render động vào `<tbody>`
    - `viewProductDetailAjax(id)`: Gọi `GET /api/products/{id}` và đổ vào Modal chi tiết
    - `saveProductAjax()`: Gọi `POST /api/products` (Thêm) hoặc `PUT /api/products/{id}` (Sửa) kèm header `Authorization: Bearer <token>`
    - `deleteProductAjax(id)`: Gọi `DELETE /api/products/{id}` kèm token, xóa dòng DOM tức thì không reload trang.
    - Tương tự cho Thú cưng (`loadPetsAjax`, `savePetAjax`, `deletePetAjax`).

### 8️⃣ Sử dụng JWT cho Authentication và Authorization
- **Minh chứng:**
  - Đăng ký xác thực Bearer: `AddAuthentication(JwtBearerDefaults.AuthenticationScheme).AddJwtBearer(...)` trong `Program.cs`.
  - Phân quyền theo Role: `[Authorize(Roles = Roles.Staff)]`, `[Authorize(Roles = Roles.Admin)]`, `[Authorize(Roles = Roles.Customer)]`.
  - Token Service: Phát hành access token kèm claims (`NameIdentifier`, `Name`, `Email`, `Role`) và refresh token trong `PetShop.API/Services/AuthService.cs`.
  - JavaScript tự động lấy JWT token từ Session/Storage để gửi trong header `Authorization: Bearer <token>`.

---

## 🎬 3. HƯỚNG DẪN KHỞI CHẠY & KỊCH BẢN DEMO KHI BẢO VỆ

### Bước 1: Khởi động dự án
1. Mở Solution `PetShop.sln` bằng Visual Studio 2022 hoặc VS Code.
2. Thiết lập chạy đồng thời:
   - Backend API: `PetShop.API` (Chạy tại port cấu hình trong `launchSettings.json`, ví dụ: `https://localhost:50141;http://localhost:50142`)
   - Client Web: `PetShop.Client` (Chạy tại `http://localhost:50140` hoặc port mặc định)
   *(Hoặc chạy bộ microservices nếu sử dụng `ApiGateway` port 5000)*.

### Bước 2: Đăng nhập tài khoản Admin/Staff
- Tài khoản Admin: `admin@petshop.com` / `Admin@123`
- Tài khoản Staff: `staff@petshop.com` / `Staff@123`

### Bước 3: Demo giao diện AJAX CRUD
1. Trên thanh Sidebar, bấm vào mục: **"Quản lý AJAX (Demo 2 người)"** (`/Admin/AjaxCrud`).
2. Mở tab **F12 -> Network -> filter Fetch/XHR**.
3. **Thành viên 1 demo Module Sản phẩm:**
   - Bấm xem danh sách -> F12 thấy `GET /api/products` trả về JSON, dữ liệu tự hiển thị lên bảng.
   - Bấm icon con mắt -> F12 thấy `GET /api/products/1` -> Modal hiện chi tiết.
   - Bấm "Thêm sản phẩm" -> Nhập dữ liệu -> Bấm Lưu -> F12 thấy `POST /api/products` có header `Authorization: Bearer ...` trả về mã `201 Created`.
   - Bấm icon cây bút -> Sửa giá -> Bấm Lưu -> F12 thấy `PUT /api/products/{id}`.
   - Bấm icon thùng rác -> Bấm Đồng ý -> F12 thấy `DELETE /api/products/{id}` -> Hàng tự động biến mất trên giao diện mà **trang không hề bị F5/reload**!
4. **Thành viên 2 demo Module Thú cưng:**
   - Chuyển sang Tab "Module Thú cưng (Thành viên 2)" và thực hiện tương tự 5 API CRUD cho Thú cưng.

### Bước 4: Demo Content Negotiation (Đàm phán nội dung)
1. Chuyển sang Tab **"Content Negotiation & OData Playground"**.
2. Chọn Endpoint `/api/products`.
3. Lần 1: Chọn radio **JSON** -> Bấm "Gửi AJAX Request" -> Nhận chuỗi JSON.
4. Lần 2: Chọn radio **XML** -> Bấm "Gửi AJAX Request" -> F12 thấy header `Accept: application/xml`, nhận chuỗi XML (`<ApiResponseOfListOfProductResponse...><Data>...`).
5. Lần 3: Chọn radio **CSV** -> Bấm "Gửi AJAX Request" -> F12 thấy header `Accept: text/csv`, nhận chuỗi CSV (`Id,Name,Price,Stock...`) do chính **`CsvOutputFormatter`** tự sinh ra!

### Bước 5: Demo OData Query
1. Trong phần **OData Query Engine**, chọn EntitySet `/odata/Products`.
2. Bấm nút query mẫu `$filter=Price gt 200000` -> Bấm "Thực thi OData Query" -> Nhận danh sách sản phẩm có giá trên 200k.
3. Bấm nút `$orderby=Price desc` -> Nhận danh sách đã được sắp xếp từ cao xuống thấp trực tiếp từ database server.
4. Bấm nút `$select=Name,Price,Stock` -> Chỉ nhận về 3 cột tương ứng.

