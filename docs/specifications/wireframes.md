# 🎨 Wireframes & Tổng kết
### Hệ thống Quản lý Bán hàng Quán Cà phê / Trà sữa
**Phiên bản:** 1.2 | **Ngày:** 22/06/2026

---

## Tổng quan các màn hình hệ thống

| # | Màn hình | Người dùng | Thiết bị |
|---|----------|-----------|---------|
| 1 | **POS** (Point of Sale) | CSR / SL | Tablet / Desktop |
| 2 | **PDS** (Production Display) | Barista / Pastry Staff | Monitor lớn |
| 3 | **Customer Website** | Khách hàng | Mobile / Tablet |
| 4 | **Admin Dashboard** | Owner / Manager | Desktop |

---

## 1. 🖥️ Màn hình POS

*(Hình ảnh Mockup Wireframe đang được cập nhật)*

### Mô tả giao diện
- **Layout:** 2 cột — Trái: Menu sản phẩm | Phải: Đơn hàng hiện tại
- **Top bar:** Tên nhân viên, loại đơn (Tại Chỗ / Mang Về), đồng hồ
- **Panel trái:** Tabs danh mục → Grid sản phẩm (ảnh + tên + giá)
- **Panel phải:** Danh sách món đã chọn + tùy chọn (size/đường/đá) + số lượng
- **Footer:** Tổng tiền + Loyalty badge + Voucher input + Nút thanh toán

### Luồng chính trên POS
```
1. Chọn loại đơn (Tại Chỗ / Mang Về)
2. Nhập SĐT khách (tùy chọn) để tích điểm/kiểm tra loyalty tier
3. Chọn sản phẩm từ menu → Cấu hình biến thể
4. Áp dụng voucher / loyalty (tự động nhận diện)
5. Nhấn "Thanh toán" -> Thu tiền khách -> Nhấn "Xác nhận Thu tiền"
6. Hệ thống tạo Payment record, đơn chuyển Confirmed và tự động đẩy qua PDS
```

---

## 2. 📺 Màn hình PDS (Bar Station)

*(Hình ảnh Mockup Wireframe đang được cập nhật)*

### Mô tả giao diện
- **Layout:** Grid các card đơn hàng, real-time cập nhật qua SignalR
- **Card đơn hàng:** Mã đơn, loại đơn, timer, danh sách món (chỉ món thuộc quầy mình)
- **Color coding:**
  - 🟡 Vàng = Đang chờ (Pending)
  - 🔵 Xanh dương = Đang làm (In Progress)
  - 🟢 Xanh lá = Hoàn thành (Done)
- **Actions:** Check từng món → Nút "Hoàn thành quầy"
- **Notification:** Toast "Đơn #XXXXX đã hoàn thành"

### Cài đặt hiển thị PDS (Manager config)
- **Chế độ 2 màn hình riêng:** Bar screen chỉ thấy đồ uống, Pastry screen chỉ thấy đồ ăn
- **Chế độ 1 màn hình gộp:** Split view — Trái: Bar | Phải: Pastry

---

## 3. 📱 Website Đặt Hàng Online (Khách hàng)

*(Hình ảnh Mockup Wireframe đang được cập nhật)*

### Mô tả giao diện
- **Design:** Mobile-first, cream & coffee brown palette với gold accents
- **Header:** Logo + điểm thưởng + tier badge (Silver/Gold)
- **Hero:** Banner với CTA "Đặt ngay, nhận trong 10 phút"
- **Menu:** Filter pills theo danh mục + Product cards 2 cột
- **Modal chọn món:** Size / Đường / Đá / Topping / Số lượng
- **Checkout:** Chọn giờ lấy + Voucher + Loyalty discount + Xác nhận
- **Thanh toán:** Mặc định COD (khách đến quầy nhận đồ và thanh toán trước khi lấy đồ)

