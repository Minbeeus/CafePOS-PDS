using Microsoft.EntityFrameworkCore;
using CafePOS.Domain.Entities;

namespace CafePOS.Infrastructure.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
    }

    public DbSet<Staff> Staffs => Set<Staff>();
    public DbSet<Customer> Customers => Set<Customer>();
    public DbSet<Category> Categories => Set<Category>();
    public DbSet<Product> Products => Set<Product>();
    public DbSet<ProductSize> ProductSizes => Set<ProductSize>();
    public DbSet<Topping> Toppings => Set<Topping>();
    public DbSet<Order> Orders => Set<Order>();
    public DbSet<OrderItem> OrderItems => Set<OrderItem>();
    public DbSet<OrderItemTopping> OrderItemToppings => Set<OrderItemTopping>();
    public DbSet<OrderDiscount> OrderDiscounts => Set<OrderDiscount>();
    public DbSet<LoyaltyTierConfig> LoyaltyTierConfigs => Set<LoyaltyTierConfig>();
    public DbSet<PointTransaction> PointTransactions => Set<PointTransaction>();
    public DbSet<Voucher> Vouchers => Set<Voucher>();
    public DbSet<VoucherUsage> VoucherUsages => Set<VoucherUsage>();
    public DbSet<Shift> Shifts => Set<Shift>();
    public DbSet<Ingredient> Ingredients => Set<Ingredient>();
    public DbSet<InventoryCheck> InventoryChecks => Set<InventoryCheck>();
    public DbSet<InventoryCheckItem> InventoryCheckItems => Set<InventoryCheckItem>();
    public DbSet<IngredientTransaction> IngredientTransactions => Set<IngredientTransaction>();
    public DbSet<PointProduct> PointProducts => Set<PointProduct>();
    public DbSet<Payment> Payments => Set<Payment>();
    public DbSet<AuditLog> AuditLogs => Set<AuditLog>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        
        // Apply global query filters for Soft Delete
        modelBuilder.Entity<Product>().HasQueryFilter(p => !p.IsDeleted);
        modelBuilder.Entity<Category>().HasQueryFilter(c => !c.IsDeleted);
        modelBuilder.Entity<Topping>().HasQueryFilter(t => !t.IsDeleted);

        // Apply separate configurations from assembly
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);
        
        // Configure Shift relationships with Staff
        modelBuilder.Entity<Shift>()
            .HasOne(s => s.OpenedBy)
            .WithMany()
            .HasForeignKey(s => s.OpenedByStaffId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<Shift>()
            .HasOne(s => s.ClosedBy)
            .WithMany()
            .HasForeignKey(s => s.ClosedByStaffId)
            .OnDelete(DeleteBehavior.Restrict);

        // Configure InventoryCheck relationships with Staff
        modelBuilder.Entity<InventoryCheck>()
            .HasOne(ic => ic.Staff)
            .WithMany()
            .HasForeignKey(ic => ic.StaffId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<InventoryCheck>()
            .HasOne(ic => ic.ApprovedBy)
            .WithMany()
            .HasForeignKey(ic => ic.ApprovedByStaffId)
            .OnDelete(DeleteBehavior.Restrict);

        // Configure InventoryCheckItem relationships
        modelBuilder.Entity<InventoryCheckItem>()
            .HasOne(ici => ici.InventoryCheck)
            .WithMany(ic => ic.CheckItems)
            .HasForeignKey(ici => ici.InventoryCheckId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<InventoryCheckItem>()
            .HasOne(ici => ici.Ingredient)
            .WithMany()
            .HasForeignKey(ici => ici.IngredientId)
            .OnDelete(DeleteBehavior.Restrict);

        // Configure IngredientTransaction relationships
        modelBuilder.Entity<IngredientTransaction>()
            .HasOne(it => it.Ingredient)
            .WithMany()
            .HasForeignKey(it => it.IngredientId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<IngredientTransaction>()
            .HasOne(it => it.RelatedOrder)
            .WithMany()
            .HasForeignKey(it => it.RelatedOrderId)
            .OnDelete(DeleteBehavior.SetNull);

        modelBuilder.Entity<IngredientTransaction>()
            .HasOne(it => it.CreatedByStaff)
            .WithMany()
            .HasForeignKey(it => it.CreatedByStaffId)
            .OnDelete(DeleteBehavior.Restrict);

        // Configure PointTransaction relationships
        modelBuilder.Entity<PointTransaction>()
            .HasOne(pt => pt.Customer)
            .WithMany()
            .HasForeignKey(pt => pt.CustomerId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<PointTransaction>()
            .HasOne(pt => pt.Order)
            .WithMany()
            .HasForeignKey(pt => pt.OrderId)
            .OnDelete(DeleteBehavior.SetNull);

        // Configure VoucherUsage relationships
        modelBuilder.Entity<VoucherUsage>()
            .HasOne(vu => vu.Voucher)
            .WithMany()
            .HasForeignKey(vu => vu.VoucherId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<VoucherUsage>()
            .HasOne(vu => vu.Order)
            .WithMany()
            .HasForeignKey(vu => vu.OrderId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<VoucherUsage>()
            .HasOne(vu => vu.Customer)
            .WithMany()
            .HasForeignKey(vu => vu.CustomerId)
            .OnDelete(DeleteBehavior.SetNull);

        // Configure OrderDiscount relationships
        modelBuilder.Entity<OrderDiscount>()
            .HasOne(od => od.Order)
            .WithMany(o => o.Discounts)
            .HasForeignKey(od => od.OrderId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<OrderDiscount>()
            .HasOne(od => od.Voucher)
            .WithMany()
            .HasForeignKey(od => od.VoucherId)
            .OnDelete(DeleteBehavior.SetNull);

        modelBuilder.Entity<OrderDiscount>()
            .HasOne(od => od.ApprovedBy)
            .WithMany()
            .HasForeignKey(od => od.ApprovedByStaffId)
            .OnDelete(DeleteBehavior.Restrict);

        // Configure OrderItemTopping relationships
        modelBuilder.Entity<OrderItemTopping>()
            .HasOne(oit => oit.OrderItem)
            .WithMany(oi => oi.Toppings)
            .HasForeignKey(oit => oit.OrderItemId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<OrderItemTopping>()
            .HasOne(oit => oit.Topping)
            .WithMany()
            .HasForeignKey(oit => oit.ToppingId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}

