# 🌐 API Conventions

## CafePOS & PDS

Tài liệu này quy định các tiêu chuẩn thiết kế REST API cho toàn bộ hệ thống CafePOS & PDS nhằm đảm bảo tính nhất quán giữa Backend, POS Client, PDS, Customer Portal và Admin Dashboard.

---

# 1. General Principles

## MUST DO

* Tuân thủ RESTful API.
* Sử dụng JSON cho request và response.
* Sử dụng camelCase cho tất cả JSON properties.
* API phải có version.
* API phải trả về HTTP Status Code chính xác.
* API phải hỗ trợ validation.
* API phải hỗ trợ authorization.

## MUST NOT

* Không sử dụng động từ trong URL.
* Không tạo endpoint search riêng nếu có thể dùng query string.
* Không trả Entity trực tiếp.
* Không trả Exception StackTrace cho client.
* Không thay đổi API contract khi chưa nâng version.

---

# 2. API Versioning

Tất cả API phải có version.

Đúng:

```http
/api/v1/products
/api/v1/orders
/api/v1/customers
```

Sai:

```http
/products
/orders
```

## Version Rule

Chỉ nâng version khi có breaking changes.

Ví dụ:

```text
v1 -> v2
```

khi:

* Xóa field cũ.
* Đổi cấu trúc response.
* Đổi request bắt buộc.

Không nâng version khi:

* Thêm endpoint mới.
* Thêm field mới trong response.
* Bổ sung filter.

---

# 3. URI Design

## Resources

Sử dụng danh từ số nhiều.

Đúng:

```http
/api/v1/products
/api/v1/orders
/api/v1/customers
```

Sai:

```http
/api/v1/product
/api/v1/getProduct
```

---

## Nested Resources

Đúng:

```http
/api/v1/orders/{orderId}/items
/api/v1/orders/{orderId}/payments
```

Sai:

```http
/api/v1/get-order-items
```

---

## Naming Convention

Sử dụng:

```text
kebab-case
```

Đúng:

```http
/api/v1/inventory-transactions
/api/v1/point-products
```

Sai:

```http
/api/v1/inventoryTransactions
/api/v1/PointProducts
```

---

# 4. HTTP Methods

## GET

Lấy dữ liệu.

```http
GET /api/v1/products
```

---

## POST

Tạo mới dữ liệu.

```http
POST /api/v1/orders
```

---

## PUT

Thay thế toàn bộ tài nguyên.

```http
PUT /api/v1/products/1
```

---

## PATCH

Cập nhật một phần.

```http
PATCH /api/v1/orders/1
```

---

## DELETE

Xóa mềm dữ liệu.

```http
DELETE /api/v1/products/1
```

---

# 5. HTTP Status Codes

## Success

| Code | Meaning    |
| ---- | ---------- |
| 200  | OK         |
| 201  | Created    |
| 204  | No Content |

---

## Client Errors

| Code | Meaning           |
| ---- | ----------------- |
| 400  | Bad Request       |
| 401  | Unauthorized      |
| 403  | Forbidden         |
| 404  | Not Found         |
| 409  | Conflict          |
| 422  | Validation Failed |
| 429  | Too Many Requests |

---

## Server Errors

| Code | Meaning               |
| ---- | --------------------- |
| 500  | Internal Server Error |

---

# 6. Request Convention

## JSON Format

Sử dụng camelCase.

Ví dụ:

```json
{
  "customerId": 1,
  "paymentMethod": "Cash"
}
```

---

## DTO Convention

Không nhận Entity trực tiếp.

Luôn sử dụng:

```text
CreateOrderRequest
UpdateProductRequest
```

---

# 7. Response Convention

## Success Response

Trả object trực tiếp.

Ví dụ:

```json
{
  "id": 1,
  "name": "Matcha Latte",
  "basePrice": 45000
}
```

---

## List Response

```json
[
  {
    "id": 1,
    "name": "Matcha Latte"
  }
]
```

---

## Error Response

Tất cả lỗi phải trả về cùng schema.

```json
{
  "message": "Validation failed.",
  "errors": [
    {
      "field": "phone",
      "message": "Phone number is invalid."
    }
  ]
}
```

---

# 8. Pagination Convention

