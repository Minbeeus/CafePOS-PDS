using CafePOS.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CafePOS.Infrastructure.Persistence.Configurations;

public class PaymentConfiguration : IEntityTypeConfiguration<Payment>
{
    public void Configure(EntityTypeBuilder<Payment> builder)
    {
        builder.ToTable("Payment");
        builder.HasKey(p => p.Id);

        builder.Property(p => p.Amount)
            .HasColumnType("decimal(18,2)")
            .IsRequired();

        builder.Property(p => p.AmountReceived)
            .HasColumnType("decimal(18,2)");

        builder.Property(p => p.AmountChange)
            .HasColumnType("decimal(18,2)");

        builder.Property(p => p.ReferenceCode)
            .HasMaxLength(100);

        // Relationships
        builder.HasOne(p => p.Order)
            .WithMany()
            .HasForeignKey(p => p.OrderId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(p => p.CreatedByStaff)
            .WithMany()
            .HasForeignKey(p => p.CreatedByStaffId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
