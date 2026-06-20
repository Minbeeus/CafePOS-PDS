# 📡 Sprint 0 — API Contract (Sơ bộ)
### Hệ thống Quản lý Bán hàng Quán Cà phê / Trà sữa
**Phiên bản:** 1.1 | **Ngày:** 11/06/2026  
**Base URL:** `https://localhost:{port}/api/v1`

---

## Quy ước chung

### Authentication
```
Header: Authorization: Bearer {JWT_TOKEN}
```

### Response Format chuẩn
```json
{
  "success": true,
  "data": { ... },
  "message": "OK",
  "errors": []
}
```

### HTTP Status Codes
| Code | Ý nghĩa |
|------|---------|
| 200 | OK |
| 201 | Created |
| 400 | Bad Request (validation error) |
| 401 | Unauthorized |
| 403 | Forbidden (không đủ quyền) |
| 404 | Not Found |
| 409 | Conflict (trùng dữ liệu) |
| 500 | Internal Server Error |

### Role-based Access
```
[Owner]       → Tất cả
[ShiftLeader] → POS + Shift + Reports (giới hạn)
[Cashier]     → POS only
[Barista]     → PDS Bar only
[PastryStaff] → PDS Pastry only
[Customer]    → Online ordering only
```

---

## 🔐 Module 1: Authentication

### 1.1 Staff Login (Admin Dashboard)
```
POST /auth/staff/login
Auth: None

Request:
{
  "username": "admin",
  "password": "string"
}

Response 200:
{
  "token": "eyJ...",
  "refreshToken": "string",
  "staff": {
    "id": 1,
    "fullName": "Nguyễn Văn A",
    "role": "Owner"
  },
  "expiresAt": "2026-06-12T00:00:00Z"
}
```

### 1.2 POS Quick Login (bằng mã code)
```
POST /auth/pos/login
Auth: None

Request:
{
  "posCode": "2018000520"
}

Response 200:
{
  "token": "eyJ...",
  "staff": {
    "id": 2,
    "fullName": "Trần Thị B",
    "role": "Cashier"
  }
}
```

### 1.3 Customer Register
```
POST /auth/customer/register
Auth: None

Request:
{
  "fullName": "Lê Văn C",
  "phone": "0912345678",
  "password": "string"
}

Response 201:
{
  "customerId": 10,
  "fullName": "Lê Văn C",
  "phone": "0912345678"
}
```

### 1.4 Customer Login (SĐT + Password)
```
POST /auth/customer/login
Auth: None

Request:
{
  "phone": "0912345678",
  "password": "string"
}

Response 200: { "token": "...", "customer": { ... } }
```

### 1.5 Customer Login (Google OAuth)
```
POST /auth/customer/google
Auth: None

Request:
{
  "idToken": "google_id_token"
}

Response 200: { "token": "...", "customer": { ... }, "isNewUser": true }
```

### 1.6 Xác thực Mã SL/Owner (cho discount)
```
POST /auth/staff/verify-code
Auth: Bearer [Cashier/SL]

Request:
{
  "posCode": "SL_CODE",
  "requiredRole": "ShiftLeader"
}

Response 200:
{
  "isValid": true,
  "staffId": 3,
  "staffName": "Phạm SL",
  "role": "ShiftLeader"
}
```

---

## 🍽️ Module 2: Menu Management

### 2.1 Lấy danh sách danh mục
```
GET /categories
Auth: None (public cho customer web) / Bearer cho admin

Response 200:
{
  "data": [
    {
      "id": 1,
      "name": "Trà Sữa",
      "displayStation": "Bar",
      "displayOrder": 1,
      "isActive": true,
      "productCount": 12
    }
  ]
}
```

### 2.2 CRUD Categories (Manager only)
```
POST   /categories          → Tạo danh mục
PUT    /categories/{id}     → Cập nhật
DELETE /categories/{id}     → Soft delete (IsActive = false)
```

