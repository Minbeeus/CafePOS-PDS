using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CafePOS.Domain.Entities;
using CafePOS.Domain.Enums;
using CafePOS.Application.DTOs.Orders;
using CafePOS.Application.Interfaces.Repositories;
using CafePOS.Application.Interfaces;

namespace CafePOS.Application.Services;

public class OrderService
{
    private readonly IOrderRepository _orderRepository;
    private readonly IProductRepository _productRepository;
    private readonly IShiftRepository _shiftRepository;
    private readonly IVoucherRepository _voucherRepository;
    private readonly IPaymentRepository _paymentRepository;
    private readonly ICustomerRepository _customerRepository;
    private readonly IStaffRepository _staffRepository;
    private readonly ITransactionManager _transactionManager;

    public OrderService(
        IOrderRepository orderRepository,
        IProductRepository productRepository,
        IShiftRepository shiftRepository,
        IVoucherRepository voucherRepository,
        IPaymentRepository paymentRepository,
        ICustomerRepository customerRepository,
        IStaffRepository staffRepository,
        ITransactionManager transactionManager)
    {
        _orderRepository = orderRepository;
        _productRepository = productRepository;
        _shiftRepository = shiftRepository;
        _voucherRepository = voucherRepository;
        _paymentRepository = paymentRepository;
        _customerRepository = customerRepository;
        _staffRepository = staffRepository;
        _transactionManager = transactionManager;
    }

