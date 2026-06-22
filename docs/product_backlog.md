# Product Backlog — CafePOS & PDS Project

Tài liệu này xác định lộ trình phát triển chi tiết cho dự án **CafePOS & PDS** trong thời gian **2 tháng (8 tuần)** của các Sprint thực thi code, với **Sprint 0** là tuần thiết lập nền tảng và viết tài liệu (đã hoàn thành). Mỗi tuần tương ứng với **1 Sprint**. Mỗi Sprint được chia nhỏ thành các nhiệm vụ cụ thể, đi kèm mô tả yêu cầu, hướng dẫn thực hiện kỹ thuật, điều kiện hoàn thành (DoD) và các liên kết tham chiếu tài liệu liên quan.

---

## Bảng tổng hợp các Sprint (Roadmap)

| Sprint | Thời gian | Trọng tâm công việc | Trạng thái | Epics liên quan |
|---|---|---|---|---|
| **Sprint 0** | Trước tuần 1 | Thiết lập nền tảng, Viết tài liệu & Khởi tạo cấu trúc Clean Architecture | **Hoàn thành** | Epic 1, Epic 11 |
| **Sprint 1** | Tuần 1 | Cấu hình EF Core DbContext, Migrations & Menu Management | Chưa bắt đầu | Epic 1, Epic 2 |
| **Sprint 2** | Tuần 2 | Giao diện POS Core, Xử lý Gọi món & Concurrency | Chưa bắt đầu | Epic 3, Epic 11 |
| **Sprint 3** | Tuần 3 | Quy chuẩn API & Luồng Thanh toán POS (Cash & VietQR động) | Chưa bắt đầu | Epic 4 |
| **Sprint 4** | Tuần 4 | Real-time Hub & Màn hình hiển thị pha chế PDS | Chưa bắt đầu | Epic 5 |
| **Sprint 5** | Tuần 5 | Quản lý Ca làm việc, Chiết khấu duyệt thủ công & Hoàn tiền | Chưa bắt đầu | Epic 8, Epic 4 |
| **Sprint 6** | Tuần 6 | Web đặt hàng cho Khách hàng (COD) & Tự động hẹn giờ lấy | Chưa bắt đầu | Epic 6 |
| **Sprint 7** | Tuần 7 | Chương trình Khách hàng thân thiết, Voucher & Công thức món (Recipe/BOM) | Chưa bắt đầu | Epic 7, Epic 9 |
| **Sprint 8** | Tuần 8 | Nhập kiểm kho hai bước, Báo cáo Doanh thu & Tra cứu Nhật ký hệ thống | Chưa bắt đầu | Epic 9, Epic 10 |

---

## Chi tiết các Sprint

### Sprint 0: Thiết lập nền tảng, Viết tài liệu & Khởi tạo Clean Architecture (Đã hoàn thành)

