# Luồng đơn hàng, thanh toán và giao hàng

## COD

1. User thêm sản phẩm vào giỏ.
2. User đặt hàng bằng COD.
3. Đơn được tạo với `Status = Pending`, `PaymentStatus = CODPending`.
4. Staff kiểm tra đơn và bấm `Xác nhận COD`.
5. Đơn chuyển sang `Confirmed`.
6. Staff tạo vận đơn với hãng ship ngoài.
7. Hệ thống gửi `codAmount = TotalAmount` cho dịch vụ vận chuyển.
8. Đơn chuyển sang `Shipping`.
9. Khi shipper/staff báo đã giao tới khách, staff chuyển đơn sang `AwaitingReceipt`.
10. User bấm `Đã nhận được hàng`; đơn mới chuyển `Delivered`, lúc này xem như COD đã thu.

## PayOS

1. User thêm sản phẩm vào giỏ.
2. User chọn thanh toán PayOS.
3. Đơn được tạo với `Status = Pending`, `PaymentStatus = PayOSPending`.
4. Hệ thống tạo checkout URL PayOS và chuyển user sang trang PayOS.
5. Sau khi user thanh toán, PayOS gọi webhook về backend.
6. Backend verify webhook và tự chuyển đơn sang `Confirmed`, `PaymentStatus = Paid`.
7. Staff tạo vận đơn với hãng ship ngoài.
8. Hệ thống gửi `codAmount = 0` cho dịch vụ vận chuyển vì user đã trả tiền qua PayOS.
9. Đơn chuyển sang `Shipping`.
10. Khi shipper/staff báo đã giao tới khách, staff chuyển đơn sang `AwaitingReceipt`.
11. User bấm `Đã nhận được hàng`; đơn mới chuyển `Delivered`.

Nếu user không hoàn tất thanh toán, background service (`PendingPayOsOrderCleanupService`) sẽ tự hủy đơn PayOS đang `Pending` sau `PayOS:PendingTimeoutMinutes` phút và hoàn tồn kho.

## Quy tắc Staff

- Staff chỉ bấm `Xác nhận COD` cho đơn COD đang `Pending`.
- Staff không xác nhận thủ công đơn PayOS đang `Pending`; phải chờ webhook PayOS hợp lệ.
- Staff chỉ tạo vận đơn khi đơn đã `Confirmed`.
- Staff không chốt thẳng `Delivered`. Staff chỉ báo đã giao tới khách bằng trạng thái `AwaitingReceipt`.
- User là người xác nhận cuối cùng bằng nút `Đã nhận được hàng`; sau đó đơn mới `Delivered`.
- Đơn PayOS đã thanh toán và đơn COD đã xác nhận đều đi chung luồng giao hàng từ bước `Confirmed`.
- User chỉ được hủy đơn khi đơn còn `Pending`. Đơn PayOS đã thanh toán hoặc đơn đã xác nhận phải xử lý qua admin/hoàn tiền ngoài luồng nếu cần.

## Vòng đời trạng thái đơn hàng

```
Pending ──→ Confirmed ──→ Shipping ──→ AwaitingReceipt ──→ Delivered
   │            │                          │
   └──→ Cancelled ←──┘                      └──→ Shipping (rollback)
```

## Ship ngoài hiện tại

Code hiện dùng `MockShippingService` để giả lập GHN/GHTK:

- Sinh mã vận đơn giả.
- Lưu `TrackingCode`, `ShippingProvider`, `TrackingUrl`.
- Chuyển trạng thái đơn sang `Shipping`.

Khi làm production, thay `MockShippingService` bằng service gọi API thật của GHN, GHTK hoặc Viettel Post. Với COD gửi tiền thu hộ bằng tổng đơn; với PayOS gửi tiền thu hộ bằng `0`.
