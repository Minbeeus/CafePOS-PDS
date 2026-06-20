namespace CafePOS.Domain.Entities;

public class InventoryCheckItem
{
    public int Id { get; set; }
    public int InventoryCheckId { get; set; }
    public InventoryCheck? InventoryCheck { get; set; }
    public int IngredientId { get; set; }
    public Ingredient? Ingredient { get; set; }
    public decimal SystemQuantity { get; set; }
    public decimal ActualQuantity { get; set; }
    public decimal Difference { get; set; }
}
