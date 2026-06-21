# 💻 Quy chuẩn Lập trình (Coding Conventions)
### Hệ thống Quản lý Bán hàng Quán Cà phê / Trà sữa (CafePOS & PDS)

Tài liệu này quy định các quy tắc lập trình (coding standards), quy tắc đặt tên (Naming Conventions) và tổ chức mã nguồn cho cả Backend (C#/.NET) lẫn Frontend (HTML, CSS, JavaScript, Razor Pages).

---

## 1. Quy chuẩn Backend (C# / .NET)

Chúng ta tuân theo các chuẩn thiết kế và lập trình hiện đại của Microsoft .NET:

### 1.1 Quy tắc đặt tên (Naming Rules)

| Thành phần | Quy tắc đặt tên | Ví dụ |
|---|---|---|
| **Class / Struct / Record** | `PascalCase` | `OrderService`, `ProductController` |
| **Interface** | `PascalCase` bắt đầu bằng `I` | `IOrderService`, `IInventoryRepository` |
| **Method** | `PascalCase` | `GetOrderByIdAsync`, `CalculateTotal` |
| **Method bất đồng bộ** | Kết thúc bằng hậu tố `Async` | `SaveChangesAsync` (Không dùng `SaveChangeAsync`) |
| **Local Variable** | `camelCase` | `totalAmount`, `currentShift` |
| **Parameter** | `camelCase` | `orderId`, `updatedBy` |
| **Private Field** | `camelCase` có dấu gạch dưới `_` ở đầu | `_dbContext`, `_logger` |
| **Public Property** | `PascalCase` | `Id`, `CustomerName`, `TotalPrice` |
| **Constant / Enum** | `PascalCase` | `MaxRetryCount`, `OrderStatus.Pending` |

### 1.2 Quy tắc viết Code & Xử lý Bất đồng bộ
* Luôn sử dụng Dependency Injection (DI) để inject các dịch vụ thông qua Constructor.
* Các hàm xử lý I/O (Database, Network) bắt buộc phải dùng lập trình bất đồng bộ (`async` / `await`).
* **Sửa lỗi phổ biến**: Sử dụng chính xác phương thức bất đồng bộ của EF Core là `SaveChangesAsync()` để lưu thay đổi vào cơ sở dữ liệu. Nghiêm cấm viết thiếu chữ "s" thành `SaveChangeAsync()`.
* Không viết trực tiếp các Magic Numbers/Strings vào logic. Khai báo hằng số (`const`) hoặc cấu hình trong `appsettings.json`.

### 1.3 Quy tắc Nullable Reference Types
* Toàn bộ các project trong Solution bắt buộc phải kích hoạt tính năng kiểm tra Nullable:
  ```xml
  <Nullable>enable</Nullable>
  ```
* Khai báo rõ ràng các biến/thuộc tính có thể nhận giá trị `null` bằng dấu chấm hỏi `?`:
  ```csharp
  string? phoneNumber;
  Customer? customer;
  ```
* Tránh cảnh báo từ compiler bằng cách xử lý kiểm tra `null` cẩn thận hoặc sử dụng các giá trị mặc định thích hợp.

### 1.4 Quy chuẩn đặt tên DTO (DTO Naming Convention)
Hệ thống không sử dụng các class DTO chung chung (như `OrderDto`, `ProductDto`) cho mọi hành động CRUD. Thay vào đó, bắt buộc đặt tên DTO phân tách rõ ràng theo Request/Response của từng chức năng:
* **Tạo đơn hàng**: `CreateOrderRequest` và `CreateOrderResponse`
* **Cập nhật sản phẩm**: `UpdateProductRequest` và `UpdateProductResponse`
* **Lấy thông tin**: `GetProductDetailResponse`
* Quy định này giúp dễ dàng tùy biến cấu trúc dữ liệu truyền nhận mà không ảnh hưởng đến các API khác.

### 1.5 Quy chuẩn ghi nhận nhật ký (Logging Convention)
* Sử dụng interface `ILogger` được inject để ghi nhận thông tin hệ thống.
* Sử dụng các mức độ log phù hợp:
  * `_logger.LogInformation()` cho các sự kiện thông thường trong hệ thống.
  * `_logger.LogWarning()` cho các trường hợp bất thường nhưng không gây lỗi ứng dụng.
  * `_logger.LogError()` cho các lỗi ngoại lệ (Exceptions), lỗi hệ thống cần can thiệp.
* **Quy tắc nghiêm ngặt**: **Tuyệt đối không sử dụng `Console.WriteLine()`** trong code production để in thông tin ra màn hình console.

### 1.6 Quy chuẩn sử dụng Enum (Enum Convention)
* Khai báo các enum nghiệp vụ rõ ràng:
  ```csharp
  public enum OrderStatus
  {
      Draft,
      Confirmed,
      Preparing,
      Completed,
      Closed,
      Cancelled
  }
  ```
* **Lưu trữ Database**: Enum được lưu trữ dưới dạng số nguyên **`int`** trong SQL Server để tối ưu hiệu năng truy vấn và dung lượng lưu trữ.
* **Truyền nhận API**: Khi serialize qua API trả về Client, enum bắt buộc được chuyển đổi sang dạng **`string`** (ví dụ: `"status": "Preparing"` thay vì số `2`) để Client dễ đọc và tránh phụ thuộc vào thứ tự khai báo số của enum.

---

## 2. Quy chuẩn Frontend (MVC Views, HTML, CSS, JavaScript)

### 2.1 Quy tắc đặt tên trong HTML & Views
* **HTML Tags**: Viết chữ thường toàn bộ (ví dụ: `<div>`, `<span>`, `<input>`).
* **Thuộc tính ID (`id`)**: Định dạng bằng `kebab-case` (ví dụ: `btn-checkout`, `input-voucher-code`). Phải là duy nhất trên trang.
* **Thuộc tính Class (`class`)**: Định dạng bằng `kebab-case` (ví dụ: `product-card`, `btn-primary`).

### 2.2 Quy tắc đặt tên & sử dụng màu trong CSS
* **Quy tắc biến màu (CSS Variables)**:
  * Tất cả các mã màu sử dụng trong CSS **bắt buộc** phải được định nghĩa tại `:root` trong file [site.css](file:///d:/VisualStudio/CafePOS&PDS/CafePOS.Web/wwwroot/css/site.css).
  * Đặt tên biến dạng `kebab-case` (ví dụ: `--pos-bg`, `--status-done`, `--customer-primary`).
  * Tuyệt đối không viết trực tiếp mã màu HEX hoặc RGB vào các CSS selector khác. Phải gọi màu thông qua biến: `color: var(--color-white);`.

### 2.3 Quy tắc đặt tên & Chia sẻ mã nguồn JavaScript
* **Quy tắc đặt tên JS**:
  * Biến & Hàm: `camelCase` (ví dụ: `selectedProductId`, `handleCheckoutClick()`).
  * Hằng số: `UPPER_SNAKE_CASE` (ví dụ: `API_BASE_URL`).
  * Lớp (Classes): `PascalCase` (ví dụ: `OrderTracker`).
* **Quy chuẩn chia sẻ code JS (Shared JS Convention)**:
  * Thay vì viết tất cả mã nguồn JS cô lập trong các tệp riêng lẻ của từng trang (như `Orders.js`, `Products.js`), cho phép và khuyến khích tạo các file JavaScript dùng chung phục vụ các hạ tầng dùng chung:
    * `api-client.js`: Thư viện xử lý gọi AJAX/fetch API chung.
    * `signalr.service.js`: Quản lý kết nối và nhận sự kiện SignalR.
    * `toast.service.js`: Quản lý hiển thị thông báo nhanh (toast messages).
    * `modal.service.js`: Quản lý hiển thị các popup dialog.
  * Các file này phải được lưu trữ trong thư mục dùng chung và khai báo dùng chung ở Layout chính.

### 2.4 Cấu trúc thư mục tĩnh Frontend (Folder Structure - Web)
Quy hoạch cấu trúc thư mục chứa file tĩnh trong project MVC Web:
```
wwwroot/
 ├── css/              # Chứa các file site.css và css dùng chung
 │    └── [Controller]/# Thư mục CSS tương ứng với Controller
 ├── js/               # Chứa các tệp JavaScript dùng chung
 │    ├── shared/      # Chứa api-client.js, toast.service.js, ...
 │    └── [Controller]/# Thư mục JS tương ứng với Controller
 └── images/           # Chứa các hình ảnh, assets tĩnh của dự án
```

---

## 3. Quy chuẩn cấu trúc thư mục Dự án (Solution Structure)

Để tránh tình trạng lệch cấu trúc khi hệ thống phình to, toàn bộ ứng dụng CafePOS & PDS được tổ chức theo kiến trúc phân lớp chuẩn:
* **Backend Solution**:
  * `CafePOS.Domain`: Chứa các thực thể Database (Entities), Enums và các logic lõi không phụ thuộc thư viện ngoài.
  * `CafePOS.Infrastructure`: Chứa cấu hình EF Core (DbContext), Migrations, Repositories, Background Jobs, kết nối dịch vụ ngoài.
  * `CafePOS.Application`: Chứa các Service nghiệp vụ, interface, DTOs (Request/Response models), mapper, validator.
  * `CafePOS.API`: Project ASP.NET Core Web API chứa các Controllers, Hubs SignalR, Middlewares, Program.cs.
  * `CafePOS.Web`: Project MVC UI (Frontend) chứa các Views (.cshtml), Controllers điều hướng, CSS/JS tĩnh.

---

## 4. Quy chuẩn quản lý mã nguồn Git (Git Convention)

### 4.1 Quy chuẩn đặt tên nhánh (Branch Naming)
Tên nhánh được đặt theo tiếng Anh, viết thường toàn bộ và phân tách bằng dấu gạch ngang `-`, bắt đầu bằng các tiền tố quy định loại nhánh:
* Nhánh tính năng mới: `feature/` (Ví dụ: `feature/vietqr-payment`, `feature/order-management`).
* Nhánh sửa lỗi: `bugfix/` (Ví dụ: `bugfix/pds-order-duplicate`).
* Nhánh sửa lỗi khẩn cấp trên production: `hotfix/` (Ví dụ: `hotfix/payment-error`).

### 4.2 Quy chuẩn viết thông điệp Commit (Conventional Commits)
Thông điệp commit bắt buộc phải sử dụng các tiền tố chuẩn sau để mô tả ngắn gọn hành động:
* `feat:` thêm tính năng mới (Ví dụ: `feat: add vietqr payment`).
* `fix:` sửa lỗi (Ví dụ: `fix: prevent duplicate payment`).
* `refactor:` tái cấu trúc code nhưng không đổi tính năng (Ví dụ: `refactor: extract order service`).
* `docs:` cập nhật tài liệu (Ví dụ: `docs: update api convention`).
* `chore:` cập nhật thư viện, cấu hình build, hoặc tool (Ví dụ: `chore: update packages`).
