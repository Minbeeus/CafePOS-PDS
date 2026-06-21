# 🌐 Quy chuẩn Thiết kế API (API Conventions)
### Hệ thống Quản lý Bán hàng Quán Cà phê / Trà sữa (CafePOS & PDS)

Tài liệu này quy định các chuẩn mực thiết kế RESTful API để đảm bảo sự đồng bộ và nhất quán giữa hệ thống Backend và các ứng dụng Client (POS, PDS, Customer Website, Admin Dashboard).

---

## 1. Cấu trúc URL & Phiên bản (URI Design & API Versioning)

* **Sử dụng danh từ số nhiều** cho các tài nguyên (Resources):
  * Đúng: `/api/v1/products`, `/api/v1/orders`
  * Sai: `/api/v1/getProduct`, `/api/v1/order`
* **Sử dụng chữ thường và phân tách bằng dấu gạch ngang (kebab-case)**:
  * Đúng: `/api/v1/point-products`, `/api/v1/inventory-checks`
  * Sai: `/api/v1/pointProducts`, `/api/v1/point_products`
* **Cấp bậc phân cấp tài nguyên rõ ràng**:
  * Đúng: `/api/v1/orders/{orderId}/items/{itemId}`
  * Sai: `/api/v1/orders/get-item?orderId=1&itemId=2`
* **Quy tắc phiên bản (API Versioning)**:
  * Bắt buộc sử dụng tiền tố phiên bản `/api/v{N}/` trong đường dẫn URL.
  * Phiên bản hiện tại là `/api/v1/`.
  * **Nguyên tắc nâng phiên bản**: 
    * Chỉ nâng lên `v2` khi có các thay đổi phá vỡ tương thích (breaking changes) không thể duy trì ở `v1` (ví dụ: thay đổi hoàn toàn cấu trúc payload request bắt buộc, xóa trường dữ liệu quan trọng).
    * Các thay đổi mở rộng (non-breaking changes) như thêm trường mới trong response, thêm endpoint mới vẫn giữ nguyên ở `v1`.

---

## 2. Phương thức HTTP & Quy tắc Soft Delete (HTTP Methods & Soft Delete)

* `GET`: Lấy thông tin tài nguyên.
* `POST`: Tạo mới tài nguyên.
* `PUT`: Cập nhật toàn bộ hoặc thay thế tài nguyên.
* `PATCH`: Cập nhật một phần tài nguyên.
* `DELETE`: Xóa tài nguyên.
* **Quy chuẩn Soft Delete (Xóa mềm)**:
  * Trong cơ sở dữ liệu, không thực hiện lệnh DELETE vật lý xóa bản ghi đối với các thực thể nghiệp vụ. Thay vào đó, sử dụng các trường trạng thái:
    * `IsDeleted` (bool): Đánh dấu bản ghi đã bị xóa.
    * `DeletedAt` (datetime?): Lưu thời điểm xóa.
    * `DeletedBy` (int?): Lưu StaffId người thực hiện xóa.
  * Khi client gọi endpoint xóa: `DELETE /api/v1/products/5`
  * API phải xử lý cập nhật trạng thái xóa mềm trong cơ sở dữ liệu và trả về mã trạng thái **`204 No Content`** nếu thành công.

---

## 3. Mã phản hồi HTTP (HTTP Status Codes)

* **200 OK**: Request thành công và trả về kết quả (thường dùng cho GET, PUT, PATCH).
* **201 Created**: Tạo mới tài nguyên thành công (trả về đối tượng vừa tạo kèm thông tin định danh mới).
* **204 No Content**: Thực hiện thành công nhưng không cần trả về dữ liệu trong body (dùng cho DELETE xóa mềm hoặc thao tác không cần trả kết quả).
* **400 Bad Request**: Lỗi dữ liệu đầu vào không hợp lệ (Validation Error).
* **401 Unauthorized**: Chưa xác thực (chưa đăng nhập hoặc token hết hạn/không hợp lệ).
* **403 Forbidden**: Đã xác thực nhưng không đủ quyền truy cập (ví dụ: Cashier truy cập API của Manager).
* **404 Not Found**: Không tìm thấy tài nguyên yêu cầu.
* **409 Conflict**: Xung đột tài nguyên (ví dụ: lỗi trùng lặp dữ liệu, hoặc xung đột Concurrency).
* **429 Too Many Requests**: Rate limit vượt ngưỡng cho phép.
* **500 Internal Server Error**: Lỗi hệ thống phát sinh từ phía Server.

---

## 4. Định dạng dữ liệu phản hồi (Response Format)

Tất cả payload phản hồi và yêu cầu phải sử dụng định dạng dữ liệu **JSON** với các thuộc tính (keys) viết theo chuẩn **camelCase**.

