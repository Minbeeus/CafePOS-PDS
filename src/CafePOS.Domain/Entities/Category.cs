namespace CafePOS.Domain.Entities;

public class Category
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string DisplayStation { get; set; } = string.Empty; // Bar, Pastry, Both
    public int DisplayOrder { get; set; }
    public bool IsActive { get; set; }
    public ICollection<Product> Products { get; set; } = new List<Product>();
    public bool IsDeleted { get; set; }
    public DateTime? DeletedAt { get; set; }
    public int? DeletedBy { get; set; }
}
