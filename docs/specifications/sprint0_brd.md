# 📄 Sprint 0 — Business Requirements Document (BRD)
### Hệ thống Quản lý Bán hàng Quán Cà phê / Trà sữa
**Phiên bản:** 1.2 | **Ngày:** 11/06/2026 | **BA phụ trách:** Antigravity BA

> ✅ **BRD đã được Stakeholder xác nhận ngày 11/06/2026**

---

## 1. Tổng quan Dự án

### 1.1 Bối cảnh kinh doanh
Cửa hàng cà phê / trà sữa hiện đang vận hành thủ công (viết tay, nhận order qua điện thoại). Các vấn đề hiện tại:
- ❌ Dễ miss order do truyền thông thủ công giữa các khâu
- ❌ Chậm trễ trong việc chuyển thông tin từ thu ngân → pha chế → đồ ăn
- ❌ Khó tổng hợp và báo cáo doanh thu, tồn kho

### 1.2 Mục tiêu dự án
Xây dựng hệ thống quản lý bán hàng toàn diện trên nền tảng **web**, bao gồm:
- **POS** (Point of Sale): Màn hình đặt hàng và thanh toán tại quầy
- **PDS** (Production Display Screen): Màn hình hiển thị đơn hàng cho quầy Bar và quầy Đồ ăn nhẹ
- **Online Ordering Website**: Trang đặt hàng trực tuyến cho khách hàng
- **Admin Dashboard**: Quản lý toàn bộ hệ thống (menu, nhân viên, báo cáo, khuyến mãi)

### 1.3 Phạm vi triển khai
- **Số chi nhánh:** 1 (single-store)
- **Mô hình phục vụ:** Dine-in + Take-away (Pick-up)
- **Nền tảng:** Web (ASP.NET Core + SQL Server)
- **Kiến trúc:** API-first (RESTful API) để hỗ trợ mở rộng mobile trong tương lai

---

## 2. Stakeholders & Người dùng hệ thống

| Role | Mô tả | Hệ thống sử dụng |
|------|--------|-----------------|
| **Owner / Manager** | Quản lý toàn bộ hệ thống | Admin Dashboard |
| **Shift Leader (SL)** | Mở/đóng ca, xác nhận discount, xuất báo cáo cuối ca | POS + Admin (giới hạn) |
| **Cashier (CSR)** | Nhận order, thanh toán tại quầy | POS |
| **Barista** | Pha chế đồ uống theo order | PDS - Bar |
| **Pastry Counter Staff** | Chuẩn bị đồ ăn nhẹ theo order | PDS - Đồ ăn nhẹ |
| **Customer** | Đặt hàng trực tuyến | Online Ordering Website |

### Ma trận phân quyền (RACI sơ bộ)

| Chức năng | Owner | SL | CSR | Barista | Pastry | Customer |
|-----------|-------|----|-----|---------|--------|----------|
| Quản lý Menu | ✅ | — | — | — | — | — |
| Quản lý nhân viên | ✅ | — | — | — | — | — |
| Quản lý khuyến mãi | ✅ | — | — | — | — | — |
| Quản lý tồn kho | ✅ | — | — | — | — | — |
| Xem báo cáo (full) | ✅ | — | — | — | — | — |
| Mở/Đóng ca | ✅ | ✅ | — | — | — | — |
| Xác nhận discount | ✅ | ✅ | — | — | — | — |
| Tạo đơn hàng POS | ✅ | ✅ | ✅ | — | — | — |
| Thanh toán | ✅ | ✅ | ✅ | — | — | — |
| Xem PDS + Done món | — | — | — | ✅ | ✅ | — |
| Báo cáo cuối ca | — | ✅ | — | — | — | — |
| Đặt hàng online | — | — | — | — | — | ✅ |

---

## 3. Đăng nhập & Xác thực

### 3.1 POS Login
- Đăng nhập bằng **mã code nhân viên** (ví dụ: `2018000520`)
- Không cần username/password đầy đủ → tối ưu tốc độ tại quầy
- Màn hình xác nhận OTP/code khi thực hiện các thao tác nhạy cảm (discount)

