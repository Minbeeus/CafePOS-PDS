using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using CafePOS.Domain.Entities;
using CafePOS.Domain.Enums;
using CafePOS.Infrastructure.Data;
using CafePOS.Application.Services;
using CafePOS.Application.DTOs.Orders;
using CafePOS.Infrastructure.Repositories;
using CafePOS.Infrastructure.Persistence;
using Xunit;

namespace CafePOS.Tests.UnitTests;

public class OrderTests : IDisposable
{
    private readonly AppDbContext _context;
    private readonly OrderRepository _orderRepository;
    private readonly ProductRepository _productRepository;
    private readonly ShiftRepository _shiftRepository;
    private readonly VoucherRepository _voucherRepository;
    private readonly PaymentRepository _paymentRepository;
    private readonly CustomerRepository _customerRepository;
    private readonly StaffRepository _staffRepository;
    private readonly OrderService _orderService;

    public OrderTests()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .ConfigureWarnings(x => x.Ignore(Microsoft.EntityFrameworkCore.Diagnostics.InMemoryEventId.TransactionIgnoredWarning))
            .Options;

        _context = new AppDbContext(options);
        _context.Database.EnsureCreated();

        _orderRepository = new OrderRepository(_context);
        _productRepository = new ProductRepository(_context);
        _shiftRepository = new ShiftRepository(_context);
        _voucherRepository = new VoucherRepository(_context);
        _paymentRepository = new PaymentRepository(_context);
        _customerRepository = new CustomerRepository(_context);
        _staffRepository = new StaffRepository(_context);

        var transactionManager = new TransactionManager(_context);