### 4.1 Quy chuẩn phản hồi thành công (Success Response Convention)
Hệ thống thống nhất **trả về đối tượng hoặc mảng JSON trực tiếp** mà không bọc qua các class wrapper generic (như `{ success: true, data: ... }`) để tối giản hóa code client và chuẩn hóa RESTful:
* Ví dụ lấy chi tiết sản phẩm thành công (`GET /api/v1/products/1`):
  ```json
  {
    "id": 1,
    "name": "Trà Sữa Truyền Thống",
    "basePrice": 35000,
    "status": "Active"
  }
  ```

### 4.2 Quy chuẩn phản hồi lỗi (Error Response Schema)
Khi gặp lỗi (ví dụ: 400 Bad Request), API bắt buộc trả về cấu trúc lỗi nhất quán để Client dễ dàng bóc tách hiển thị:
```json
{
  "message": "Thông tin đầu vào không hợp lệ hoặc thiếu thông tin bắt buộc.",
  "errors": [
    {
      "field": "phone",
      "message": "Số điện thoại phải có định dạng 10 chữ số."
    }
  ]
}
```

---

## 5. Quy chuẩn phân trang & Truy vấn (Pagination & Query Parameters)

### 5.1 Quy chuẩn phân trang (Pagination Convention)
Các API trả về danh sách lớn bắt buộc phải hỗ trợ phân trang để đảm bảo hiệu năng hệ thống. Dữ liệu phân trang được truyền qua Query String và response trả về theo cấu trúc chuẩn sau:
* URL mẫu: `GET /api/v1/products?page=1&pageSize=20`
* Cấu trúc Response phân trang:
  ```json
  {
    "items": [
      {
        "id": 1,
        "name": "Trà Sữa Truyền Thống",
        "basePrice": 35000
      }
    ],
    "page": 1,
    "pageSize": 20,
    "totalItems": 150,
    "totalPages": 8
  }
  ```
* **Phạm vi áp dụng bắt buộc**:
  * `Products` (Sản phẩm)
  * `Orders` (Đơn hàng)
  * `Customers` (Khách hàng)
  * `Audit Logs` (Nhật ký hệ thống)
  * `Ingredient Transactions` (Giao dịch kho nguyên liệu)

### 5.2 Quy chuẩn tham số truy vấn (Query Parameters Convention)
Tất cả các bộ lọc, tìm kiếm, sắp xếp và phân trang phải được truyền dưới dạng **Query String (Query Parameters)** thay vì truyền trong body request hoặc tạo endpoint riêng biệt:
* **Bộ lọc (Filter)**: `GET /api/v1/orders?status=Preparing` hoặc `GET /api/v1/products?categoryId=1`
* **Tìm kiếm (Search / Standard Search Endpoint)**: 
  * Sử dụng tham số `keyword` để tìm kiếm: `GET /api/v1/customers?keyword=minh` hoặc `GET /api/v1/products?keyword=milk`
  * **Tuyệt đối không tạo endpoint dạng `/search` riêng** (ví dụ: `/products/search`), ngoại trừ trường hợp nghiệp vụ tìm kiếm cực kỳ đặc thù hoặc phức tạp.
* **Sắp xếp (Sort)**: Sử dụng cặp tham số `sortBy` (trường cần sắp xếp) và `sortDir` (`asc` hoặc `desc` để chỉ hướng sắp xếp).
  * Ví dụ: `GET /api/v1/orders?sortBy=createdAt&sortDir=desc`
* **Phân trang (Paging)**: Sử dụng cặp tham số `page` và `pageSize`.

---

## 6. Quy tắc bất biến trong thanh toán (Payment Idempotency Rule)

Một lỗi rất phổ biến và nghiêm trọng trong hệ thống POS bán lẻ là việc nhân viên hoặc hệ thống kích hoạt thanh toán nhiều lần cho cùng một đơn hàng do đơ mạng hoặc thao tác click đúp, dẫn đến tạo các bản ghi thanh toán trùng lặp.
* **Quy tắc (Rule)**: **Một Order chỉ có tối đa một giao dịch Payment thành công.**
* **Xử lý Backend**:
  * Khi nhận request thanh toán `POST /api/v1/orders/{id}/payment`:
  * Hệ thống phải kiểm tra xem đơn hàng đã có bản ghi `Payment` nào thành công chưa.
  * Nếu đã có `Payment` thành công cho `OrderId` đó, API lập tức từ chối và trả về lỗi **`409 Conflict`** kèm thông báo `"Đơn hàng đã được thanh toán trước đó"`.
  * Sử dụng cơ chế khóa (Locking) hoặc Database Unique Constraint trên cột `OrderId` trong bảng `Payments` (chỉ cho phép tối đa 1 bản ghi thanh toán không âm cho mỗi đơn hàng) để đảm bảo tính toàn vẹn dữ liệu.

---

## 7. SignalR Hub Events (Real-time Communication)

* Đặt tên cho các Event (sự kiện) phát đi từ server bằng dạng **PascalCase** và mô tả rõ trạng thái:
  * Đúng: `OrderCreated`, `OrderConfirmed`, `OrderCompleted`, `OrderItemStatusChanged`
  * Payload truyền qua SignalR cũng phải áp dụng quy chuẩn đặt tên camelCase cho các thuộc tính JSON.
