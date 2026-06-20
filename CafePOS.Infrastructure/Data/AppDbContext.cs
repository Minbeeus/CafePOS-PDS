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
    public DbSet<Topping> Toppings => Set<Topping>();
    public DbSet<Order> Orders => Set<Order>();
    public DbSet<OrderItem> OrderItems => Set<OrderItem>();
    public DbSet<Shift> Shifts => Set<Shift>();
    public DbSet<Voucher> Vouchers => Set<Voucher>();
    public DbSet<PointProduct> PointProducts => Set<PointProduct>();
    public DbSet<Ingredient> Ingredients => Set<Ingredient>();
    public DbSet<InventoryCheck> InventoryChecks => Set<InventoryCheck>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        
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
    }
}