### 2.3 Lấy danh sách sản phẩm (có phân trang, filter)
```
GET /products?categoryId=1&status=Active&page=1&pageSize=20
Auth: None (public) / Bearer

Response 200:
{
  "data": {
    "items": [
      {
        "id": 1,
        "name": "Trà Sữa Truyền Thống",
        "description": "...",
        "imageUrl": "...",
        "basePrice": 35000,
        "status": "Active",
        "categoryId": 1,
        "categoryName": "Trà Sữa",
        "hasSizeOption": true,
        "hasSugarOption": true,
        "hasIceOption": true,
        "sizes": [
          { "sizeLabel": "S", "priceModifier": 0, "isDefault": true },
          { "sizeLabel": "M", "priceModifier": 5000 },
          { "sizeLabel": "L", "priceModifier": 10000 }
        ],
        "toppings": [
          { "id": 1, "name": "Trân châu đen", "price": 5000 }
        ]
      }
    ],
    "totalCount": 45,
    "page": 1,
    "pageSize": 20
  }
}
```

### 2.4 CRUD Products
```
POST   /products            → Tạo sản phẩm [Owner]
PUT    /products/{id}       → Cập nhật [Owner]
PATCH  /products/{id}/status → Đổi trạng thái Active/Inactive/OutOfStock [Owner/SL]
DELETE /products/{id}       → Soft delete [Owner]
```

### 2.5 CRUD Toppings
```
GET    /toppings            → Lấy danh sách
POST   /toppings            → Tạo [Owner]
PUT    /toppings/{id}       → Cập nhật [Owner]
PATCH  /toppings/{id}/toggle → Bật/tắt [Owner]
```

---

## 🛒 Module 3: Orders (POS)

### 3.1 Tạo đơn hàng mới
```
POST /orders
Auth: Bearer [CSR/SL]

Request:
{
  "type": "DineIn",           // DineIn | TakeAway | Online
  "tableNumber": "05",        // Null nếu TakeAway
  "customerPhone": "091...",  // Optional, tra cứu khách
  "customerName": "Anh",      // Fallback nếu chưa có tài khoản
  "scheduledPickupTime": null,
  "items": [
    {
      "productId": 1,
      "quantity": 2,
      "sizeLabel": "M",
      "sugarLevel": "50",
      "iceLevel": "100",
      "toppingIds": [1, 3],
      "notes": "Ít đường hơn",
      "isPointRedemption": false
    }
  ],
  "voucherCode": "SUMMER10",
  "applyLoyaltyDiscount": true,
  "manualDiscountPercent": 0
}

Response 201:
{
  "data": {
    "orderId": 100,
    "orderCode": "CF2406110001",
    "subOrderIndex": 1,
    "status": "Confirmed",
    "subTotal": 80000,
    "discountAmount": 8000,
    "totalAmount": 72000,
    "estimatedReadyTime": "2026-06-11T10:15:00"
  }
}
```

### 3.2 Thêm món vào bàn đang mở
```
POST /orders/{parentOrderId}/sub-order
Auth: Bearer [CSR/SL]

Request: (tương tự tạo đơn, không cần tableNumber)
{
  "items": [ ... ]
}

Response 201:
{
  "orderId": 101,
  "orderCode": "CF2406110001",
  "subOrderIndex": 2,
  ...
}
```

### 3.3 Lấy danh sách đơn (POS view - trong ca)
```
GET /orders?status=Confirmed,Preparing&date=today
Auth: Bearer [CSR/SL]

Response: Danh sách đơn hàng kèm trạng thái
```

### 3.4 Lấy chi tiết đơn
```
GET /orders/{id}
Auth: Bearer [CSR/SL/Customer]

Response: Full order detail với items, discounts, payment info
```

### 3.5 Xác nhận đơn Online
```
PATCH /orders/{id}/confirm
Auth: Bearer [CSR/SL]

Response 200: { "orderId": ..., "status": "Confirmed" }
```

