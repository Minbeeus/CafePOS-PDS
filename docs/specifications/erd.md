# 🗄️ Entity Relationship Diagram (ERD)
### Hệ thống Quản lý Bán hàng Quán Cà phê / Trà sữa
**Phiên bản:** 1.2 | **Ngày:** 22/06/2026

---

## Tổng quan Database Schema

```mermaid
erDiagram

    Staffs {
        int Id PK
        string FullName
        string Phone
        string Email
        string PosCode
        string PasswordHash
        string Role
        decimal BaseSalary
        string Status
        datetime CreatedAt
    }

    Customers {
        int Id PK
        string FullName
        string Phone
        string Email
        string GoogleId
        string PasswordHash
        decimal TotalSpend
        string LoyaltyTier
        int CurrentPoints
        datetime PointsResetAt
        datetime CreatedAt
    }

    Categories {
        int Id PK
        string Name
        string DisplayStation
        int DisplayOrder
        bool IsActive
    }

    Products {
        int Id PK
        int CategoryId FK
        string Name
        string Description
        string ImageUrl
        decimal BasePrice
        string Status
        bool HasSizeOption
        bool HasSugarOption
        bool HasIceOption
        datetime CreatedAt
    }

    ProductSizes {
        int Id PK
        int ProductId FK
        string SizeLabel
        decimal PriceModifier
        bool IsDefault
    }

    Toppings {
        int Id PK
        string Name
        decimal Price
        bool IsActive
        datetime CreatedAt
    }

    Orders {
        int Id PK
        string OrderCode
        string Type "DineIn|TakeAway|Online"
        int CustomerId FK
        int StaffId FK
        string CustomerName
        string CustomerPhone
        string Status "Draft|Pending|Confirmed|Preparing|Completed|Closed|Cancelled"
        decimal SubTotal
        decimal DiscountAmount
        decimal TotalAmount
        datetime ScheduledPickupTime
        datetime ConfirmedAt
        datetime CompletedAt
        datetime ClosedAt
        byte[] RowVersion "Concurrency Token"
        datetime CreatedAt
    }

    Payments {
        int Id PK
        int OrderId FK
        string Method "Cash|Transfer|Mixed"
        decimal Amount
        decimal AmountReceived
        decimal AmountChange
        string ReferenceCode
        int CreatedByStaffId FK
        datetime PaidAt
    }

    OrderItems {
        int Id PK
        int OrderId FK
        int ProductId FK
        int Quantity
        decimal UnitPrice
        decimal ItemTotal
        string Notes
        string SizeLabel
        string SugarLevel
        string IceLevel
        bool IsPointRedemption
        string BarStatus
        string PastryStatus
    }

    OrderItemToppings {
        int Id PK
        int OrderItemId FK
        int ToppingId FK
        decimal ToppingPrice
    }

    OrderDiscounts {
        int Id PK
        int OrderId FK
        string DiscountType
        decimal DiscountValue
        string DiscountDescription
        int VoucherId FK
        int ApprovedByStaffId FK
        datetime ApprovedAt
    }

    LoyaltyTierConfig {
        int Id PK
        string TierName
        decimal MinSpendThreshold
        decimal DiscountPercent
        bool IsActive
    }

    PointProducts {
        int Id PK
        string Name
        int PointCost
        int LinkedProductId FK
        bool IsActive
        datetime CreatedAt
    }

    PointTransactions {
        int Id PK
        int CustomerId FK
        int OrderId FK
        string TransactionType
        int Points
        string Description
        datetime CreatedAt
    }

    Vouchers {
        int Id PK
        string Code
        string DiscountType
        decimal DiscountValue
        decimal MinOrderValue
        int MaxUsageCount
        int UsedCount
        bool IsPermanent
        datetime ExpiresAt
        bool IsActive
        int CreatedByStaffId FK
        datetime CreatedAt
    }

    VoucherUsages {
        int Id PK
        int VoucherId FK
        int OrderId FK
        int CustomerId FK
        datetime UsedAt
    }

    Shifts {
        int Id PK
        datetime ShiftDate
        int OpenedByStaffId FK
        int ClosedByStaffId FK
        datetime OpenedAt
        datetime ClosedAt
        decimal OpeningCash
        decimal ExpectedCash
        decimal ExpectedTransfer
        decimal ActualCash
        decimal ActualTransfer
        decimal CashDifference
        decimal TransferDifference
        string Notes
        string Status
    }

    Ingredients {
        int Id PK
        string Name
        string Unit
        decimal CurrentQuantity
        decimal MinAlertQuantity
        int ExpiryAlertDays
        datetime ExpiresAt
        bool IsActive
        datetime CreatedAt
    }

    ProductIngredients {
        int Id PK
        int ProductId FK
        int IngredientId FK
        decimal Quantity
        string Unit
    }

    InventoryChecks {
        int Id PK
        int ShiftId FK
        int StaffId FK
        datetime CheckedAt
        string Status
        int ApprovedByStaffId FK
        datetime ApprovedAt
        string Notes
    }

    InventoryCheckItems {
        int Id PK
        int InventoryCheckId FK
        int IngredientId FK
        decimal SystemQuantity
        decimal ActualQuantity
        decimal Difference
    }

    IngredientTransactions {
        int Id PK
        int IngredientId FK
        string TransactionType
        decimal Quantity
        int RelatedOrderId FK
        int CreatedByStaffId FK
        string Notes
        datetime CreatedAt
    }

    AuditLogs {
        int Id PK
        int StaffId FK
        string Action
        string EntityName
        int EntityId
        string OldValue
        string NewValue
        string IPAddress
        datetime CreatedAt
    }

    Categories ||--o{ Products : "has"
    Products ||--o{ ProductSizes : "has"
    Orders ||--o{ OrderItems : "contains"
    Orders }o--o| Customers : "placed by"
    Orders }o--|| Staffs : "created by"
    Orders ||--o{ Payments : "paid via"
    Payments }o--|| Staffs : "processed by"
    OrderItems }o--|| Products : "references"
    OrderItems ||--o{ OrderItemToppings : "has"
    OrderItemToppings }o--|| Toppings : "is"
    Orders ||--o{ OrderDiscounts : "has"
    OrderDiscounts }o--o| Vouchers : "uses"
    OrderDiscounts }o--o| Staffs : "approved by"
    PointTransactions }o--|| Customers : "belongs to"
    PointTransactions }o--o| Orders : "from"
    PointProducts }o--|| Products : "redeems"
    VoucherUsages }o--|| Vouchers : "uses"
    VoucherUsages }o--o| Orders : "in"
    VoucherUsages }o--o| Customers : "by"
    Vouchers }o--|| Staffs : "created by"
    Shifts }o--|| Staffs : "opened by"
    Shifts }o--o| Staffs : "closed by"
    InventoryChecks }o--|| Shifts : "in"
    InventoryChecks }o--|| Staffs : "by"
    InventoryChecks ||--o{ InventoryCheckItems : "contains"
    InventoryCheckItems }o--|| Ingredients : "checks"
    IngredientTransactions }o--|| Ingredients : "for"
    IngredientTransactions }o--o| Orders : "from"
    IngredientTransactions }o--o| Staffs : "by"
    Products ||--o{ ProductIngredients : "requires ingredient"
    ProductIngredients }o--|| Ingredients : "is"
    AuditLogs }o--o| Staffs : "performed by"
```

