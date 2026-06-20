using System;
using System.Collections.Generic;
using System.Linq;
using CafePOS.Domain.Entities;
using CafePOS.Domain.Enums;

namespace CafePOS.Infrastructure.Data;

public static class DbSeeder
{
    public static void Seed(AppDbContext context)
    {
        context.Database.EnsureCreated();

        // 1. Seed Staff
        if (!context.Staffs.Any())
        {
            var staffs = new List<Staff>
            {
                new Staff
                {
                    FullName = "Nguyễn Manager",
                    Phone = "0901234567",
                    Email = "owner@cafepos.com",
                    PosCode = BCrypt.Net.BCrypt.HashPassword("111111"), // bcrypt hashed
                    PasswordHash = BCrypt.Net.BCrypt.HashPassword("admin123"), // bcrypt hashed
                    Role = StaffRole.Owner,
                    BaseSalary = 15000000,
                    Status = "Active",
                    CreatedAt = DateTime.UtcNow
                },
                new Staff
                {
                    FullName = "Phạm ShiftLeader",
                    Phone = "0902345678",
                    Email = "leader@cafepos.com",
                    PosCode = BCrypt.Net.BCrypt.HashPassword("222222"),
                    PasswordHash = BCrypt.Net.BCrypt.HashPassword("leader123"),
                    Role = StaffRole.ShiftLeader,
                    BaseSalary = 8000000,
                    Status = "Active",
                    CreatedAt = DateTime.UtcNow
                },
                new Staff
                {
                    FullName = "Trần Thu Ngân",
                    Phone = "0903456789",
                    Email = "cashier@cafepos.com",
                    PosCode = BCrypt.Net.BCrypt.HashPassword("333333"),
                    PasswordHash = "",
                    Role = StaffRole.Cashier,
                    BaseSalary = 6000000,
                    Status = "Active",
                    CreatedAt = DateTime.UtcNow
                },
                new Staff
                {
                    FullName = "Lê Pha Chế",
                    Phone = "0904567890",
                    Email = "barista@cafepos.com",
                    PosCode = BCrypt.Net.BCrypt.HashPassword("444444"),
                    PasswordHash = "",
                    Role = StaffRole.Barista,
                    BaseSalary = 6500000,
                    Status = "Active",
                    CreatedAt = DateTime.UtcNow
                },
                new Staff
                {
                    FullName = "Nguyễn Bánh Ngọt",
                    Phone = "0905678901",
                    Email = "pastry@cafepos.com",
                    PosCode = BCrypt.Net.BCrypt.HashPassword("555555"),
                    PasswordHash = "",
                    Role = StaffRole.PastryStaff,
                    BaseSalary = 6500000,
                    Status = "Active",
                    CreatedAt = DateTime.UtcNow
                }
            };

            context.Staffs.AddRange(staffs);
            context.SaveChanges();
        }

        var defaultStaff = context.Staffs.First(s => s.Role == StaffRole.Owner);

        // 2. Seed Customer
        if (!context.Customers.Any())
        {
            var customers = new List<Customer>
            {
                new Customer
                {
                    FullName = "Lê Văn C",
                    Phone = "0912345678",
                    Email = "customer1@gmail.com",
                    GoogleId = null,
                    PasswordHash = BCrypt.Net.BCrypt.HashPassword("customer123"),
                    TotalSpend = 0,
                    LoyaltyTier = LoyaltyTier.None,
                    CurrentPoints = 0,
                    PointsResetAt = DateTime.UtcNow.AddMonths(2),
                    CreatedAt = DateTime.UtcNow
                },
                new Customer
                {
                    FullName = "Nguyễn Văn VIP",
                    Phone = "0987654321",
                    Email = "customer2@gmail.com",
                    GoogleId = null,
                    PasswordHash = BCrypt.Net.BCrypt.HashPassword("vip123"),
                    TotalSpend = 1200000,
                    LoyaltyTier = LoyaltyTier.Silver,
                    CurrentPoints = 45,
                    PointsResetAt = DateTime.UtcNow.AddMonths(1),
                    CreatedAt = DateTime.UtcNow
                },
                new Customer
                {
                    FullName = "Trần Gold Member",
                    Phone = "0966666666",
                    Email = "customer3@gmail.com",
                    GoogleId = "google-oauth-id-12345",
                    PasswordHash = "",
                    TotalSpend = 1800000,
                    LoyaltyTier = LoyaltyTier.Gold,
                    CurrentPoints = 120,
                    PointsResetAt = DateTime.UtcNow.AddMonths(2),
                    CreatedAt = DateTime.UtcNow
                }
            };

            context.Customers.AddRange(customers);
            context.SaveChanges();
        }

        // 3. Seed Category
        if (!context.Categories.Any())
        {
            var categories = new List<Category>
            {
                new Category { Name = "Trà Sữa", DisplayStation = "Bar", DisplayOrder = 1, IsActive = true },
                new Category { Name = "Cà Phê", DisplayStation = "Bar", DisplayOrder = 2, IsActive = true },
                new Category { Name = "Đá Xay", DisplayStation = "Bar", DisplayOrder = 3, IsActive = true },
                new Category { Name = "Bánh Ngọt", DisplayStation = "Pastry", DisplayOrder = 4, IsActive = true },
                new Category { Name = "Đồ Ăn Nhẹ", DisplayStation = "Pastry", DisplayOrder = 5, IsActive = true }
            };

            context.Categories.AddRange(categories);
            context.SaveChanges();
        }

        var categoriesMap = context.Categories.ToDictionary(c => c.Name);

        // 4. Seed Product
        if (!context.Products.Any())
        {
            var products = new List<Product>
            {
                new Product
                {
                    Name = "Trà Sữa Truyền Thống",
                    Description = "Trà sữa đậm vị trà đen kết hợp sữa thơm béo",
                    BasePrice = 35000,
                    ImageUrl = "/images/products/tra-sua-truyen-thong.jpg",
                    Status = "Active",
                    CategoryId = categoriesMap["Trà Sữa"].Id,
                    HasSizeOption = true,
                    HasSugarOption = true,
                    HasIceOption = true,
                    CreatedAt = DateTime.UtcNow
                },
                new Product
                {
                    Name = "Trà Sữa Ô Long",
                    Description = "Trà sữa ô long thơm lừng, hậu vị thanh khiết",
                    BasePrice = 38000,
                    ImageUrl = "/images/products/tra-sua-o-long.jpg",
                    Status = "Active",
                    CategoryId = categoriesMap["Trà Sữa"].Id,
                    HasSizeOption = true,
                    HasSugarOption = true,
                    HasIceOption = true,
                    CreatedAt = DateTime.UtcNow
                },
                new Product
                {
                    Name = "Cà Phê Sữa Đá",
                    Description = "Cà phê Robusta pha phin truyền thống cùng sữa đặc và đá",
                    BasePrice = 29000,
                    ImageUrl = "/images/products/ca-phe-sua-da.jpg",
                    Status = "Active",
                    CategoryId = categoriesMap["Cà Phê"].Id,
                    HasSizeOption = true,
                    HasSugarOption = false,
                    HasIceOption = true,
                    CreatedAt = DateTime.UtcNow
                },
                new Product
                {
                    Name = "Cà Phê Đen Đá",
                    Description = "Cà phê Robusta nguyên chất không đường thơm nồng",
                    BasePrice = 25000,
                    ImageUrl = "/images/products/ca-phe-den-da.jpg",
                    Status = "Active",
                    CategoryId = categoriesMap["Cà Phê"].Id,
                    HasSizeOption = true,
                    HasSugarOption = true,
                    HasIceOption = true,
                    CreatedAt = DateTime.UtcNow
                },
                new Product
                {
                    Name = "Matcha Đá Xay",
                    Description = "Bột Matcha Uji Nhật Bản đá xay kèm kem tươi béo ngậy",
                    BasePrice = 49000,
                    ImageUrl = "/images/products/matcha-da-xay.jpg",
                    Status = "Active",
                    CategoryId = categoriesMap["Đá Xay"].Id,
                    HasSizeOption = false,
                    HasSugarOption = true,
                    HasIceOption = false,
                    CreatedAt = DateTime.UtcNow
                },
                new Product
                {
                    Name = "Bánh Tiramisu",
                    Description = "Bánh kem vị cà phê truyền thống từ Ý mềm mịn",
                    BasePrice = 35000,
                    ImageUrl = "/images/products/tiramisu.jpg",
                    Status = "Active",
                    CategoryId = categoriesMap["Bánh Ngọt"].Id,
                    HasSizeOption = false,
                    HasSugarOption = false,
                    HasIceOption = false,
                    CreatedAt = DateTime.UtcNow
                },
                new Product
                {
                    Name = "Bánh Croissant",
                    Description = "Bánh sừng bò ngập hương bơ thơm giòn",
                    BasePrice = 28000,
                    ImageUrl = "/images/products/croissant.jpg",
                    Status = "Active",
                    CategoryId = categoriesMap["Bánh Ngọt"].Id,
                    HasSizeOption = false,
                    HasSugarOption = false,
                    HasIceOption = false,
                    CreatedAt = DateTime.UtcNow
                },
                new Product
                {
                    Name = "Khoai Tây Chiên",
                    Description = "Khoai tây chiên giòn rụm kèm tương cà/tương ớt",
                    BasePrice = 30000,
                    ImageUrl = "/images/products/khoai-tay-chien.jpg",
                    Status = "Active",
                    CategoryId = categoriesMap["Đồ Ăn Nhẹ"].Id,
                    HasSizeOption = false,
                    HasSugarOption = false,
                    HasIceOption = false,
                    CreatedAt = DateTime.UtcNow
                }
            };

            context.Products.AddRange(products);
            context.SaveChanges();
        }

        // 4b. Seed ProductSize
        if (!context.ProductSizes.Any())
        {
            var traSuaTruyenThong = context.Products.FirstOrDefault(p => p.Name == "Trà Sữa Truyền Thống");
            var traSuaOLong = context.Products.FirstOrDefault(p => p.Name == "Trà Sữa Ô Long");
            var caPheSuaDa = context.Products.FirstOrDefault(p => p.Name == "Cà Phê Sữa Đá");
            var caPheDenDa = context.Products.FirstOrDefault(p => p.Name == "Cà Phê Đen Đá");

            var productSizes = new List<ProductSize>();

            if (traSuaTruyenThong != null)
            {
                productSizes.AddRange(new[]
                {
                    new ProductSize { ProductId = traSuaTruyenThong.Id, SizeLabel = "S", PriceModifier = 0, IsDefault = true },
                    new ProductSize { ProductId = traSuaTruyenThong.Id, SizeLabel = "M", PriceModifier = 5000, IsDefault = false },
                    new ProductSize { ProductId = traSuaTruyenThong.Id, SizeLabel = "L", PriceModifier = 10000, IsDefault = false }
                });
            }

            if (traSuaOLong != null)
            {
                productSizes.AddRange(new[]
                {
                    new ProductSize { ProductId = traSuaOLong.Id, SizeLabel = "S", PriceModifier = 0, IsDefault = true },
                    new ProductSize { ProductId = traSuaOLong.Id, SizeLabel = "M", PriceModifier = 5000, IsDefault = false },
                    new ProductSize { ProductId = traSuaOLong.Id, SizeLabel = "L", PriceModifier = 10000, IsDefault = false }
                });
            }

            if (caPheSuaDa != null)
            {
                productSizes.AddRange(new[]
                {
                    new ProductSize { ProductId = caPheSuaDa.Id, SizeLabel = "S", PriceModifier = 0, IsDefault = true },
                    new ProductSize { ProductId = caPheSuaDa.Id, SizeLabel = "M", PriceModifier = 4000, IsDefault = false }
                });
            }

            if (caPheDenDa != null)
            {
                productSizes.AddRange(new[]
                {
                    new ProductSize { ProductId = caPheDenDa.Id, SizeLabel = "S", PriceModifier = 0, IsDefault = true },
                    new ProductSize { ProductId = caPheDenDa.Id, SizeLabel = "M", PriceModifier = 4000, IsDefault = false }
                });
            }

            if (productSizes.Any())
            {
                context.ProductSizes.AddRange(productSizes);
                context.SaveChanges();
            }
        }

        // 5. Seed Topping
        if (!context.Toppings.Any())
        {
            var toppings = new List<Topping>
            {
                new Topping { Name = "Trân châu đen", Price = 5000, IsActive = true, CreatedAt = DateTime.UtcNow },
                new Topping { Name = "Trân châu trắng", Price = 6000, IsActive = true, CreatedAt = DateTime.UtcNow },
                new Topping { Name = "Thạch trái cây", Price = 5000, IsActive = true, CreatedAt = DateTime.UtcNow },
                new Topping { Name = "Kem Cheese (Macchiato)", Price = 10000, IsActive = true, CreatedAt = DateTime.UtcNow }
            };

            context.Toppings.AddRange(toppings);
            context.SaveChanges();
        }

        // 6. Seed PointProduct (Sản phẩm đổi điểm)
        if (!context.PointProducts.Any())
        {
            var traSua = context.Products.FirstOrDefault(p => p.Name == "Trà Sữa Truyền Thống");
            if (traSua != null)
            {
                var pointProducts = new List<PointProduct>
                {
                    new PointProduct
                    {
                        Name = "Đổi Trà Sữa Truyền Thống",
                        PointCost = 50,
                        LinkedProductId = traSua.Id,
                        IsActive = true,
                        CreatedAt = DateTime.UtcNow
                    }
                };
                context.PointProducts.AddRange(pointProducts);
                context.SaveChanges();
            }
        }

        // 7. Seed Ingredient (Nguyên liệu thô)
        if (!context.Ingredients.Any())
        {
            var ingredients = new List<Ingredient>
            {
                new Ingredient
                {
                    Name = "Trà đen Oolong",
                    Unit = "kg",
                    CurrentQuantity = 5.0m,
                    MinAlertQuantity = 1.0m,
                    ExpiryAlertDays = 30,
                    ExpiresAt = DateTime.UtcNow.AddMonths(6),
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow
                },
                new Ingredient
                {
                    Name = "Bột trà sữa",
                    Unit = "kg",
                    CurrentQuantity = 10.0m,
                    MinAlertQuantity = 2.0m,
                    ExpiryAlertDays = 30,
                    ExpiresAt = DateTime.UtcNow.AddMonths(6),
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow
                },
                new Ingredient
                {
                    Name = "Sữa đặc",
                    Unit = "lon",
                    CurrentQuantity = 24.0m,
                    MinAlertQuantity = 6.0m,
                    ExpiryAlertDays = 15,
                    ExpiresAt = DateTime.UtcNow.AddMonths(3),
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow
                },
                new Ingredient
                {
                    Name = "Hạt cà phê Robusta",
                    Unit = "kg",
                    CurrentQuantity = 15.0m,
                    MinAlertQuantity = 3.0m,
                    ExpiryAlertDays = 45,
                    ExpiresAt = DateTime.UtcNow.AddMonths(12),
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow
                },
                new Ingredient
                {
                    Name = "Bột Matcha Uji",
                    Unit = "kg",
                    CurrentQuantity = 3.0m,
                    MinAlertQuantity = 0.5m,
                    ExpiryAlertDays = 30,
                    ExpiresAt = DateTime.UtcNow.AddMonths(6),
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow
                }
            };

            context.Ingredients.AddRange(ingredients);
            context.SaveChanges();
        }

        // 8. Seed Voucher
        if (!context.Vouchers.Any())
        {
            var vouchers = new List<Voucher>
            {
                new Voucher
                {
                    Code = "SUMMER10",
                    DiscountType = "Percent",
                    DiscountValue = 10.0m,
                    MinOrderValue = 50000.0m,
                    MaxUsageCount = 100,
                    UsedCount = 0,
                    IsPermanent = false,
                    ExpiresAt = DateTime.UtcNow.AddMonths(2),
                    IsActive = true,
                    CreatedByStaffId = defaultStaff.Id,
                    CreatedAt = DateTime.UtcNow
                },
                new Voucher
                {
                    Code = "CAFE5K",
                    DiscountType = "Fixed",
                    DiscountValue = 5000.0m,
                    MinOrderValue = 30000.0m,
                    MaxUsageCount = null,
                    UsedCount = 0,
                    IsPermanent = true,
                    ExpiresAt = null,
                    IsActive = true,
                    CreatedByStaffId = defaultStaff.Id,
                    CreatedAt = DateTime.UtcNow
                }
            };

            context.Vouchers.AddRange(vouchers);
            context.SaveChanges();
        }

        // 9. Seed LoyaltyTierConfig
        if (!context.LoyaltyTierConfigs.Any())
        {
            var loyaltyConfigs = new List<LoyaltyTierConfig>
            {
                new LoyaltyTierConfig { TierName = "Silver", MinSpendThreshold = 1000000, DiscountPercent = 2.0m, IsActive = true },
                new LoyaltyTierConfig { TierName = "Gold", MinSpendThreshold = 3000000, DiscountPercent = 5.0m, IsActive = true },
                new LoyaltyTierConfig { TierName = "Diamond", MinSpendThreshold = 7000000, DiscountPercent = 10.0m, IsActive = true }
            };

            context.LoyaltyTierConfigs.AddRange(loyaltyConfigs);
            context.SaveChanges();
        }

        // 10. Seed Orders for Dashboard charts
        if (!context.Orders.Any())
        {
            var random = new Random();
            var orders = new List<Order>();
            
            var traSua = context.Products.FirstOrDefault(p => p.Name == "Trà Sữa Truyền Thống");
            var caPheSua = context.Products.FirstOrDefault(p => p.Name == "Cà Phê Sữa Đá");
            var tiramisu = context.Products.FirstOrDefault(p => p.Name == "Bánh Tiramisu");
            
            var staff = context.Staffs.FirstOrDefault(s => s.Role == StaffRole.Cashier) ?? context.Staffs.First();
            var customer = context.Customers.FirstOrDefault();

            var now = DateTime.UtcNow;
            
            for (int i = 6; i >= 0; i--)
            {
                var date = now.AddDays(-i);
                // Seed 2-4 orders per day
                int ordersCount = random.Next(2, 5);
                for (int j = 0; j < ordersCount; j++)
                {
                    var orderTime = new DateTime(date.Year, date.Month, date.Day, random.Next(8, 21), random.Next(0, 60), 0, DateTimeKind.Utc);
                    var isCompleted = random.Next(0, 10) < 9; // 90% completed
                    var subtotal = 0m;
                    
                    var orderItems = new List<OrderItem>();
                    
                    if (traSua != null && random.Next(0, 2) == 0)
                    {
                        var qty = random.Next(1, 3);
                        var price = traSua.BasePrice;
                        subtotal += price * qty;
                        orderItems.Add(new OrderItem
                        {
                            ProductId = traSua.Id,
                            Quantity = qty,
                            UnitPrice = price,
                            ItemTotal = price * qty,
                            Notes = "",
                            SizeLabel = "S",
                            SugarLevel = "100",
                            IceLevel = "100",
                            BarStatus = isCompleted ? "Done" : "Pending",
                            PastryStatus = "NA"
                        });
                    }

                    if (caPheSua != null && (orderItems.Count == 0 || random.Next(0, 2) == 0))
                    {
                        var qty = random.Next(1, 3);
                        var price = caPheSua.BasePrice;
                        subtotal += price * qty;
                        orderItems.Add(new OrderItem
                        {
                            ProductId = caPheSua.Id,
                            Quantity = qty,
                            UnitPrice = price,
                            ItemTotal = price * qty,
                            Notes = "",
                            SizeLabel = "S",
                            SugarLevel = "100",
                            IceLevel = "100",
                            BarStatus = isCompleted ? "Done" : "Pending",
                            PastryStatus = "NA"
                        });
                    }

                    if (tiramisu != null && random.Next(0, 3) == 0)
                    {
                        var qty = 1;
                        var price = tiramisu.BasePrice;
                        subtotal += price * qty;
                        orderItems.Add(new OrderItem
                        {
                            ProductId = tiramisu.Id,
                            Quantity = qty,
                            UnitPrice = price,
                            ItemTotal = price * qty,
                            Notes = "",
                            SizeLabel = "Normal",
                            SugarLevel = "NA",
                            IceLevel = "NA",
                            BarStatus = "NA",
                            PastryStatus = isCompleted ? "Done" : "Pending"
                        });
                    }

                    if (orderItems.Count == 0) continue; // Skip if empty

                    var discount = random.Next(0, 4) == 0 ? 5000m : 0m;
                    var total = subtotal - discount;

                    orders.Add(new Order
                    {
                        OrderCode = $"ORD{orderTime:yyyyMMddHHmmss}{random.Next(10, 99)}",
                        Type = random.Next(0, 2) == 0 ? "DineIn" : "TakeAway",
                        TableNumber = random.Next(0, 2) == 0 ? random.Next(1, 10).ToString() : null,
                        SubOrderIndex = 1,
                        CustomerId = customer?.Id,
                        StaffId = staff.Id,
                        CustomerName = customer?.FullName ?? "Khách vãng lai",
                        CustomerPhone = customer?.Phone ?? "",
                        Status = isCompleted ? OrderStatus.Completed : OrderStatus.Cancelled,
                        PaymentStatus = isCompleted ? "Paid" : "Unpaid",
                        PaymentMethod = isCompleted ? (random.Next(0, 2) == 0 ? "Cash" : "Transfer") : "",
                        SubTotal = subtotal,
                        DiscountAmount = discount,
                        TotalAmount = total,
                        AmountReceived = isCompleted ? total : 0m,
                        AmountChange = 0m,
                        ConfirmedAt = orderTime.AddMinutes(2),
                        CompletedAt = isCompleted ? orderTime.AddMinutes(15) : null,
                        CreatedAt = orderTime,
                        OrderItems = orderItems
                    });
                }
            }

            context.Orders.AddRange(orders);
            context.SaveChanges();
        }
    }
}
