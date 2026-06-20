namespace CafePOS.Domain.Entities;

public class InventoryCheck
{
    public int Id { get; set; }
    public int ShiftId { get; set; }
    public Shift? Shift { get; set; }
    public int StaffId { get; set; }
    public Staff? Staff { get; set; }
    public DateTime CheckedAt { get; set; }
    public string Status { get; set; } = string.Empty; // PendingApproval, Approved
    public int? ApprovedByStaffId { get; set; }
    public Staff? ApprovedBy { get; set; }
    public DateTime? ApprovedAt { get; set; }
    public string Notes { get; set; } = string.Empty;
}
