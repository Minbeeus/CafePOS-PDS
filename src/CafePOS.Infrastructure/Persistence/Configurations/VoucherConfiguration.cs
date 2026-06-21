using CafePOS.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CafePOS.Infrastructure.Persistence.Configurations;

public class VoucherConfiguration : IEntityTypeConfiguration<Voucher>
{
    public void Configure(EntityTypeBuilder<Voucher> builder)
    {
    }
}
