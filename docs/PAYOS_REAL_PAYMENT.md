# PayOS real payment setup

Ngay cap nhat: 2026-07-26

## Luong thanh toan that

1. User bam `Thanh toan PayOS` tu gio hang hoac don `Pending`.
2. PetShop.Client goi Order.Service tao order/link thanh toan.
3. Order.Service goi PayOS API `POST /v2/payment-requests`.
4. PayOS tra `checkoutUrl`; client redirect user sang trang checkout PayOS.
5. User thanh toan tren PayOS.
6. PayOS goi webhook `POST /api/orders/payments/payos/webhook`.
7. Order.Service verify signature, verify amount khop tong don, roi doi order `Pending` -> `Confirmed`.

UI khong con nut `Toi da thanh toan`; xac nhan thanh toan production chi di qua webhook PayOS hop le.

## Config can co

Trong `Services/Order.Service/appsettings.json` da co block mau:

```json
"PayOS": {
  "BaseUrl": "https://api-merchant.payos.vn/",
  "ClientId": "CHANGE_ME",
  "ApiKey": "CHANGE_ME",
  "ChecksumKey": "CHANGE_ME",
  "ReturnUrl": "http://localhost:50140/User/Orders?expand={orderId}",
  "CancelUrl": "http://localhost:50140/User/Orders?expand={orderId}",
  "AllowManualConfirmation": false
}
```

Khi deploy, nen cau hinh bang environment variables thay vi commit secret:

```powershell
PayOS__ClientId=<client-id-tu-payos>
PayOS__ApiKey=<api-key-tu-payos>
PayOS__ChecksumKey=<checksum-key-tu-kenh-thanh-toan>
PayOS__ReturnUrl=https://your-domain/User/Orders?expand={orderId}
PayOS__CancelUrl=https://your-domain/User/Orders?expand={orderId}
PayOS__AllowManualConfirmation=false
PayOS__PendingTimeoutMinutes=30
```

## Webhook tren dashboard PayOS

Dat webhook URL cua kenh thanh toan PayOS ve gateway public:

```text
https://your-api-domain/api/orders/payments/payos/webhook
```

Neu chay local, PayOS can goi duoc vao may cua ban, nen can public tunnel HTTPS nhu ngrok/cloudflared va dat webhook theo URL tunnel.

## Ghi chu van hanh

- `AllowManualConfirmation` nen giu `false` cho production.
- `description` gui PayOS duoc rut gon theo mau `PS{orderId}` de phu hop gioi han ngan cua noi dung chuyen khoan.
- Order chi confirmed neu webhook PayOS hop le va `amount` khop `TotalAmount`.
- Neu tao order thanh cong nhung tao payment link loi, order van nam `Pending`; user co the bam thanh toan lai tu `/User/Orders`.
- `PendingPayOsOrderCleanupService` (BackgroundService) tu dong huy don PayOS `Pending` qua thoi gian timeout (mac dinh 30 phut) va hoan ton kho. Cau hinh qua `PayOS:PendingTimeoutMinutes`.