### 3.2 Admin Dashboard Login
- Đăng nhập bằng **username + password**
- Phân quyền theo Role (Owner / SL)

### 3.3 Customer Website Login
- Đăng ký/Đăng nhập bằng **số điện thoại + mật khẩu**
- Đăng nhập bằng **Google OAuth**

---

## 4. Quy trình nghiệp vụ (Business Process Flows)

### 4.1 Luồng đặt hàng tại POS — Dine-in

```
CSR nhập số bàn
    → Chọn món (size, đường, đá, topping)
    → Áp dụng voucher / loyalty (tuỳ chọn)
    → Xác nhận đơn hàng
    → [Đơn tự động xuất hiện trên PDS với trạng thái "Đã xác nhận"]
    → In thẻ bàn / hoá đơn tạm
    → [Song song] Barista/Pastry chuẩn bị → Done từng món → Hoàn thành đơn
    → [Song song] CSR thanh toán bất kỳ lúc nào
    → Khi cả 2 quầy Done + Đã thanh toán → Hệ thống tự đóng đơn
```

### 4.2 Luồng đặt hàng tại POS — Take-away (Pick-up)

```
CSR tạo đơn PU mới
    → Nhập SĐT khách (tự động điền thông tin nếu đã có tài khoản)
    → Chọn món
    → Áp dụng voucher / loyalty (tuỳ chọn)
    → Xác nhận đơn
    → [PDS hiển thị "Đã xác nhận"]
    → Chuẩn bị → Done → Thanh toán → Tự đóng đơn
```

### 4.3 Luồng đặt hàng Online (Customer)

```
Khách đăng nhập
    → Chọn món (size, đường, đá, topping)
    → Chọn giờ lấy: Mặc định 10 phút hoặc hẹn giờ cụ thể
    → Áp dụng voucher / điểm (tuỳ chọn)
    → Đặt hàng → Thanh toán khi nhận
    → [PDS hiển thị "Chờ xác nhận"]
    → CSR/SL bấm Xác nhận → "Đã xác nhận"
    → [Nếu hẹn giờ: Hệ thống tự kích hoạt trước 10 phút]
    → Chuẩn bị → Done → Thanh toán khi nhận → Đóng đơn
```

### 4.4 Vòng đời đơn hàng (Order Lifecycle)

```
                    ┌─────────────────────────────┐
                    │         LUỒNG CHÍNH          │
                    └─────────────────────────────┘

[Tạo đơn] ──→ [Chờ xác nhận*] ──→ [Đã xác nhận / Đang chuẩn bị]
                                            │
                          ┌─────────────────┴──────────────────┐
                          ▼                                     ▼
              [Bar: Đang làm / Done]           [Thanh toán: Chờ / Đã thanh toán]
                          │
              [Pastry: Đang làm / Done]
                          │
                    [Hoàn thành] ──── (+ Đã thanh toán) ───→ [Đóng đơn ✅]

* Chỉ áp dụng cho đơn Online. Đơn POS tự động → "Đã xác nhận"
```

### 4.5 Luồng Discount đặc biệt tại POS

```
CSR nhập % giảm giá trên hoá đơn
    → Hệ thống yêu cầu nhập Mã xác nhận của SL hoặc Manager
    → SL/Manager nhập mã code cá nhân
    → Hệ thống xác thực → Áp dụng discount
    → Lưu log: ai xác nhận, mức giảm, thời gian
```

### 4.6 Luồng PDS — Hiển thị & Xử lý

```
[Đơn hàng mới vào PDS]
    → Hiển thị trên màn hình Bar VÀ màn hình Đồ ăn nhẹ
       (Mỗi màn hình chỉ thấy sản phẩm thuộc danh mục quầy mình)
    → Staff nhấn "Bắt đầu làm" → [Đang chuẩn bị]
    → Staff nhấn "Done" từng món → Nhấn "Hoàn thành quầy"
    → Khi CẢ 2 QUẦY done → Đơn chuyển [Hoàn thành]
    → POS nhận thông báo: "Đơn hàng #XXXXX đã hoàn thành"
```

