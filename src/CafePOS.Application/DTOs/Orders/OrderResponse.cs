using System;
using System.Collections.Generic;

namespace CafePOS.Application.DTOs.Orders;

public class OrderResponse
{
    public int Id { get; set; }
    public string OrderCode { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty;
    public string? TableNumber { get; set; }
    public int SubOrderIndex { get; set; }
    public int? ParentOrderId { get; set; }
    public int? CustomerId { get; set; }
    public string CustomerName { get; set; } = string.Empty;
    public string CustomerPhone { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public string PaymentStatus { get; set; } = string.Empty;
    public string PaymentMethod { get; set; } = string.Empty;
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
    public string? RowVersionBase64 { get; set; }
    public List<OrderItemResponse> Items { get; set; } = new();
}

public class OrderItemResponse
{
    public int Id { get; set; }
    public int ProductId { get; set; }
    public string ProductName { get; set; } = string.Empty;
    public int Quantity { get; set; }
    public decimal UnitPrice { get; set; }
    public decimal ItemTotal { get; set; }
    public string SizeLabel { get; set; } = string.Empty;
    public string SugarLevel { get; set; } = string.Empty;
    public string IceLevel { get; set; } = string.Empty;
    public string Notes { get; set; } = string.Empty;
    public bool IsPointRedemption { get; set; }
    public string BarStatus { get; set; } = string.Empty;
    public string PastryStatus { get; set; } = string.Empty;
    public List<OrderItemToppingResponse> Toppings { get; set; } = new();
}

public class OrderItemToppingResponse
{
    public int Id { get; set; }
    public int ToppingId { get; set; }
    public string ToppingName { get; set; } = string.Empty;
    public decimal ToppingPrice { get; set; }
}