### 3.6 Huỷ đơn
```
PATCH /orders/{id}/cancel
Auth: Bearer [CSR/SL/Owner]

Request: { "reason": "Khách không lấy" }
```

### 3.7 Áp dụng discount thủ công
```
POST /orders/{id}/manual-discount
Auth: Bearer [CSR/SL]

Request:
{
  "discountPercent": 20,
  "approverPosCode": "SL_CODE",
  "approverNote": "VIP customer"
}

Response 200:
{
  "discountAmount": 14400,
  "totalAmount": 57600,
  "approvedBy": "Phạm SL"
}
```

---

## 💳 Module 4: Payment

### 4.1 Thanh toán đơn hàng
```
POST /orders/{id}/payment
Auth: Bearer [CSR/SL]

Request:
{
  "paymentMethod": "Cash",    // Cash | Transfer | Mixed
  "amountReceived": 100000,   // Tiền khách đưa (với Cash)
  "transferAmount": 0,        // Số tiền CK (với Mixed)
  "pointsToRedeem": 0         // Điểm muốn dùng
}

Response 200:
{
  "paymentStatus": "Paid",
  "amountReceived": 100000,
  "amountChange": 28000,
  "pointsEarned": 7,
  "newTotalPoints": 35,
  "newTotalSpend": 1072000,
  "tierUpgraded": false
}
```

### 4.2 Xoá món khỏi đơn (Online order chưa confirm)
```
DELETE /orders/{orderId}/items/{itemId}
Auth: Bearer [CSR/SL]
Note: Chỉ được xoá khi đơn ở trạng thái Pending (chưa confirm)

Response 200:
{
  "orderId": 200,
  "removedItemId": 301,
  "newSubTotal": 70000,
  "newDiscountAmount": 7000,
  "newTotalAmount": 63000
}
```

### 4.3 Đổi món trong đơn (Online order chưa confirm)
```
PUT /orders/{orderId}/items/{itemId}
Auth: Bearer [CSR/SL]
Note: Thay thế item bằng món mới, recalculate tổng

Request:
{
  "productId": 5,
  "quantity": 1,
  "sizeLabel": "M",
  "sugarLevel": "50",
  "iceLevel": "100",
  "toppingIds": [],
  "notes": ""
}

Response 200: Full order detail sau khi đổi món
```

> ❌ ~~**4.x In hoá đơn**~~ — Bỏ qua trong phạm vi hiện tại

---

## 📺 Module 5: PDS (Production Display)

### 5.1 Lấy danh sách đơn cần xử lý
```
GET /pds/orders?station=Bar
Auth: Bearer [Barista/PastryStaff]
Query: station = Bar | Pastry

Response 200:
{
  "data": [
    {
      "orderId": 100,
      "orderCode": "CF2406110001",
      "subOrderIndex": 1,
      "displayCode": "CF2406110001/1",
      "type": "DineIn",
      "tableNumber": "05",
      "status": "Confirmed",
      "createdAt": "...",
      "items": [
        {
          "itemId": 201,
          "productName": "Trà Sữa Truyền Thống",
          "quantity": 2,
          "sizeLabel": "M",
          "sugarLevel": "50",
          "iceLevel": "100",
          "toppings": ["Trân châu đen"],
          "notes": "Ít đường hơn",
          "barStatus": "Pending"
        }
      ]
    }
  ]
}
```

### 5.2 Bắt đầu làm món (cập nhật item status)
```
PATCH /pds/order-items/{itemId}/status
Auth: Bearer [Barista/PastryStaff]

Request:
{
  "station": "Bar",
  "status": "InProgress"
}
```

### 5.3 Đánh dấu Done từng món
```
PATCH /pds/order-items/{itemId}/done
Auth: Bearer [Barista/PastryStaff]

Request: { "station": "Bar" }
```