> **Cài đặt hiển thị PDS** (Manager config): Chọn hiển thị đơn trên 2 màn hình riêng biệt, hoặc 1 màn hình gộp chia 2 cột (Bar | Đồ ăn nhẹ).

---

## 5. Phạm vi tính năng hệ thống (Feature Scope)

### 5.1 Module POS

| # | Tính năng | Mô tả |
|---|-----------|-------|
| P01 | Đăng nhập POS | Nhập mã code nhân viên |
| P02 | Tạo đơn Dine-in | Nhập số bàn, chọn món |
| P03 | Tạo đơn Take-away | Nhập SĐT, tự điền thông tin khách |
| P04 | Chọn món với biến thể | Size (S/M/L), Đường, Đá, Topping |
| P05 | Gọi thêm món | Với bàn đang mở (tạo đơn con /n) |
| P06 | Áp dụng Voucher | Nhập mã, validate, áp dụng |
| P07 | Áp dụng Loyalty Discount | Tự động nhận diện tier khách |
| P08 | Discount thủ công + xác nhận | CSR nhập %, SL/Manager xác nhận bằng code |
| P09 | Thanh toán tiền mặt | CSR kiểm soát, nhập số tiền nhận |
| P10 | Thanh toán chuyển khoản | Ghi nhận thanh toán CK |
| ~~P11~~ | ~~In hoá đơn~~ | ~~Bỏ qua trong phạm vi hiện tại~~ |
| P12 | Xác nhận đơn Online | CSR/SL duyệt đơn Chờ xác nhận |
| P13 | Nhận thông báo đơn hoàn thành | Toast notification trên POS |
| P14 | Xem lịch sử đơn trong ca | Danh sách đơn đã xử lý trong ca |

### 5.2 Module PDS (Bar & Đồ ăn nhẹ)

| # | Tính năng | Mô tả |
|---|-----------|-------|
| D01 | Hiển thị danh sách đơn | Real-time, theo thứ tự thời gian |
| D02 | Hiển thị trạng thái đơn | Chờ XN / Đã XN / Đang làm / Xong |
| D03 | Bắt đầu làm | Staff bấm nhận đơn |
| D04 | Done từng món | Check từng item trong đơn |
| D05 | Hoàn thành quầy | Đánh dấu quầy đã xong toàn bộ |
| D06 | Hiển thị đơn con | Ký hiệu #XXXXX/1, #XXXXX/2... |
| D07 | Cài đặt chế độ hiển thị | Split-screen hoặc 2 màn hình riêng |

### 5.3 Module Online Ordering (Customer Website)

| # | Tính năng | Mô tả |
|---|-----------|-------|
| O01 | Đăng ký / Đăng nhập | SĐT + mật khẩu, Google OAuth |
| O02 | Xem Menu | Theo danh mục, có ảnh + mô tả + giá |
| O03 | Chọn món + biến thể | Size, đường, đá, topping |
| O04 | Giỏ hàng | Thêm/xoá/sửa số lượng |
| O05 | Chọn giờ lấy | Ngay (10 phút) hoặc hẹn giờ cụ thể |
| O06 | Áp dụng Voucher | Nhập mã giảm giá |
| O07 | Đổi điểm | Dùng điểm đổi sản phẩm miễn phí |
| O08 | Xác nhận đặt hàng | Thanh toán khi nhận |
| O09 | Theo dõi trạng thái đơn | Real-time update trạng thái |
| O10 | Lịch sử đơn hàng | Xem lại các đơn đã đặt |
| O11 | Thông tin tài khoản | Điểm, tier, lịch sử chi tiêu |

### 5.4 Module Admin Dashboard (Manager)

