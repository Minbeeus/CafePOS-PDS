using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using CafePOS.Infrastructure.Data;
using CafePOS.Domain.Entities;
using Microsoft.AspNetCore.Authorization;
using System;
using System.Threading.Tasks;
using System.Linq;

namespace CafePOS.API.Controllers;

[ApiController]
[Route("api/v1/[controller]")]
public class ShiftsController : ControllerBase
{
    private readonly AppDbContext _context;

    public ShiftsController(AppDbContext context)
    {
        _context = context;
    }

    [HttpGet("active")]
    [Authorize]
    public async Task<IActionResult> GetActiveShift()
    {
        var activeShift = await _context.Shifts
            .Include(s => s.OpenedBy)
            .FirstOrDefaultAsync(s => s.Status == "Open");

        if (activeShift == null)
        {
            return Ok(new { success = true, data = (object?)null, message = "Không có ca làm việc nào đang mở." });
        }

        return Ok(new
        {
            success = true,
            data = new
            {
                activeShift.Id,
                activeShift.ShiftDate,
                activeShift.OpenedAt,
                activeShift.OpeningCash,
                OpenedByName = activeShift.OpenedBy?.FullName ?? "Unknown",
                activeShift.Status
            }
        });
    }

    [HttpPost("open")]
    [Authorize(Roles = "Owner,ShiftLeader")]
    public async Task<IActionResult> OpenShift([FromBody] OpenShiftRequest request)
    {
        var activeShift = await _context.Shifts.AnyAsync(s => s.Status == "Open");
        if (activeShift)
        {
            return BadRequest(new { success = false, message = "Đã có một ca làm việc đang mở." });
        }

        var staffIdClaim = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value 
            ?? User.FindFirst("sub")?.Value;
        
        if (!int.TryParse(staffIdClaim, out int staffId))
        {
            return Unauthorized(new { success = false, message = "Không xác định được nhân viên." });
        }

        var shift = new Shift
        {
            ShiftDate = DateTime.UtcNow.Date,
            OpenedByStaffId = staffId,
            OpenedAt = DateTime.UtcNow,
            OpeningCash = request.OpeningCash,
            Status = "Open"
        };

        _context.Shifts.Add(shift);
        await _context.SaveChangesAsync();

        return Ok(new { success = true, data = shift, message = "Mở ca làm việc thành công." });
    }

    [HttpPost("close")]
    [Authorize(Roles = "Owner,ShiftLeader")]
    public async Task<IActionResult> CloseShift([FromBody] CloseShiftRequest request)
    {
        var activeShift = await _context.Shifts
            .FirstOrDefaultAsync(s => s.Status == "Open");

        if (activeShift == null)
        {
            return BadRequest(new { success = false, message = "Không có ca làm việc nào đang mở." });
        }

        var staffIdClaim = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value 
            ?? User.FindFirst("sub")?.Value;
        
        if (!int.TryParse(staffIdClaim, out int staffId))
        {
            return Unauthorized(new { success = false, message = "Không xác định được nhân viên." });
        }

        // Calculate expected sales from completed orders in this shift
        var shiftOrders = await _context.Orders
            .Where(o => o.CreatedAt >= activeShift.OpenedAt && o.Status == Domain.Enums.OrderStatus.Completed)
            .ToListAsync();

        decimal expectedCash = activeShift.OpeningCash + shiftOrders.Where(o => o.PaymentMethod == "Cash").Sum(o => o.TotalAmount);
        decimal expectedTransfer = shiftOrders.Where(o => o.PaymentMethod == "Transfer").Sum(o => o.TotalAmount);

        activeShift.ClosedByStaffId = staffId;
        activeShift.ClosedAt = DateTime.UtcNow;
        activeShift.ExpectedCash = expectedCash;
        activeShift.ExpectedTransfer = expectedTransfer;
        activeShift.ActualCash = request.ActualCash;
        activeShift.ActualTransfer = request.ActualTransfer;
        activeShift.CashDifference = request.ActualCash - expectedCash;
        activeShift.TransferDifference = request.ActualTransfer - expectedTransfer;
        activeShift.Notes = request.Notes ?? "";
        activeShift.Status = "Closed";

        await _context.SaveChangesAsync();

        return Ok(new { success = true, data = activeShift, message = "Đóng ca làm việc thành công." });
    }
}

public class OpenShiftRequest
{
    public decimal OpeningCash { get; set; }
}

public class CloseShiftRequest
{
    public decimal ActualCash { get; set; }
    public decimal ActualTransfer { get; set; }
    public string? Notes { get; set; }
}
