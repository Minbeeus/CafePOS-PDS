# 🎨 Sprint 0 — Wireframes & Tổng kết
### Hệ thống Quản lý Bán hàng Quán Cà phê / Trà sữa
**Ngày:** 11/06/2026

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

![POS Screen Wireframe](C:\Users\Con Moè xanh lá\.gemini\antigravity\brain\aec557cc-29c0-467f-bb3c-81b4d6f645da\wireframe_pos_1781130626718.png)

### Mô tả giao diện
- **Layout:** 2 cột — Trái: Menu sản phẩm | Phải: Đơn hàng hiện tại
- **Top bar:** Tên nhân viên, loại đơn (Tại Chỗ/Mang Về), số bàn, đồng hồ
- **Panel trái:** Tabs danh mục → Grid sản phẩm (ảnh + tên + giá)
- **Panel phải:** Danh sách món đã chọn + tùy chọn (size/đường/đá) + số lượng
- **Footer:** Tổng tiền + Loyalty badge + Voucher input + Nút thanh toán

### Luồng chính trên POS
```
1. Chọn loại đơn (Tại Chỗ / Mang Về)
2. Nhập số bàn (Dine-in) hoặc SĐT khách (Take-away)
3. Chọn sản phẩm từ menu → Cấu hình biến thể
4. Áp dụng voucher / loyalty (tự động nhận diện)
5. Thanh toán (Tiền mặt / Chuyển khoản)
6. In hoá đơn
```

---

## 2. 📺 Màn hình PDS (Bar Station)

![PDS Screen Wireframe](C:\Users\Con Moè xanh lá\.gemini\antigravity\brain\aec557cc-29c0-467f-bb3c-81b4d6f645da\wireframe_pds_1781130637078.png)

### Mô tả giao diện
- **Layout:** Grid các card đơn hàng, real-time cập nhật qua SignalR
- **Card đơn hàng:** Mã đơn, loại/bàn, timer, danh sách món (chỉ món thuộc quầy mình)
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

![Customer Website Wireframe](C:\Users\Con Moè xanh lá\.gemini\antigravity\brain\aec557cc-29c0-467f-bb3c-81b4d6f645da\wireframe_customer_web_1781130663783.png)

### Mô tả giao diện
- **Design:** Mobile-first, cream & coffee brown palette với gold accents
- **Header:** Logo + điểm thưởng + tier badge (Silver/Gold)
- **Hero:** Banner với CTA "Đặt ngay, nhận trong 10 phút"
- **Menu:** Filter pills theo danh mục + Product cards 2 cột
- **Modal chọn món:** Size / Đường / Đá / Topping / Số lượng
- **Checkout:** Chọn giờ lấy + Voucher + Loyalty discount + Xác nhận

### Luồng khách đặt online
```
1. Đăng nhập (SĐT + MK) hoặc Google
2. Duyệt menu, thêm vào giỏ hàng
3. Chọn giờ lấy (ASAP 10p hoặc hẹn giờ)
4. Nhập voucher / dùng điểm (tùy chọn)
5. Xác nhận đặt hàng (COD khi nhận)
6. Xem trạng thái đơn real-time
```

---

## 4. 📊 Admin Dashboard

![Admin Dashboard Wireframe](C:\Users\Con Moè xanh lá\.gemini\antigravity\brain\aec557cc-29c0-467f-bb3c-81b4d6f645da\wireframe_admin_1781130674424.png)

### Mô tả giao diện
- **Layout:** Sidebar điều hướng + Main content area
- **Sidebar:** Dashboard, Menu, Đơn Hàng, Nhân Viên, Khuyến Mãi, Tồn Kho, Báo Cáo, Cài Đặt
- **Dashboard:** KPI cards + Revenue chart theo giờ + Top products + Recent orders table
- **Báo cáo:** Filter theo ngày/tuần/tháng + Export

---

## 📋 Checklist Sprint 0 — Hoàn thành

| Deliverable | Trạng thái | Link |
|-------------|-----------|------|
| ✅ Business Requirements Document (BRD) | Đã xác nhận v1.1 | [sprint0_brd.md](C:\Users\Con Moè xanh lá\.gemini\antigravity\brain\aec557cc-29c0-467f-bb3c-81b4d6f645da\sprint0_brd.md) |
| ✅ Entity Relationship Diagram (ERD) | Hoàn thành | [sprint0_erd.md](C:\Users\Con Moè xanh lá\.gemini\antigravity\brain\aec557cc-29c0-467f-bb3c-81b4d6f645da\sprint0_erd.md) |
| ✅ API Contract | Hoàn thành | [sprint0_api_contract.md](C:\Users\Con Moè xanh lá\.gemini\antigravity\brain\aec557cc-29c0-467f-bb3c-81b4d6f645da\sprint0_api_contract.md) |
| ✅ Wireframes (4 màn hình) | Hoàn thành | Tài liệu này |
| ⏳ Acceptance Criteria | Chưa thực hiện | — |

---

## ❓ Open Questions còn lại (cần quyết định trước Sprint 1)

> [!IMPORTANT]
> **Q02 — Tồn kho thực tế:** Khi SL nhập kiểm kho, hệ thống có **tự động điều chỉnh** số lượng tồn kho trong hệ thống (overwrite) không, hay chỉ **ghi nhận chênh lệch** để báo cáo mà không thay đổi số liệu hệ thống?

> [!IMPORTANT]
> **Q04 — Đơn online bị từ chối:** Nếu CSR không xác nhận đơn online (ví dụ: hết nguyên liệu), hệ thống có **tự động thông báo** cho khách qua app/SMS không? Hay chỉ để đơn ở trạng thái chờ?

> [!NOTE]
> **Q06 — Kết nối máy in:** Printer kết nối qua USB, LAN (Network), hay Cloud print? Cần biết để thiết kế giải pháp in phù hợp.

---

## 🚀 Bước tiếp theo: Kế hoạch Sprint 1

Sau khi Sprint 0 hoàn tất, team sẽ bắt đầu **Sprint 1** với các mục tiêu:

| # | Hạng mục | Ưu tiên |
|---|---------|---------|
| 1 | Setup project ASP.NET Core 8 + EF Core + SQL Server | 🔴 Must |
| 2 | Database migrations (Code-First) từ ERD | 🔴 Must |
| 3 | JWT Authentication (Staff + Customer) | 🔴 Must |
| 4 | POS Quick Login (mã code) | 🔴 Must |
| 5 | CRUD Categories + Products (Admin) | 🔴 Must |
| 6 | Swagger/OpenAPI documentation | 🟡 Should |
| 7 | Google OAuth integration | 🟡 Should |
| 8 | Seed data (roles, admin account, sample menu) | 🟡 Should |

---

*Sprint 0 hoàn thành — Sẵn sàng bắt đầu phát triển* 🎉
