# 🌐 Quy chuẩn Thiết kế API (API Conventions)
### Hệ thống Quản lý Bán hàng Quán Cà phê / Trà sữa (CafePOS & PDS)

Tài liệu này quy định các chuẩn mực thiết kế RESTful API để đảm bảo sự đồng bộ giữa hệ thống Backend và các ứng dụng Client (POS, PDS, Customer Website, Admin Dashboard).

---

## 1. Cấu trúc URL (URI Design)

* **Sử dụng danh từ số nhiều** cho các tài nguyên (Resources):
  * Đúng: `/api/v1/products`, `/api/v1/orders`
  * Sai: `/api/v1/getProduct`, `/api/v1/order`
* **Sử dụng chữ thường và phân tách bằng dấu gạch ngang (kebab-case)**:
  * Đúng: `/api/v1/point-products`, `/api/v1/inventory-checks`
  * Sai: `/api/v1/pointProducts`, `/api/v1/point_products`
* **Cấp bậc phân cấp tài nguyên rõ ràng**:
  * Đúng: `/api/v1/orders/{orderId}/items/{itemId}`
  * Sai: `/api/v1/orders/get-item?orderId=1&itemId=2`

---

## 2. Phương thức HTTP (HTTP Methods)

* `GET`: Lấy thông tin tài nguyên (không làm thay đổi dữ liệu trên server).
* `POST`: Tạo mới tài nguyên.
* `PUT`: Cập nhật toàn bộ hoặc thay thế tài nguyên.
* `PATCH`: Cập nhật một phần tài nguyên (ví dụ: cập nhật riêng trạng thái đơn hàng).
* `DELETE`: Xóa tài nguyên (trong dự án sử dụng Soft Delete - ẩn bản ghi).

---

## 3. Mã phản hồi HTTP (HTTP Status Codes)

* **200 OK**: Request thành công và trả về kết quả.
* **201 Created**: Tạo mới tài nguyên thành công (thường trả về thông tin tài nguyên vừa tạo).
* **204 No Content**: Thực hiện thành công nhưng không trả về dữ liệu (ví dụ: DELETE thành công).
* **400 Bad Request**: Lỗi dữ liệu đầu vào không hợp lệ (Validation Error).
* **401 Unauthorized**: Chưa xác thực (chưa đăng nhập hoặc token hết hạn/không hợp lệ).
* **403 Forbidden**: Đã xác thực nhưng không đủ quyền truy cập (ví dụ: Cashier cố gắng truy cập API của Manager).
* **404 Not Found**: Không tìm thấy tài nguyên yêu cầu.
* **500 Internal Server Error**: Lỗi hệ thống phát sinh từ phía Server.

---

## 4. Định dạng dữ liệu (Payload Format)

* Luôn sử dụng định dạng dữ liệu **JSON**.
* Định dạng các thuộc tính (keys) dưới dạng **camelCase**:
  * Đúng: `orderId`, `customerName`, `totalPrice`
  * Sai: `OrderId`, `customer_name`

### 4.1 Cấu trúc phản hồi lỗi (Error Response Schema)
Khi gặp lỗi (ví dụ: 400 Bad Request), API phải trả về một cấu trúc lỗi nhất quán:

```json
{
  "success": false,
  "message": "Thông tin đầu vào không hợp lệ hoặc thiếu thông tin bắt buộc.",
  "errors": [
    {
      "field": "phone",
      "message": "Số điện thoại phải có định dạng 10 chữ số."
    },
    {
      "field": "items",
      "message": "Danh sách món đặt không được để trống."
    }
  ]
}
```

---

## 5. SignalR Hub Events (Real-time Communication)

* Đặt tên cho các Event (sự kiện) phát đi từ server bằng dạng **PascalCase** và mô tả rõ trạng thái:
  * Đúng: `OrderCreated`, `OnlinePendingOrder`, `OrderCompleted`
  * Payload truyền qua SignalR cũng phải áp dụng quy chuẩn đặt tên camelCase cho các thuộc tính JSON.