### 5.4 Hoàn thành toàn bộ quầy
```
PATCH /pds/orders/{orderId}/complete-station
Auth: Bearer [Barista/PastryStaff]

Request: { "station": "Bar" }

Response 200:
{
  "orderId": 100,
  "barCompleted": true,
  "pastryCompleted": false,
  "orderCompleted": false   // true khi cả 2 quầy done
}
```

> **SignalR Hub:** `/hubs/orders`  
> Events: `OrderCreated`, `OrderStatusChanged`, `ItemStatusChanged`, `OrderCompleted`

---

## 🌐 Module 6: Online Ordering (Customer)

### 6.1 Đặt hàng online
```
POST /online-orders
Auth: Bearer [Customer]

Request:
{
  "items": [ ... ],            // Tương tự POS
  "scheduledPickupTime": "2026-06-11T10:30:00",  // Null = ASAP (10 phút)
  "voucherCode": "SUMMER10",
  "pointsToRedeem": 0
}

Response 201:
{
  "orderId": 200,
  "orderCode": "CF2406110010",
  "status": "Pending",
  "estimatedPickupTime": "2026-06-11T10:30:00",
  "totalAmount": 72000
}
```

### 6.2 Xem trạng thái đơn (Customer)
```
GET /online-orders/{id}/status
Auth: Bearer [Customer]

Response: { "status": "Preparing", "estimatedReadyTime": "..." }
```

### 6.3 Lịch sử đơn của khách
```
GET /online-orders/my-orders?page=1&pageSize=10
Auth: Bearer [Customer]
```

---

## 👤 Module 7: Customer Profile & Loyalty

### 7.1 Thông tin khách hàng
```
GET /customers/me
Auth: Bearer [Customer]

Response 200:
{
  "id": 10,
  "fullName": "Lê Văn C",
  "phone": "0912345678",
  "totalSpend": 1200000,
  "loyaltyTier": "Silver",
  "tierDiscountPercent": 10,
  "currentPoints": 45,
  "pointsResetAt": "2026-07-01",
  "nextTierThreshold": 1500000,
  "nextTierName": "Gold"
}
```

### 7.2 Tra cứu khách tại POS
```
GET /customers/lookup?phone=0912345678
Auth: Bearer [CSR/SL]

Response 200:
{
  "id": 10,
  "fullName": "Lê Văn C",
  "loyaltyTier": "Silver",
  "tierDiscountPercent": 10,
  "currentPoints": 45
}
```

### 7.3 Lịch sử điểm
```
GET /customers/me/points?page=1
Auth: Bearer [Customer]
```

### 7.4 Danh sách sản phẩm đổi điểm
```
GET /point-products
Auth: Bearer [Customer/CSR]

Response 200:
{
  "data": [
    {
      "id": 1,
      "name": "Trà Sữa Size S miễn phí",
      "pointCost": 50,
      "linkedProduct": { "id": 1, "name": "Trà Sữa Truyền Thống" }
    }
  ]
}
```

---

## 🏷️ Module 8: Voucher

### 8.1 Validate voucher
```
POST /vouchers/validate
Auth: Bearer [Any]

Request:
{
  "code": "SUMMER10",
  "orderSubTotal": 80000,
  "customerId": 10
}

Response 200:
{
  "isValid": true,
  "voucherId": 5,
  "discountType": "Percent",
  "discountValue": 10,
  "discountAmount": 8000,
  "errorMessage": null
}
```

### 8.2 CRUD Voucher (Manager)
```
GET    /vouchers?page=1           → Danh sách voucher
POST   /vouchers                  → Tạo voucher
PUT    /vouchers/{id}             → Cập nhật
PATCH  /vouchers/{id}/toggle      → Bật/tắt
```

---

## ⏰ Module 9: Shift Management

### 9.1 Mở ca
```
POST /shifts/open
Auth: Bearer [SL/Owner]

Request:
{
  "openingCash": 500000
}

Response 201:
{
  "shiftId": 50,
  "shiftDate": "2026-06-11",
  "openedBy": "Phạm SL",
  "openedAt": "2026-06-11T07:00:00",
  "openingCash": 500000
}
```

