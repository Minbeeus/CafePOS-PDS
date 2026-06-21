namespace CafePOS.Domain.Entities;

public class OrderItemTopping
{
    public int Id { get; set; }
    public int OrderItemId { get; set; }
    public OrderItem? OrderItem { get; set; }
    public int ToppingId { get; set; }
    public Topping? Topping { get; set; }
    public decimal ToppingPrice { get; set; }
}
