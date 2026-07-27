# PROMPT PHÂN TÍCH DỰ ÁN PETSHOP

Bạn là một Technical Analyst chuyên nghiệp. Hãy đọc và phân tích toàn bộ dự án PetShop này, sau đó viết một báo cáo kỹ thuật chi tiết bao gồm:

## 1. TỔNG QUAN DỰ ÁN
- Mô tả tổng quan về hệ thống PetShop (mục đích, chức năng chính)
- Kiến trúc tổng thể đang sử dụng (Monolithic, Microservices, ...)
- Tech stack và frameworks chính
- Cấu trúc thư mục và mục đích từng project/folder

## 2. PHÂN TÍCH CHI TIẾT TỪNG PROJECT/SERVICE

Với mỗi project/service trong solution, hãy phân tích:

### 2.1. ApiGateway
- Vai trò và chức năng
- Công nghệ sử dụng (Ocelot, ...)
- Cấu hình routing (đọc ocelot.json)
- Cách routing từ client requests tới các microservices
- Authentication/Authorization configuration
- Ports và endpoints

### 2.2. Services (Auth.Service, Catalog.Service, Order.Service, User.Service)
Với mỗi service:
- **Mục đích và trách nhiệm** của service
- **Database schema**:
  - Các entities/models có gì
  - Relationships giữa các entities
  - Migration history
  - Seed data mặc định
- **Controllers**:
  - List tất cả endpoints (GET/POST/PUT/DELETE/PATCH)
  - Parameters và return types
  - Authorization requirements (roles)
- **Services/Business Logic**:
  - Các service classes
  - Business rules và validations
  - Xử lý logic phức tạp
- **DTOs (Data Transfer Objects)**:
  - Request DTOs
  - Response DTOs
  - Mapping giữa Entity và DTO
- **Middleware**:
  - Exception handling
  - Logging
  - Custom middleware nào khác
- **SignalR Hubs** (nếu có):
  - Real-time communication features
  - Events và methods
- **Dependencies**:
  - NuGet packages sử dụng
  - Connections tới services khác
- **Configuration**:
  - appsettings.json
  - Connection strings
  - JWT settings
  - Ports

### 2.3. PetShop.Repository
- Database context setup
- Generic Repository pattern implementation
- Unit of Work pattern
- Entities và relationships
- Seed data

### 2.4. PetShop.Shared
- Shared DTOs giữa các projects
- Enums (ProductCategory, PetCategory, OrderStatus, UserRole, ...)
- Validators (FluentValidation)
- Common utilities

### 2.5. PetShop.Client
- Kiến trúc Razor Pages
- **Pages structure**:
  - List tất cả pages (Index, Login, Register, Admin pages, ...)
  - PageModel classes và logic
- **Services**:
  - HTTP client services
  - API communication
  - How it calls API Gateway
- **Authentication & Authorization**:
  - Session management
  - JWT token handling
  - Role-based access control
- **Client-side features**:
  - Shopping cart (sessionStorage)
  - SignalR real-time updates
  - JavaScript interactions

### 2.6. PetShop.API (nếu còn tồn tại)
- Vai trò trong kiến trúc hiện tại
- So sánh với microservices approach

## 3. LUỒNG HOẠT ĐỘNG (WORKFLOWS)

Phân tích chi tiết các luồng nghiệp vụ chính:

### 3.1. Luồng đăng ký (Registration Flow)
```
Bước 1: User nhập form → ...
Bước 2: Client gửi request → ...
Bước 3: API Gateway route tới → ...
Bước 4: Auth.Service xử lý → ...
Bước 5: Database lưu user → ...
Bước 6: Return JWT token → ...
...
```
- Request/Response ở mỗi bước
- Validations được thực hiện
- Error handling

### 3.2. Luồng đăng nhập (Login Flow)
- Chi tiết từng bước
- JWT token generation
- Session storage
- Redirect logic

### 3.3. Luồng xem trang chủ chưa đăng nhập (Browse Homepage)
- Load products từ Catalog.Service
- Load pets từ Catalog.Service
- Filter và pagination
- API calls sequence

### 3.4. Luồng thêm sản phẩm vào giỏ hàng (Add to Cart)
- Client-side cart management (sessionStorage)
- Cart data structure
- Quantity handling

### 3.5. Luồng đặt hàng (Checkout Flow)
- Cart → Order creation
- Order.Service processing
- Inventory update in Catalog.Service
- Payment handling (COD)
- Order status transitions
- SignalR notifications

### 3.6. Luồng quản lý đơn hàng (Admin/Staff)
- View all orders
- Update order status
- Role-based permissions
- Real-time updates via SignalR

