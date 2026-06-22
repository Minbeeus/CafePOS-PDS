using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;
using CafePOS.Application.Services;
using CafePOS.Application.DTOs.Orders;
using CafePOS.Infrastructure.Data;
using System;
using System.Linq;

namespace CafePOS.API.Controllers;

[ApiController]
[Route("api/v1/[controller]")]
public class OrdersController : ControllerBase
{
    private readonly OrderService _orderService;
    private readonly AppDbContext _context;

    public OrdersController(OrderService orderService, AppDbContext context)
    {
        _orderService = orderService;
        _context = context;
    }

    [HttpPost]
    [Authorize(Roles = "Owner,ShiftLeader,Cashier")]
    public async Task<IActionResult> CreateOrder([FromBody] CreateOrderRequest request)
    {
        var staffId = GetCurrentStaffId();
        if (staffId == 0)
        {
            return Unauthorized(new { success = false, message = "Nhân viên chưa đăng nhập hoặc Token không hợp lệ." });
        }

        try
        {
            var response = await _orderService.CreateOrderAsync(request, staffId);
            return CreatedAtAction(nameof(GetOrderById), new { id = response.Id }, new { success = true, data = response, message = "Tạo đơn hàng thành công" });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { success = false, message = ex.Message });
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { success = false, message = ex.Message });
        }
        catch (UnauthorizedAccessException ex)
        {
            return StatusCode(403, new { success = false, message = ex.Message });
        }
        catch (DbUpdateConcurrencyException)
        {
            return Conflict(new { success = false, message = "Đơn hàng đã được cập nhật bởi một nhân viên khác. Vui lòng tải lại trang." });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { success = false, message = "Đã xảy ra lỗi hệ thống: " + ex.Message });
        }
    }

    [HttpGet("{id}")]
    [Authorize]
    public async Task<IActionResult> GetOrderById(int id)
    {
        var order = await _context.Orders
            .Include(o => o.OrderItems)
                .ThenInclude(oi => oi.Toppings)
                    .ThenInclude(oit => oit.Topping)
            .Include(o => o.Customer)
            .Include(o => o.Staff)
            .Include(o => o.Discounts)
            .FirstOrDefaultAsync(o => o.Id == id);

        if (order == null)
        {
            return NotFound(new { success = false, message = "Không tìm thấy đơn hàng." });
        }

        var response = _orderService.MapToResponse(order);
        return Ok(new { success = true, data = response });
    }

    [HttpGet]
    [Authorize(Roles = "Owner,ShiftLeader,Cashier")]
    public async Task<IActionResult> GetOrders([FromQuery] string? status)
    {
        var today = DateTime.UtcNow.Date;
        var query = _context.Orders
            .Include(o => o.OrderItems)
                .ThenInclude(oi => oi.Toppings)
            .Include(o => o.Customer)
            .Where(o => o.CreatedAt >= today);

        if (!string.IsNullOrWhiteSpace(status))
        {
            var statuses = status.Split(',').Select(s => s.Trim()).ToList();
            query = query.Where(o => statuses.Contains(o.Status.ToString()));
        }

        var orders = await query
            .OrderByDescending(o => o.CreatedAt)
            .ToListAsync();

        var responseList = orders.Select(o => _orderService.MapToResponse(o)).ToList();
        return Ok(new { success = true, data = responseList });
    }

    [HttpPost("{id}/payment")]
    [Authorize(Roles = "Owner,ShiftLeader,Cashier")]
    public async Task<IActionResult> ProcessPayment(int id, [FromBody] PaymentMethodRequest request)
    {
        var staffId = GetCurrentStaffId();
        if (staffId == 0)
        {
            return Unauthorized(new { success = false, message = "Nhân viên chưa đăng nhập hoặc Token không hợp lệ." });
        }

        try
        {
            var response = await _orderService.ProcessPaymentAsync(id, request, staffId);
            return Ok(new { success = true, data = response, message = "Thanh toán đơn hàng thành công" });
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { success = false, message = ex.Message });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { success = false, message = ex.Message });
        }
        catch (DbUpdateConcurrencyException)
        {
            return Conflict(new { success = false, message = "Đơn hàng đã được cập nhật bởi một nhân viên khác. Vui lòng tải lại trang." });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { success = false, message = "Đã xảy ra lỗi hệ thống: " + ex.Message });
        }
    }

    [HttpPost("{id}/manual-discount")]
    [Authorize(Roles = "Owner,ShiftLeader,Cashier")]
    public async Task<IActionResult> ApplyManualDiscount(int id, [FromBody] ManualDiscountRequest request)
    {
        try
        {
            var response = await _orderService.ApplyManualDiscountAsync(id, request);
            return Ok(new { success = true, data = response, message = "Áp dụng giảm giá thủ công thành công" });
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { success = false, message = ex.Message });
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { success = false, message = ex.Message });
        }
        catch (UnauthorizedAccessException ex)
        {
            return StatusCode(403, new { success = false, message = ex.Message });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { success = false, message = ex.Message });
        }
        catch (DbUpdateConcurrencyException)
        {
            return Conflict(new { success = false, message = "Đơn hàng đã được cập nhật bởi một nhân viên khác. Vui lòng tải lại trang." });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { success = false, message = "Đã xảy ra lỗi hệ thống: " + ex.Message });
        }
    }

    private int GetCurrentStaffId()
    {
        var subClaim = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value 
            ?? User.FindFirst("sub")?.Value;
        
        if (int.TryParse(subClaim, out int staffId))
        {
            return staffId;
        }
        return 0;
    }
}
