namespace CafePOS.Domain.Entities;

public class ProductIngredient
{
    public int ProductId { get; set; }
    public int IngredientId { get; set; }
    public decimal Quantity { get; set; }
}
