# 🎨 UI Guidelines

## CafePOS & PDS

Tài liệu này quy định toàn bộ tiêu chuẩn giao diện cho hệ thống CafePOS & PDS nhằm đảm bảo trải nghiệm người dùng nhất quán giữa POS, PDS, Customer Portal và Admin Dashboard.

---

# 1. Design Philosophy

## Brand Identity

CafePOS & PDS là hệ thống dành cho:

* Quán Cafe
* Quán Trà sữa
* Quán Matcha
* Chuỗi F&B hiện đại

Thương hiệu hướng tới:

* Premium
* Warm
* Natural
* Modern
* Clean
* Fast

---

## Design Keywords

Luôn ưu tiên:

* Simple
* Clean
* Comfortable
* Readable
* Professional

---

## Avoid

Không sử dụng:

* Neon colors
* Pure black (#000000)
* Pure white (#FFFFFF) trên diện tích lớn
* Excessive gradients
* Excessive animations
* Excessive shadows
* Glassmorphism lạm dụng
* Overly rounded UI

---

# 2. Design Tokens

## Mandatory Rule

Chỉ sử dụng màu được định nghĩa trong:

```text
wwwroot/css/site.css
```

Không hardcode:

```css
#4c7031
rgb(...)
hsl(...)
```

Đúng:

```css
background-color: var(--matcha-primary);
color: var(--coffee-dark);
```

---

## Brand Colors

Primary Accent:

```css
--matcha-primary
```

Secondary Accent:

```css
--coffee-medium
```

Background:

```css
--bg-milk
--bg-cream
```

Text:

```css
--text-dark
--text-muted
```

---

# 3. Layout System

## Desktop Container

Maximum width:

```text
1400px
```

Content should remain centered.

---

## Grid System

Ưu tiên:

```text
12-column grid
```

hoặc Bootstrap Grid.

---

## Section Spacing

Cho phép:

```text
4px
8px
16px
24px
32px
48px
64px
```

Không sử dụng spacing ngẫu nhiên.

Sai:

```css
margin: 13px;
padding: 27px;
```

---

# 4. Typography

## Font Priority

Ưu tiên:

```text
Inter
Segoe UI
Roboto
sans-serif
```

---

## Font Weight

Heading:

```text
600-700
```

Body:

```text
400-500
```

---

## Text Hierarchy

Page Title:

```text
32px
```

Section Title:

```text
24px
```

Card Title:

```text
18px
```

Body:

```text
14-16px
```

Caption:

```text
12px
```

---

# 5. Buttons

## Primary Action

Sử dụng:

```css
.btn-matcha
```

Ví dụ:

* Thanh toán
* Xác nhận
* Lưu

---

## Secondary Action

Sử dụng:

```css
.btn-coffee
```

Ví dụ:

* Chỉnh sửa
* Quay lại

---

## Outline Action

Sử dụng:

```css
.btn-outline-matcha
.btn-outline-coffee
```

---

## Rules

Primary CTA phải là nút nổi bật nhất màn hình.

Không có nhiều hơn 1 Primary CTA trong cùng khu vực.

---

# 6. Forms

## Input Rules

* Label luôn hiển thị.
* Không dùng placeholder thay cho label.
* Validation message phải rõ ràng.

Ví dụ:

Đúng:

```text
Phone number is required.
```

Sai:

```text
Invalid value.
```

---

## Form Layout

Ưu tiên:

```text
1 cột trên mobile
2 cột trên desktop
```

---

# 7. Cards

Cards phải sử dụng:

```css
.card-custom
```

hoặc:

```css
.card-custom-dark
```

---

## Card Content

Thứ tự:

1. Title
2. Description
3. Actions

---

# 8. Tables

## Rules

* Có hover state.
* Có sorting nếu phù hợp.
* Có pagination.
* Có empty state.

---

## Empty State

Không hiển thị bảng rỗng.

Ví dụ:

```text
No products found.
```

---

# 9. Responsive Design

## Mobile First

Tất cả giao diện phải responsive.

---

## Breakpoints

Mobile:

```text
<768px
```

Tablet:

```text
768px - 1024px
```

Desktop:

```text
>1024px
```

---

# 10. Accessibility

## Contrast

Tất cả text phải đạt tối thiểu:

```text
WCAG AA
```

---

## Keyboard Navigation

Người dùng phải có thể:

* Tab
* Enter
* Escape

để thao tác các thành phần chính.

---

## Focus State

Không được xóa focus ring.

Sử dụng:

```css
--focus-ring-color
```

---

## Icons

Không dùng icon làm phương thức truyền tải thông tin duy nhất.

Luôn có text hoặc tooltip.

---

# 11. POS UI Rules

## Purpose

Tối ưu tốc độ bán hàng.

---

## Layout

```text
┌───────────────────────┬──────────────┐
│ Product List          │ Cart         │
│                       │              │
│                       │              │
├───────────────────────┴──────────────┤
│ Payment Section                      │
└──────────────────────────────────────┘
```

---

## Requirements

* Cart luôn hiển thị.
* Nút thanh toán luôn hiển thị.
* Không cần scroll để thanh toán.
* Hạn chế popup.
* Hạn chế nhập liệu.
* Hỗ trợ màn hình cảm ứng.

---

## Product Cards

Hiển thị:

* Hình ảnh
* Tên món
* Giá

Không hiển thị thông tin dư thừa.

---

# 12. PDS UI Rules

## Purpose

Nhân viên pha chế phải đọc đơn từ khoảng cách 2-3 mét.

---

## Requirements

* Font lớn.
* Độ tương phản cao.
* Trạng thái nổi bật.
* Cập nhật realtime.

---

## Status Colors

Queued:

```css
--pds-accent-blue
```

Preparing:

```css
--pds-accent-orange
```

Completed:

```css
--pds-accent-green
```

---

## Order Card Priority

Hiển thị:

1. Order Number
2. Waiting Time
3. Items
4. Notes

---

# 13. Customer Portal Rules

## Purpose

Khuyến khích đặt hàng nhanh.

---

## Requirements

* Mobile-first.
* CTA rõ ràng.
* Hình ảnh sản phẩm nổi bật.
* Menu dễ duyệt.

---

## Product Page

Ưu tiên:

* Hình ảnh lớn.
* Giá rõ ràng.
* Nút đặt hàng nổi bật.

---

## Checkout

Quy trình tối đa:

```text
Cart
→ Payment
→ Confirmation
```

Không thêm bước không cần thiết.

---

# 14. Admin Dashboard Rules

## Purpose

Quản trị dữ liệu hiệu quả.

---

## Requirements

* Data-first.
* Table-first.
* Search-first.

---

## Standard Page Layout

```text
Header

Search
Filters
Actions

Data Table

Pagination
```

---

## CRUD Pages

Luôn có:

* Search
* Filter
* Create
* Edit
* Delete

---

## Reports

Ưu tiên:

* KPI Cards
* Charts
* Tables

---

# 15. Loading States

Tất cả thao tác bất đồng bộ phải có loading state.

Ví dụ:

* Spinner
* Skeleton
* Loading Button

Không để giao diện đứng yên.

---

# 16. Error States

Tất cả lỗi phải có:

* Message rõ ràng
* Hành động tiếp theo

Ví dụ:

```text
Unable to load products.
Please try again.
```

---

# 17. Empty States

Tất cả màn hình danh sách phải có Empty State.

Ví dụ:

```text
No orders available.
```

---

# 18. Notifications

## Success

Sử dụng:

* Toast
* Alert Success

---

## Error

Sử dụng:

* Toast Error
* Alert Error

---

## Do Not

Không sử dụng:

* alert()
* confirm()

---

# 19. AI UI Generation Rules

Trước khi tạo giao diện, AI phải:

* Đọc toàn bộ CSS Variables hiện có.
* Tái sử dụng component hiện có.
* Tái sử dụng layout hiện có.
* Không tạo màu mới.
* Không tạo spacing mới.
* Không dùng inline style.
* Không dùng inline color.
* Không dùng inline width/height nếu có thể dùng class.

---

# 20. Definition Of Done

Trước khi hoàn thành bất kỳ UI nào:

* Responsive trên Mobile, Tablet và Desktop.
* Sử dụng Design Tokens.
* Không hardcode màu.
* Không hardcode spacing.
* Không inline style.
* Có loading state.
* Có error state.
* Có empty state.
* Có hover state.
* Có focus state.
* Tuân thủ theme Coffee & Matcha.
* Giữ tính nhất quán với giao diện hiện có.
* Đảm bảo khả năng sử dụng thực tế của nhân viên và khách hàng.

```