### 3.7. Luồng quản lý sản phẩm/thú cưng (Admin/Staff)
- CRUD operations
- Image upload (nếu có)
- Stock management
- Authorization checks

### 3.8. Luồng shipper nhận đơn và giao hàng
- Assign order to shipper
- Status updates (Picked Up → Delivering → Delivered)
- COD collection tracking
- SignalR updates to customer

## 4. KIẾN TRÚC VÀ PATTERNS

### 4.1. Architectural Patterns
- Microservices architecture
- API Gateway pattern
- Repository pattern
- Unit of Work pattern
- CQRS (nếu có)

### 4.2. Communication Patterns
- Synchronous (REST API via Gateway)
- Asynchronous (SignalR for real-time)
- Service-to-service communication (có trực tiếp không?)

### 4.3. Data Management
- Database per service
- Data consistency strategies
- Transaction handling across services

## 5. AUTHENTICATION & AUTHORIZATION

### 5.1. JWT Implementation
- Token generation trong Auth.Service
- Token structure (claims)
- Token validation
- Expiration handling

### 5.2. Role-Based Access Control
- Roles: Admin, Staff/Shipper, Customer
- Permission matrix (ai làm được gì)
- Middleware implementation
- Authorization policies

### 5.3. Session Management
- Client-side session storage
- Token refresh (nếu có)

## 6. DATABASE SCHEMA

### 6.1. PetShopAuth Database
- Tables: Users
- Fields và types
- Indexes
- Seed data

### 6.2. PetShopCatalog Database
- Tables: Products, Pets, Reviews, Categories
- Relationships
- Indexes
- Seed data

### 6.3. PetShopOrders Database
- Tables: Orders, OrderItems
- Relationships
- Status enum values
- Seed data

### 6.4. PetShopUsers Database (User.Service)
- Tables và structure
- Relationship với Auth database

## 7. API ENDPOINTS DOCUMENTATION

Liệt kê TẤT CẢ endpoints theo format:

### Auth.Service (http://localhost:5001)
```
POST /api/auth/register
  Request: { email, password, fullName, phone }
  Response: { success, data: { token, user }, message }
  Auth: None
  
POST /api/auth/login
  Request: { email, password }
  Response: { success, data: { token, user }, message }
  Auth: None

POST /api/auth/change-password
  Request: { oldPassword, newPassword }
  Response: { success, data: boolean, message }
  Auth: Required (Bearer token)
  
...
```

### Catalog.Service (http://localhost:5002)
```
GET /api/products
  Response: { success, data: Product[], message }
  Auth: None
  
GET /api/products/search?search=&category=&minPrice=&maxPrice=&page=&pageSize=
  Response: { success, data: PagedResponse<Product>, message }
  Auth: None

GET /api/products/{id}
  Response: { success, data: Product, message }
  Auth: None

POST /api/products
  Request: { name, description, price, stock, category }
  Response: { success, data: Product, message }
  Auth: Required (Admin, Staff)

...
```

(Tương tự cho Pets, Orders, Users)

## 8. SIGNALR REAL-TIME FEATURES

### 8.1. OrderHub (Order.Service)
- Connection URL
- Hub methods
- Events:
  - `NewOrder` → sent to which group?
  - `OrderStatusChanged` → sent to who?
- Client-side handling

### 8.2. CatalogHub (Catalog.Service) - nếu có
- Features
- Events

## 9. CONFIGURATION & ENVIRONMENT

### 9.1. Connection Strings
- Mỗi service connect tới database nào
- LocalDB configuration

### 9.2. Ports Configuration
- ApiGateway: 5000
- Auth.Service: 5001
- Catalog.Service: 5002
- Order.Service: 5003
- User.Service: 5004
- Client: 50140

### 9.3. Environment Variables
- ASPNETCORE_ENVIRONMENT
- JWT secrets
- Other configs

## 10. DEPENDENCIES & PACKAGES

List tất cả NuGet packages sử dụng trong mỗi project:
- Entity Framework Core
- AutoMapper
- FluentValidation
- BCrypt
- Ocelot
- SignalR
- JWT Bearer
- Swashbuckle (Swagger)
- ...

## 11. ERROR HANDLING & VALIDATION

### 11.1. Exception Middleware
- Global exception handling
- Error response format
- Logging

### 11.2. Validation
- FluentValidation rules
- Model validation
- Business rule validations

### 11.3. Status Codes
- Success: 200, 201
- Client errors: 400, 401, 403, 404
- Server errors: 500

## 12. BUSINESS RULES & CONSTRAINTS

