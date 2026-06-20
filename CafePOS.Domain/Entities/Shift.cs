namespace CafePOS.Domain.Entities;

public class Shift
{
    public int Id { get; set; }
    public DateTime ShiftDate { get; set; }
    public int OpenedByStaffId { get; set; }
    public Staff? OpenedBy { get; set; }
    public int? ClosedByStaffId { get; set; }
    public Staff? ClosedBy { get; set; }
    public DateTime OpenedAt { get; set; }
    public DateTime? ClosedAt { get; set; }
    public decimal OpeningCash { get; set; }
    public decimal ExpectedCash { get; set; }
    public decimal ExpectedTransfer { get; set; }
    public decimal ActualCash { get; set; }
    public decimal ActualTransfer { get; set; }
    public decimal CashDifference { get; set; }
    public decimal TransferDifference { get; set; }
    public string Notes { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty; // Open, Closed
}