    public async Task<OrderResponse> CreateOrderAsync(CreateOrderRequest request, int staffId)
    {
        // 1. Verify that there is an open shift
        var activeShift = await _shiftRepository.GetActiveShiftAsync();
        if (activeShift == null)
        {
            throw new InvalidOperationException("Ca làm việc chưa được mở. Vui lòng liên hệ Shift Leader để mở ca.");
        }

        // 2. Validate Order Type
        if (string.IsNullOrWhiteSpace(request.Type))
        {
            throw new ArgumentException("Loại đơn hàng không được trống.");
        }

        // 3. Resolve Customer (Optional)
        Customer? customer = null;
        if (!string.IsNullOrWhiteSpace(request.CustomerPhone))
        {
            customer = await _customerRepository.GetByPhoneAsync(request.CustomerPhone);
        }

        // 4. Calculate pricing & map items
        decimal subTotal = 0;
        var orderItems = new List<OrderItem>();
        var pointTransactions = new List<PointTransaction>();

        foreach (var itemReq in request.Items)
        {
            var product = await _productRepository.GetWithSizesAndToppingsByIdAsync(itemReq.ProductId);
            if (product == null)
            {
                throw new KeyNotFoundException($"Không tìm thấy sản phẩm có ID = {itemReq.ProductId}.");
            }

            if (product.Status == "OutOfStock" || product.Status == "Inactive")
            {
                throw new InvalidOperationException($"Sản phẩm '{product.Name}' hiện đã hết hàng hoặc ngưng hoạt động.");
            }

            // Find size modifier
            decimal sizeModifier = 0;
            if (product.HasSizeOption && !string.IsNullOrWhiteSpace(itemReq.SizeLabel))
            {
                var size = product.Sizes.FirstOrDefault(s => s.SizeLabel == itemReq.SizeLabel);
                if (size == null)
                {
                    throw new ArgumentException($"Kích thước '{itemReq.SizeLabel}' không hợp lệ cho sản phẩm '{product.Name}'.");
                }
                sizeModifier = size.PriceModifier;
            }

            // Calculate unit price
            decimal unitPrice = product.BasePrice + sizeModifier;
            if (itemReq.IsPointRedemption)
            {
                // Verify points redemption
                var pointProduct = await _productRepository.GetActivePointProductByProductIdAsync(product.Id);
                if (pointProduct == null)
                {
                    throw new InvalidOperationException($"Sản phẩm '{product.Name}' không hỗ trợ đổi điểm.");
                }

                if (customer == null)
                {
                    throw new InvalidOperationException("Yêu cầu thông tin khách hàng để đổi điểm.");
                }

                int totalPointsRequired = pointProduct.PointCost * itemReq.Quantity;
                if (customer.CurrentPoints < totalPointsRequired)
                {
                    throw new InvalidOperationException($"Khách hàng không đủ điểm để đổi món này. Cần {totalPointsRequired} điểm, hiện có {customer.CurrentPoints} điểm.");
                }

                // Deduct points
                customer.CurrentPoints -= totalPointsRequired;
                pointTransactions.Add(new PointTransaction
                {
                    CustomerId = customer.Id,
                    TransactionType = "Redeem",
                    Points = -totalPointsRequired,
                    Description = $"Đổi điểm lấy món '{pointProduct.Name}' (x{itemReq.Quantity})",
                    CreatedAt = DateTime.UtcNow
                });

                // Set price to 0 for redemption
                unitPrice = 0;
            }

            // Add toppings
            decimal toppingsTotal = 0;
            var itemToppings = new List<OrderItemTopping>();

            foreach (var toppingId in itemReq.ToppingIds)
            {
                var topping = product.Toppings.FirstOrDefault(t => t.Id == toppingId);
                if (topping == null)
                {
                    throw new ArgumentException($"Topping ID {toppingId} không tồn tại hoặc không áp dụng cho sản phẩm '{product.Name}'.");
                }

                toppingsTotal += topping.Price;
                itemToppings.Add(new OrderItemTopping
                {
                    ToppingId = topping.Id,
                    ToppingPrice = topping.Price
                });
            }

            decimal itemTotal = (unitPrice + toppingsTotal) * itemReq.Quantity;
            subTotal += itemTotal;

            orderItems.Add(new OrderItem
            {
                ProductId = product.Id,
                Quantity = itemReq.Quantity,
                UnitPrice = unitPrice,
                ItemTotal = itemTotal,
                Notes = itemReq.Notes ?? "",
                SizeLabel = itemReq.SizeLabel ?? "Regular",
                SugarLevel = itemReq.SugarLevel ?? "100",
                IceLevel = itemReq.IceLevel ?? "100",
                IsPointRedemption = itemReq.IsPointRedemption,
                BarStatus = (product.Category?.DisplayStation == "Bar" || product.Category?.DisplayStation == "Both" || string.IsNullOrEmpty(product.Category?.DisplayStation)) ? "Pending" : "NA",
                PastryStatus = (product.Category?.DisplayStation == "Pastry" || product.Category?.DisplayStation == "Both") ? "Pending" : "NA",
                Toppings = itemToppings
            });
        }

        // 5. Apply Discounts
        decimal discountAmount = 0;
        decimal loyaltyDiscount = 0;
        decimal voucherDiscount = 0;
        decimal manualDiscount = 0;

        // Auto loyalty discount
        if (request.ApplyLoyaltyDiscount && customer != null)
        {
            var discountPercent = customer.LoyaltyTier switch
            {
                LoyaltyTier.Silver => 10,
                LoyaltyTier.Gold => 15,
                _ => 0
            };
            loyaltyDiscount = subTotal * (discountPercent / 100m);
        }

        // Voucher discount
        Voucher? voucher = null;
        if (!string.IsNullOrWhiteSpace(request.VoucherCode))
        {
            voucher = await _voucherRepository.GetActiveByCodeAsync(request.VoucherCode);
            if (voucher == null)
            {
                throw new ArgumentException("Mã giảm giá không hợp lệ hoặc đã bị khóa.");
            }

            if (voucher.ExpiresAt < DateTime.UtcNow)
            {
                throw new InvalidOperationException("Mã giảm giá đã hết hạn sử dụng.");
            }

            if (voucher.MaxUsageCount.HasValue && voucher.UsedCount >= voucher.MaxUsageCount.Value)
            {
                throw new InvalidOperationException("Mã giảm giá đã hết lượt sử dụng.");
            }

            if (subTotal < voucher.MinOrderValue)
            {
                throw new InvalidOperationException($"Giá trị đơn hàng chưa đạt giá trị tối thiểu {voucher.MinOrderValue:N0}đ để áp dụng voucher này.");
            }

            if (voucher.DiscountType == "Percent")
            {
                voucherDiscount = subTotal * (voucher.DiscountValue / 100m);
            }
            else // Fixed amount
            {
                voucherDiscount = voucher.DiscountValue;
            }
        }

        // Manual discount
        Staff? approver = null;
        if (request.ManualDiscountPercent > 0)
        {
            if (string.IsNullOrWhiteSpace(request.ManualDiscountApproverCode))
            {
                throw new ArgumentException("Yêu cầu mã xác nhận của Quản lý để áp dụng giảm giá thủ công.");
            }

            approver = await _staffRepository.GetByPosCodeAsync(request.ManualDiscountApproverCode);
            if (approver == null || (approver.Role != StaffRole.Owner && approver.Role != StaffRole.ShiftLeader))
            {
                throw new UnauthorizedAccessException("Mã xác thực Quản lý không hợp lệ hoặc không đủ quyền phê duyệt.");
            }

            manualDiscount = subTotal * (request.ManualDiscountPercent / 100m);
        }

        // Sum and cap discounts
        discountAmount = loyaltyDiscount + voucherDiscount + manualDiscount;
        if (discountAmount > subTotal)
        {
            discountAmount = subTotal;
        }

        decimal totalAmount = subTotal - discountAmount;

        // 6. Generate Order Code
        var orderCode = await _orderRepository.GenerateOrderCodeAsync();

        // Determine Status
        var status = request.Type.Equals("Online", StringComparison.OrdinalIgnoreCase) 
            ? OrderStatus.Pending 
            : OrderStatus.Draft;

        var order = new Order
        {
            OrderCode = orderCode,
            Type = request.Type,
            CustomerId = customer?.Id,
            StaffId = staffId,
            CustomerName = customer?.FullName ?? request.CustomerName ?? "Khách vãng lai",
            CustomerPhone = customer?.Phone ?? request.CustomerPhone ?? "",
            Status = status,
            PaymentStatus = "Unpaid",
            PaymentMethod = "None",
            SubTotal = subTotal,
            DiscountAmount = discountAmount,
            TotalAmount = totalAmount,
            ScheduledPickupTime = request.ScheduledPickupTime,
            CreatedAt = DateTime.UtcNow,
            OrderItems = orderItems
        };

        // Attach discounts to order details
        if (loyaltyDiscount > 0)
        {
            order.Discounts.Add(new OrderDiscount
            {
                DiscountType = "Loyalty",
                DiscountValue = loyaltyDiscount,
                DiscountDescription = $"Giảm giá thành viên tier {customer?.LoyaltyTier.ToString()}"
            });
        }
        if (voucherDiscount > 0 && voucher != null)
        {
            order.Discounts.Add(new OrderDiscount
            {
                DiscountType = "Voucher",
                DiscountValue = voucherDiscount,
                DiscountDescription = $"Áp dụng Voucher {voucher.Code}",
                VoucherId = voucher.Id
            });
            voucher.UsedCount++; // Increment usage
            await _voucherRepository.UpdateAsync(voucher);
        }
        if (manualDiscount > 0 && approver != null)
        {
            order.Discounts.Add(new OrderDiscount
            {
                DiscountType = "Manual",
                DiscountValue = manualDiscount,
                DiscountDescription = $"Chiết khấu thủ công {request.ManualDiscountPercent}%",
                ApprovedByStaffId = approver.Id,
                ApprovedAt = DateTime.UtcNow
            });
        }

        // Save customer points deductions
        if (customer != null)
        {
            await _customerRepository.UpdateAsync(customer);
            foreach (var tx in pointTransactions)
            {
                await _orderRepository.AddPointTransactionAsync(tx);
            }
        }

        // Save order (Optimistic Concurrency rowversion is handled by EF Core automatically)
        await _orderRepository.AddAsync(order);

        // Save Audit Log and point transaction audit
        if (manualDiscount > 0 && approver != null)
        {
            await _orderRepository.AddAuditLogAsync(new AuditLog
            {
                UserId = staffId.ToString(),
                Action = "UPDATE",
                EntityName = "Orders",
                EntityId = order.Id.ToString(),
                OldValues = "{}",
                NewValues = $"{{\"manualDiscountPercent\":{request.ManualDiscountPercent},\"approverId\":{approver.Id}}}",
                Timestamp = DateTime.UtcNow
            });
        }

        return MapToResponse(order);
    }

