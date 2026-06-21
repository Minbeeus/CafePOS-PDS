using System;

namespace CafePOS.Domain.Entities;

public class VoucherUsage
{
    public int Id { get; set; }
    public int VoucherId { get; set; }
    public Voucher? Voucher { get; set; }
    public int OrderId { get; set; }
    public Order? Order { get; set; }
    public int? CustomerId { get; set; }
    public Customer? Customer { get; set; }
    public DateTime UsedAt { get; set; }
}