| # | Tính năng | Mô tả |
|---|-----------|-------|
| A01 | Quản lý Menu | CRUD danh mục, sản phẩm, biến thể, topping |
| A02 | Cấu hình Category → PDS | Gán danh mục vào quầy Bar / Đồ ăn nhẹ |
| A03 | Quản lý Nhân viên | CRUD nhân viên, gán role, mã POS, lương cơ bản |
| A04 | Quản lý Ca làm việc | Xem lịch sử ca, SL mở/đóng |
| A05 | Quản lý Voucher / Coupon | Tạo mã: %, số tiền, giới hạn, thời hạn |
| A06 | Quản lý Loyalty | Cấu hình tier, tỉ lệ tích điểm, sản phẩm đổi điểm |
| A07 | Quản lý Tồn kho | CRUD nguyên liệu, đặt mức cảnh báo, hạn sử dụng |
| A08 | Nhập kiểm kho (SL) | SL nhập số lượng thực tế → Ghi nhận chênh lệch, KHÔNG tự overwrite |
| A08b | Duyệt điều chỉnh tồn kho (Owner) | Owner xem chênh lệch → Xác nhận → Hệ thống cập nhật về số thực tế |
| A09 | Cấu hình hệ thống | Cài đặt PDS, thông báo |
| A10 | Báo cáo Doanh thu | Theo ngày/tuần/tháng, theo khung giờ |
| A11 | Báo cáo Sản phẩm | Top 5 bán chạy, Top 3 bán ít |
| A12 | Báo cáo Voucher | Thống kê sử dụng voucher |
| A13 | Báo cáo Loyalty | Điểm khách hàng, tier distribution |
| A14 | Báo cáo Tồn kho | Sắp hết, sắp hết hạn, lệch thực tế |
| A15 | Quản lý Người dùng hệ thống | Tạo/khoá tài khoản nhân viên |

### 5.5 Module Shift Management (SL)

| # | Tính năng | Mô tả |
|---|-----------|-------|
| S01 | Mở ca | Nhập số tiền đầu ca (float cash) |
| S02 | Đóng ca | Nhập tiền mặt thực tế + CK, đối soát với hệ thống |
| S03 | Báo cáo doanh thu tức thời | Xem doanh thu tại thời điểm bất kỳ trong ca |
| S04 | Xuất báo cáo cuối ca | PDF/print báo cáo tổng kết ca |
| S05 | Nhập tồn kho | Kiểm kho thủ công và nhập vào hệ thống |

---

## 6. Yêu cầu nghiệp vụ chi tiết

### 6.1 Sản phẩm & Biến thể

```
Sản phẩm
├── Tên, Mô tả, Ảnh, Giá cơ bản
├── Danh mục (Category) → gán vào PDS quầy nào
├── Trạng thái: Đang bán / Tạm ngưng / Hết hàng
└── Biến thể:
    ├── Size: S / M / L (có thể cấu hình chênh lệch giá)
    ├── Đường: 0% / 30% / 50% / 70% / Thêm đường
    ├── Đá: 0% / 50% / 100%
    └── Topping: Quản lý tạo tự do (tên + giá)
```

### 6.2 Loyalty & Điểm thưởng

```
Tích điểm:
- 10.000đ chi tiêu = 1 điểm
- Reset điểm mỗi 2 tháng

Loyalty Tier (dựa trên tổng chi tiêu cộng dồn vĩnh viễn):
- Dưới 1.000.000đ  → Không giảm
- Từ 1.000.000đ    → Giảm 10% (áp dụng từ đơn TIẾP THEO)
- Từ 1.500.000đ    → Giảm 15% (áp dụng từ đơn TIẾP THEO)

Đổi điểm:
- Sản phẩm đổi điểm do Manager tạo (tên, số điểm cần, sản phẩm tương ứng)
- Khi đổi: Xuất hiện trên đơn với giá 0đ (để tính cost)
- Điểm bị trừ ngay khi đổi
```

### 6.3 Voucher / Coupon

```
Loại giảm giá: % hoặc số tiền cố định (do Manager chọn khi tạo)
Giới hạn sử dụng: Mặc định = 1 lần, có thể cài Không giới hạn
Thời hạn: Mặc định = 1 tháng, có thể cài Vĩnh viễn
Mỗi đơn: Chỉ dùng 1 mã, có thể dùng kết hợp với loyalty discount
```

### 6.4 Ca làm việc (Shift)

