namespace CafePOS.Domain.Entities;

public class Ingredient
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Unit { get; set; } = string.Empty; // kg, lít, gói, hộp...
    public decimal CurrentQuantity { get; set; }
    public decimal MinAlertQuantity { get; set; }
    public int ExpiryAlertDays { get; set; }
    public DateTime ExpiresAt { get; set; }
    public bool IsActive { get; set; }
    public DateTime CreatedAt { get; set; }
}
