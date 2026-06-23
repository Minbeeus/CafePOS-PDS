# 📜 CafePOS & PDS - AI Development Rules

## Purpose

Tài liệu này định nghĩa cách mọi AI Coding Agent
(Cursor, Claude Code, Roo Code, Cline, Windsurf, GitHub Copilot, ChatGPT...)
phải làm việc trong dự án CafePOS & PDS.

AI phải coi tài liệu này là nguồn hướng dẫn chính trước khi tạo hoặc chỉnh sửa mã nguồn.

---

# 1. Required Reading Order

Trước khi thực hiện bất kỳ thay đổi nào, AI phải đọc các tài liệu sau theo đúng thứ tự:

1. docs/conventions/business_rules.md
2. docs/conventions/api_conventions.md
3. docs/conventions/coding_conventions.md
4. docs/conventions/ui_guidelines.md

---

# 2. Rule Priority

Nếu có xung đột giữa các tài liệu:

Business Rules
↓
API Conventions
↓
Coding Conventions
↓
UI Guidelines

Tài liệu ở cấp cao hơn luôn được ưu tiên.

Ví dụ:

- Business Rules yêu cầu không cho phép thanh toán trùng.
- API Conventions đề xuất endpoint khác.

=> Business Rules thắng.

---

# 3. Before Writing Code

AI phải:

- Đọc code liên quan trước khi sửa.
- Hiểu luồng nghiệp vụ hiện tại.
- Tìm component hoặc service đã tồn tại.
- Tái sử dụng code hiện có nếu có thể.
- Kiểm tra các interface hiện tại.
- Kiểm tra DTO hiện tại.
- Kiểm tra API contract hiện tại.

Không được viết code khi chưa hiểu luồng hiện tại.

---

# 4. General Principles

## MUST DO

- Tuân thủ kiến trúc hiện có.
- Tuân thủ Dependency Injection.
- Tuân thủ SOLID.
- Tuân thủ Separation Of Concerns.
- Sử dụng async/await cho I/O.
- Sử dụng ILogger cho logging.
- Tạo mã nguồn dễ đọc và dễ bảo trì.

## MUST NOT

- Không tạo kiến trúc mới nếu chưa được yêu cầu.
- Không tạo framework riêng.
- Không thêm package mới nếu chưa được yêu cầu.
- Không đổi API contract hiện có.
- Không đổi database schema ngoài phạm vi task.
- Không hardcode configuration.
- Không hardcode connection string.
- Không hardcode secret.
- Không sử dụng Console.WriteLine().

---

# 5. Architecture Rules

Controller

- Nhận request
- Validate cơ bản
- Gọi service
- Trả response

Không chứa business logic.

---

Service

- Chứa business logic
- Validation nghiệp vụ
- Transaction handling

---

Repository

- CRUD
- Query dữ liệu

Không chứa business logic.

---

# 6. UI Rules

Trước khi tạo giao diện:

AI phải:

1. Đọc ui_guidelines.md
2. Đọc site.css
3. Tái sử dụng CSS variables hiện có
4. Tái sử dụng component hiện có

---

Không được:

- Hardcode màu sắc
- Hardcode spacing
- Dùng inline style nếu không cần thiết
- Tạo theme mới

---

Bắt buộc:

- Responsive
- Loading State
- Empty State
- Error State
- Focus State

---

# 7. API Rules

Mọi API phải tuân thủ:

docs/conventions/api_conventions.md

Đặc biệt:

- API versioning
- DTO usage
- Enum serialization
- Status code consistency
- Authentication
- Authorization

---

# 8. Business Rules

Mọi thay đổi nghiệp vụ phải tuân thủ:

docs/conventions/business_rules.md

Đặc biệt:

- Order Lifecycle
- Payment Rules
- Inventory Rules
- Loyalty Rules
- Promotion Rules
- Role Permissions

AI không được tự ý thay đổi các quy tắc này.

---

# 9. Critical Rules

## Payment Idempotency

Một Order chỉ được có tối đa:

1 Successful Payment

Trước khi tạo Payment:

- Kiểm tra Payment Completed đã tồn tại chưa.

Nếu tồn tại:

409 Conflict

---

## Soft Delete

Không xóa vật lý dữ liệu nghiệp vụ.

Sử dụng:

- IsDeleted
- DeletedAt
- DeletedBy

---

## Enum Serialization

Database:

int

API:

string

---

## Logging

Không sử dụng:

Console.WriteLine()

Sử dụng:

ILogger

---

## SaveChanges

Luôn sử dụng:

await _dbContext.SaveChangesAsync();

Không sử dụng:

SaveChangeAsync()

---

# 10. Migration Rules

Khi thay đổi Entity:

1. Tạo migration mới.
2. Kiểm tra migration script.
3. Không chỉnh sửa migration đã deploy.

---

# 11. Testing Rules

Mọi thay đổi business logic phải có test.

Ưu tiên:

- Unit Test
- Integration Test

Không merge khi test thất bại.

---

# 12. Code Review Checklist

Trước khi hoàn thành task:

AI phải tự kiểm tra:

- Build thành công
- Không compile error
- Không warning nghiêm trọng
- Không dead code
- Không unused dependency
- Không Console.WriteLine()
- Không hardcoded secret
- Không phá vỡ API contract
- Không phá vỡ Business Rules
- Không phá vỡ Soft Delete
- Không phá vỡ Payment Rules

---

# 13. Required Output Format

Sau khi hoàn thành task:

AI phải tóm tắt:

## Summary

- Mục tiêu thay đổi
- Cách triển khai

## Files Changed

- Danh sách file đã sửa

## Database Changes

- Migration mới (nếu có)

## API Impact

- Endpoint bị ảnh hưởng

## Risks

- Các rủi ro tiềm ẩn

## Manual Testing

- Các bước kiểm thử đề xuất

---

# 14. Definition Of Done

Một task chỉ được xem là hoàn thành khi:

✓ Build thành công

✓ Test thành công

✓ Tuân thủ Business Rules

✓ Tuân thủ API Conventions

✓ Tuân thủ Coding Conventions

✓ Tuân thủ UI Guidelines

✓ Không phá vỡ chức năng hiện có

✓ Có báo cáo thay đổi đầy đủ

# 15. AI Decision Rules

Before creating new code:

1. Reuse existing code if possible.
2. Extend existing modules before creating new modules.
3. Modify existing pages before creating new pages.
4. Modify existing services before creating new services.
5. Prefer consistency over creativity.
6. Prefer maintainability over clever solutions.
7. Prefer project conventions over personal preferences.