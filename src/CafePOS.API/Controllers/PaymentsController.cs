using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using CafePOS.Application.Interfaces.Repositories;
using System;
using System.Linq;

namespace CafePOS.API.Controllers;

[ApiController]
[Route("api/v1/[controller]")]
public class PaymentsController : ControllerBase
{
    private readonly IPaymentRepository _paymentRepository;

    public PaymentsController(IPaymentRepository paymentRepository)
    {
        _paymentRepository = paymentRepository;
    }

    [HttpGet]
    [Authorize(Roles = "Owner,ShiftLeader")]
    public async Task<IActionResult> GetPayments(
        [FromQuery] DateTime? startDate = null,
        [FromQuery] string? method = null,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20)
    {
        if (page < 1) page = 1;
        if (pageSize < 1) pageSize = 10;
        if (pageSize > 1000) pageSize = 1000;

        var (items, totalItems) = await _paymentRepository.GetPagedPaymentsAsync(page, pageSize, startDate, method);
        var totalPages = (int)Math.Ceiling((double)totalItems / pageSize);

        var mapped = items.Select(p => new
        {
            p.Id,
            p.OrderId,
            OrderCode = p.Order?.OrderCode ?? "Unknown",
            Method = p.Method.ToString(),
            p.Amount,
            p.ReferenceCode,
            p.CreatedByStaffId,
            CreatedByStaffName = p.CreatedByStaff?.FullName ?? "Unknown",
            p.PaidAt
        }).ToList();

        return Ok(new
        {
            items = mapped,
            page,
            pageSize,
            totalItems,
            totalPages
        });
    }
}