Các API danh sách lớn bắt buộc hỗ trợ phân trang.

Request:

```http
GET /api/v1/products?page=1&pageSize=20
```

Response:

```json
{
  "items": [],
  "page": 1,
  "pageSize": 20,
  "totalItems": 100,
  "totalPages": 5
}
```

---

## Default Values

```text
page = 1
pageSize = 20
```

Maximum:

```text
pageSize = 100
```

---

# 9. Query Parameters Convention

## Search

```http
GET /api/v1/products?keyword=matcha
```

Không tạo:

```http
/api/v1/products/search
```

---

## Filter

```http
GET /api/v1/orders?status=Preparing
```

---

## Sorting

```http
GET /api/v1/orders?sortBy=createdAt&sortDir=desc
```

---

## Pagination

```http
GET /api/v1/orders?page=1&pageSize=20
```

---

# 10. DateTime Convention

Tất cả DateTime sử dụng UTC.

Format:

```text
2026-06-24T14:30:00Z
```

Không sử dụng local time trong API contract.

---

# 11. Enum Convention

## Database

Enum lưu dưới dạng int.

---

## API

Enum trả về dạng string.

Đúng:

```json
{
  "status": "Preparing"
}
```

Sai:

```json
{
  "status": 2
}
```

---

## Query String

Đúng:

```http
GET /api/v1/orders?status=Preparing
```

Sai:

```http
GET /api/v1/orders?status=2
```

---

# 12. Authentication

Sử dụng JWT Bearer Authentication.

Header:

```http
Authorization: Bearer {token}
```

---

## Unauthorized

```http
401 Unauthorized
```

Khi:

* Chưa đăng nhập
* Token hết hạn
* Token không hợp lệ

---

## Forbidden

```http
403 Forbidden
```

Khi:

* Không đủ quyền

Ví dụ:

Cashier truy cập API quản trị.

---

# 13. Soft Delete Convention

Không sử dụng DELETE vật lý cho dữ liệu nghiệp vụ.

Entity phải có:

```csharp
IsDeleted
DeletedAt
DeletedBy
```

DELETE API:

```http
DELETE /api/v1/products/1
```

Response:

```http
204 No Content
```

---

# 14. Concurrency Convention

Các thực thể quan trọng phải hỗ trợ Optimistic Concurrency.

Ví dụ:

* Product
* Order
* Inventory

Sử dụng:

```csharp
RowVersion
```

Nếu xung đột:

```http
409 Conflict
```

---

# 15. Payment Idempotency

## Critical Rule

Một Order chỉ được có tối đa một Payment thành công.

Endpoint:

```http
POST /api/v1/orders/{id}/payment
```

Trước khi tạo payment:

* Kiểm tra payment thành công đã tồn tại hay chưa.

Nếu tồn tại:

```http
409 Conflict
```

Response:

```json
{
  "message": "Order has already been paid."
}
```

---

## Database Protection

Bắt buộc có Unique Constraint hoặc Unique Index phù hợp để ngăn thanh toán trùng lặp ở tầng dữ liệu.

---

# 16. SignalR Convention

## Hub Events

Sử dụng PascalCase.

Đúng:

```text
OrderCreated
OrderConfirmed
OrderPreparing
OrderCompleted
OrderCancelled
OrderItemStatusChanged
```

Sai:

```text
orderCreated
order_completed
```

---

## Payload

JSON payload phải dùng camelCase.

Ví dụ:

```json
{
  "orderId": 1001,
  "status": "Preparing"
}
```

---

# 17. Security Convention

Không trả về:

* StackTrace
* Connection String
* SQL Query
* Internal Exception Message

Ví dụ sai:

```json
{
  "stackTrace": "..."
}
```

---

# 18. Definition Of Done

Trước khi publish API:

* Endpoint có version.
* DTO được sử dụng đúng.
* Validation đầy đủ.
* Authorization đầy đủ.
* Soft Delete hoạt động đúng.
* Enum trả về string.
* DateTime sử dụng UTC.
* Status Code chính xác.
* Payment Idempotency được đảm bảo.
* SignalR event tuân thủ naming convention.
* Không có dữ liệu nhạy cảm trong response.
* Swagger/OpenAPI được cập nhật.

```