### 9.2 Xem ca hiện tại
```
GET /shifts/current
Auth: Bearer [SL/Owner/CSR]

Response: Thông tin ca đang mở + doanh thu tạm tính
```

### 9.3 Báo cáo doanh thu tức thời
```
GET /shifts/current/revenue-summary
Auth: Bearer [SL/Owner]

Response 200:
{
  "totalRevenue": 3500000,
  "cashRevenue": 2000000,
  "transferRevenue": 1500000,
  "orderCount": 45,
  "itemsSold": 120
}
```

### 9.4 Đóng ca
```
POST /shifts/{id}/close
Auth: Bearer [SL/Owner]

Request:
{
  "actualCash": 2480000,
  "actualTransfer": 1500000,
  "notes": "Ca sáng ổn định"
}

Response 200:
{
  "shiftId": 50,
  "expectedCash": 2500000,
  "actualCash": 2480000,
  "cashDifference": -20000,
  "expectedTransfer": 1500000,
  "actualTransfer": 1500000,
  "transferDifference": 0,
  "totalRevenue": 3500000,
  "status": "Closed"
}
```

---

## 📦 Module 10: Inventory

### 10.1 Danh sách nguyên liệu
```
GET /ingredients?alert=low,expiring&page=1
Auth: Bearer [Owner/SL]

Response: Danh sách kèm cảnh báo (sắp hết, sắp hết hạn)
```

### 10.2 CRUD Nguyên liệu
```
POST   /ingredients         → Tạo [Owner]
PUT    /ingredients/{id}    → Cập nhật [Owner]
PATCH  /ingredients/{id}/stock-in → Nhập kho thêm [Owner/SL]
```

### 10.3 Nhập kiểm kho (SL — Bước 1)
```
POST /inventory-checks
Auth: Bearer [SL/Owner]

Request:
{
  "shiftId": 50,
  "notes": "Kiểm kho cuối ngày",
  "items": [
    {
      "ingredientId": 1,
      "actualQuantity": 4.5
    }
  ]
}

Response 201:
{
  "checkId": 20,
  "status": "PendingApproval",
  "items": [
    {
      "ingredientId": 1,
      "ingredientName": "Trà Oolong",
      "systemQuantity": 5.0,
      "actualQuantity": 4.5,
      "difference": -0.5
    }
  ]
}
```

### 10.4 Duyệt điều chỉnh tồn kho (Owner — Bước 2)
```
POST /inventory-checks/{checkId}/approve
Auth: Bearer [Owner]
Note: Cập nhật CurrentQuantity của từng Ingredient về số ActualQuantity
      Tạo IngredientTransaction type = Adjustment cho từng mặt hàng

Request:
{
  "notes": "Xác nhận kiểm kho ngày 11/06/2026"
}

Response 200:
{
  "checkId": 20,
  "approvedAt": "2026-06-11T19:00:00",
  "approvedBy": "Nguyễn Manager",
  "updatedIngredients": 8,
  "status": "Approved"
}
```

### 10.5 Danh sách phiếu kiểm kho chờ duyệt
```
GET /inventory-checks?status=PendingApproval
Auth: Bearer [Owner]

Response: Danh sách phiếu kiểm kho + chi tiết chênh lệch
```

---

## 📊 Module 11: Reports

### 11.1 Báo cáo doanh thu
```
GET /reports/revenue?period=daily&date=2026-06-11
GET /reports/revenue?period=weekly&date=2026-06-11
GET /reports/revenue?period=monthly&year=2026&month=6
Auth: Bearer [Owner]

Response 200:
{
  "period": "daily",
  "date": "2026-06-11",
  "totalRevenue": 5200000,
  "cashRevenue": 3000000,
  "transferRevenue": 2200000,
  "orderCount": 65,
  "avgOrderValue": 80000,
  "discountTotal": 450000
}
```

