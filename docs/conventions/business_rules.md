# ☕ Business Rules

## CafePOS & PDS

Tài liệu này mô tả các quy tắc nghiệp vụ cốt lõi của hệ thống CafePOS & PDS. Mọi tính năng, API, database schema và giao diện phải tuân thủ các quy tắc dưới đây.

---

# 1. System Overview

Hệ thống bao gồm:

* POS (Point Of Sale)
* PDS (Preparation Display System)
* Customer Portal
* Admin Dashboard

---

## POS

Dành cho:

* Thu ngân
* Nhân viên bán hàng

Mục tiêu:

* Tạo đơn nhanh
* Thanh toán nhanh
* Hạn chế thao tác nhập liệu

---

## PDS

Dành cho:

* Nhân viên pha chế
* Khu vực bếp

Mục tiêu:

* Theo dõi đơn hàng theo thời gian thực
* Quản lý trạng thái pha chế

---

## Customer Portal

Dành cho:

* Khách hàng

Mục tiêu:

* Xem menu
* Đặt hàng
* Theo dõi đơn
* Xem điểm thưởng

---

## Admin Dashboard

Dành cho:

* Quản lý cửa hàng
* Chủ cửa hàng

Mục tiêu:

* Quản lý dữ liệu
* Quản lý tồn kho
* Theo dõi doanh thu

---

# 2. User Roles

## Administrator

Quyền:

* Quản lý toàn bộ hệ thống
* Quản lý người dùng
* Quản lý cấu hình
* Xem báo cáo

---

## Manager

Quyền:

* Quản lý sản phẩm
* Quản lý tồn kho
* Quản lý nhân viên
* Xem báo cáo

Không được:

* Thay đổi cấu hình hệ thống

---

## Cashier

Quyền:

* Tạo đơn hàng
* Thanh toán
* Xem đơn hàng

Không được:

* Quản lý tồn kho
* Truy cập báo cáo tài chính

---

## Barista

Quyền:

* Xem đơn hàng trên PDS
* Cập nhật trạng thái pha chế

Không được:

* Thanh toán
* Quản lý sản phẩm

---

## Customer

Quyền:

* Xem menu
* Tạo đơn hàng
* Theo dõi đơn hàng
* Xem điểm thưởng

Không được:

* Truy cập dữ liệu nội bộ

---

# 3. Product Rules

## Product Status

Sản phẩm có thể:

* Active
* Inactive

---

## Inactive Product

Sản phẩm Inactive:

* Không hiển thị cho khách hàng
* Không cho phép tạo đơn mới

Nhưng:

* Vẫn giữ lịch sử đơn hàng cũ

---

## Product Price

Giá sản phẩm:

* Không được âm
* Lớn hơn 0

---

# 4. Order Lifecycle

## Standard Flow

```text
Draft
→ Confirmed
→ Preparing
→ Completed
→ Closed
```

---

## Cancel Flow

```text
Draft
→ Cancelled
```

hoặc

```text
Confirmed
→ Cancelled
```

---

## Final States

Các trạng thái kết thúc:

* Completed
* Closed
* Cancelled

---

## Immutable Orders

Sau khi:

```text
Completed
```

hoặc

```text
Closed
```

Không được:

* Sửa món
* Thêm món
* Xóa món

---

# 5. Order Item Rules

Mỗi Order phải có ít nhất:

```text
1 Order Item
```

---

Không cho phép:

```text
Quantity <= 0
```

---

Order Item phải lưu:

* Product Snapshot Name
* Product Snapshot Price

Để đảm bảo lịch sử không thay đổi khi giá sản phẩm thay đổi.

---

# 6. Payment Rules

## Critical Rule

Một Order chỉ được có tối đa:

```text
1 Successful Payment
```

---

## Payment Status

* Pending
* Completed
* Failed
* Refunded

---

## Duplicate Payment Prevention

Trước khi tạo Payment:

Hệ thống phải kiểm tra:

```text
Order đã có Payment Completed chưa?
```

Nếu có:

```text
Reject
409 Conflict
```

---

## Payment Amount

Payment Amount phải bằng:

```text
Order Total
```

tại thời điểm thanh toán.

---

## Refund

Refund không được:

* Lớn hơn giá trị thanh toán

---

# 7. Inventory Rules

