namespace CafePOS.Domain.Entities;

public class OrderItem
{
    public int Id { get; set; }
    public int OrderId { get; set; }
    public Order? Order { get; set; }
    public int ProductId { get; set; }
    public Product? Product { get; set; }
    public int Quantity { get; set; }
    public decimal UnitPrice { get; set; }
    public decimal ItemTotal { get; set; }
    public string Notes { get; set; } = string.Empty;
    public string SizeLabel { get; set; } = string.Empty; // S, M, L
    public string SugarLevel { get; set; } = string.Empty; // 0, 30, 50, 70, Extra
    public string IceLevel { get; set; } = string.Empty; // 0, 50, 100
    public bool IsPointRedemption { get; set; }
    public string BarStatus { get; set; } = string.Empty; // NA, Pending, InProgress, Done
    public string PastryStatus { get; set; } = string.Empty; // NA, Pending, InProgress, Done
    public ICollection<OrderItemTopping> Toppings { get; set; } = new List<OrderItemTopping>();
}