### 12.1. Order Management Rules
- Order status transitions:
  - Pending → Confirmed → AssignedToShipper → PickedUp → Delivering → Delivered / Cancelled
- Who can change to what status
- COD handling rules

### 12.2. Inventory Rules
- Stock decrements when order confirmed
- Stock returns when order cancelled
- Out of stock handling

### 12.3. Pet Adoption Rules
- Pet status: Available → Reserved/Sold
- One pet per order item
- Special handling for pets vs products

### 12.4. User Roles & Permissions
- Admin: full access
- Staff/Shipper: manage orders, view inventory
- Customer: browse, order, view own orders

## 13. FRONTEND (CLIENT) DETAILS

### 13.1. Page Structure
- Layout and navigation
- Razor Pages vs Components
- Shared layouts

### 13.2. Client-Side Logic
- JavaScript files
- Shopping cart implementation
- Toast notifications
- Form validations

### 13.3. SignalR Client
- Connection setup
- Event subscriptions
- Real-time UI updates

## 14. SECURITY CONSIDERATIONS

### 14.1. Authentication Security
- Password hashing (BCrypt)
- Token security
- Session security

### 14.2. Authorization Security
- Role-based access
- Endpoint protection
- CORS configuration

### 14.3. Input Validation
- XSS prevention
- SQL injection prevention (EF Core)
- CSRF protection

## 15. TESTING & DEBUGGING

### 15.1. Test Projects (nếu có)
- Unit tests
- Integration tests
- Coverage

### 15.2. HTTP Files
- .http files để test API
- Sample requests

## 16. DEPLOYMENT & RUNNING

### 16.1. How to Run Locally
- Database setup
- Migration commands
- Running multiple services
- Startup order

### 16.2. Troubleshooting Common Issues
- Port conflicts
- Database connection errors
- Services not communicating

## 17. CODE QUALITY & PATTERNS

### 17.1. Code Organization
- Clean architecture
- Separation of concerns
- DRY principle usage

### 17.2. Naming Conventions
- Controllers
- Services
- Variables
- DTOs

### 17.3. Comments & Documentation
- XML documentation
- Inline comments
- README files

## 18. EXTENSIBILITY & FUTURE IMPROVEMENTS

### 18.1. Missing Features
- Payment gateway integration
- Email notifications
- Image upload/storage
- Advanced search
- Reporting dashboard

### 18.2. Scalability Considerations
- Service scaling
- Database scaling
- Caching strategy
- Load balancing

## 19. DEPENDENCIES BETWEEN SERVICES

Vẽ hoặc mô tả:
```
Client
  ↓
ApiGateway
  ├→ Auth.Service (PetShopAuth DB)
  ├→ Catalog.Service (PetShopCatalog DB)
  ├→ Order.Service (PetShopOrders DB)
  └→ User.Service (PetShopUsers DB)
```

Chi tiết:
- Service nào gọi service nào?
- Data sharing giữa services?
- Coupling degree

## 20. SUMMARY & CONCLUSION

Tóm tắt:
- Strengths của hệ thống
- Weaknesses cần cải thiện
- Overall architecture assessment
- Learning points từ codebase này

---

## YÊU CẦU KHI PHÂN TÍCH:

1. **ĐỌC TẤT CẢ FILES**: Đừng bỏ qua file nào, đọc toàn bộ:
   - .cs files (Program.cs, Controllers, Services, Entities, DTOs, Middleware)
   - .json files (appsettings, ocelot, launchSettings)
   - .cshtml files (Views/Pages)
   - .csproj files (dependencies)
   - Migration files
   - README và documentation

2. **KHÔNG CODE**: Không viết thêm code mới, chỉ phân tích code hiện có

3. **CHI TIẾT**: Giải thích rõ ràng, dễ hiểu cho người không biết code

4. **CÓ VÍ DỤ CỤ THỂ**: Với mỗi khái niệm, đưa ví dụ từ code thực tế

5. **FLOW DIAGRAMS**: Mô tả luồng bằng text/pseudo-code rõ ràng

6. **CODE SNIPPETS**: Trích dẫn code quan trọng để minh họa

7. **COMPLETENESS**: Báo cáo phải đầy đủ, không bỏ sót phần nào

---

## OUTPUT FORMAT:

Xuất báo cáo dưới dạng **Markdown** với:
- Headers rõ ràng (H1, H2, H3)
- Code blocks với syntax highlighting
- Tables cho data có cấu trúc
- Lists và bullet points
- Diagrams dạng text/ASCII

---

**BẮT ĐẦU PHÂN TÍCH NGAY BÂY GIỜ!**
