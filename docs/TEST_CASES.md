# Test Cases PetShop

## Chuẩn bị

- Chạy đủ service: Auth, User, Catalog, Order, ApiGateway, Client.
- Có 3 tài khoản mặc định:
  - Admin: `admin@petshop.com` / `Admin@123`
  - Staff: `staff@petshop.com` / `Staff@123`
  - Customer: `customer@petshop.com` / `Customer@123`
- Có ít nhất 1 sản phẩm còn tồn kho và 1 thú cưng trạng thái `Available`.
- PayOS đã cấu hình `ClientId`, `ApiKey`, `ChecksumKey`.
- Ngrok/webhook PayOS đang trỏ về `POST /api/orders/payments/payos/webhook`.

## User - Giỏ hàng và đặt COD

| ID | Test case | Bước test | Kết quả mong đợi |
|---|---|---|---|
| U01 | Thêm sản phẩm vào giỏ | User vào trang chủ, thêm 1 sản phẩm | Badge giỏ hàng tăng, vào giỏ thấy đúng sản phẩm |
| U02 | Tăng giảm số lượng sản phẩm | Trong giỏ bấm `+`/`-` | Số lượng và tổng tiền cập nhật đúng |
| U03 | Thêm thú cưng vào giỏ | Thêm 1 pet vào giỏ | Pet chỉ có số lượng `1`, không cho mua nhiều |
| U04 | Đặt COD thành công | Nhập địa chỉ, bấm `Đặt hàng ngay` | Tạo đơn `Pending`, thanh toán `CODPending`, giỏ được xóa |
| U05 | Không nhập địa chỉ | Bỏ trống địa chỉ rồi đặt hàng | Không tạo đơn, hiện lỗi/đánh dấu input |
| U06 | Giỏ rỗng | Xóa hết giỏ rồi đặt hàng | Không tạo đơn, báo giỏ hàng trống |

## User - PayOS

| ID | Test case | Bước test | Kết quả mong đợi |
|---|---|---|---|
| P01 | Tạo checkout PayOS | Thêm sản phẩm, nhập địa chỉ, bấm `Thanh toán PayOS` | Tạo đơn `Pending/PayOSPending` và redirect sang checkout PayOS |
| P02 | Thanh toán PayOS thành công | Chuyển khoản/thanh toán theo checkout PayOS | Webhook chuyển đơn sang `Confirmed/Paid`; user thấy `Đã thanh toán` |
| P03 | User quay lại trước khi webhook về | Thanh toán xong quay lại lịch sử ngay | Có thể thấy `PayOSPending` tạm thời; refresh/webhook xong chuyển `Paid` |
| P04 | Tạo lại link PayOS | Với đơn PayOS `Pending`, bấm `Tiếp tục thanh toán PayOS` | Mở lại checkout PayOS |
| P05 | PayOS chưa cấu hình key | Xóa/chưa set PayOS key rồi bấm thanh toán | Không tạo được checkout, hiện lỗi cấu hình PayOS |
| P06 | Auto-cancel PayOS timeout | Đặt `PayOS:PendingTimeoutMinutes = 2`, tạo PayOS nhưng không trả | Sau 2-3 phút đơn chuyển `Cancelled`, tồn kho được hoàn |

## User - Lịch sử đơn hàng

| ID | Test case | Bước test | Kết quả mong đợi |
|---|---|---|---|
| O01 | Xem danh sách đơn | User vào `Đơn hàng` | Thấy đơn mới nhất, trạng thái và thanh toán đúng |
| O02 | Expand chi tiết đơn | Bấm mở chi tiết | Thấy địa chỉ, ghi chú, item, tổng tiền, thanh toán |
| O03 | Hủy đơn pending | Với đơn `Pending`, bấm hủy | Đơn chuyển `Cancelled`, hiện thông báo, tồn kho hoàn |
| O04 | Không hủy đơn đã xác nhận | Với đơn `Confirmed/Shipping`, gọi hủy từ UI/API | Backend trả lỗi `Chỉ có thể hủy đơn hàng đang chờ xử lý.` |
| O05 | Filter trạng thái | Bấm filter `Pending`, `Shipping`, `AwaitingReceipt`, `Delivered` | Danh sách chỉ hiện đúng trạng thái |
| O06 | User xác nhận đã nhận | Với đơn `AwaitingReceipt`, bấm `Đã nhận được hàng` | Đơn chuyển `Delivered`, tổng chi tiêu tăng, mở quyền đánh giá |

## Staff - Xác nhận và vận đơn