    public async Task<OrderResponse> ProcessPaymentAsync(int orderId, PaymentMethodRequest request, int staffId)
    {
        var order = await _orderRepository.GetWithDetailsByIdAsync(orderId);
        if (order == null)
        {
            throw new KeyNotFoundException("Không tìm thấy đơn hàng cần thanh toán.");
        }

        // Check for idempotency: One order can only have at most one successful payment
        var hasPaid = await _paymentRepository.HasCompletedPaymentForOrderAsync(orderId);
        if (hasPaid)
        {
            throw new InvalidOperationException("Đơn hàng đã được thanh toán trước đó.");
        }

        if (order.Status != OrderStatus.Draft && order.Status != OrderStatus.Pending)
        {
            throw new InvalidOperationException("Đơn hàng này đã được thanh toán hoặc không ở trạng thái hợp lệ để thanh toán.");
        }

        // Map Payment method string to Enum safely
        PaymentMethod method;
        var methodStr = request.PaymentMethod;
        if (methodStr.Equals("Transfer", StringComparison.OrdinalIgnoreCase))
        {
            method = PaymentMethod.VietQr;
        }
        else if (methodStr.Equals("Mixed", StringComparison.OrdinalIgnoreCase))
        {
            method = PaymentMethod.Mixed;
        }
        else if (!Enum.TryParse(methodStr, true, out method))
        {
            throw new ArgumentException($"Phương thức thanh toán '{methodStr}' không hợp lệ.");
        }

        await _transactionManager.BeginTransactionAsync();
        try
        {
            // Create Payment record
            var payment = new Payment
            {
                OrderId = order.Id,
                Amount = order.TotalAmount,
                AmountReceived = request.AmountReceived,
                AmountChange = Math.Max(0, request.AmountReceived - order.TotalAmount),
                ReferenceCode = request.ReferenceCode ?? "",
                CreatedByStaffId = staffId,
                Method = method,
                Status = PaymentStatus.Completed,
                PaidAt = DateTime.UtcNow,
                CreatedAt = DateTime.UtcNow
            };

            await _paymentRepository.AddAsync(payment);

            // Update Order details
            order.PaymentStatus = "Paid";
            order.PaymentMethod = request.PaymentMethod;
            order.AmountReceived = request.AmountReceived;
            order.AmountChange = payment.AmountChange;
            order.Status = OrderStatus.Confirmed;
            order.ConfirmedAt = DateTime.UtcNow;

            // Process loyalty points if customer exists
            if (order.CustomerId.HasValue)
            {
                var customer = await _customerRepository.GetByIdAsync(order.CustomerId.Value);
                if (customer != null)
                {
                    int pointsEarned = (int)(order.TotalAmount / 10000m); // 1 point for every 10k VND
                    customer.CurrentPoints += pointsEarned;
                    customer.TotalSpend += order.TotalAmount;

                    // Create point transaction
                    await _orderRepository.AddPointTransactionAsync(new PointTransaction
                    {
                        CustomerId = customer.Id,
                        OrderId = order.Id,
                        TransactionType = "Earn",
                        Points = pointsEarned,
                        Description = $"Tích điểm từ đơn hàng #{order.OrderCode}",
                        CreatedAt = DateTime.UtcNow
                    });

                    // Tier Upgrade Check
                    if (customer.TotalSpend >= 1500000)
                    {
                        customer.LoyaltyTier = LoyaltyTier.Gold;
                    }
                    else if (customer.TotalSpend >= 1000000)
                    {
                        if (customer.LoyaltyTier != LoyaltyTier.Gold)
                        {
                            customer.LoyaltyTier = LoyaltyTier.Silver;
                        }
                    }

                    await _customerRepository.UpdateAsync(customer);
                }
            }

            await _orderRepository.UpdateAsync(order);
            await _transactionManager.CommitTransactionAsync();
        }
        catch
        {
            await _transactionManager.RollbackTransactionAsync();
            throw;
        }

        return MapToResponse(order);
    }

