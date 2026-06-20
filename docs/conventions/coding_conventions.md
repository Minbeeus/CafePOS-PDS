# 💻 Quy chuẩn Lập trình (Coding Conventions)
### Hệ thống Quản lý Bán hàng Quán Cà phê / Trà sữa (CafePOS & PDS)

Tài liệu này quy định các quy tắc đặt tên (Naming Conventions) và viết code cho cả Backend (C#/.NET) lẫn Frontend (HTML, CSS, JavaScript, Razor Pages).

---

## 1. Quy chuẩn Backend (C# / .NET)

Chúng ta tuân theo chuẩn khuyến nghị chung của Microsoft:

### 1.1 Quy tắc đặt tên (Naming Rules)

| Thành phần | Quy tắc đặt tên | Ví dụ |
|---|---|---|
| **Class / Struct / Record** | `PascalCase` | `OrderService`, `ProductController` |
| **Interface** | `PascalCase` bắt đầu bằng `I` | `IOrderService`, `IInventoryRepository` |
| **Method** | `PascalCase` | `GetOrderByIdAsync`, `CalculateTotal` |
| **Method bất đồng bộ** | Kết thúc bằng hậu tố `Async` | `SaveChangeAsync`, `ProcessPaymentAsync` |
| **Local Variable** | `camelCase` | `totalAmount`, `currentShift` |
| **Parameter** | `camelCase` | `orderId`, `updatedBy` |
| **Private Field** | `camelCase` có dấu gạch dưới `_` ở đầu | `_dbContext`, `_logger` |
| **Public Property** | `PascalCase` | `Id`, `CustomerName`, `TotalPrice` |
| **Constant / Enum** | `PascalCase` | `MaxRetryCount`, `OrderStatus.Pending` |

### 1.2 Quy tắc viết Code
* Luôn sử dụng Dependency Injection (DI) để inject các dịch vụ thông qua Constructor.
* Các hàm xử lý I/O (Database, Network) bắt buộc phải dùng lập trình bất đồng bộ (`async` / `await`).
* Không viết trực tiếp các Magic Numbers/Strings vào logic. Khai báo hằng số (`const`) hoặc cấu hình trong `appsettings.json`.

---

## 2. Quy chuẩn Frontend (Razor Pages, HTML, CSS, JavaScript)

### 2.1 Quy tắc đặt tên trong HTML & Razor Pages
* **HTML Tags**: Viết chữ thường toàn bộ (ví dụ: `<div>`, `<span>`, `<input>`).
* **Thuộc tính ID (`id`)**: 
  * Định dạng bằng **kebab-case** (ví dụ: `order-summary`, `btn-checkout`, `input-voucher-code`).
  * Bắt buộc phải là duy nhất trên toàn bộ trang.
* **Thuộc tính Class (`class`)**:
  * Định dạng bằng **kebab-case** (ví dụ: `product-card`, `btn-primary`, `text-muted`).
* **Thuộc tính Custom (`data-*`)**:
  * Định dạng bằng **kebab-case** (ví dụ: `data-product-id`, `data-order-status`).

### 2.2 Quy tắc đặt tên & sử dụng màu trong CSS (site.css)
* **Quy tắc biến màu (CSS Variables)**:
  * Tất cả các mã màu sử dụng trong CSS **bắt buộc** phải được định nghĩa tại `:root` trong file [site.css](file:///d:/VisualStudio/CafePOS&PDS/CafePOS.Web/wwwroot/css/site.css).
  * Đặt tên biến dạng `kebab-case` (ví dụ: `--pos-bg`, `--status-done`, `--customer-primary`).
  * Tuyệt đối không viết trực tiếp mã màu HEX hoặc RGB vào các CSS selector khác. Phải gọi màu thông qua biến: `color: var(--color-white);`.

### 2.3 Quy tắc đặt tên trong JavaScript
* **Biến & Hàm (Variables & Functions)**:
  * Định dạng bằng **camelCase** (ví dụ: `selectedProductId`, `handleCheckoutClick()`).
* **Hằng số (Constants)**:
  * Định dạng bằng **UPPER_SNAKE_CASE** (ví dụ: `API_BASE_URL`, `MAX_RETRIES`).
* **Lớp (Classes)**:
  * Định dạng bằng **PascalCase** (ví dụ: `OrderTracker`, `SignalRService`).
* **DOM Selector**:
  * Khi select phần tử DOM, đặt tên biến JavaScript tương ứng có dạng camelCase:
    ```javascript
    const btnCheckout = document.getElementById('btn-checkout');
    const orderSummaryContainer = document.querySelector('.order-summary');
    ```

### 2.4 Quy tắc tổ chức tệp tin tĩnh (Static File Structure)
* **Quy tắc tệp tin theo thành phần/trang:**
  * Mỗi trang giao diện hoặc thành phần HTML/Razor Page sẽ đi kèm một file CSS và một file JS (nếu có) **cùng tên** với file HTML đó để dễ dàng quản lý cô lập (Ví dụ: `POS.cshtml` sẽ đi kèm `POS.css` và `POS.js`).
* **Quy tắc cấu trúc thư mục đồng bộ giữa HTML, CSS, và JS:**
  * Cấu trúc thư mục chứa các file giao diện (HTML/Razor Pages) và cấu trúc thư mục chứa các file tĩnh (CSS/JS) tương ứng phải hoàn toàn giống nhau.
  * Đường dẫn tương ứng:
    * Razor Page: `Pages/[Module]/[PageName].cshtml`
    * CSS tương ứng: `wwwroot/css/[Module]/[PageName].css`
    * JS tương ứng: `wwwroot/js/[Module]/[PageName].js`
  * Ví dụ cụ thể:
    * Trang POS: `Pages/POS/Index.cshtml` -> CSS: `wwwroot/css/POS/Index.css`, JS: `wwwroot/js/POS/Index.js`.
* **Quy tắc thành phần dùng chung (Shared/Common Components):**
  * Các thành phần chung xuất hiện ở nhiều trang như Thanh điều hướng (`nav`), Chân trang (`footer`), Thanh bên (`sidebar`) sẽ được tách thành các file CSS/JS riêng biệt (Ví dụ: `nav.css` / `nav.js`, `footer.css` / `footer.js`). Không gộp chung vào style/script của một trang cụ thể.

---

## 3. Tóm tắt nhanh quy tắc đặt tên

```
C# Class/Interface/Method ──> PascalCase (OrderService / IOrderService / GetOrder)
C# Local Var/Parameter    ──> camelCase (orderId / totalAmount)
C# Private Field          ──> _camelCase (_orderRepository)

HTML ID / Class / data    ──> kebab-case (btn-submit / product-card / data-item-id)
CSS Variables             ──> kebab-case (--pos-accent / --status-pending)

JS Variables / Functions  ──> camelCase (customerId / handlePayment())
JS Constants              ──> UPPER_SNAKE_CASE (API_URL)
JS Classes                ──> PascalCase (OrderManager)
```