```
- Mỗi ngày: 1 phiên làm việc (mở đầu ngày, đóng cuối ngày)
- SL A có thể mở ca, SL B có thể đóng ca
- Mở ca: Ghi nhận SL, thời gian, số tiền đầu ca
- Đóng ca: Nhập tiền mặt thực tế + CK thực tế
           Hệ thống đối chiếu với doanh thu hệ thống
           Hiển thị chênh lệch (thừa/thiếu)
```

### 6.5 Quản lý Nhân viên

```
Thông tin lưu trữ:
- Họ tên, SĐT, email, địa chỉ
- Role (Owner / SL / CSR / Barista / Pastry Staff)
- Mã đăng nhập POS
- Lương cơ bản
- Lịch sử ca làm việc
- Trạng thái: Đang làm / Nghỉ việc
```

### 6.6 Quản lý Tồn kho

```
Nguyên liệu thô:
- Tên, Đơn vị tính, Số lượng hiện tại
- Mức cảnh báo sắp hết (do Manager đặt khi thêm hàng)
- Hạn sử dụng

Quy trình kiểm kho 2 bước:
  Bước 1 — SL nhập kiểm kho:
    - Nhập số lượng thực tế từng nguyên liệu
    - Hệ thống tính chênh lệch (Thực tế - Hệ thống)
    - KHÔNG tự động overwrite số liệu hệ thống
    - Trạng thái: "Chờ duyệt"

  Bước 2 — Manager duyệt điều chỉnh:
    - Xem báo cáo chênh lệch
    - Xác nhận → Hệ thống cập nhật CurrentQuantity về số thực tế
    - Ghi nhận IngredientTransaction type = Adjustment

Báo cáo tồn kho:
- Hàng sắp hết: Số lượng ≤ mức cảnh báo
- Hàng sắp hết hạn: Trong vòng N ngày (Manager cấu hình)
- Chênh lệch thực tế: So sánh nhập SL vs. hệ thống tính toán
```

### 6.7 Xử lý vấn đề đơn hàng Online

```
CSR/SL KHÔNG thể từ chối đơn online.
Nếu có vấn đề (hết nguyên liệu, không thể làm món):

Quy trình xử lý:
  1. CSR/SL liên hệ khách bằng điện thoại hoặc tin nhắn (ngoài hệ thống)
  2. Dựa theo thoả thuận với khách:
     a. Khách đồng ý đổi món:
        → CSR/SL xoá món cũ, thêm món mới trên POS (trước khi confirm)
        → Xác nhận đơn bình thường
     b. Khách muốn huỷ một món:
        → CSR/SL xoá món đó khỏi đơn
        → Hệ thống recalculate tổng tiền, loyalty, voucher
     c. Khách muốn huỷ cả đơn:
        → CSR/SL bấm "Huỷ đơn" trên POS
        → Đơn chuyển trạng thái Cancelled
        → Hoàn điểm nếu đã dùng điểm

Lưu ý: Hệ thống không tích hợp notification tự động cho khách.
        Toàn bộ liên lạc là thủ công (gọi điện / nhắn tin).
```

---

## 7. Kiến trúc hệ thống (High-level Architecture)

### 7.1 Tổng quan kiến trúc

```
┌─────────────────────────────────────────────────────────────┐
│                        CLIENT LAYER                          │
├──────────────┬──────────────┬──────────────┬────────────────┤
│   POS Web    │   PDS Web    │ Customer Web │  Admin Web     │
│  (Browser)   │  (Browser)   │  (Browser)   │  (Browser)     │
└──────┬───────┴──────┬───────┴──────┬───────┴───────┬────────┘
       │              │              │               │
       └──────────────┴──────────────┴───────────────┘
                              │ HTTPS/WebSocket
┌─────────────────────────────▼───────────────────────────────┐
│                    API GATEWAY LAYER                         │
│              ASP.NET Core Web API (RESTful)                  │
│         + SignalR (Real-time: PDS, Notifications)           │
├─────────────────────────────────────────────────────────────┤
│                    APPLICATION LAYER                         │
│  ┌──────────┐ ┌──────────┐ ┌──────────┐ ┌──────────────┐  │
│  │  Order   │ │  Menu    │ │  Auth    │ │   Report     │  │
│  │ Service  │ │ Service  │ │ Service  │ │   Service    │  │
│  └──────────┘ └──────────┘ └──────────┘ └──────────────┘  │
│  ┌──────────┐ ┌──────────┐ ┌──────────┐ ┌──────────────┐  │
│  │Loyalty & │ │Inventory │ │  Shift   │ │Notification  │  │
│  │ Voucher  │ │ Service  │ │ Service  │ │  Service     │  │
│  └──────────┘ └──────────┘ └──────────┘ └──────────────┘  │
├─────────────────────────────────────────────────────────────┤
│                    DATA ACCESS LAYER                         │
│              Entity Framework Core (Code-First)              │
├─────────────────────────────────────────────────────────────┤
│                     DATABASE LAYER                           │
│                    SQL Server 2022                            │
└─────────────────────────────────────────────────────────────┘
```

