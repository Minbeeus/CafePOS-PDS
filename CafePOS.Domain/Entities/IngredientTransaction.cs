using System;

namespace CafePOS.Domain.Entities;

public class IngredientTransaction
{
    public int Id { get; set; }
    public int IngredientId { get; set; }
    public Ingredient? Ingredient { get; set; }
    public string TransactionType { get; set; } = string.Empty; // In, Out, Adjust
    public decimal Quantity { get; set; }
    public int? RelatedOrderId { get; set; }
    public Order? RelatedOrder { get; set; }
    public int? CreatedByStaffId { get; set; }
    public Staff? CreatedByStaff { get; set; }
    public string Notes { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
}
