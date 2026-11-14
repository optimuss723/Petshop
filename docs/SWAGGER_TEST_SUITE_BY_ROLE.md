# 🧪 BỘ DỮ LIỆU TEST TOÀN DIỆN TRÊN SWAGGER THEO TỪNG ROLE (PETSHOP)

> **Swagger URL:** `http://localhost:50142/swagger`  
> **Hướng dẫn Authorize:**  
> 1. Gọi `POST /api/auth/login` với tài khoản tương ứng bên dưới để lấy chuỗi `token`.  
> 2. Bấm nút **Authorize 🔓** (màu xanh lá ở góc trên bên phải Swagger).  
> 3. Dán trực tiếp chuỗi token (hoặc gõ `Bearer <token>`) $\rightarrow$ Bấm **Authorize** $\rightarrow$ **Close**.

---

## 📑 MỤC LỤC THEO ROLE
1. [GUEST / KHÁCH VÃNG LAI (Chưa đăng nhập)](#1-guest--khách-vãng-lai-chưa-đăng-nhập)
2. [CUSTOMER / KHÁCH HÀNG (Role: Customer)](#2-customer--khách-hàng-role-customer)
3. [STAFF / NHÂN VIÊN (Role: Staff)](#3-staff--nhân-viên-role-staff)
4. [ADMIN / QUẢN TRỊ VIÊN (Role: Admin)](#4-admin--quản-trị-viên-role-admin)
5. [TEST ODATA & CONTENT NEGOTIATION (Biểu diễn đồ án)](#5-test-odata--content-negotiation-biểu-diễn-đồ-án)

---

## 1. GUEST / KHÁCH VÃNG LAI (Chưa đăng nhập)

### 1.1. Đăng ký tài khoản mới (`POST /api/auth/register`)

#### Mẫu 1: Đăng ký khách hàng Hà Nội
```json
{
  "fullName": "Nguyễn Hoàng Nam",
  "email": "nam.nguyen@gmail.com",
  "password": "Password123",
  "phone": "0987654321",
  "address": "Số 45 Phố Huế, Hoàn Kiếm, Hà Nội"
}
```

#### Mẫu 2: Đăng ký khách hàng Đà Nẵng
```json
{
  "fullName": "Trần Thị Thu Thảo",
  "email": "thao.tran@gmail.com",
  "password": "Password123",
  "phone": "0905123456",
  "address": "128 Bạch Đằng, Hải Châu, Đà Nẵng"
}
```

---

### 1.2. Đăng nhập hệ thống (`POST /api/auth/login`)

#### Mẫu Customer (Khách hàng):
```json
{
  "email": "customer@petshop.com",
  "password": "Customer@123"
}
```

#### Mẫu Staff (Nhân viên):
```json
{
  "email": "staff@petshop.com",
  "password": "Staff@123"
}
```

#### Mẫu Admin (Quản trị viên):
```json
{
  "email": "admin@petshop.com",
  "password": "Admin@123"
}
```

---

### 1.3. Khám phá & Tìm kiếm (Công khai không cần Token)

* **`GET /api/products/search` (Tìm thức ăn chó mèo phân trang 8 sản phẩm/trang):**
  * `search`: `Royal`
  * `category`: `Food`
  * `page`: `1`
  * `pageSize`: `8`

* **`GET /api/pets/search` (Tìm thú cưng loài Chó giá từ 5 triệu đến 15 triệu):**
  * `category`: `Dog`
  * `minPrice`: `5000000`
  * `maxPrice`: `15000000`
  * `status`: `Available`
  * `page`: `1`
  * `pageSize`: `8`

* **`GET /api/products/{id}` & `GET /api/pets/{id}`:**
  * Lấy chi tiết sản phẩm ID: `1`
  * Lấy chi tiết thú cưng ID: `1`

---

## 2. CUSTOMER / KHÁCH HÀNG (Role: Customer)
> **Tài khoản test:** `customer@petshop.com` | Pass: `Customer@123` (hoặc tài khoản vừa đăng ký ở mục 1.1)

---

### 2.1. Đặt đơn hàng mới COD (`POST /api/orders`)

#### Mẫu 1: Đơn mua hỗn hợp (1 Bé Cún + 2 Sản phẩm thức ăn & đồ chơi)
```json
{
  "items": [
    {
      "productId": null,
      "petId": 3,
      "quantity": 1,
      "unitPrice": 9000000
    },
    {
      "productId": 1,
      "petId": null,
      "quantity": 2,
      "unitPrice": 350000
    },
    {
      "productId": 11,
      "petId": null,
      "quantity": 1,
      "unitPrice": 75000
    }
  ],
  "shippingAddress": "Số 123 Đường Nguyễn Huệ, Phường Bến Nghé, Quận 1, TP.HCM",
  "note": "Giao giờ hành chính, gọi điện trước khi giao hàng 15 phút",
  "isCod": true
}
```

#### Mẫu 2: Đơn chỉ mua sản phẩm phụ kiện & chăm sóc
```json
{
  "items": [
    {
      "productId": 5,
      "petId": null,
      "quantity": 1,
      "unitPrice": 120000
    },
    {
      "productId": 7,
      "petId": null,
      "quantity": 1,
      "unitPrice": 450000
    },
    {
      "productId": 15,
      "petId": null,
      "quantity": 2,
      "unitPrice": 195000
    }
  ],
  "shippingAddress": "Toà nhà Landmark 81, Phường 22, Bình Thạnh, TP.HCM",
  "note": "Gửi lễ tân toà nhà nếu không gọi được",
  "isCod": true
}
```

#### Mẫu 3: Đơn mua bé Mèo Ba Tư thuần chủng
```json
{
  "items": [
    {
      "productId": null,
      "petId": 13,
      "quantity": 1,
      "unitPrice": 6800000
    }
  ],
  "shippingAddress": "Số 88 Lê Duẩn, Phường Bến Nghé, Quận 1, TP.HCM",
  "note": "Vận chuyển lồng cẩn thận, bé mèo con nhát người",
  "isCod": true
}
```

---

### 2.2. Xem đơn & Theo dõi trạng thái đơn

* **`GET /api/orders/my`:** Lấy toàn bộ lịch sử đơn hàng của chính tài khoản đang đăng nhập.
* **`GET /api/orders/{id}`:** Xem chi tiết đơn hàng (ví dụ: `id = 1` hoặc `id = 2`).

---

### 2.3. Khách hàng hủy đơn (`POST /api/orders/{id}/cancel` hoặc `DELETE /api/orders/{id}`)
> *Lưu ý: Chỉ áp dụng được cho đơn đang ở trạng thái `Pending` (Chờ xử lý).*
* Điền `id`: `1` (hoặc id đơn mới tạo).

---

### 2.4. Khách hàng xác nhận đã nhận hàng (`POST /api/orders/{id}/confirm-received`)
> *Chuyển trạng thái đơn sang `Delivered` (Giao thành công).*
* Điền `id`: `1` (hoặc id đơn của bạn).

---

### 2.5. Gửi đánh giá Review (`POST /api/reviews`)

#### Mẫu 1: Đánh giá bé cún đã mua
```json
{
  "productId": null,
  "petId": 1,
  "rating": 5,
  "comment": "Bé Golden rất ngoan, ăn khỏe, sổ tiêm phòng đầy đủ. Shop tư vấn chăm sóc cực kỳ nhiệt tình!"
}
```

#### Mẫu 2: Đánh giá thức ăn cho mèo
```json
{
  "productId": 2,
  "petId": null,
  "rating": 5,
  "comment": "Pate Whiskas mèo nhà mình rất thích ăn, đóng gói cẩn thận, hạn sử dụng còn rất xa."
}
```

---

## 3. STAFF / NHÂN VIÊN (Role: Staff)
> **Tài khoản test:** `staff@petshop.com` | Pass: `Staff@123`

---

### 3.1. Quản lý Sản phẩm mới (`POST /api/products`)

#### Mẫu 1: Thức ăn dinh dưỡng
```json
{
  "name": "Thức ăn hạt Orijen Six Fish cho mèo 1.8kg",
  "description": "Thức ăn cao cấp từ 6 loại cá tươi đại dương, 85% thành phần từ thịt cá tươi giàu đạm và Omega-3 giúp lông bóng mượt.",
  "price": 620000,
  "stock": 40,
  "category": "Food",
  "imageUrl": "https://images.unsplash.com/photo-1589924691995-400dc9ecc119?auto=format&fit=crop&w=800&q=80"
}
```

#### Mẫu 2: Phụ kiện chuồng đệm
```json
{
  "name": "Nệm tròn lông cừu êm ái cho chó mèo size XL",
  "description": "Nệm thú cưng siêu êm ái, bọc vải lông cừu giữ ấm tốt, ruột bông PP kháng khuẩn đàn hồi, đáy chống trượt an toàn.",
  "price": 320000,
  "stock": 25,
  "category": "Accessory",
  "imageUrl": "https://images.unsplash.com/photo-1541599540903-216a46ca1dc0?auto=format&fit=crop&w=800&q=80"
}
```

#### Mẫu 3: Đồ chơi huấn luyện
```json
{
  "name": "Tháp bóng đồ chơi 4 tầng cho mèo kèm bóng lăn",
  "description": "Tháp bóng nhựa ABS chịu lực cao cấp, gồm 4 tầng rãnh bóng kích thích bản năng vồ bắt của mèo cưng.",
  "price": 115000,
  "stock": 60,
  "category": "Toy",
  "imageUrl": "https://images.unsplash.com/photo-1533738363-b7f9aef128ce?auto=format&fit=crop&w=800&q=80"
}
```

#### Mẫu 4: Sản phẩm Grooming vệ sinh
```json
{
  "name": "Sữa tắm trị ve nấm thảo dược Davis 355ml",
  "description": "Sữa tắm đặc trị viêm da, nấm ngứa, ve rận chiết xuất tràm trà thiên nhiên, an toàn cho da nhạy cảm chó mèo.",
  "price": 285000,
  "stock": 35,
  "category": "Grooming",
  "imageUrl": "https://images.unsplash.com/photo-1608248597359-bb47401a09d6?auto=format&fit=crop&w=800&q=80"
}
```

---

### 3.2. Cập nhật Sản phẩm (`PUT /api/products/{id}`)
> *Điền `id`: `1`*
```json
{
  "name": "Thức ăn Royal Canin Maxi Adult (Tăng dung tích khuyến mãi)",
  "description": "Thức ăn hạt cao cấp cho chó trưởng thành giống lớn từ 26-44kg, tặng thêm 10% trọng lượng, bổ sung bảo vệ xương khớp.",
  "price": 365000,
  "stock": 90,
  "category": "Food",
  "imageUrl": "https://images.unsplash.com/photo-1589924691995-400dc9ecc119?auto=format&fit=crop&w=800&q=80"
}
```

---

### 3.3. Quản lý Thú cưng mới (`POST /api/pets`)

#### Mẫu 1: Thêm bé Chó Corgi
```json
{
  "name": "Bắp",
  "breed": "Corgi Pembroke Mông Trái Tim",
  "category": "Dog",
  "ageMonths": 2,
  "price": 9500000,
  "description": "Corgi Pembroke thuần chủng lưng thẳng chân ngắn mông trái tim cực kỳ đáng yêu, ăn uống tốt, tiêm phòng 2 mũi.",
  "imageUrl": "https://images.unsplash.com/photo-1612536057832-2ff7ead58194?auto=format&fit=crop&w=800&q=80",
  "status": "Available"
}
```

#### Mẫu 2: Thêm bé Mèo Anh lông dài
```json
{
  "name": "Mây",
  "breed": "Mèo Anh Lông Dài (BLH) Màu Bicolor",
  "category": "Cat",
  "ageMonths": 3,
  "price": 8200000,
  "description": "Bé mèo Anh lông dài trắng xám tơ dày mượt như bông gòn, mắt màu đồng hổ phách tuyệt đẹp, quấn người.",
  "imageUrl": "https://images.unsplash.com/photo-1574158622682-e40e69881006?auto=format&fit=crop&w=800&q=80",
  "status": "Available"
}
```

#### Mẫu 3: Thêm bé Vẹt Sun Conure
```json
{
  "name": "Sunny",
  "breed": "Vẹt Sun Conure Sắc Màu",
  "category": "Bird",
  "ageMonths": 5,
  "price": 3800000,
  "description": "Vẹt Sun Conure màu vàng cam rực rỡ, đã thuần người thả bay tự do trong nhà, tập nói được các từ ngắn.",
  "imageUrl": "https://images.unsplash.com/photo-1552728089-57bdde30beb3?auto=format&fit=crop&w=800&q=80",
  "status": "Available"
}
```

---

### 3.4. Cập nhật Thú cưng (`PUT /api/pets/{id}`)
> *Điền `id`: `1`*
```json
{
  "name": "Buddy (Đạt chứng nhận VKA)",
  "breed": "Golden Retriever Thuần Chủng VKA",
  "category": "Dog",
  "ageMonths": 4,
  "price": 9000000,
  "description": "Chó Golden Retriever thuần chủng có phả hệ VKA, bố mẹ nhập khẩu, tiêm đủ 3 mũi vaccine và vi chip định danh.",
  "imageUrl": "https://images.unsplash.com/photo-1552053831-71594a27632d?auto=format&fit=crop&w=800&q=80",
  "status": "Available"
}
```

---

### 3.5. Xử lý & Điều phối Đơn hàng

* **Xem tất cả đơn hàng:** `GET /api/orders`
* **Duyệt đơn COD (`PATCH /api/orders/{id}/status`):**
  * Điền `id`: `1` (hoặc id đơn muốn duyệt)
  ```json
  {
    "status": "Confirmed"
  }
  ```
* **Tạo vận đơn giao nhận GHN (`POST /api/orders/{id}/ship`):**
  * Điền `id`: `1`
  ```json
  {
    "note": "Đơn hàng có thú cưng, yêu cầu xe có thông thoáng nhiệt độ mát"
  }
  ```
  *(API tự sinh mã vận đơn `GHN...`, cập nhật đơn sang `Shipping`, sinh URL tra cứu và đẩy SignalR)*

---

## 4. ADMIN / QUẢN TRỊ VIÊN (Role: Admin)
> **Tài khoản test:** `admin@petshop.com` | Pass: `Admin@123`

---

### 4.1. Thống kê & Báo cáo cấp cao

* **`GET /api/orders/stats`:** Thống kê tổng doanh thu, số đơn chờ, đơn đang giao, đơn hủy và biểu đồ doanh thu.
* **`GET /api/orders/audit-logs?page=1`:** Kiểm tra lịch sử thao tác của các tài khoản nhân viên trong hệ thống.
* **`GET /api/orders/export`:** Tải file dữ liệu báo cáo danh sách đơn hàng.

---

### 4.2. Quản trị Người dùng & Phân quyền

* **`GET /api/users`:** Lấy danh sách toàn bộ khách hàng và nhân viên trong hệ thống.
* **`GET /api/users/{id}`:** Xem chi tiết tài khoản (ví dụ `id = 2`).
* **`PATCH /api/users/{id}/deactivate`:** Khóa / vô hiệu hóa tài khoản vi phạm (ví dụ `id = 3`).

---

### 4.3. Xóa dữ liệu (Quyền tối cao Admin)

* **`DELETE /api/products/{id}`:** Xóa sản phẩm khỏi hệ thống (ví dụ `id = 15`).
* **`DELETE /api/pets/{id}`:** Xóa thú cưng (ví dụ `id = 8`).

---

## 5. TEST ODATA & CONTENT NEGOTIATION (Biểu diễn đồ án)

### 5.1. OData Query URLs (Dán trực tiếp trên thanh địa chỉ trình duyệt hoặc Swagger)

* **Lọc sản phẩm giá > 200.000đ, sắp xếp giảm dần, chỉ lấy Tên + Giá:**
  ```
  http://localhost:50142/odata/Products?$filter=Price gt 200000&$orderby=Price desc&$select=Id,Name,Price,Stock
  ```
* **Lọc thú cưng loài Mèo (`Cat`) giá dưới 10.000.000đ:**
  ```
  http://localhost:50142/odata/Pets?$filter=Category eq 'Cat' and Price lt 10000000&$orderby=Price asc
  ```
* **Lấy Top 3 sản phẩm đắt nhất:**
  ```
  http://localhost:50142/odata/Products?$orderby=Price desc&$top=3
  ```

---

### 5.2. Content Negotiation (Đàm phán định dạng trả về)

Khi gọi `GET /api/products` hoặc `GET /api/pets`, đổi header **`Accept`**:
* `Accept: application/json` $\rightarrow$ Trả về JSON (mặc định).
* `Accept: application/xml` $\rightarrow$ Trả về định dạng XML.
* `Accept: text/csv` $\rightarrow$ Kích hoạt **Custom `CsvOutputFormatter`** tự sinh định dạng bảng CSV.
* `Accept: application/pdf` $\rightarrow$ Trả về mã lỗi **`406 Not Acceptable`** (Chứng minh tính chặt chẽ của Content Negotiation).
