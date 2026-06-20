namespace CafePOS.Domain.Entities;

public class PointProduct
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public int PointCost { get; set; }
    public int LinkedProductId { get; set; }
    public Product? LinkedProduct { get; set; }
    public bool IsActive { get; set; }
    public DateTime CreatedAt { get; set; }
}