### 7.2 Technology Stack

| Layer | Technology |
|-------|-----------|
| **Backend API** | ASP.NET Core 8 (Web API) |
| **Real-time** | SignalR (WebSocket) — PDS updates, notifications |
| **ORM** | Entity Framework Core 8 (Code-First) |
| **Database** | SQL Server 2022 |
| **Auth** | ASP.NET Identity + JWT Bearer Token |
| **Google OAuth** | Google Identity / OAuth 2.0 |
| **Frontend (All)** | Razor Pages (ASP.NET MPA) — POS, PDS, Admin, Customer |
| **Caching** | In-memory Cache |
| **API Docs** | Swagger / OpenAPI |
| ~~**In ấn**~~ | ~~Bỏ qua trong phạm vi hiện tại~~ |

### 7.3 Database Schema — Các Entity chính

```
Users (Nhân viên & Khách hàng)
├── Staff: Id, Name, Phone, Role, PosCode, BaseSalary, Status
└── Customer: Id, Name, Phone, Email, GoogleId, TotalSpend, LoyaltyTier, Points

Products & Menu
├── Category: Id, Name, DisplayStation (Bar/Pastry/Both), DisplayOrder
├── Product: Id, CategoryId, Name, Description, ImageUrl, BasePrice, Status
├── ProductVariant: Id, ProductId, Size, PriceModifier
├── SugarLevel: Id, Label (0%, 30%...) 
├── IceLevel: Id, Label (0%, 50%...)
└── Topping: Id, Name, Price, IsActive

Orders
├── Order: Id, Type(DineIn/TakeAway/Online), TableNumber, CustomerId, StaffId, 
│          Status, PaymentStatus, PaymentMethod, DiscountAmount, TotalAmount,
│          ScheduledPickupTime, CreatedAt, ClosedAt
├── OrderItem: Id, OrderId, ProductId, Quantity, UnitPrice, Notes,
│              BarStatus, PastryStatus (Done/Pending/NA)
├── OrderItemOption: Id, OrderItemId, OptionType, OptionValue
└── OrderDiscount: Id, OrderId, Type(Loyalty/Voucher/Manual), Value, ApprovedBy

Loyalty & Voucher
├── Voucher: Id, Code, DiscountType(%/Fixed), Value, MaxUse, UsedCount, 
│            ExpiresAt, IsUnlimited, IsPermanent
├── VoucherUsage: Id, VoucherId, OrderId, CustomerId, UsedAt
├── PointProduct: Id, Name, PointCost, LinkedProductId
└── PointTransaction: Id, CustomerId, Points, Type(Earn/Redeem/Reset), OrderId, CreatedAt

Shifts & Reports  
├── Shift: Id, OpenedBy, ClosedBy, OpenedAt, ClosedAt, OpeningCash,
│          ActualCash, ActualTransfer, Notes
└── InventoryCheck: Id, ShiftId, StaffId, CheckedAt, Items[]

Inventory
├── Ingredient: Id, Name, Unit, CurrentQty, MinAlertQty, ExpiresAt
└── IngredientTransaction: Id, IngredientId, Type(In/Out/Adjust), Qty, CreatedAt
```

---

## 8. Yêu cầu phi chức năng (Non-Functional Requirements)