    public async Task<OrderResponse> ApplyManualDiscountAsync(int orderId, ManualDiscountRequest request)
    {
        var order = await _orderRepository.GetWithDetailsByIdAsync(orderId);
        if (order == null)
        {
            throw new KeyNotFoundException("Không tìm thấy đơn hàng.");
        }

        if (order.Status != OrderStatus.Draft && order.Status != OrderStatus.Pending)
        {
            throw new InvalidOperationException("Chỉ có thể giảm giá cho đơn hàng nháp hoặc đơn online chờ xác nhận.");
        }

        var approver = await _staffRepository.GetByPosCodeAsync(request.ApproverPosCode);
        if (approver == null || (approver.Role != StaffRole.Owner && approver.Role != StaffRole.ShiftLeader))
        {
            throw new UnauthorizedAccessException("Mã xác thực Quản lý không hợp lệ hoặc không có quyền phê duyệt.");
        }

        // Apply discount percentage
        var discountVal = order.SubTotal * (request.DiscountPercent / 100m);
        order.DiscountAmount += discountVal;
        if (order.DiscountAmount > order.SubTotal)
        {
            order.DiscountAmount = order.SubTotal;
        }

        order.TotalAmount = order.SubTotal - order.DiscountAmount;

        order.Discounts.Add(new OrderDiscount
        {
            DiscountType = "Manual",
            DiscountValue = discountVal,
            DiscountDescription = request.ApproverNote ?? $"Chiết khấu thủ công {request.DiscountPercent}%",
            ApprovedByStaffId = approver.Id,
            ApprovedAt = DateTime.UtcNow
        });

        await _orderRepository.UpdateAsync(order);

        return MapToResponse(order);
    }

