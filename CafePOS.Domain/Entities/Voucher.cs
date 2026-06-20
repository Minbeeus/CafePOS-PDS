namespace CafePOS.Domain.Entities;

public class Voucher
{
    public int Id { get; set; }
    public string Code { get; set; } = string.Empty;
    public string DiscountType { get; set; } = string.Empty; // Percent, Fixed
    public decimal DiscountValue { get; set; }
    public decimal MinOrderValue { get; set; }
    public int? MaxUsageCount { get; set; }
    public int UsedCount { get; set; }
    public bool IsPermanent { get; set; }
    public DateTime? ExpiresAt { get; set; }
    public bool IsActive { get; set; }
    public int CreatedByStaffId { get; set; }
    public Staff? CreatedBy { get; set; }
    public DateTime CreatedAt { get; set; }
}