| # | Loại | Yêu cầu |
|---|------|---------|
| NF01 | **Hiệu năng** | POS response < 500ms; PDS real-time update < 1 giây |
| NF02 | **Độ tin cậy** | Hệ thống hoạt động ổn định trong giờ cao điểm (rush hour) |
| NF03 | **Bảo mật** | JWT authentication, mã POS được hash, log audit trail cho discount |
| NF04 | **Khả dụng** | Uptime ≥ 99% trong giờ mở cửa |
| NF05 | **Responsive** | POS/PDS tối ưu cho tablet (10"); Customer web mobile-first |
| NF06 | **Mở rộng** | API-first design, chuẩn bị sẵn cho mobile app tương lai |
| ~~NF07~~ | ~~**In ấn**~~ | ~~Bỏ qua trong phạm vi hiện tại~~ |
| NF08 | **Audit Log** | Ghi log mọi thao tác discount, mở/đóng ca, xóa/sửa đơn, duyệt tồn kho |

---

## 9. Product Backlog sơ bộ (theo Epic)

### Epic 1 — Foundation & Auth
- [ ] Thiết lập project ASP.NET Core, EF Core, SQL Server
- [ ] JWT Authentication, Role-based Authorization
- [ ] Google OAuth integration
- [ ] Seed data: Roles, Admin account
- [ ] Swagger/OpenAPI setup

### Epic 2 — Menu Management
- [ ] CRUD Category (gán quầy PDS)
- [ ] CRUD Product + upload ảnh
- [ ] CRUD Biến thể (Size, chênh lệch giá)
- [ ] CRUD Sugar/Ice levels
- [ ] CRUD Topping

### Epic 3 — POS Ordering
- [ ] Đăng nhập POS bằng mã code
- [ ] Tạo đơn Dine-in (nhập số bàn)
- [ ] Tạo đơn Take-away (tra cứu khách)
- [ ] Chọn món + biến thể
- [ ] Gọi thêm món vào bàn đang mở
- [ ] Xem danh sách đơn trong ca

### Epic 4 — Payment & Discount
- [ ] Thanh toán tiền mặt
- [ ] Thanh toán chuyển khoản
- [ ] Áp dụng voucher
- [ ] Áp dụng loyalty discount tự động
- [ ] Discount thủ công + xác nhận SL/Manager
- [x] ~~In hoá đơn~~ (Bỏ qua)

### Epic 5 — PDS (Production Display)
- [ ] SignalR hub cho real-time
- [ ] Màn hình Bar: hiển thị đơn theo quầy
- [ ] Màn hình Pastry: hiển thị đơn theo quầy
- [ ] Done từng món, hoàn thành quầy
- [ ] Hiển thị đơn con (#XXXXX/n)
- [ ] Cài đặt chế độ hiển thị
- [ ] Notification khi đơn hoàn thành

### Epic 6 — Online Ordering
- [ ] Customer registration/login
- [ ] Xem menu online
- [ ] Giỏ hàng + chọn biến thể
- [ ] Chọn giờ lấy
- [ ] Áp dụng voucher / đổi điểm
- [ ] Đặt hàng (COD)
- [ ] Xem trạng thái đơn real-time
- [ ] CSR/SL xác nhận đơn online
- [ ] Auto-trigger đơn hẹn giờ (trước 10 phút)

### Epic 7 — Loyalty & Voucher
- [ ] Hệ thống tích điểm (10k = 1 điểm)
- [ ] Reset điểm mỗi 2 tháng (scheduled job)
- [ ] Loyalty tier tự động upgrade
- [ ] Áp dụng tier discount
- [ ] CRUD sản phẩm đổi điểm
- [ ] Đổi điểm lấy sản phẩm (giá 0đ trên đơn)
- [ ] CRUD Voucher (%, fixed, giới hạn, thời hạn)

### Epic 8 — Shift Management
- [ ] Mở ca (SL, float cash)
- [ ] Đóng ca (đối soát tiền mặt + CK)
- [ ] Báo cáo doanh thu tức thời trong ca
- [ ] Xuất báo cáo cuối ca

### Epic 9 — Inventory
- [ ] CRUD Nguyên liệu (mức cảnh báo, hạn dùng)
- [ ] SL nhập kiểm kho → Ghi nhận chênh lệch (Chờ duyệt)
- [ ] Manager duyệt điều chỉnh → Overwrite CurrentQuantity
- [ ] Báo cáo: sắp hết, sắp hết hạn, chênh lệch thực tế

### Epic 10 — Reporting (Manager)
- [ ] Doanh thu theo ngày/tuần/tháng
- [ ] Doanh thu theo khung giờ
- [ ] Top 5 bán chạy, Top 3 bán ít
- [ ] Báo cáo voucher
- [ ] Báo cáo loyalty/điểm khách
- [ ] Báo cáo tồn kho

### Epic 11 — Admin & Settings
- [ ] Quản lý nhân viên (CRUD, lương cơ bản, ca làm)
- [ ] Quản lý người dùng hệ thống
- [ ] Cài đặt hệ thống (PDS mode, thông báo)

---

## 10. Open Questions / TBD

> ✅ **Tất cả Open Questions đã được giải quyết — Sprint 0 HOÀN THÀNH**

| # | Câu hỏi | Quyết định | Trạng thái |
|---|---------|-----------|------------|
| Q01 | ~~Frontend framework~~ | ✅ Razor Pages (ASP.NET MPA) | Đóng |
| Q02 | ~~Tồn kho: overwrite hay chỉ ghi nhận?~~ | ✅ SL chỉ ghi nhận chênh lệch. Manager duyệt mới overwrite | Đóng |
| Q03 | Auto-trừ tồn kho khi bán | ⏩ Defer sang giai đoạn sau (không trong scope hiện tại) | Defer |
| Q04 | ~~Đơn online bị từ chối?~~ | ✅ Không từ chối. CSR liên hệ thủ công → Sửa/huỷ món theo yêu cầu khách | Đóng |
| Q05 | ~~Môi trường deploy~~ | ✅ Local trước, deploy tính sau | Đóng |
| Q06 | ~~In hoá đơn~~ | ✅ Bỏ qua hoàn toàn trong phạm vi hiện tại | Đóng |
| Q07 | Ngôn ngữ giao diện | ✅ Tiếng Việt hoàn toàn (theo mặc định) | Đóng |

---

## 11. Định nghĩa "Xong" (Definition of Done)

### Cho mỗi User Story:
- [ ] Code đã được review
- [ ] Unit test đã viết và pass
- [ ] API đã có Swagger documentation
- [ ] Không có lỗi critical/blocker
- [ ] Stakeholder đã demo và chấp nhận

### Cho mỗi Sprint:
- [ ] Tất cả story trong sprint đã Done
- [ ] Regression test pass
- [ ] Demo cho Stakeholder
- [ ] Release notes cập nhật

---

## 12. Gợi ý phân chia Sprint

| Sprint | Nội dung chính | Kết quả có thể demo |
|--------|---------------|---------------------|
| **Sprint 1** | Foundation + Auth + Menu Management | Login POS/Admin, CRUD menu |
| **Sprint 2** | POS Ordering (Dine-in + Take-away) + PDS cơ bản | Đặt món → PDS hiển thị |
| **Sprint 3** | Payment + Discount | Thanh toán đầy đủ (tiền mặt + CK) |
| **Sprint 4** | Online Ordering + Customer Auth | Web đặt hàng, hẹn giờ |
| **Sprint 5** | Loyalty + Voucher | Tích điểm, đổi điểm, mã giảm giá |
| **Sprint 6** | Shift Management + Inventory (2 bước) | Mở/đóng ca, kiểm kho, duyệt tồn kho |
| **Sprint 7** | Reporting Dashboard | Toàn bộ báo cáo Manager + SL |
| **Sprint 8** | Polish, Performance, UAT | System hoàn chỉnh |

---

*Tài liệu này được tạo bởi BA dựa trên Stakeholder Interview — Sprint 0*  
*v1.2 — Cập nhật: Đóng tất cả Open Questions, loại bỏ in hoá đơn, cập nhật quy trình tồn kho 2 bước và xử lý đơn online*
