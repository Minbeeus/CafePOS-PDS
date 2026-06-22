using CafePOS.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System.Collections.Generic;

namespace CafePOS.Infrastructure.Persistence.Configurations;

public class ProductConfiguration : IEntityTypeConfiguration<Product>
{
    public void Configure(EntityTypeBuilder<Product> builder)
    {
        builder.HasKey(p => p.Id);

        builder.Property(p => p.Name)
            .IsRequired()
            .HasMaxLength(150);

        builder.Property(p => p.Description)
            .HasMaxLength(500);

        builder.Property(p => p.ImageUrl)
            .HasMaxLength(255);

        builder.Property(p => p.BasePrice)
            .HasColumnType("decimal(18,2)")
            .IsRequired();

        builder.Property(p => p.Status)
            .HasMaxLength(50);

        // Relationships
        builder.HasOne(p => p.Category)
            .WithMany(c => c.Products)
            .HasForeignKey(p => p.CategoryId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(p => p.Sizes)
            .WithOne(ps => ps.Product)
            .HasForeignKey(ps => ps.ProductId)
            .OnDelete(DeleteBehavior.Cascade);

        // Many-to-many Product - Topping
        builder.HasMany(p => p.Toppings)
            .WithMany(t => t.Products)
            .UsingEntity<Dictionary<string, object>>(
                "ProductToppings",
                j => j.HasOne<Topping>().WithMany().HasForeignKey("ToppingId").OnDelete(DeleteBehavior.Cascade),
                j => j.HasOne<Product>().WithMany().HasForeignKey("ProductId").OnDelete(DeleteBehavior.Cascade),
                t => t.ToTable("ProductToppings")
            );
    }
}
