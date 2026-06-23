using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using CafePOS.Application.Interfaces.Repositories;
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
    private readonly IShiftRepository _shiftRepository;
    private readonly IOrderRepository _orderRepository;

    public ShiftsController(IShiftRepository shiftRepository, IOrderRepository orderRepository)
    {
        _shiftRepository = shiftRepository;
        _orderRepository = orderRepository;
    }

    [HttpGet("active")]
    [Authorize]
    public async Task<IActionResult> GetActiveShift()
    {
        var activeShift = await _shiftRepository.GetActiveShiftWithOpenedByAsync();

        if (activeShift == null)
        {
            return Ok(null);
        }

        return Ok(new
        {
            activeShift.Id,
            activeShift.ShiftDate,
            activeShift.OpenedAt,
            activeShift.OpeningCash,
            OpenedByName = activeShift.OpenedBy?.FullName ?? "Unknown",
            activeShift.Status
        });
    }

    [HttpPost("open")]
    [Authorize(Roles = "Owner,ShiftLeader")]
    public async Task<IActionResult> OpenShift([FromBody] OpenShiftRequest request)
    {
        var hasActiveShift = await _shiftRepository.GetActiveShiftAsync() != null;
        if (hasActiveShift)
        {
            return BadRequest(new { message = "Đã có một ca làm việc đang mở." });
        }

        var staffIdClaim = User?.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value 
            ?? User?.FindFirst("sub")?.Value;
        
        if (!int.TryParse(staffIdClaim, out int staffId))
        {
            return Unauthorized(new { message = "Không xác định được nhân viên." });
        }

        var shift = new Shift
        {
            ShiftDate = DateTime.UtcNow.Date,
            OpenedByStaffId = staffId,
            OpenedAt = DateTime.UtcNow,
            OpeningCash = request.OpeningCash,
            Status = "Open"
        };

        await _shiftRepository.AddAsync(shift);
 
        return Ok(shift);
    }

    [HttpPost("close")]
    [Authorize(Roles = "Owner,ShiftLeader")]
    public async Task<IActionResult> CloseShift([FromBody] CloseShiftRequest request)
    {
        var activeShift = await _shiftRepository.GetActiveShiftAsync();

        if (activeShift == null)
        {
            return BadRequest(new { message = "Không có ca làm việc nào đang mở." });
        }

        var staffIdClaim = User?.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value 
            ?? User?.FindFirst("sub")?.Value;
        
        if (!int.TryParse(staffIdClaim, out int staffId))
        {
            return Unauthorized(new { message = "Không xác định được nhân viên." });
        }

        // Calculate expected sales from completed orders in this shift
        var shiftOrders = await _orderRepository.GetCompletedOrdersSinceAsync(activeShift.OpenedAt);

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

        await _shiftRepository.UpdateAsync(activeShift);
 
        return Ok(activeShift);
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
