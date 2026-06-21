using System;

namespace CafePOS.Domain.Entities;

public class PointTransaction
{
    public int Id { get; set; }
    public int CustomerId { get; set; }
    public Customer? Customer { get; set; }
    public int? OrderId { get; set; }
    public Order? Order { get; set; }
    public string TransactionType { get; set; } = string.Empty; // Earn, Redeem, Reset
    public int Points { get; set; }
    public string Description { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
}