        _orderService = new OrderService(
            _orderRepository,
            _productRepository,
            _shiftRepository,
            _voucherRepository,
            _paymentRepository,
            _customerRepository,
            _staffRepository,
            transactionManager);
    }

    public void Dispose()
    {
        _context.Database.EnsureDeleted();
        _context.Dispose();
    }

    private async Task SeedBaseDataAsync()
    {
        // Seed Staff
        var owner = new Staff
        {
            Id = 1,
            FullName = "Owner Staff",
            Phone = "0901",
            Email = "owner@test.com",
            PosCode = BCrypt.Net.BCrypt.HashPassword("111111"),
            PasswordHash = "",
            Role = StaffRole.Owner,
            Status = "Active"
        };
        var cashier = new Staff
        {
            Id = 2,
            FullName = "Cashier Staff",
            Phone = "0902",
            Email = "cashier@test.com",
            PosCode = BCrypt.Net.BCrypt.HashPassword("333333"),
            PasswordHash = "",
            Role = StaffRole.Cashier,
            Status = "Active"
        };
        _context.Staffs.AddRange(owner, cashier);

        // Seed Category
        var category = new Category { Id = 1, Name = "Coffee", DisplayStation = "Bar", IsActive = true };
        _context.Categories.Add(category);

        // Seed Products
        var coffee = new Product
        {
            Id = 1,
            CategoryId = 1,
            Name = "Americano",
            BasePrice = 30000,
            Status = "Active",
            HasSizeOption = true
        };
        coffee.Sizes.Add(new ProductSize { Id = 1, ProductId = 1, SizeLabel = "M", PriceModifier = 5000, IsDefault = true });
        coffee.Sizes.Add(new ProductSize { Id = 2, ProductId = 1, SizeLabel = "L", PriceModifier = 10000, IsDefault = false });

        var outOfStockProduct = new Product
        {
            Id = 2,
            CategoryId = 1,
            Name = "Latte",
            BasePrice = 45000,
            Status = "OutOfStock",
            HasSizeOption = false
        };

        _context.Products.AddRange(coffee, outOfStockProduct);

        // Seed Topping
        var topping = new Topping { Id = 1, Name = "Espresso Shot", Price = 10000, IsActive = true };
        _context.Toppings.Add(topping);
        coffee.Toppings.Add(topping);

        // Seed Customer
        var customerSilver = new Customer
        {
            Id = 1,
            FullName = "Silver Customer",
            Phone = "0987654321",
            Email = "silver@test.com",
            PasswordHash = "",
            TotalSpend = 1200000,
            LoyaltyTier = LoyaltyTier.Silver,
            CurrentPoints = 50
        };
        _context.Customers.Add(customerSilver);

        // Seed Voucher
        var voucher = new Voucher
        {
            Id = 1,
            Code = "VOUCHER10",
            DiscountType = "Percent",
            DiscountValue = 10,
            MinOrderValue = 20000,
            IsActive = true,
            ExpiresAt = DateTime.UtcNow.AddDays(10),
            CreatedByStaffId = 1
        };
        _context.Vouchers.Add(voucher);

        await _context.SaveChangesAsync();
    }

    [Fact]
    public async Task CreateOrder_NoOpenShift_ThrowsInvalidOperationException()
    {
        // Arrange
        await SeedBaseDataAsync();
        var request = new CreateOrderRequest
        {
            Type = "DineIn",
            Items = new List<OrderItemRequest> { new OrderItemRequest { ProductId = 1, Quantity = 1 } }
        };

        // Act & Assert
        var ex = await Assert.ThrowsAsync<InvalidOperationException>(() => _orderService.CreateOrderAsync(request, 2));
        Assert.Contains("Ca làm việc chưa được mở", ex.Message);
    }

    [Fact]
    public async Task CreateOrder_WithOpenShift_CalculatesTotalCorrectl()
    {
        // Arrange
        await SeedBaseDataAsync();
        _context.Shifts.Add(new Shift { Status = "Open", OpenedByStaffId = 1, ShiftDate = DateTime.UtcNow.Date, OpenedAt = DateTime.UtcNow });
        await _context.SaveChangesAsync();

        var request = new CreateOrderRequest
        {
            Type = "DineIn",
            Items = new List<OrderItemRequest>
            {
                new OrderItemRequest
                {
                    ProductId = 1,
                    Quantity = 2,
                    SizeLabel = "L",
                    ToppingIds = new List<int> { 1 } // Topping price = 10000
                }
            }
        };

        // Act
        var response = await _orderService.CreateOrderAsync(request, 2);

        // Assert
        Assert.NotNull(response);
        Assert.Equal("Draft", response.Status);
        
        // BasePrice = 30000, Size L modifier = +10000 -> Unit price = 40000.
        // Topping = 10000. Item Total = (40000 + 10000) * 2 = 100000.
        Assert.Equal(100000, response.SubTotal);
        Assert.Equal(100000, response.TotalAmount);
    }

    [Fact]
    public async Task CreateOrder_OutOfStockProduct_ThrowsInvalidOperationException()
    {
        // Arrange
        await SeedBaseDataAsync();
        _context.Shifts.Add(new Shift { Status = "Open", OpenedByStaffId = 1, ShiftDate = DateTime.UtcNow.Date, OpenedAt = DateTime.UtcNow });
        await _context.SaveChangesAsync();

        var request = new CreateOrderRequest
        {
            Type = "DineIn",
            Items = new List<OrderItemRequest>
            {
                new OrderItemRequest { ProductId = 2, Quantity = 1 } // Out of Stock
            }
        };

        // Act & Assert
        var ex = await Assert.ThrowsAsync<InvalidOperationException>(() => _orderService.CreateOrderAsync(request, 2));
        Assert.Contains("hết hàng", ex.Message);
    }

    [Fact]
    public async Task CreateOrder_SilverCustomer_AutoAppliesLoyaltyDiscount()
    {
        // Arrange
        await SeedBaseDataAsync();
        _context.Shifts.Add(new Shift { Status = "Open", OpenedByStaffId = 1, ShiftDate = DateTime.UtcNow.Date, OpenedAt = DateTime.UtcNow });
        await _context.SaveChangesAsync();

        var request = new CreateOrderRequest
        {
            Type = "DineIn",
            CustomerPhone = "0987654321", // Silver -> 10% discount
            ApplyLoyaltyDiscount = true,
            Items = new List<OrderItemRequest>
            {
                new OrderItemRequest { ProductId = 1, Quantity = 1, SizeLabel = "M" } // UnitPrice = 35000
            }
        };

        // Act
        var response = await _orderService.CreateOrderAsync(request, 2);

        // Assert
        Assert.NotNull(response);
        Assert.Equal(35000, response.SubTotal);
        Assert.Equal(3500, response.DiscountAmount); // 10% of 35000
        Assert.Equal(31500, response.TotalAmount);
    }

    [Fact]
    public async Task CreateOrder_WithVoucher_AppliesDiscountSuccessfully()
    {
        // Arrange
        await SeedBaseDataAsync();
        _context.Shifts.Add(new Shift { Status = "Open", OpenedByStaffId = 1, ShiftDate = DateTime.UtcNow.Date, OpenedAt = DateTime.UtcNow });
        await _context.SaveChangesAsync();

        var request = new CreateOrderRequest
        {
            Type = "DineIn",
            VoucherCode = "VOUCHER10", // 10%
            Items = new List<OrderItemRequest>
            {
                new OrderItemRequest { ProductId = 1, Quantity = 1, SizeLabel = "M" } // UnitPrice = 35000
            }
        };

        // Act
        var response = await _orderService.CreateOrderAsync(request, 2);

        // Assert
        Assert.NotNull(response);
        Assert.Equal(35000, response.SubTotal);
        Assert.Equal(3500, response.DiscountAmount);
        Assert.Equal(31500, response.TotalAmount);
    }

    [Fact]
    public async Task CreateOrder_ManualDiscount_AppliesCorrectly()
    {
        // Arrange
        await SeedBaseDataAsync();
        _context.Shifts.Add(new Shift { Status = "Open", OpenedByStaffId = 1, ShiftDate = DateTime.UtcNow.Date, OpenedAt = DateTime.UtcNow });
        await _context.SaveChangesAsync();

        var request = new CreateOrderRequest
        {
            Type = "DineIn",
            ManualDiscountPercent = 20, // 20%
            ManualDiscountApproverCode = "111111", // Owner PosCode
            Items = new List<OrderItemRequest>
            {
                new OrderItemRequest { ProductId = 1, Quantity = 1, SizeLabel = "M" } // UnitPrice = 35000
            }
        };

        // Act
        var response = await _orderService.CreateOrderAsync(request, 2);

        // Assert
        Assert.NotNull(response);
        Assert.Equal(35000, response.SubTotal);
        Assert.Equal(7000, response.DiscountAmount); // 20%
        Assert.Equal(28000, response.TotalAmount);
    }

    [Fact]
    public async Task ProcessPayment_DineInOrder_SavesPaymentAndUpdatesPoints()
    {
        // Arrange
        await SeedBaseDataAsync();
        _context.Shifts.Add(new Shift { Status = "Open", OpenedByStaffId = 1, ShiftDate = DateTime.UtcNow.Date, OpenedAt = DateTime.UtcNow });
        await _context.SaveChangesAsync();

        var createReq = new CreateOrderRequest
        {
            Type = "DineIn",
            CustomerPhone = "0987654321",
            Items = new List<OrderItemRequest>
            {
                new OrderItemRequest { ProductId = 1, Quantity = 3, SizeLabel = "L" } // Subtotal = (30000 + 10000) * 3 = 120000
            }
        };
        var orderDto = await _orderService.CreateOrderAsync(createReq, 2);

        var paymentReq = new PaymentMethodRequest
        {
            PaymentMethod = "Cash",
            AmountReceived = 150000
        };

        // Act
        var result = await _orderService.ProcessPaymentAsync(orderDto.Id, paymentReq, 2);

        // Assert
        Assert.Equal("Confirmed", result.Status);
        Assert.Equal("Paid", result.PaymentStatus);
        Assert.Equal(120000, result.TotalAmount);
        Assert.Equal(30000, result.AmountChange);

        // Assert customer points update
        var customer = await _context.Customers.FindAsync(1);
        Assert.NotNull(customer);
        // Points earned = 120000 / 10000 = 12 points. Starting point = 50 -> 62.
        Assert.Equal(62, customer.CurrentPoints);
        // TotalSpend updated from 1200000 + 120000 = 1320000 -> tier is upgraded to Silver (already Silver, not yet 1.5M Gold)
        Assert.Equal(1320000, customer.TotalSpend);
        Assert.Equal(LoyaltyTier.Silver, customer.LoyaltyTier);
    }

    [Fact]
    public async Task ProcessPayment_DoublePayment_ThrowsInvalidOperationException()
    {
        // Arrange
        await SeedBaseDataAsync();
        _context.Shifts.Add(new Shift { Status = "Open", OpenedByStaffId = 1, ShiftDate = DateTime.UtcNow.Date, OpenedAt = DateTime.UtcNow });
        await _context.SaveChangesAsync();

        var createReq = new CreateOrderRequest
        {
            Type = "DineIn",
            Items = new List<OrderItemRequest>
            {
                new OrderItemRequest { ProductId = 1, Quantity = 1, SizeLabel = "M" }
            }
        };
        var orderDto = await _orderService.CreateOrderAsync(createReq, 2);

        var paymentReq = new PaymentMethodRequest
        {
            PaymentMethod = "Cash",
            AmountReceived = 35000
        };

        // First payment
        await _orderService.ProcessPaymentAsync(orderDto.Id, paymentReq, 2);

        // Act & Assert (Second payment throws)
        var ex = await Assert.ThrowsAsync<InvalidOperationException>(() => _orderService.ProcessPaymentAsync(orderDto.Id, paymentReq, 2));
        Assert.Contains("thanh toán trước đó", ex.Message);
    }

    [Fact]
    public void GenerateVietQr_ReturnsValidImageProviderUrl()
    {
        // Arrange
        var vietQrService = new CafePOS.Infrastructure.Services.VietQrService();
        var orderCode = "ORD12345";
        decimal amount = 35000;

        // Act
        var url = vietQrService.GenerateQrCodeUrl(orderCode, amount);

        // Assert
        Assert.NotNull(url);
        Assert.Contains("img.vietqr.io", url);
        Assert.Contains("amount=35000", url);
        Assert.Contains("CAFEPOS%20ORD12345", url);
    }

    [Fact]
    public async Task UpdateOrderStatus_ValidTransition_UpdatesStatusSuccessfully()
    {
        // Arrange
        await SeedBaseDataAsync();
        _context.Shifts.Add(new Shift { Status = "Open", OpenedByStaffId = 1, ShiftDate = DateTime.UtcNow.Date, OpenedAt = DateTime.UtcNow });
        await _context.SaveChangesAsync();

        var createReq = new CreateOrderRequest
        {
            Type = "DineIn",
            Items = new List<OrderItemRequest> { new OrderItemRequest { ProductId = 1, Quantity = 1, SizeLabel = "M" } }
        };
        var orderDto = await _orderService.CreateOrderAsync(createReq, 2);

        // Confirm the order first by processing payment
        var paymentReq = new PaymentMethodRequest { PaymentMethod = "Cash", AmountReceived = 35000 };
        var confirmedOrder = await _orderService.ProcessPaymentAsync(orderDto.Id, paymentReq, 2);
        Assert.Equal("Confirmed", confirmedOrder.Status);

        // Act & Assert: transition Confirmed -> Preparing
        var preparingOrder = await _orderService.UpdateOrderStatusAsync(orderDto.Id, OrderStatus.Preparing);
        Assert.Equal("Preparing", preparingOrder.Status);

        // Act & Assert: transition Preparing -> Completed
        var completedOrder = await _orderService.UpdateOrderStatusAsync(orderDto.Id, OrderStatus.Completed);
        Assert.Equal("Completed", completedOrder.Status);
        Assert.NotNull(completedOrder.CompletedAt);
    }

    [Fact]
    public async Task UpdateOrderStatus_InvalidTransition_ThrowsInvalidOperationException()
    {
        // Arrange
        await SeedBaseDataAsync();
        _context.Shifts.Add(new Shift { Status = "Open", OpenedByStaffId = 1, ShiftDate = DateTime.UtcNow.Date, OpenedAt = DateTime.UtcNow });
        await _context.SaveChangesAsync();

        var createReq = new CreateOrderRequest
        {
            Type = "DineIn",
            Items = new List<OrderItemRequest> { new OrderItemRequest { ProductId = 1, Quantity = 1, SizeLabel = "M" } }
        };
        var orderDto = await _orderService.CreateOrderAsync(createReq, 2);

        // Act & Assert: transition Draft directly to Completed (invalid)
        var ex = await Assert.ThrowsAsync<InvalidOperationException>(() => 
            _orderService.UpdateOrderStatusAsync(orderDto.Id, OrderStatus.Completed));
        Assert.Contains("Không thể chuyển đổi trạng thái đơn hàng", ex.Message);
    }

    [Fact]
    public async Task UpdateStationPrepStatus_ValidTransitions_UpdatesSuccessfully()
    {
        // Arrange
        await SeedBaseDataAsync();
        
        // Add another category and product for Pastry
        var pastryCat = new Category { Id = 2, Name = "Pastry", DisplayStation = "Pastry", IsActive = true };
        var cake = new Product
        {
            Id = 2,
            Name = "Cake",
            BasePrice = 30000,
            Status = "Active",
            CategoryId = 2,
            HasSizeOption = true,
            HasSugarOption = false,
            HasIceOption = false,
            CreatedAt = DateTime.UtcNow
        };
        _context.Categories.Add(pastryCat);
        _context.Products.Add(cake);
        _context.ProductSizes.Add(new ProductSize { ProductId = 2, SizeLabel = "M", PriceModifier = 0, IsDefault = true });
        
        _context.Shifts.Add(new Shift { Status = "Open", OpenedByStaffId = 1, ShiftDate = DateTime.UtcNow.Date, OpenedAt = DateTime.UtcNow });
        await _context.SaveChangesAsync();

        var createReq = new CreateOrderRequest
        {
            Type = "DineIn",
            Items = new List<OrderItemRequest> 
            { 
                new OrderItemRequest { ProductId = 1, Quantity = 1, SizeLabel = "M" }, // Bar item
                new OrderItemRequest { ProductId = 2, Quantity = 1, SizeLabel = "M" }  // Pastry item
            }
        };
        var orderDto = await _orderService.CreateOrderAsync(createReq, 2);

        // Confirm order
        var paymentReq = new PaymentMethodRequest { PaymentMethod = "Cash", AmountReceived = 65000 };
        var confirmedOrder = await _orderService.ProcessPaymentAsync(orderDto.Id, paymentReq, 2);
        Assert.Equal("Confirmed", confirmedOrder.Status);

        // Act: Barista starts preparing Bar items
        var prepOrderBar = await _orderService.UpdateStationPrepStatusAsync(orderDto.Id, "Bar", "Preparing");
        Assert.Equal("Preparing", prepOrderBar.Status);
        
        // Assert: Bar item is Preparing, Pastry item is still Pending
        var barItem = prepOrderBar.Items.First(i => i.ProductId == 1);
        var pastryItem = prepOrderBar.Items.First(i => i.ProductId == 2);
        Assert.Equal("Preparing", barItem.BarStatus);
        Assert.Equal("Pending", pastryItem.PastryStatus);

        // Act: Barista completes Bar items
        var doneOrderBar = await _orderService.UpdateStationPrepStatusAsync(orderDto.Id, "Bar", "Completed");
        // Assert: Overall status is still Preparing because Pastry is Pending
        Assert.Equal("Preparing", doneOrderBar.Status);
        
        barItem = doneOrderBar.Items.First(i => i.ProductId == 1);
        pastryItem = doneOrderBar.Items.First(i => i.ProductId == 2);
        Assert.Equal("Done", barItem.BarStatus);
        Assert.Equal("Pending", pastryItem.PastryStatus);

        // Act: Pastry staff completes Pastry items
        var doneOrderPastry = await _orderService.UpdateStationPrepStatusAsync(orderDto.Id, "Pastry", "Completed");
        // Assert: Overall status is now Completed since all items are Done
        Assert.Equal("Completed", doneOrderPastry.Status);
        
        barItem = doneOrderPastry.Items.First(i => i.ProductId == 1);
        pastryItem = doneOrderPastry.Items.First(i => i.ProductId == 2);
        Assert.Equal("Done", barItem.BarStatus);
        Assert.Equal("Done", pastryItem.PastryStatus);
    }
}
