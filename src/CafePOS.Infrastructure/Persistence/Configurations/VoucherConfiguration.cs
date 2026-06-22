using CafePOS.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CafePOS.Infrastructure.Persistence.Configurations;

public class VoucherConfiguration : IEntityTypeConfiguration<Voucher>
{
    public void Configure(EntityTypeBuilder<Voucher> builder)
    {
        builder.HasKey(v => v.Id);

        builder.Property(v => v.Code)
            .IsRequired()
            .HasMaxLength(50);

        builder.HasIndex(v => v.Code)
            .IsUnique();

        builder.Property(v => v.DiscountType)
            .IsRequired()
            .HasMaxLength(20);

        builder.Property(v => v.DiscountValue)
            .HasColumnType("decimal(18,2)")
            .IsRequired();

        builder.Property(v => v.MinOrderValue)
            .HasColumnType("decimal(18,2)")
            .IsRequired();

        // Relationships
        builder.HasOne(v => v.CreatedBy)
            .WithMany()
            .HasForeignKey(v => v.CreatedByStaffId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