### 11.2 Doanh thu theo khung giờ
```
GET /reports/revenue/by-hour?date=2026-06-11
Auth: Bearer [Owner]

Response:
{
  "data": [
    { "hour": 7, "revenue": 350000, "orderCount": 5 },
    { "hour": 8, "revenue": 820000, "orderCount": 12 }
  ]
}
```

### 11.3 Sản phẩm bán chạy
```
GET /reports/products/top-selling?period=daily&date=2026-06-11&top=5
GET /reports/products/least-selling?period=daily&date=2026-06-11&top=3
Auth: Bearer [Owner]
```

### 11.4 Báo cáo voucher
```
GET /reports/vouchers?from=2026-06-01&to=2026-06-11
Auth: Bearer [Owner]
```

### 11.5 Báo cáo loyalty / điểm
```
GET /reports/loyalty?page=1
Auth: Bearer [Owner]

Response: Danh sách khách, điểm, tier, tổng chi tiêu
```

### 11.6 Báo cáo tồn kho
```
GET /reports/inventory?alert=all
Auth: Bearer [Owner/SL]

Response:
{
  "lowStock": [ { "id": 1, "name": "Trà Oolong", "currentQty": 2, "minAlert": 3 } ],
  "expiringSoon": [ { "id": 2, "name": "Sữa tươi", "expiresAt": "2026-06-13" } ],
  "discrepancies": [ { ... } ]
}
```

---

## 🔔 SignalR Hubs

### Hub: `/hubs/orders`
| Event | Mô tả | Receiver |
|-------|-------|---------|
| `OrderCreated` | Đơn mới được tạo | PDS, POS |
| `OrderConfirmed` | Đơn Online được xác nhận | PDS, Customer Web |
| `OrderItemStatusChanged` | Item Done/InProgress | POS |
| `OrderStationCompleted` | Một quầy hoàn thành | POS (partial), PDS |
| `OrderCompleted` | Cả 2 quầy Done | POS (notification) |
| `ScheduledOrderTriggered` | Đơn hẹn giờ được kích hoạt (trước 10p) | PDS |

---

## 📁 Cấu trúc Project (ASP.NET Core)

```
CafeManager.sln
├── CafeManager.API/              # ASP.NET Core Web API
│   ├── Controllers/
│   │   ├── AuthController.cs
│   │   ├── OrdersController.cs
│   │   ├── ProductsController.cs
│   │   ├── PdsController.cs
│   │   ├── ShiftsController.cs
│   │   ├── ReportsController.cs
│   │   └── ...
│   ├── Hubs/
│   │   └── OrderHub.cs           # SignalR
│   ├── Middlewares/
│   └── Program.cs
│
├── CafeManager.Application/      # Business Logic (Services)
│   ├── Services/
│   │   ├── OrderService.cs
│   │   ├── PaymentService.cs
│   │   ├── LoyaltyService.cs
│   │   ├── VoucherService.cs
│   │   ├── ShiftService.cs
│   │   └── ReportService.cs
│   ├── DTOs/
│   └── Interfaces/
│
├── CafeManager.Domain/           # Entities & Domain Logic
│   ├── Entities/
│   │   ├── Order.cs
│   │   ├── Product.cs
│   │   ├── Customer.cs
│   │   └── ...
│   └── Enums/
│
├── CafeManager.Infrastructure/   # Data Access
│   ├── Data/
│   │   ├── AppDbContext.cs
│   │   └── Migrations/
│   ├── Repositories/
│   └── BackgroundJobs/
│       └── PointResetJob.cs      # Reset điểm mỗi 2 tháng
│
└── CafeManager.Web/              # ASP.NET Core MVC (Frontend)
    ├── Controllers/              # Điều hướng các luồng yêu cầu
    ├── Views/                    # Giao diện người dùng
    │   ├── POS/
    │   ├── PDS/
    │   ├── Admin/
    │   ├── Customer/
    │   └── Home/
    └── wwwroot/
```
