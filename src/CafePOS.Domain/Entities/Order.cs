using CafePOS.Domain.Enums;

namespace CafePOS.Domain.Entities;

public class Order
{
    public int Id { get; set; }
    public string OrderCode { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty; // DineIn, TakeAway, Online
    public string? TableNumber { get; set; }
    public int SubOrderIndex { get; set; } // 1=gốc, 2+=gọi thêm
    public int? ParentOrderId { get; set; }
    public Order? ParentOrder { get; set; }
    public int? CustomerId { get; set; }
    public Customer? Customer { get; set; }
    public int StaffId { get; set; }
    public Staff? Staff { get; set; }
    public string CustomerName { get; set; } = string.Empty;
    public string CustomerPhone { get; set; } = string.Empty;
    public OrderStatus Status { get; set; }
    public string PaymentStatus { get; set; } = string.Empty; // Unpaid, Paid
    public string PaymentMethod { get; set; } = string.Empty; // Cash, Transfer, Mixed
    public decimal SubTotal { get; set; }
    public decimal DiscountAmount { get; set; }
    public decimal TotalAmount { get; set; }
    public decimal AmountReceived { get; set; }
    public decimal AmountChange { get; set; }
    public DateTime? ScheduledPickupTime { get; set; }
    public DateTime? ConfirmedAt { get; set; }
    public DateTime? CompletedAt { get; set; }
    public DateTime? ClosedAt { get; set; }
    public DateTime CreatedAt { get; set; }
    public ICollection<OrderItem> OrderItems { get; set; } = new List<OrderItem>();
    public ICollection<OrderDiscount> Discounts { get; set; } = new List<OrderDiscount>();
    public byte[]? RowVersion { get; set; }
}

