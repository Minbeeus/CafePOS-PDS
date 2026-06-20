namespace CafePOS.Domain.Entities;

public class ProductSize
{
    public int Id { get; set; }
    public int ProductId { get; set; }
    public Product? Product { get; set; }
    public string SizeLabel { get; set; } = string.Empty; // S, M, L
    public decimal PriceModifier { get; set; }
    public bool IsDefault { get; set; }
}
