# OData Demo

OData đã được cấu hình trong Catalog.Service:

`PetShop.Client -> ApiGateway -> Catalog.Service`

## Endpoint qua ApiGateway

Products:

```http
GET http://localhost:5000/odata/Products?$count=true&$filter=contains(Name,'thức')&$orderby=Price desc&$top=5&$skip=0
```

Pets:

```http
GET http://localhost:5000/odata/Pets?$count=true&$filter=Status eq 'Available'&$orderby=Price asc&$top=5&$skip=0
```

## Màn hình demo

Trang Staff Products (`/Staff/Products`) sử dụng OData để query:

- Tìm tên sản phẩm: `$filter=contains(Name,'...')`
- Lọc danh mục: `$filter=Category eq '...'`
- Sắp xếp: `$orderby` (CreatedAt desc, Price asc/desc, Name asc/desc, Stock asc)
- Phân trang: `$top` và `$skip`
- Đếm tổng: `$count=true`

Nếu gọi OData thành công, footer của bảng sẽ hiện badge `OData`. Nếu không (fallback), Client tự filter/sort/paginate dữ liệu từ API thường.

## Route Gateway

```json
{
  "UpstreamPathTemplate": "/odata/{everything}",
  "UpstreamHttpMethod": ["GET"],
  "DownstreamPathTemplate": "/odata/{everything}",
  "DownstreamScheme": "http",
  "DownstreamHostAndPorts": [{"Host": "localhost","Port": 5002}],
  "Key": "catalog-odata"
}
```

## File liên quan

- `Services/Catalog.Service/Program.cs` — Cấu hình OData `AddOData()`, `EntitySet<Product>`, `EntitySet<Pet>`
- `Services/Catalog.Service/Controllers/CatalogControllers.cs` — `ProductsODataController`, `PetsODataController`
- `ApiGateway/ocelot.json` — Route `/odata/{everything}` → Catalog.Service:5002
- `PetShop.Client/Services/ClientServices.cs` — `ProductClientService.GetODataAsync()`
- `PetShop.Client/Pages/Staff/Products/Index.cshtml.cs` — Staff product list dùng OData
