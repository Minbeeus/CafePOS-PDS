using System;
using CafePOS.Domain.Enums;

namespace CafePOS.Domain.Entities;

public class Payment
{
    public int Id { get; set; }
    public int OrderId { get; set; }
    public Order? Order { get; set; }
    public decimal Amount { get; set; }
    public decimal AmountReceived { get; set; }
    public decimal AmountChange { get; set; }
    public string ReferenceCode { get; set; } = string.Empty;
    public int CreatedByStaffId { get; set; }
    public Staff? CreatedByStaff { get; set; }
    public PaymentMethod Method { get; set; }
    public PaymentStatus Status { get; set; }
    public DateTime PaidAt { get; set; }
    public DateTime CreatedAt { get; set; }
}