    public OrderResponse MapToResponse(Order order)
    {
        var rowVersionBase64 = order.RowVersion != null ? Convert.ToBase64String(order.RowVersion) : null;
        
        return new OrderResponse
        {
            Id = order.Id,
            OrderCode = order.OrderCode,
            Type = order.Type,
            TableNumber = order.TableNumber,
            SubOrderIndex = order.SubOrderIndex,
            ParentOrderId = order.ParentOrderId,
            CustomerId = order.CustomerId,
            CustomerName = order.CustomerName,
            CustomerPhone = order.CustomerPhone,
            Status = order.Status.ToString(),
            PaymentStatus = order.PaymentStatus,
            PaymentMethod = order.PaymentMethod,
            SubTotal = order.SubTotal,
            DiscountAmount = order.DiscountAmount,
            TotalAmount = order.TotalAmount,
            AmountReceived = order.AmountReceived,
            AmountChange = order.AmountChange,
            ScheduledPickupTime = order.ScheduledPickupTime,
            ConfirmedAt = order.ConfirmedAt,
            CompletedAt = order.CompletedAt,
            ClosedAt = order.ClosedAt,
            CreatedAt = order.CreatedAt,
            RowVersionBase64 = rowVersionBase64,
            Items = order.OrderItems.Select(oi => new OrderItemResponse
            {
                Id = oi.Id,
                ProductId = oi.ProductId,
                ProductName = oi.Product?.Name ?? "Sản phẩm",
                Quantity = oi.Quantity,
                UnitPrice = oi.UnitPrice,
                ItemTotal = oi.ItemTotal,
                SizeLabel = oi.SizeLabel,
                SugarLevel = oi.SugarLevel,
                IceLevel = oi.IceLevel,
                Notes = oi.Notes,
                IsPointRedemption = oi.IsPointRedemption,
                BarStatus = oi.BarStatus,
                PastryStatus = oi.PastryStatus,
                Toppings = oi.Toppings.Select(oit => new OrderItemToppingResponse
                {
                    Id = oit.Id,
                    ToppingId = oit.ToppingId,
                    ToppingName = oit.Topping?.Name ?? "Topping",
                    ToppingPrice = oit.ToppingPrice
                }).ToList()
            }).ToList()
        };
    }

    public async Task<OrderResponse> UpdateOrderStatusAsync(int orderId, OrderStatus newStatus)
    {
        var order = await _orderRepository.GetWithDetailsByIdAsync(orderId);
        if (order == null)
        {
            throw new KeyNotFoundException("Không tìm thấy đơn hàng.");
        }

        ValidateStatusTransition(order.Status, newStatus);

        order.Status = newStatus;
        if (newStatus == OrderStatus.Confirmed)
        {
            order.ConfirmedAt = DateTime.UtcNow;
        }
        else if (newStatus == OrderStatus.Completed)
        {
            order.CompletedAt = DateTime.UtcNow;
        }
        else if (newStatus == OrderStatus.Closed)
        {
            order.ClosedAt = DateTime.UtcNow;
        }

        await _orderRepository.UpdateAsync(order);
        return MapToResponse(order);
    }

