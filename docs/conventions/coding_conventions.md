# 💻 Coding Conventions

## CafePOS & PDS

Tài liệu này quy định các chuẩn lập trình, tổ chức mã nguồn và kiến trúc cho toàn bộ hệ thống CafePOS & PDS.

---

# 1. General Principles

## MUST DO

* Luôn đọc code hiện có trước khi sửa đổi.
* Ưu tiên tái sử dụng code hiện có.
* Tuân thủ kiến trúc hiện tại của dự án.
* Tuân thủ Separation of Concerns.
* Tuân thủ Dependency Injection.
* Viết code dễ đọc hơn là code ngắn.

## MUST NOT

* Không tạo kiến trúc mới nếu chưa được yêu cầu.
* Không thêm package mới nếu chưa được chấp thuận.
* Không hardcode dữ liệu nghiệp vụ.
* Không hardcode connection string.
* Không hardcode secret hoặc API key.
* Không copy-paste logic giữa các module.

---

# 2. Backend Naming Convention (C#)

## Classes

Sử dụng PascalCase.

```csharp
OrderService
ProductController
InventoryRepository
```

---

## Interfaces

Bắt đầu bằng chữ I.

```csharp
IOrderService
IProductRepository
```

---

## Methods

Sử dụng PascalCase.

```csharp
GetOrderById()
CalculateTotal()
```

---

## Async Methods

Bắt buộc có hậu tố Async.

```csharp
GetOrderByIdAsync()
CreateOrderAsync()
SaveChangesAsync()
```

Không được:

```csharp
SaveChangeAsync()
```

---

## Variables & Parameters

Sử dụng camelCase.

```csharp
orderId
customerName
totalAmount
```

---

## Private Fields

Sử dụng _camelCase.

```csharp
_dbContext
_logger
_orderRepository
```

---

## Constants

Sử dụng PascalCase.

```csharp
MaxRetryCount
DefaultPageSize
```

---

## Enums

Sử dụng PascalCase.

```csharp
OrderStatus
PaymentMethod
```

---

# 3. Nullable Reference Types

Tất cả project phải bật:

```xml
<Nullable>enable</Nullable>
```

Ví dụ:

```csharp
string? phoneNumber;
Customer? customer;
```

Không sử dụng toán tử ! nếu không thực sự cần thiết.

---

# 4. Layer Responsibilities

## Controller

Controller chỉ được:

* Nhận request
* Validate cơ bản
* Gọi service
* Trả response

Controller không được:

* Chứa business logic
* Truy cập DbContext trực tiếp
* Gọi Repository trực tiếp

---

## Service

Service chịu trách nhiệm:

* Business logic
* Validation nghiệp vụ
* Transaction
* Gọi Repository

Service không được:

* Chứa code giao diện
* Chứa SQL raw không cần thiết

---

## Repository

Repository chịu trách nhiệm:

* CRUD
* Query dữ liệu

Repository không được:

* Chứa business logic
* Chứa validation nghiệp vụ

---

# 5. DTO Convention

Không trả Entity trực tiếp cho API.

Luôn sử dụng DTO.

Ví dụ:

```csharp
CreateOrderRequest
CreateOrderResponse

UpdateProductRequest
UpdateProductResponse

GetOrderDetailResponse
```

Không sử dụng:

```csharp
OrderDto
ProductDto
```

trừ khi thực sự cần thiết.

---

# 6. Validation Convention

Ưu tiên FluentValidation.

Validation nghiệp vụ phải nằm ở Application Layer.

Không validate nghiệp vụ trong Controller.

Ví dụ:

* Số lượng phải lớn hơn 0
* Không được thanh toán đơn đã thanh toán

---

# 7. Mapping Convention

Sử dụng AutoMapper hoặc Mapper riêng.

Luôn map:

```text
Request DTO
↓
Entity

Entity
↓
Response DTO
```

Không trả Entity trực tiếp ra API.

---

