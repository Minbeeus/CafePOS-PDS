# 📜 CafePOS & PDS - AI Development Rules

## Project Overview

Hệ thống Quản lý Bán hàng Quán Cà phê / Trà sữa (CafePOS & PDS)

Mục tiêu của tài liệu này là đảm bảo mọi AI Coding Agent (Cursor, Claude Code, Cline, Roo Code, Windsurf...) tạo ra mã nguồn nhất quán, an toàn và tuân thủ kiến trúc dự án.

---

# 1. General Principles

## MUST DO

* Luôn đọc code liên quan trước khi sửa đổi.
* Ưu tiên tái sử dụng code hiện có.
* Tuân thủ kiến trúc hiện tại của dự án.
* Giữ nguyên API contract hiện có.
* Sử dụng Dependency Injection cho mọi service.
* Sử dụng async/await cho các thao tác I/O.
* Sử dụng ILogger để ghi log.

## MUST NOT

* Không tạo kiến trúc mới khi chưa được yêu cầu.
* Không thêm package/dependency mới nếu chưa được phép.
* Không đổi tên API endpoint đang hoạt động.
* Không thay đổi schema database ngoài phạm vi yêu cầu.
* Không sử dụng Console.WriteLine().
* Không hardcode configuration, connection string hoặc secret.

---

# 2. Git Workflow

## Branch Naming

### Feature

feature/<feature-name>

Ví dụ:

feature/vietqr-payment

feature/order-management

### Bug Fix

bugfix/<bug-name>

Ví dụ:

bugfix/payment-timeout

bugfix/order-duplicate

### Hot Fix

hotfix/<issue-name>

Ví dụ:

hotfix/payment-error

---

## Commit Convention

Sử dụng Conventional Commits:

feat: add vietqr payment

fix: prevent duplicate payment

refactor: extract payment service

docs: update api convention

chore: update packages

test: add payment service tests

---

# 3. Coding Conventions

## C# Naming Convention

### PascalCase

* Class
* Interface
* Property
* Method
* Enum

Ví dụ:

OrderService

PaymentStatus

GetOrderByIdAsync()

### Interface

Bắt đầu bằng chữ I

Ví dụ:

IOrderService

IPaymentRepository

### camelCase

Biến cục bộ và tham số

Ví dụ:

orderId

paymentRequest

### _camelCase

Private fields

Ví dụ:

_dbContext

_logger

_orderRepository

### Async Method

Bắt buộc có hậu tố Async

Ví dụ:

CreateOrderAsync()

SavePaymentAsync()

---

# 4. Architecture Rules

## Controller

Controller chỉ:

* Nhận request
* Validate request cơ bản
* Gọi service
* Trả response

Không chứa business logic.

---

## Service Layer

Service chứa:

* Business logic
* Validation nghiệp vụ
* Transaction handling

---

## Repository Layer

Repository chỉ:

* Truy vấn dữ liệu
* CRUD

Không chứa business logic.

---

## DTO Usage

Không trả Entity trực tiếp cho API.

Luôn sử dụng:

* Request DTO
* Response DTO

Ví dụ:

CreateOrderRequest

OrderResponse

---

# 5. Entity Framework Core Rules

## Save Changes

Luôn sử dụng:

```csharp
await _dbContext.SaveChangesAsync();
```

Không được viết:

```csharp
SaveChangeAsync();
```

---

## Query Optimization

Ưu tiên:

```csharp
.AsNoTracking()
```

cho các API chỉ đọc dữ liệu.

---

## Include

Chỉ Include các navigation property thực sự cần thiết.

Tránh eager loading quá mức.

---

## Transaction

Các nghiệp vụ thanh toán hoặc tạo đơn hàng phải được thực hiện trong transaction.

---

# 6. Logging Rules

Không sử dụng:

```csharp
Console.WriteLine();
```

Bắt buộc:

```csharp
_logger.LogInformation();
_logger.LogWarning();
_logger.LogError();
```

---

# 7. Enum Rules

## Database

Lưu dưới dạng int.

Ví dụ:

PaymentStatus = 2

---

## API Response

Serialize thành string.

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

# 8. Soft Delete Rules

Không sử dụng DELETE vật lý cho dữ liệu nghiệp vụ.

Bắt buộc dùng:

```csharp
IsDeleted
DeletedAt
DeletedBy
```

DELETE API chỉ đánh dấu xóa mềm.

Response:

204 No Content

---

# 9. Payment Idempotency

## Critical Business Rule

Một Order chỉ được có tối đa một Payment thành công.

### API Validation

Trước khi tạo payment:

* Kiểm tra payment thành công đã tồn tại chưa.
* Nếu tồn tại:

```http
409 Conflict
```

### Database Constraint

Bắt buộc có Unique Constraint:

```sql
UNIQUE(OrderId)
```

hoặc unique index tương đương cho payment thành công.

---

# 10. Migration Rules

Khi thay đổi Entity:

1. Tạo Migration.
2. Kiểm tra migration script.
3. Không chỉnh sửa migration cũ đã deploy.

Chỉ tạo migration mới.

---

# 11. Testing Rules

Mọi thay đổi business logic phải có test.

Ưu tiên:

* Unit Test cho Service
* Integration Test cho API

Không merge code nếu test thất bại.

---

# 12. Before Completing Any Task

AI phải kiểm tra:

* Build thành công.
* Không có compile error.
* Không có warning nghiêm trọng.
* API contract không bị thay đổi ngoài yêu cầu.
* Migration hợp lệ.
* Business rule thanh toán không bị ảnh hưởng.
* Soft delete vẫn hoạt động đúng.

Sau khi hoàn thành phải tóm tắt:

* File đã thay đổi.
* Mục đích thay đổi.
* Rủi ro có thể phát sinh.
* Migration được thêm mới (nếu có).
* API bị ảnh hưởng (nếu có).

```
```