---

## Mô tả chi tiết các Entity

### 📦 Staffs — Nhân viên
| Column | Type | Mô tả |
|--------|------|-------|
| PosCode | string | Mã đăng nhập POS (ví dụ: 2018000520), được hash bcrypt |
| PasswordHash | string | Mật khẩu Admin Dashboard |
| Role | enum | `Owner` `ShiftLeader` `Cashier` `Barista` `PastryStaff` |
| BaseSalary | decimal | Lương cơ bản (VNĐ) |
| Status | enum | `Active` `Inactive` |

### 👤 Customers — Khách hàng đăng ký
| Column | Type | Mô tả |
|--------|------|-------|
| Phone | string | Unique, dùng để đăng nhập và tra cứu tại POS |
| GoogleId | string | Null nếu đăng ký bằng SĐT |
| TotalSpend | decimal | Tổng chi tiêu cộng dồn vĩnh viễn (tính tier) |
| LoyaltyTier | enum | `None` `Silver` (10%) `Gold` (15%) |
| CurrentPoints | int | Điểm hiện tại (reset mỗi 2 tháng) |
| PointsResetAt | datetime | Thời điểm reset gần nhất |

### 🗂️ Categories — Danh mục
| Column | Type | Mô tả |
|--------|------|-------|
| DisplayStation | enum | `Bar` `Pastry` `Both` — quyết định hiển thị trên PDS nào |
| DisplayOrder | int | Thứ tự hiển thị trên menu |

### 🥤 Products — Sản phẩm
| Column | Type | Mô tả |
|--------|------|-------|
| Status | enum | `Active` `Inactive` `OutOfStock` |
| HasSizeOption | bool | Có chọn size S/M/L không |
| HasSugarOption | bool | Có chọn mức đường không |
| HasIceOption | bool | Có chọn mức đá không |

### 📋 Orders — Đơn hàng
| Column | Type | Mô tả |
|--------|------|-------|
| OrderCode | string | Unique, format: `CF{YYMMDD}{seq}`, ví dụ: `CF2406110001` |
| Type | enum | `DineIn` `TakeAway` `Online` |
| Status | enum | `Draft` (POS nháp) `Pending` (Online chờ duyệt) `Confirmed` `Preparing` `Completed` `Closed` `Cancelled` |
| RowVersion | byte[] | EF Core Concurrency Token phục vụ Optimistic Concurrency |

