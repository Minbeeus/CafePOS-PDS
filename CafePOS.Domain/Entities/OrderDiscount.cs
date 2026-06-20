using System;

namespace CafePOS.Domain.Entities;

public class OrderDiscount
{
    public int Id { get; set; }
    public int OrderId { get; set; }
    public Order? Order { get; set; }
    public string DiscountType { get; set; } = string.Empty; // Loyalty, Voucher, Manual
    public decimal DiscountValue { get; set; }
    public string DiscountDescription { get; set; } = string.Empty;
    public int? VoucherId { get; set; }
    public Voucher? Voucher { get; set; }
    public int? ApprovedByStaffId { get; set; }
    public Staff? ApprovedBy { get; set; }
    public DateTime? ApprovedAt { get; set; }
}