## Inventory Quantity

Không cho phép tồn kho âm.

Sai:

```text
Milk = -2
```

---

## Inventory Deduction

Khi Order được xác nhận:

```text
Confirmed
```

hoặc

```text
Preparing
```

(tùy cấu hình hệ thống)

phải trừ nguyên liệu.

---

## Cancelled Orders

Nếu nguyên liệu đã bị trừ:

Khi hủy đơn:

```text
Restore Inventory
```

---

## Inventory Audit

Mọi thay đổi tồn kho phải được ghi nhận lịch sử.

---

# 8. Recipe Rules

Mỗi sản phẩm có thể:

* Không có công thức
* Có công thức

---

Ví dụ:

```text
Matcha Latte

- Matcha Powder
- Milk
- Sugar
```

---

## Recipe Update

Thay đổi công thức:

Không được ảnh hưởng các đơn hàng cũ.

---

# 9. PDS Rules

## Purpose

PDS là màn hình dành cho pha chế.

---

## PDS Status Flow

```text
Queued
→ Preparing
→ Completed
```

---

## Priority Rule

Ưu tiên:

```text
First In First Out
```

(FIFO)

---

## Display Rule

PDS phải hiển thị:

* Order Number
* Waiting Time
* Items
* Notes

---

# 10. Loyalty Rules

## Points

Khách hàng có thể tích điểm.

---

## Point Balance

Không được âm.

---

## Earn Points

Điểm thưởng chỉ được cộng khi:

```text
Payment Completed
```

---

## Cancelled Orders

Không cộng điểm.

---

# 11. Promotion Rules

Khuyến mãi có:

* Start Date
* End Date

---

## Promotion Validation

Không áp dụng:

* Khuyến mãi hết hạn
* Khuyến mãi chưa bắt đầu

---

## Discount Validation

Giảm giá không được:

```text
> 100%
```

---

# 12. Soft Delete Rules

Dữ liệu nghiệp vụ không được xóa vật lý.

Áp dụng cho:

* Product
* Customer
* Employee
* Inventory Item

---

Sử dụng:

```text
IsDeleted
DeletedAt
DeletedBy
```

---

# 13. Reporting Rules

Báo cáo chỉ sử dụng:

```text
Payment Completed
```

để tính doanh thu.

---

Không tính:

* Pending
* Failed

---

## Revenue Calculation

```text
Revenue
=
Total Successful Payments
-
Refunds
```

---

# 14. Security Rules

Người dùng chỉ được truy cập dữ liệu theo Role.

---

Không được:

* Truy cập dữ liệu của Role cao hơn.
* Truy cập dữ liệu nội bộ bằng URL trực tiếp.

---

# 15. Audit Rules

Các hành động quan trọng phải được ghi log:

* Create Product
* Update Product
* Delete Product
* Create Order
* Cancel Order
* Payment Completed
* Refund
* Inventory Adjustment

---

# 16. Real-Time Rules

Các sự kiện sau phải được phát realtime:

* Order Created
* Order Confirmed
* Order Preparing
* Order Completed
* Order Cancelled

---

PDS phải cập nhật mà không cần refresh trang.

---

# 17. Data Integrity Rules

Không được phép tồn tại:

* Payment không thuộc Order
* Order Item không thuộc Order
* Inventory Transaction không thuộc Inventory Item

---

Tất cả Foreign Key phải hợp lệ.

---

# 18. AI Business Rules

Khi tạo code mới, AI phải:

1. Đọc toàn bộ business_rules.md.
2. Không thay đổi Order Lifecycle.
3. Không thay đổi Payment Rules.
4. Không thay đổi Inventory Rules.
5. Không thay đổi Role Permissions.
6. Không thay đổi Loyalty Rules.
7. Không thay đổi Promotion Rules nếu không được yêu cầu rõ ràng.

---

# 19. Definition Of Done

Trước khi hoàn thành bất kỳ tính năng nào:

* Business Rules vẫn được đảm bảo.
* Payment Idempotency vẫn hoạt động.
* Soft Delete vẫn hoạt động.
* Inventory không thể âm.
* Role Permissions không bị phá vỡ.
* Audit Logs vẫn được ghi nhận.
* Realtime Events vẫn hoạt động.
* Không có dữ liệu mồ côi (orphan data).

```