### 💳 Payments — Bản ghi thanh toán
| Column | Type | Mô tả |
|--------|------|-------|
| OrderId | int FK | Liên kết đến bảng Orders |
| Method | enum | `Cash` `Transfer` `Mixed` |
| Amount | decimal | Số tiền thực tế thu |
| AmountReceived | decimal | Số tiền khách đưa (Cash) |
| AmountChange | decimal | Tiền thối lại cho khách (Cash) |
| ReferenceCode | string | Mã đối chiếu CK / Mã đơn hàng cho VietQR |
| CreatedByStaffId | int FK | CSR thực hiện giao dịch |
| PaidAt | datetime | Thời điểm thanh toán thành công |

### 🥗 ProductIngredients — Định mức nguyên liệu (Recipe/BOM)
| Column | Type | Mô tả |
|--------|------|-------|
| ProductId | int FK | Sản phẩm liên kết |
| IngredientId | int FK | Nguyên liệu liên kết |
| Quantity | decimal | Định mức tiêu hao nguyên liệu |
| Unit | string | Đơn vị đo (`g`, `ml`, `cái`) |

### 📝 AuditLogs — Nhật ký hệ thống
| Column | Type | Mô tả |
|--------|------|-------|
| StaffId | int FK | Nhân viên thực hiện (Null nếu là hệ thống) |
| Action | enum | `CREATE` `UPDATE` `DELETE` `APPROVE` `CANCEL` `REFUND` |
| EntityName | string | Bảng bị ảnh hưởng (`Orders`, `Staffs`, `Ingredients`, `Shifts`...) |
| EntityId | int | ID bản ghi bị ảnh hưởng |
| OldValue | string | JSON snapshot trước khi đổi |
| NewValue | string | JSON snapshot sau khi đổi |
| IPAddress | string | IP client |
| CreatedAt | datetime | Thời gian ghi log |

> **Quy tắc đóng đơn:** 
> - **POS (Pay-First):** Tự động đóng (`Status = Closed`, `ClosedAt = now()`) ngay khi pha chế xong (`Status = Completed`) vì tiền đã thu trước.
> - **Online (Pay-Later COD):** Khi khách đến quầy nhận đồ, CSR thu tiền và bấm Đóng đơn trên POS -> Tạo `Payment` record và chuyển đơn sang `Closed`.

### 🍹 OrderItems — Món trong đơn
| Column | Type | Mô tả |
|--------|------|-------|
| SugarLevel | enum | `0` `30` `50` `70` `Extra` |
| IceLevel | enum | `0` `50` `100` |
| IsPointRedemption | bool | Đổi điểm → UnitPrice = 0 |
| BarStatus | enum | `NA` `Pending` `InProgress` `Done` |
| PastryStatus | enum | `NA` `Pending` `InProgress` `Done` |

### 💰 Vouchers — Mã giảm giá
| Column | Type | Mô tả |
|--------|------|-------|
| DiscountType | enum | `Percent` `Fixed` |
| MaxUsageCount | int? | Null = không giới hạn |
| IsPermanent | bool | True = không có ngày hết hạn |
| ExpiresAt | datetime? | Null nếu `IsPermanent = true` |

---

## Order Status Flow

```
POS (Dine-in / Take-away):
  [Draft] ──Thanh toán thành công──▶ [Confirmed] ──▶ [Preparing] ──▶ [Completed] ──Tự động đóng──▶ [Closed✅]

Online (COD):
  [Pending] ──CSR xác nhận──▶ [Confirmed] ──▶ [Preparing] ──▶ [Completed] ──Thu tiền và Đóng đơn──▶ [Closed✅]

Từ bất kỳ trạng thái nào trước Completed:
  ──▶ [Cancelled❌] (Kích hoạt quy trình hoàn tiền nếu đơn đã thanh toán)
```

---

## Conventions

| Quy tắc | Giá trị |
|---------|----------|
| Soft Delete | Dùng `IsActive = false` thay vì DELETE vật lý |
| Giá lưu lịch sử | `UnitPrice`, `ToppingPrice` lưu giá **tại thời điểm đặt** |
| Audit Discount | Ghi nhận chi tiết vào bảng `AuditLogs` với ID nhân viên duyệt |
| Audit Logs | Ghi nhật ký cho discount, mở/đóng ca, hủy đơn, hoàn tiền, duyệt kho, sửa nhân viên |
| Point Reset | Scheduled job mỗi 2 tháng, tạo `PointTransaction` type `Reset` |
| Order Code | Reset sequence mỗi ngày (theo ngày trong code) |
| Concurrency | Optimistic Concurrency với `RowVersion` (byte[]) trên bảng `Orders` |
| InventoryCheck Status | `PendingApproval` → `Approved` — SL tạo, Owner duyệt mới cập nhật kho |
| In ấn | Không có trong phạm vi hiện tại |
| Huỷ món Online | CSR xóa `OrderItem` khi đơn ở `Pending` — Hệ thống recalculate tổng đơn |