    private void ValidateStatusTransition(OrderStatus current, OrderStatus next)
    {
        if (current == next) return;

        if (current == OrderStatus.Closed || current == OrderStatus.Cancelled)
        {
            throw new InvalidOperationException($"Không thể chuyển đổi trạng thái đơn hàng từ {current} sang {next}.");
        }

        if (next == OrderStatus.Cancelled)
        {
            if (current == OrderStatus.Completed)
            {
                throw new InvalidOperationException("Không thể hủy đơn hàng đã hoàn thành chế biến.");
            }
            return;
        }

        bool isValid = false;
        switch (current)
        {
            case OrderStatus.Draft:
                isValid = next == OrderStatus.Pending || next == OrderStatus.Confirmed;
                break;
            case OrderStatus.Pending:
                isValid = next == OrderStatus.Confirmed;
                break;
            case OrderStatus.Confirmed:
                isValid = next == OrderStatus.Preparing || next == OrderStatus.Completed;
                break;
            case OrderStatus.Preparing:
                isValid = next == OrderStatus.Completed;
                break;
            case OrderStatus.Completed:
                isValid = next == OrderStatus.Closed;
                break;
        }

        if (!isValid)
        {
            throw new InvalidOperationException($"Không thể chuyển đổi trạng thái đơn hàng từ {current} sang {next}.");
        }
    }

    public async Task<OrderResponse> UpdateStationPrepStatusAsync(int orderId, string station, string targetStatus)
    {
        var order = await _orderRepository.GetWithDetailsByIdAsync(orderId);
        if (order == null)
        {
            throw new KeyNotFoundException("Không tìm thấy đơn hàng.");
        }

        if (order.Status == OrderStatus.Completed || order.Status == OrderStatus.Closed || order.Status == OrderStatus.Cancelled)
        {
            throw new InvalidOperationException("Không thể cập nhật trạng thái chế biến cho đơn hàng đã hoàn thành, đã đóng hoặc đã hủy.");
        }

        // Determine item station status string to set ("Preparing" or "Done")
        string itemStatusValue = targetStatus == "Preparing" ? "Preparing" : "Done";

        // Update items belonging to the station
        bool updatedAny = false;
        foreach (var item in order.OrderItems)
        {
            if (station.Equals("Bar", StringComparison.OrdinalIgnoreCase))
            {
                if (item.BarStatus != "NA")
                {
                    item.BarStatus = itemStatusValue;
                    updatedAny = true;
                }
            }
            else if (station.Equals("Pastry", StringComparison.OrdinalIgnoreCase))
            {
                if (item.PastryStatus != "NA")
                {
                    item.PastryStatus = itemStatusValue;
                    updatedAny = true;
                }
            }
        }

        if (!updatedAny)
        {
            throw new InvalidOperationException($"Đơn hàng này không chứa sản phẩm nào cần chế biến tại quầy {station}.");
        }

        // Determine overall order status
        bool allDone = true;
        bool anyPreparingOrDone = false;

        foreach (var item in order.OrderItems)
        {
            bool itemBarDone = item.BarStatus == "Done" || item.BarStatus == "NA";
            bool itemPastryDone = item.PastryStatus == "Done" || item.PastryStatus == "NA";

            if (!itemBarDone || !itemPastryDone)
            {
                allDone = false;
            }

            if (item.BarStatus == "Preparing" || item.BarStatus == "Done" || 
                item.PastryStatus == "Preparing" || item.PastryStatus == "Done")
            {
                anyPreparingOrDone = true;
            }
        }

        if (allDone)
        {
            order.Status = OrderStatus.Completed;
            order.CompletedAt = DateTime.UtcNow;
        }
        else if (anyPreparingOrDone)
        {
            order.Status = OrderStatus.Preparing;
        }
        else
        {
            order.Status = OrderStatus.Confirmed;
        }

        await _orderRepository.UpdateAsync(order);
        return MapToResponse(order);
    }
}

public class PaymentMethodRequest
{
    public string PaymentMethod { get; set; } = "Cash"; // Cash, Transfer, Mixed
    public decimal AmountReceived { get; set; }
    public string? ReferenceCode { get; set; }
}

public class ManualDiscountRequest
{
    public decimal DiscountPercent { get; set; }
    public string ApproverPosCode { get; set; } = string.Empty;
    public string? ApproverNote { get; set; }
}
