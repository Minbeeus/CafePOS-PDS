using CafePOS.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CafePOS.Infrastructure.Persistence.Configurations;

public class OrderConfiguration : IEntityTypeConfiguration<Order>
{
    public void Configure(EntityTypeBuilder<Order> builder)
    {
        builder.HasKey(o => o.Id);

        builder.Property(o => o.OrderCode)
            .IsRequired()
            .HasMaxLength(50);

        builder.HasIndex(o => o.OrderCode)
            .IsUnique();

        builder.Property(o => o.Type)
            .IsRequired()
            .HasMaxLength(20);

        builder.Property(o => o.TableNumber)
            .HasMaxLength(10);

        builder.Property(o => o.CustomerName)
            .HasMaxLength(100);

        builder.Property(o => o.CustomerPhone)
            .HasMaxLength(20);

        builder.Property(o => o.PaymentStatus)
            .HasMaxLength(20);

        builder.Property(o => o.PaymentMethod)
            .HasMaxLength(20);

        // Concurrency token
        builder.Property(o => o.RowVersion)
            .IsRowVersion();

        // Relationships
        builder.HasOne(o => o.Customer)
            .WithMany()
            .HasForeignKey(o => o.CustomerId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasOne(o => o.Staff)
            .WithMany()
            .HasForeignKey(o => o.StaffId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(o => o.ParentOrder)
            .WithMany()
            .HasForeignKey(o => o.ParentOrderId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