### Luồng khách đặt online
```
1. Đăng nhập (SĐT + MK) hoặc Google
2. Duyệt menu, thêm vào giỏ hàng
3. Chọn giờ lấy (ASAP 10p hoặc hẹn giờ)
4. Nhập voucher / dùng điểm (tùy chọn)
5. Xác nhận đặt hàng (thanh toán COD khi nhận đồ tại quầy)
6. Xem trạng thái đơn real-time
```

---

## 4. 📊 Admin Dashboard

*(Hình ảnh Mockup Wireframe đang được cập nhật)*

### Mô tả giao diện
- **Layout:** Sidebar điều hướng + Main content area
- **Sidebar:** Dashboard, Menu, Đơn Hàng, Thanh Toán (Payments), Nhân Viên, Khuyến Mãi, Tồn Kho (Recipe/BOM), Nhật ký (Audit Logs), Báo Cáo, Cài Đặt
- **Dashboard:** KPI cards + Revenue chart theo giờ + Top products + Recent orders table
- **Báo cáo:** Filter theo ngày/tuần/tháng + Export + Tra cứu lịch sử thanh toán / audit logs
- **Recipe:** Cấu hình định mức nguyên liệu (BOM) cho từng sản phẩm.

---

## 📋 Checklist Tài liệu thiết kế — Hoàn thành

| Deliverable | Trạng thái | Link |
|-------------|-----------|------|
| ✅ Business Requirements Document (BRD) | Đã xác nhận v1.3 | [brd.md](./brd.md) |
| ✅ Entity Relationship Diagram (ERD) | Hoàn thành v1.2 | [erd.md](./erd.md) |
| ✅ API Contract | Hoàn thành v1.2 | [api_contract.md](./api_contract.md) |
| ✅ Wireframes (4 màn hình) | Hoàn thành v1.2 | Tài liệu này |
| ✅ Acceptance Criteria | Hoàn thành (Xem SRS v1.2) | [CafePOS_SRS.md](./CafePOS_SRS.md) |

---

## ❓ Open Questions (Đã giải quyết)

> [!NOTE]
> **Q02 — Tồn kho thực tế:** Khi SL nhập kiểm kho, hệ thống ghi nhận chênh lệch. Manager duyệt điều chỉnh mới overwrite tồn kho thực tế và ghi giao dịch Adjustment. (Đã quyết định và đóng)

> [!NOTE]
> **Q04 — Đơn online bị từ chối:** Không tích hợp tự động thông báo app/SMS. CSR liên hệ thủ công qua điện thoại/tin nhắn ngoài hệ thống nếu cần đổi/huỷ món. (Đã quyết định và đóng)

> [!NOTE]
> **Q06 — Kết nối máy in:** Việc in hóa đơn nhiệt đã được loại bỏ hoàn toàn khỏi phạm vi (Out of scope) của phiên bản v1.x này. (Đã quyết định và đóng)

---

## 🚀 Bước tiếp theo: Kế hoạch phát triển

Sau khi giai đoạn thiết kế hoàn tất, team bắt đầu phát triển hệ thống với cấu trúc **Pay-First** mới:

| # | Hạng mục | Ưu tiên |
|---|---------|---------|
| 1 | Setup project ASP.NET Core 8 + EF Core + SQL Server | 🔴 Must |
| 2 | Database migrations (Code-First) bao gồm Payments, ProductIngredients, AuditLogs | 🔴 Must |
| 3 | JWT Authentication (Staff + Customer) & Hash POS Code | 🔴 Must |
| 4 | POS Quick Login (mã code nhân viên) | 🔴 Must |
| 5 | CRUD Categories + Products + Toppings (Admin) | 🔴 Must |
| 6 | Swagger/OpenAPI documentation | 🟡 Should |
| 7 | Google OAuth integration cho Customer Web | 🟡 Should |
| 8 | Seed data (roles, admin account, sample menu) | 🟡 Should |

---

*Thiết kế hệ thống hoàn thành — Sẵn sàng phát triển* 🎉