# 8. Entity Framework Core Convention

## Save Changes

Luôn sử dụng:

```csharp
await _dbContext.SaveChangesAsync();
```

---

## Read Only Queries

Luôn dùng:

```csharp
.AsNoTracking()
```

khi không cần cập nhật dữ liệu.

---

## Includes

Chỉ Include dữ liệu thực sự cần thiết.

Không eager load quá mức.

---

## N+1 Query

Không query trong vòng lặp.

Sai:

```csharp
foreach(var order in orders)
{
    var items = await _dbContext.OrderItems
        .Where(x => x.OrderId == order.Id)
        .ToListAsync();
}
```

---

## Transactions

Các nghiệp vụ sau bắt buộc transaction:

* Thanh toán
* Tạo đơn hàng
* Cập nhật tồn kho

---

# 9. Soft Delete Convention

Không sử dụng DELETE vật lý.

Các entity nghiệp vụ phải có:

```csharp
IsDeleted
DeletedAt
DeletedBy
```

Khi xóa:

```csharp
entity.IsDeleted = true;
```

---

# 10. Logging Convention

Bắt buộc sử dụng ILogger.

Ví dụ:

```csharp
_logger.LogInformation();
_logger.LogWarning();
_logger.LogError();
```

Không sử dụng:

```csharp
Console.WriteLine();
```

---

# 11. Enum Convention

## Database

Enum lưu dưới dạng int.

Ví dụ:

```csharp
Preparing = 2
```

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

# 12. Exception Handling

Không sử dụng try/catch rỗng.

Sai:

```csharp
try
{
}
catch
{
}
```

Đúng:

```csharp
try
{
}
catch(Exception ex)
{
    _logger.LogError(ex, "Error");
    throw;
}
```

---

# 13. Frontend Convention

## HTML

* Sử dụng thẻ chữ thường.
* Sử dụng semantic HTML.

Ví dụ:

```html
<header>
<nav>
<main>
<section>
```

---

## CSS

### Naming

Sử dụng kebab-case.

```css
product-card
checkout-button
```

### Colors

Chỉ sử dụng CSS Variables.

Đúng:

```css
color: var(--matcha-primary);
```

Sai:

```css
color: #4c7031;
```

---

## JavaScript

### Variables

camelCase

```javascript
selectedProductId
```

### Constants

UPPER_SNAKE_CASE

```javascript
API_BASE_URL
```

### Classes

PascalCase

```javascript
OrderTracker
```

---

# 14. Shared JavaScript Convention

Cho phép tạo các module dùng chung:

```text
wwwroot/js/shared/

api-client.js
signalr.service.js
toast.service.js
modal.service.js
```

Không lặp lại logic gọi API giữa các trang.

---

# 15. Solution Structure

```text
CafePOS.Domain
CafePOS.Application
CafePOS.Infrastructure
CafePOS.API
CafePOS.Web
```

## Domain

* Entities
* Enums
* Domain Models

## Application

* Services
* DTOs
* Validators
* Interfaces

## Infrastructure

* EF Core
* Repositories
* External Services
* Migrations

## API

* Controllers
* SignalR Hubs
* Middleware

## Web

* MVC Controllers
* Razor Views
* CSS
* JavaScript

```

---

# 16. Testing Convention

## Unit Test

Bắt buộc cho:

- Service
- Business Logic

## Integration Test

Bắt buộc cho:

- API
- Payment Flow
- Inventory Flow

---

# 17. Definition of Done

Trước khi hoàn thành bất kỳ task nào:

- Build thành công.
- Không có compile error.
- Không có warning nghiêm trọng.
- Không có code chết.
- Không có Console.WriteLine().
- Không có hardcoded secret.
- DTO được cập nhật đầy đủ.
- Migration được tạo nếu schema thay đổi.
- Business Rules vẫn được đảm bảo.
- Payment Idempotency không bị phá vỡ.
- Soft Delete vẫn hoạt động đúng.
```