- **Mô tả chung**: Sprint chuẩn bị nhằm hoàn thiện toàn bộ các tài liệu đặc tả thiết kế, thống nhất quy chuẩn và dựng khung xương dự án (Base code) để đội ngũ sẵn sàng bước vào code tính năng.
- **Các công việc đã thực hiện**:
  - **Task 0.1**: Viết tài liệu đặc tả yêu cầu phần mềm [CafePOS_SRS.md](file:///d:/VisualStudio/CafePOS&PDS/docs/specifications/CafePOS_SRS.md), tài liệu yêu cầu nghiệp vụ [brd.md](file:///d:/VisualStudio/CafePOS&PDS/docs/specifications/brd.md), tài liệu thiết kế cơ sở dữ liệu [erd.md](file:///d:/VisualStudio/CafePOS&PDS/docs/specifications/erd.md), tài liệu đặc tả API [api_contract.md](file:///d:/VisualStudio/CafePOS&PDS/docs/specifications/api_contract.md) và tài liệu luồng giao diện [wireframes.md](file:///d:/VisualStudio/CafePOS&PDS/docs/specifications/wireframes.md).
  - **Task 0.2**: Thiết lập quy chuẩn thiết kế API [api_conventions.md](file:///d:/VisualStudio/CafePOS&PDS/docs/conventions/api_conventions.md) và quy chuẩn lập trình mã nguồn [coding_conventions.md](file:///d:/VisualStudio/CafePOS&PDS/docs/conventions/coding_conventions.md).
  - **Task 0.3**: Dựng cấu trúc Solution C# Clean Architecture [CafePOS-PDS.sln](file:///d:/VisualStudio/CafePOS&PDS/CafePOS-PDS.sln) với 5 dự án con đặt trong thư mục `src/`: `CafePOS.API`, `CafePOS.Application`, `CafePOS.Domain`, `CafePOS.Infrastructure`, `CafePOS.Web`.
  - **Task 0.4**: Thiết lập dự án kiểm thử `CafePOS.Tests` liên kết với solution.
  - **Task 0.5**: Tạo toàn bộ các tệp stub C# (Entities, Enums, Interfaces, Configurations, Repositories) và tệp JavaScript dùng chung theo mô hình mới, đảm bảo solution biên dịch thành công.

---

### Sprint 1: Cấu hình EF Core DbContext, Migrations & Menu Management (Tuần 1)

#### Task 1.1: Cấu hình DbContext & Entity Configurations (EF Core)
- **Mô tả yêu cầu**: Triển khai ánh xạ (mapping) các thực thể (Entities) xuống cơ sở dữ liệu sử dụng Entity Framework Core. Áp dụng Fluent API để thiết lập các ràng buộc dữ liệu, khóa chính, khóa ngoại và chỉ mục cho các bảng.
- **Hướng dẫn thực hiện**:
  - Cấu hình lớp `AppDbContext` tại [AppDbContext.cs](file:///d:/VisualStudio/CafePOS&PDS/src/CafePOS.Infrastructure/Persistence/AppDbContext.cs).
  - Viết logic cấu hình trong các lớp configuration tại thư mục `src/CafePOS.Infrastructure/Persistence/Configurations/` bao gồm: `OrderConfiguration.cs`, `PaymentConfiguration.cs`, `ProductConfiguration.cs`, `VoucherConfiguration.cs`.
  - Thiết lập mối quan hệ N-N giữa Product và Topping, Product và ProductSize theo mô tả tại [erd.md](file:///d:/VisualStudio/CafePOS&PDS/docs/specifications/erd.md#2-chi-tiết-các-bảng-dữ-liệu).
- **Điều kiện hoàn thành (DoD)**:
  - Dự án `CafePOS.Infrastructure` biên dịch thành công không có lỗi cú pháp.
  - Các cấu hình Fluent API tách biệt hoàn toàn khỏi `AppDbContext` thông qua việc kế thừa `IEntityTypeConfiguration<T>`.

#### Task 1.2: Tạo Cơ sở dữ liệu vật lý qua EF Core Migrations & Seed Dữ liệu
- **Mô tả yêu cầu**: Tạo tệp migration chứa lược đồ cơ sở dữ liệu và thực hiện cập nhật lược đồ này để sinh các bảng vật lý trên SQL Server. Viết mã seed dữ liệu mẫu ban đầu (danh sách vai trò nhân viên, tài khoản Admin mặc định, danh mục và một số sản phẩm demo).
- **Hướng dẫn thực hiện**:
  - Mở terminal tại thư mục dự án API và chạy lệnh:
    `dotnet ef migrations add Initial_Sprint1 --project src/CafePOS.Infrastructure --startup-project src/CafePOS.API`
  - Thực thi lệnh cập nhật cơ sở dữ liệu: `dotnet ef database update`.
  - Viết logic gán dữ liệu mặc định trong tệp `DbSeeder.cs` tại [DbSeeder.cs](file:///d:/VisualStudio/CafePOS&PDS/src/CafePOS.Infrastructure/Persistence/DbSeeder.cs).
- **Điều kiện hoàn thành (DoD)**:
  - Sinh thành công tệp migration mới trong thư mục `Persistence/Migrations/`.
  - Cơ sở dữ liệu SQL Server được tạo lập chính xác tất cả các bảng dữ liệu thực thể.
  - Khi chạy ứng dụng, dữ liệu seed tự động được chèn vào DB thành công nếu DB trống.

#### Task 1.3: APIs Xác thực JWT & Phân quyền người dùng
- **Mô tả yêu cầu**: Triển khai chức năng đăng nhập cho nhân viên hệ thống bằng mã code POS được cấp. Sinh mã Token JWT chứa Claims vai trò của nhân viên để thực hiện phân quyền truy cập API.
- **Hướng dẫn thực hiện**:
  - Viết logic xác thực và sinh mã JWT trong `AuthService.cs` và `JwtProvider.cs`.
  - Tạo endpoint `POST /api/v1/auth/login` tại `AuthController.cs` kế thừa từ `ControllerBase`.
  - Áp dụng bộ lọc quyền hạn `[Authorize(Roles = "...")]` lên các Controller tương ứng của API.
- **Điều kiện hoàn thành (DoD)**:
  - Gửi request đăng nhập bằng mã nhân viên hợp lệ trả về HTTP 200 kèm JWT token có hạn sử dụng.
  - Token giải mã chứa đúng claim `role` của nhân viên. Các API bảo mật chặn thành công request không có token hoặc token không đúng vai trò (trả về HTTP 401 hoặc 403).

#### Task 1.4: APIs Quản lý Menu (CRUD Categories, Products, Sizes, Toppings)
- **Mô tả yêu cầu**: Viết các APIs CRUD cho danh mục (Category), sản phẩm (Product), kích thước (ProductSize) và các loại Toppings. Hỗ trợ gán Category vào trạm chế biến đích PDS (Bar hoặc Pastry).
- **Hướng dẫn thực hiện**:
  - Triển khai APIs trong `ProductsController.cs` và `InventoryController.cs` thuộc dự án API.
  - Category cần có thuộc tính `TargetStation` (Enum: `Bar` / `Pastry`) để định tuyến hiển thị trên PDS.
  - Viết mã lưu ảnh sản phẩm upload lên thư mục `wwwroot/images/products/` của dự án Web hoặc lưu trữ qua API.
- **Điều kiện hoàn thành (DoD)**:
  - APIs CRUD hoạt động chính xác, validate dữ liệu đầu vào.
  - Kiểm thử đơn vị (Unit Test) cho Service xử lý Menu đạt tỷ lệ bao phủ mã nguồn > 80% trong dự án `CafePOS.Tests`.

---

### Sprint 2: Giao diện POS Core, Xử lý Gọi món & Concurrency (Tuần 2)

#### Task 2.1: Phát triển Giao diện Bán hàng POS Layout & Menu Navigation
- **Mô tả yêu cầu**: Thiết kế và phát triển giao diện bán hàng (POS Client) chạy trên Web dành cho nhân viên thu ngân (CSR). Hiển thị danh mục món dạng lưới (Grid), tìm kiếm món nhanh, danh sách order nháp và khu vực cấu hình món ăn.
- **Hướng dẫn thực hiện**:
  - Phát triển giao diện trong `POSController.cs` và các file view liên quan tại `src/CafePOS.Web/Views/POS/`.
  - Sử dụng JS dùng chung [api-client.js](file:///d:/VisualStudio/CafePOS&PDS/src/CafePOS.Web/wwwroot/js/shared/api-client.js) để gọi API lấy danh sách menu.
  - Đảm bảo thiết kế CSS responsive trên các thiết bị máy tính bảng và màn hình POS chuyên dụng, sử dụng cấu trúc tại [wwwroot/css/pos/](file:///d:/VisualStudio/CafePOS&PDS/src/CafePOS.Web/wwwroot/css/pos/).
- **Điều kiện hoàn thành (DoD)**:
  - CSR có thể duyệt qua các danh mục sản phẩm mượt mà (< 100ms thời gian render).
  - Giao diện hiển thị đúng giá tiền và trạng thái (Còn hàng/Tạm ngưng) của từng món ăn.

#### Task 2.2: Luồng nghiệp vụ Chọn món & Tạo đơn hàng (Dine-in / Take-away)
- **Mô tả yêu cầu**: Nhân viên POS thực hiện chọn món, tuỳ chỉnh biến thể (Size S/M/L, lượng đường/đá, toppings đi kèm). Khi chọn xong, tạo bản ghi Order ở trạng thái `Draft` (Take-away cần nhập SĐT khách hàng).
- **Hướng dẫn thực hiện**:
  - Giao diện POS quản lý giỏ hàng tạm thời bằng JS client. Khi nhấn "Tạo đơn", gọi API `POST /api/v1/orders` để lưu đơn hàng vào DB.
  - Chi tiết cấu trúc dữ liệu đơn hàng xem tại [erd.md](file:///d:/VisualStudio/CafePOS&PDS/docs/specifications/erd.md#2-chi-tiết-các-bảng-dữ-liệu) (Bảng `Orders` và `OrderItems`).
- **Điều kiện hoàn thành (DoD)**:
  - Tạo đơn thành công trong DB với trạng thái `Draft`.
  - Các thông số tùy chọn đường, đá, topping của từng món được lưu chính xác trong bảng `OrderItems` và `OrderItemToppings`.

#### Task 2.3: Kiểm soát Xung đột Đồng thời (Concurrency) & Validate Đơn hàng
- **Mô tả yêu cầu**: Áp dụng cơ chế Optimistic Concurrency Control sử dụng trường `RowVersion` trên bảng `Orders`. Đồng thời, viết validator kiểm tra dữ liệu đầu vào cho quy trình tạo đơn.
- **Hướng dẫn thực hiện**:
  - Sử dụng EF Core cấu hình thuộc tính Concurrency Token trên Entity `Order`: `builder.Property(o => o.RowVersion).IsRowVersion();`.
  - Viết `CreateOrderValidator` sử dụng FluentValidation hoặc kiểm tra logic thủ công tại `src/CafePOS.Application/Validators/CreateOrderValidator.cs`.
- **Điều kiện hoàn thành (DoD)**:
  - Khi hai CSR cùng cập nhật một đơn hàng đồng thời, hệ thống báo lỗi `DbUpdateConcurrencyException` và chặn bản ghi ghi đè không mong muốn.
  - Trả về HTTP 400 Bad Request kèm thông báo lỗi rõ ràng nếu vi phạm validate (ví dụ: số lượng món âm, topping không tồn tại).

---

### Sprint 3: Quy chuẩn API & Luồng Thanh toán POS (Cash & VietQR động) (Tuần 3)

#### Task 3.1: Áp dụng Quy chuẩn Phân trang (Pagination) & Success Response
- **Mô tả yêu cầu**: Refactor các API danh sách đã viết sang định dạng phân trang chuẩn và đồng bộ cấu trúc dữ liệu phản hồi thành công (Success Response).
- **Hướng dẫn thực hiện**:
  - Cấu trúc response phân trang bắt buộc gồm các trường: `{ items, page, pageSize, totalItems, totalPages }`.
  - Áp dụng phân trang cho danh sách Products, Orders và Customers.
  - Tuân thủ nghiêm ngặt chỉ dẫn tại [api_conventions.md](file:///d:/VisualStudio/CafePOS&PDS/docs/conventions/api_conventions.md#1-pagination-convention).
- **Điều kiện hoàn thành (DoD)**:
  - Tất cả các API GET dạng danh sách trả về định dạng JSON chuẩn.
  - Kết quả phân trang chạy đúng vị trí và số lượng bản ghi tương ứng với `page` và `pageSize` truyền từ query string.

#### Task 3.2: Tích hợp Luồng Thanh toán Tiền mặt & Thẻ tại POS (Thanh toán trước)
- **Mô tả yêu cầu**: CSR thu tiền mặt hoặc thẻ của khách hàng tại POS. CSR xác nhận số tiền thực nhận, hệ thống lưu bản ghi giao dịch `Payments`, chuyển trạng thái Order sang `Confirmed` và ghi nhận doanh thu.
- **Hướng dẫn thực hiện**:
  - Viết API `POST /api/v1/orders/{id}/payment` xử lý giao dịch thanh toán.
  - Tạo bản ghi mới trong bảng `Payments` với `Method = Cash` hoặc `Card`, `Status = Completed`.
  - Đảm bảo tính Idempotency: Một đơn hàng chỉ có tối đa 1 bản ghi Payment thành công.
- **Điều kiện hoàn thành (DoD)**:
  - Bản ghi Payment được liên kết chính xác với Order.
  - Trạng thái Order cập nhật thành công từ `Draft` sang `Confirmed`.
  - Báo lỗi nếu cố thanh toán một đơn hàng đã hoàn thành thanh toán trước đó.

#### Task 3.3: Tích hợp Cổng thanh toán VietQR động
- **Mô tả yêu cầu**: Khi chọn thanh toán VietQR tại POS, hệ thống tạo mã QR động chứa đầy đủ thông tin số tiền thanh toán, tài khoản ngân hàng của quán và mã nội dung chuyển khoản định danh đơn hàng. Nhân viên kiểm tra và xác nhận giao dịch.
- **Hướng dẫn thực hiện**:
  - Sử dụng `VietQrService` tại `Infrastructure/Services/` để call API của bên thứ ba (như VietQR.io hoặc tự sinh chuỗi VietQR chuẩn EMVCo).
  - QR Code hiển thị trên màn hình POS chứa định dạng nội dung: `CAFEPOS [Mã_Đơn_Hàng]`.
- **Điều kiện hoàn thành (DoD)**:
  - Hiển thị QR động trên POS khớp chính xác số tiền đơn hàng.
  - CSR có thể nhấn "Xác nhận chuyển khoản thành công" để chuyển trạng thái đơn hàng sang `Confirmed`.

---

### Sprint 4: Real-time Hub & Màn hình hiển thị pha chế PDS (Tuần 4)

#### Task 4.1: Cấu hình SignalR Hub cho Giao tiếp Real-time
- **Mô tả yêu cầu**: Xây dựng kênh kết nối hai chiều thời gian thực giữa POS và PDS. Khi POS thanh toán thành công, đơn hàng tự động hiển thị trên PDS mà không cần tải lại trang.
- **Hướng dẫn thực hiện**:
  - Phát triển Hub SignalR mang tên `OrderHub.cs` tại `src/CafePOS.API/Hubs/`.
  - Cấu hình tự động kết nối lại (Automatic Reconnect) phía client JS theo [coding_conventions.md](file:///d:/VisualStudio/CafePOS&PDS/docs/conventions/coding_conventions.md#6-shared-javascript-utilities).
- **Điều kiện hoàn thành (DoD)**:
  - Client kết nối thành công đến endpoint `/hubs/orders`.
  - Hệ thống log lại sự kiện kết nối và ngắt kết nối của các trạm thiết bị.

#### Task 4.2: Phát triển Màn hình PDS (Kitchen/Bar Display)
- **Mô tả yêu cầu**: Phát triển giao diện PDS hiển thị danh sách đơn hàng đã xác nhận thanh toán (`Confirmed`), phân loại theo thời gian xếp hàng. Giao diện hỗ trợ cấu hình hiển thị: 2 màn hình độc lập (Bar và Pastry) hoặc 1 màn hình gộp chia cột.
- **Hướng dẫn thực hiện**:
  - Phát triển controller và views tại `src/CafePOS.Web/Views/PDS/`.
  - Các món trong đơn hàng được lọc theo `TargetStation` của danh mục (Món uống đi vào màn hình Bar, bánh ngọt đi vào quầy Pastry).
- **Điều kiện hoàn thành (DoD)**:
  - Giao diện tải danh sách đơn hàng real-time qua SignalR.
  - PDS hiển thị rõ thông tin tùy chỉnh của món (ví dụ: "Ít đá", "Thêm Trân châu").

#### Task 4.3: Tương tác PDS & Tự động hoàn thành đơn hàng
- **Mô tả yêu cầu**: Nhân viên pha chế có thể tương tác: Bấm "Bắt đầu làm" (chuyển trạng thái Order sang `Preparing`), "Hoàn thành món" và "Hoàn thành quầy". Khi cả hai quầy Bar & Pastry xong, đơn hàng tự chuyển sang `Completed` và gửi thông báo về POS.
- **Hướng dẫn thực hiện**:
  - Sử dụng Web API từ PDS gửi các lệnh cập nhật trạng thái đơn hàng: `PUT /api/v1/orders/{id}/status`.
  - Phát tín hiệu notify qua `OrderHub` đến POS để hiển thị thông báo Toast.
- **Điều kiện hoàn thành (DoD)**:
  - Trạng thái đơn hàng cập nhật đúng tiến độ pha chế trên DB.
  - POS hiển thị Toast thông báo món đã làm xong khi PDS nhấn Hoàn thành quầy cuối cùng.

---

### Sprint 5: Quản lý Ca làm việc, Chiết khấu duyệt thủ công & Hoàn tiền (Tuần 5)

#### Task 5.1: Nghiệp vụ Mở/Đóng ca làm việc và Đối soát
- **Mô tả yêu cầu**: Trưởng ca (Shift Leader) thực hiện mở ca bằng cách nhập số tiền mặt đầu ca (Float Cash). Khi kết thúc ca, thực hiện đóng ca, nhập số tiền thực tế đếm được và hệ thống đối soát chênh lệch với số liệu ghi nhận trên phần mềm.
- **Hướng dẫn thực hiện**:
  - Viết APIs quản lý ca trong `ShiftsController.cs` và Service `ShiftService.cs`.
  - Lưu trữ thông tin ca làm việc trong bảng `Shifts`. Cuối ca, xuất báo cáo tổng kết doanh thu dưới dạng PDF/In ấn.
- **Điều kiện hoàn thành (DoD)**:
  - Bản ghi ca làm việc lưu trữ đầy đủ: Tiền đầu ca, tiền thực tế cuối ca, tiền lý thuyết hệ thống, chênh lệch và người thực hiện.
  - Nhân viên không thể tạo đơn hàng nếu hệ thống chưa thực hiện mở ca.

#### Task 5.2: Phê duyệt Chiết khấu Thủ công & Ghi Audit Log
- **Mô tả yêu cầu**: Cho phép nhân viên POS nhập phần trăm giảm giá thủ công trực tiếp trên hóa đơn. Yêu cầu có sự xác nhận của Trưởng ca hoặc Quản lý bằng mã phê duyệt nội bộ. Hệ thống tự động ghi nhật ký bảo mật.
- **Hướng dẫn thực hiện**:
  - Giao diện POS hiển thị modal yêu cầu nhập mã pin/code phê duyệt của Shift Leader.
  - Gọi API xác thực quyền hạn nhân viên duyệt. Sau khi thành công, tính toán lại tổng tiền đơn hàng và ghi nhận một bản ghi vào bảng `AuditLogs`.
- **Điều kiện hoàn thành (DoD)**:
  - Áp dụng giảm giá thành công chỉ khi mã code người duyệt hợp lệ và có vai trò tối thiểu là `ShiftLeader`.
  - Bảng `AuditLogs` lưu trữ chi tiết: Người thực hiện, Người duyệt, Giá trị cũ, Giá trị mới, và Lý do giảm giá.

#### Task 5.3: Quy trình Hoàn tiền (Refund) đơn hàng đã đóng
- **Mô tả yêu cầu**: Xử lý tình huống huỷ đơn/trả món của khách hàng đối với các đơn hàng đã đóng. Tạo giao dịch Payment hoàn tiền với số tiền âm, khấu trừ điểm tích luỹ, hoàn lại điểm đổi món và ghi nhận Audit Log.
- **Hướng dẫn thực hiện**:
  - Triển khai API `POST /api/v1/orders/{id}/refund` trong `PaymentsController.cs`.
  - Chỉ cho phép vai trò từ `ShiftLeader` trở lên thực hiện phê duyệt giao dịch hoàn tiền.
  - Trừ điểm loyalty đã tích lũy từ đơn hàng đó dựa trên công thức cấu hình.
- **Điều kiện hoàn thành (DoD)**:
  - Tạo bản ghi Payment âm liên kết với hóa đơn gốc thành công.
  - Điểm tích luỹ của khách hàng được khấu trừ chính xác.
  - Nhật ký hoàn tiền được ghi nhận đầy đủ vào hệ thống Audit Log.

---

### Sprint 6: Web đặt hàng cho Khách hàng (COD) & Tự động hẹn giờ lấy (Tuần 6)

#### Task 6.1: Giao diện & Đăng ký Tài khoản Khách hàng (Loyalty Portal)
- **Mô tả yêu cầu**: Phát triển trang Web dành riêng cho Khách hàng. Hỗ trợ đăng ký/đăng nhập bằng Số điện thoại hoặc tài khoản Google. Hiển thị thông tin tích điểm, thứ hạng thẻ (Tier) và lịch sử đơn hàng.
- **Hướng dẫn thực hiện**:
  - Giao diện xây dựng tại `src/CafePOS.Web/Views/Customer/` và sử dụng tài nguyên tĩnh tại `wwwroot/css/customer/`.
  - Tích hợp Google OAuth Client SDK.
- **Điều kiện hoàn thành (DoD)**:
  - Khách hàng đăng ký tài khoản mới được tự động gán hạng thẻ mặc định (Đồng/Mới).
  - Khách hàng đăng nhập xem được số điểm tích luỹ hiện tại của mình theo thời gian thực.

#### Task 6.2: Quy trình Đặt hàng Online (COD) & Chọn giờ nhận đồ
- **Mô tả yêu cầu**: Khách hàng xem thực đơn, tuỳ biến sản phẩm, thêm vào giỏ hàng và tiến hành đặt hàng trực tuyến. Khách hàng có thể chọn lấy ngay (mặc định sau 10 phút) hoặc chọn khung giờ cụ thể trong ngày. Thanh toán COD tại quầy khi nhận đồ.
- **Hướng dẫn thực hiện**:
  - Viết API `POST /api/v1/orders` với thuộc tính `OrderType = Online` và trạng thái ban đầu là `Pending` (Chờ xác nhận).
  - Giờ nhận đồ (`PickupTime`) được lưu trữ cụ thể trong đơn hàng.
- **Điều kiện hoàn thành (DoD)**:
  - Tạo đơn hàng trực tuyến thành công với trạng thái `Pending`.
  - Giỏ hàng tự động làm trống sau khi đặt hàng thành công và hiển thị trang theo dõi tiến độ đơn hàng.

#### Task 6.3: Tiếp nhận đơn Online tại POS & Cơ chế Kích hoạt Hẹn giờ tự động
- **Mô tả yêu cầu**: POS nhận thông báo thời gian thực khi có đơn hàng trực tuyến mới. Nhân viên POS kiểm duyệt và nhấn "Xác nhận" để chuyển đơn sang PDS pha chế. Với đơn hàng đặt trước (hẹn giờ), hệ thống tự động đẩy xuống PDS đúng 10 phút trước giờ hẹn lấy đồ.
- **Hướng dẫn thực hiện**:
  - Dùng SignalR gửi thông báo đến POS client.
  - Viết Background Job bằng Hangfire hoặc `IHostedService` định kỳ quét các đơn hàng trực tuyến hẹn giờ (sử dụng cấu trúc `ScheduledOrderJob` trong `Infrastructure/BackgroundJobs/`).
- **Điều kiện hoàn thành (DoD)**:
  - Nhân viên POS duyệt đơn online trực tiếp trên giao diện POS và chuyển đơn sang `Confirmed` thành công.
  - Đơn hẹn giờ 14:00 sẽ tự động chuyển trạng thái để PDS pha chế lúc 13:50 mà không cần thao tác thủ công.

---

### Sprint 7: Chương trình Khách hàng thân thiết, Voucher & Công thức món (Recipe/BOM) (Tuần 7)

#### Task 7.1: Xử lý Tích điểm, Nâng hạng Thẻ & Tự động Reset Điểm định kỳ
- **Mô tả yêu cầu**: Áp dụng quy tắc tích điểm (10.000đ = 1 điểm). Tự động nâng hạng thẻ dựa trên doanh số tích luỹ trọn đời. Thiết lập tác vụ định kỳ tự động xoá/reset điểm tích lũy của khách hàng mỗi 2 tháng một lần.
- **Hướng dẫn thực hiện**:
  - Viết logic tích lũy điểm khi Order chuyển sang trạng thái `Closed`.
  - Viết Background Job `PointResetJob.cs` trong `Infrastructure/BackgroundJobs/` chạy vào ngày 1 của mỗi chu kỳ 2 tháng để reset điểm hiện tại của khách về 0.
- **Điều kiện hoàn thành (DoD)**:
  - Thanh toán hóa đơn 150.000đ giúp tích lũy thêm đúng 15 điểm cho tài khoản khách hàng.
  - Job reset điểm chạy thử nghiệm (đặt thời gian ngắn hơn) reset chính xác điểm hiện tại nhưng không làm ảnh hưởng đến tổng chi tiêu trọn đời của khách hàng.

#### Task 7.2: Quản lý & Áp dụng Voucher Khuyến mãi
- **Mô tả yêu cầu**: Cho phép Quản trị viên quản lý mã Voucher (giảm theo % hoặc số tiền cố định, có giới hạn số lượng và thời hạn sử dụng). APIs hỗ trợ kiểm tra tính hợp lệ và áp dụng voucher trực tiếp vào hóa đơn bán hàng.
- **Hướng dẫn thực hiện**:
  - Viết APIs CRUD voucher trong `LoyaltyService.cs` và `VoucherService.cs`.
  - Bảng dữ liệu `Vouchers` lưu cấu hình điều kiện sử dụng (ví dụ: Tổng tiền tối thiểu, hạng thẻ áp dụng).
- **Điều kiện hoàn thành (DoD)**:
  - Trả về lỗi chi tiết khi áp dụng voucher hết hạn, hết số lượng lượt dùng hoặc không đạt giá trị đơn tối thiểu.
  - Giảm giá chính xác số tiền tương ứng trên hóa đơn và cập nhật số lượt sử dụng voucher trong DB.

#### Task 7.3: Quản lý Công thức sản phẩm (BOM) & Tự động khấu trừ Tồn kho
- **Mô tả yêu cầu**: Thiết lập công thức chế biến (Recipe/BOM) cho từng món ăn (ví dụ: 1 cốc Cappuccino cần 15g Hạt cafe, 120ml Sữa tươi). Khi đơn hàng pha chế xong trên PDS, hệ thống tự động trừ kho nguyên liệu tương ứng.
- **Hướng dẫn thực hiện**:
  - Thiết lập bảng quan hệ `ProductIngredients` liên kết Product với Ingredient và định lượng.
  - Khi Order chuyển sang trạng thái `Completed`, thực hiện trừ số lượng nguyên liệu trong bảng `Ingredients`.
- **Điều kiện hoàn thành (DoD)**:
  - Khi hoàn thành pha chế 2 ly Cappuccino, số lượng tồn kho sữa tươi và hạt cafe giảm đúng định mức cấu hình nhân hai.
  - Đưa ra cảnh báo hệ thống (Warning) nếu số lượng nguyên liệu trong kho giảm xuống dưới mức cảnh báo tối thiểu đã thiết lập.

---

### Sprint 8: Nhập kiểm kho hai bước, Báo cáo Doanh thu & Tra cứu Nhật ký hệ thống (Tuần 8)

#### Task 8.1: Quy trình Kiểm kho & Điều chỉnh Tồn kho hai bước
- **Mô tả yêu cầu**: Hạn chế gian lận và sai lệch kho. Bước 1: Trưởng ca kiểm kho thực tế và nhập số lượng đếm được vào hệ thống (không tự ý ghi đè). Bước 2: Chủ cửa hàng (Owner) xem báo cáo chênh lệch, phê duyệt điều chỉnh để cập nhật số lượng tồn kho thực tế.
- **Hướng dẫn thực hiện**:
  - Lưu bản ghi kiểm kho trong bảng `InventoryChecks` và danh sách chi tiết chênh lệch trong `InventoryCheckItems`.
  - API duyệt kiểm kho cập nhật trực tiếp `CurrentQuantity` của nguyên liệu và ghi nhật ký hoạt động.
- **Điều kiện hoàn thành (DoD)**:
  - Nhân viên ca không thể thay đổi trực tiếp số lượng tồn kho mà chỉ tạo yêu cầu chênh lệch.
  - Sau khi Owner ấn phê duyệt, tồn kho thực tế mới được cập nhật và chênh lệch cũ được lưu trữ làm lịch sử đối soát.

#### Task 8.2: Trang Báo cáo Thống kê & Phân tích (Dashboard cho Manager)
- **Mô tả yêu cầu**: Xây dựng trang Dashboard quản lý hiển thị các chỉ số kinh doanh quan trọng: Doanh thu theo thời gian/khung giờ, danh sách sản phẩm bán chạy/bán chậm, hiệu quả sử dụng voucher, và biểu đồ phân bổ thứ hạng khách hàng.
- **Hướng dẫn thực hiện**:
  - Triển khai các API thống kê hiệu năng cao trong `ReportsController.cs` và Service `ReportService.cs`.
  - Vẽ biểu đồ phân tích dữ liệu trực quan bằng thư viện Chart.js hoặc thư viện tương đương ở phía frontend.
- **Điều kiện hoàn thành (DoD)**:
  - Trang báo cáo hiển thị chính xác số liệu doanh thu thực tế từ cơ sở dữ liệu.
  - Thời gian phản hồi của các API báo cáo phức tạp không vượt quá 500ms đối với dữ liệu phát sinh trong vòng 1 năm.

#### Task 8.3: Tra cứu Lịch sử Giao dịch & Nhật ký Bảo mật hệ thống (Audit Logs)
- **Mô tả yêu cầu**: Cung cấp giao diện tra cứu lịch sử tất cả các giao dịch thanh toán (`Payments`) và nhật ký hệ thống (`AuditLogs`) dành cho Người quản lý để phục vụ hoạt động giám sát bảo mật và hậu kiểm.
- **Hướng dẫn thực hiện**:
  - APIs tra cứu trong `ReportsController.cs` hỗ trợ tìm kiếm theo từ khóa, lọc theo loại hành động (Action) và khoảng thời gian.
  - Danh sách kết quả bắt buộc áp dụng phân trang và sắp xếp mặc định theo thời gian giảm dần.
- **Điều kiện hoàn thành (DoD)**:
  - Cho phép quản lý tìm kiếm và xem chi tiết nhật ký thao tác nhạy cảm (như hoàn tiền, duyệt điều chỉnh kho, chiết khấu hóa đơn duyệt bởi ai).
  - Không cho phép chỉnh sửa hoặc xóa bất kỳ bản ghi nào trong bảng `AuditLogs` thông qua APIs (đọc ghi một chiều).

---

## Tiêu chí Nghiệm thu Tổng thể Dự án (Definition of Done)

Trước khi đóng dự án và bàn giao sản phẩm, toàn bộ mã nguồn phải đáp ứng các tiêu chuẩn chất lượng sau:
1. **Biên dịch**: Solution biên dịch thành công trên môi trường sạch không có lỗi hoặc cảnh báo nghiêm trọng.
2. **Kiểm thử tự động**: Dự án [CafePOS.Tests](file:///d:/VisualStudio/CafePOS&PDS/src/CafePOS.Tests/) chạy thành công 100% các ca kiểm thử.
3. **Quy chuẩn APIs**: Toàn bộ APIs tuân thủ các quy tắc trong [api_conventions.md](file:///d:/VisualStudio/CafePOS&PDS/docs/conventions/api_conventions.md) và được mô tả chính xác trong Swagger/OpenAPI.
4. **Hiệu năng**: Các thao tác cơ bản (thêm món, thanh toán, nhận đơn trên PDS) diễn ra mượt mà dưới 200ms trên môi trường local.
