namespace CafePOS.Domain.Entities;

public class LoyaltyTierConfig
{
    public int Id { get; set; }
    public string TierName { get; set; } = string.Empty; // Silver, Gold
    public decimal MinSpendThreshold { get; set; }
    public decimal DiscountPercent { get; set; }
    public bool IsActive { get; set; }
}
