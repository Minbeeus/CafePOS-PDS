namespace CafePOS.Domain.Entities;

public class Product
{
    public int Id { get; set; }
    public int CategoryId { get; set; }
    public Category? Category { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string ImageUrl { get; set; } = string.Empty;
    public decimal BasePrice { get; set; }
    public string Status { get; set; } = string.Empty; // Active, Inactive, OutOfStock
    public bool HasSizeOption { get; set; }
    public bool HasSugarOption { get; set; }
    public bool HasIceOption { get; set; }
    public DateTime CreatedAt { get; set; }
    public ICollection<ProductSize> Sizes { get; set; } = new List<ProductSize>();
    public ICollection<Topping> Toppings { get; set; } = new List<Topping>();
    public bool IsDeleted { get; set; }
    public DateTime? DeletedAt { get; set; }
    public int? DeletedBy { get; set; }
}

