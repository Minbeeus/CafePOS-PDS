using System;
using System.Collections.Generic;

namespace CafePOS.Application.DTOs.Orders;

public class CreateOrderRequest
{
    public string Type { get; set; } = string.Empty; // DineIn, TakeAway, Online
    public string? CustomerPhone { get; set; }
    public string? CustomerName { get; set; }
    public DateTime? ScheduledPickupTime { get; set; }
    public List<OrderItemRequest> Items { get; set; } = new();
    public string? VoucherCode { get; set; }
    public bool ApplyLoyaltyDiscount { get; set; }
    public decimal ManualDiscountPercent { get; set; }
    public string? ManualDiscountApproverCode { get; set; } // PIN code of manager
}

public class OrderItemRequest
{
    public int ProductId { get; set; }
    public int Quantity { get; set; }
    public string SizeLabel { get; set; } = "Regular";
    public string SugarLevel { get; set; } = "100";
    public string IceLevel { get; set; } = "100";
    public List<int> ToppingIds { get; set; } = new();
    public string Notes { get; set; } = string.Empty;
    public bool IsPointRedemption { get; set; }
}