| ID | Test case | Bước test | Kết quả mong đợi |
|---|---|---|---|
| S01 | Staff thấy đơn COD mới | Staff vào quản lý đơn sau khi user đặt COD | Đơn hiện `Pending`, có nút `Xác nhận COD` |
| S02 | Xác nhận COD | Staff bấm `Xác nhận COD` | Đơn chuyển `Confirmed`, user thấy `Shop đã xác nhận` |
| S03 | Staff thấy PayOS pending | User tạo PayOS chưa trả tiền | Staff thấy `PayOSPending`, không có nút xác nhận thủ công |
| S04 | PayOS paid | User trả PayOS thành công | Staff thấy `Paid`, có thể tạo vận đơn |
| S05 | Tạo vận đơn COD | Staff tạo vận đơn cho đơn COD `Confirmed` | Đơn chuyển `Shipping`, có `TrackingCode`, COD amount bằng tổng đơn |
| S06 | Tạo vận đơn PayOS | Staff tạo vận đơn cho đơn PayOS `Confirmed/Paid` | Đơn chuyển `Shipping`, COD amount bằng `0` |
| S07 | Không tạo vận đơn khi Pending | Staff cố tạo vận đơn cho đơn `Pending` | Backend trả lỗi chỉ tạo vận đơn khi đã xác nhận |
| S08 | Báo đã giao tới khách | Staff chuyển `Shipping` -> `AwaitingReceipt` | User thấy nút `Đã nhận được hàng`, đơn chưa tính hoàn tất |
| S09 | Không chốt Delivered thủ công | Staff/code cố chuyển thẳng `Delivered` bằng API | Backend từ chối, yêu cầu user xác nhận nhận hàng |

## Admin - Quản lý và báo cáo

| ID | Test case | Bước test | Kết quả mong đợi |
|---|---|---|---|
| A01 | Admin xem tất cả đơn | Admin vào `/Admin/Orders` | Thấy toàn bộ đơn, có filter theo status |
| A02 | Admin xem thống kê | Admin vào `/Admin/Reports` | Thấy KPI: tổng đơn, doanh thu, theo trạng thái |
| A03 | Admin export Excel | Admin bấm Export | Tải file Excel có đầy đủ cột |
| A04 | Admin xem audit log | Admin vào `/Admin` -> audit | Xem toàn bộ lịch sử thao tác của tất cả Staff |
| A05 | Admin quản lý users | Admin vào `/Admin/Users` | Vô hiệu hóa / kích hoạt tài khoản |
| A06 | Admin xem chi tiết đơn | Admin bấm vào đơn | Xem thông tin nhưng không có nút sửa trạng thái (chỉ Staff mới có) |

## Tồn kho và pet

| ID | Test case | Bước test | Kết quả mong đợi |
|---|---|---|---|
| I01 | Trừ tồn kho sản phẩm | Đặt COD/PayOS với sản phẩm còn tồn | Stock giảm đúng số lượng |
| I02 | Hoàn tồn khi hủy | Hủy đơn `Pending` | Stock tăng lại đúng số lượng |
| I03 | Không mua quá tồn | Đặt số lượng lớn hơn tồn kho | Backend từ chối tạo đơn |
| I04 | Pet chuyển khỏi Available | Đặt đơn có pet | Pet.Status -> `Sold`, không còn mua lại được |
| I05 | Hoàn pet khi hủy pending | Hủy đơn pending có pet | Pet.Status -> `Available` |

## Review/Đánh giá

| ID | Test case | Bước test | Kết quả mong đợi |
|---|---|---|---|
| R01 | Chưa giao không đánh giá | User chưa có đơn `Delivered` nhưng tạo review | Backend từ chối |
| R02 | Đã giao được đánh giá | User có đơn `Delivered` chứa sản phẩm/pet | Tạo review thành công |
| R03 | Không review trùng | User cố review lại sản phẩm đã review | Backend từ chối "Ban da danh gia san pham nay" |

## SignalR Realtime

| ID | Test case | Bước test | Kết quả mong đợi |
|---|---|---|---|
| W01 | Staff nhận thông báo đơn mới | User đặt hàng trong khi Staff đang ở trang quản lý | Staff thấy toast "Có đơn hàng mới" |
| W02 | User nhận thông báo đổi trạng thái | Staff cập nhật trạng thái đơn của User | User thấy toast cập nhật real-time |
| W03 | Catalog update real-time | Staff thêm/sửa sản phẩm | Trang chủ cập nhật stock/status real-time |

## Smoke test sau mỗi lần sửa

1. Build solution: `dotnet build PetShop.sln --no-restore`.
2. Login Customer.
3. Thêm sản phẩm vào giỏ.
4. Đặt COD.
5. Login Staff, xác nhận COD.
6. Staff tạo vận đơn.
7. Staff chuyển `Shipping` -> `AwaitingReceipt`.
8. Login Customer, bấm `Đã nhận được hàng` để chuyển `Delivered`.
9. Tạo đơn PayOS và thanh toán thật.
10. Kiểm tra webhook chuyển đơn sang `Confirmed/Paid`.
