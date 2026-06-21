using CafePOS.Domain.Enums;

namespace CafePOS.Domain.Entities;

public class Customer
{
    public int Id { get; set; }
    public string FullName { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string? GoogleId { get; set; }
    public string PasswordHash { get; set; } = string.Empty;
    public decimal TotalSpend { get; set; }
    public LoyaltyTier LoyaltyTier { get; set; }
    public int CurrentPoints { get; set; }
    public DateTime PointsResetAt { get; set; }
    public DateTime CreatedAt { get; set; }
}
