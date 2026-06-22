namespace CafePOS.Domain.Entities;

public class Topping
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public bool IsActive { get; set; }
    public DateTime CreatedAt { get; set; }
    public ICollection<Product> Products { get; set; } = new List<Product>();
}

