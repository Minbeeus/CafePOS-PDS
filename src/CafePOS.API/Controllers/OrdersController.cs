using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;
using CafePOS.Application.Services;
using CafePOS.Application.DTOs.Orders;
using CafePOS.Infrastructure.Data;
using CafePOS.Infrastructure.Services;
using CafePOS.Application.Interfaces.Repositories;
using CafePOS.API.Hubs;
using Microsoft.AspNetCore.SignalR;
using System;
using System.Linq;
using System.Collections.Generic;

namespace CafePOS.API.Controllers;

[ApiController]
[Route("api/v1/[controller]")]
public class OrdersController : ControllerBase
{
    private readonly OrderService _orderService;
    private readonly IOrderRepository _orderRepository;
    private readonly VietQrService _vietQrService;
    private readonly IHubContext<OrderHub> _hubContext;

    public OrdersController(
        OrderService orderService,
        IOrderRepository orderRepository,
        VietQrService vietQrService,
        IHubContext<OrderHub> hubContext)
    {
        _orderService = orderService;
        _orderRepository = orderRepository;
        _vietQrService = vietQrService;
        _hubContext = hubContext;
    }

    [HttpPost]
    [Authorize(Roles = "Owner,ShiftLeader,Cashier")]
    public async Task<IActionResult> CreateOrder([FromBody] CreateOrderRequest request)
    {
        var staffId = GetCurrentStaffId();
        if (staffId == 0)
        {
            return Unauthorized(new { message = "Nhân viên chưa đăng nhập hoặc Token không hợp lệ." });
        }

        try
        {
            var response = await _orderService.CreateOrderAsync(request, staffId);
            return CreatedAtAction(nameof(GetOrderById), new { id = response.Id }, response);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (UnauthorizedAccessException ex)
        {
            return StatusCode(403, new { message = ex.Message });
        }
        catch (DbUpdateConcurrencyException)
        {
            return Conflict(new { message = "Đơn hàng đã được cập nhật bởi một nhân viên khác. Vui lòng tải lại trang." });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "Đã xảy ra lỗi hệ thống: " + ex.Message });
        }
    }

    [HttpGet("{id}")]
    [Authorize]
    public async Task<IActionResult> GetOrderById(int id)
    {
        var order = await _orderRepository.GetWithDetailsByIdAsync(id);

        if (order == null)
        {
            return NotFound(new { message = "Không tìm thấy đơn hàng." });
        }

        var response = _orderService.MapToResponse(order);
        return Ok(response);
    }

    [HttpGet]
    [Authorize(Roles = "Owner,ShiftLeader,Cashier,Barista,PastryStaff")]
    public async Task<IActionResult> GetOrders(
        [FromQuery] string? status = null,
        [FromQuery] string? date = null,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20)
    {
        if (page < 1) page = 1;
        if (pageSize < 1) pageSize = 10;
        if (pageSize > 1000) pageSize = 1000;

        var (orders, totalItems) = await _orderRepository.GetPagedOrdersAsync(page, pageSize, status, date);
        var totalPages = (int)Math.Ceiling((double)totalItems / pageSize);

        var responseList = orders.Select(o => _orderService.MapToResponse(o)).ToList();
        return Ok(new
        {
            items = responseList,
            page,
            pageSize,
            totalItems,
            totalPages
        });
    }

    [HttpPost("{id}/payment")]
    [Authorize(Roles = "Owner,ShiftLeader,Cashier")]
    public async Task<IActionResult> ProcessPayment(int id, [FromBody] PaymentMethodRequest request)
    {
        var staffId = GetCurrentStaffId();
        if (staffId == 0)
        {
            return Unauthorized(new { message = "Nhân viên chưa đăng nhập hoặc Token không hợp lệ." });
        }

        try
        {
            var response = await _orderService.ProcessPaymentAsync(id, request, staffId);
            try
            {
                if (response.Status == "Confirmed")
                {
                    await _hubContext.Clients.All.SendAsync("OrderConfirmed", response);
                }
            }
            catch (Exception hubEx)
            {
                Console.WriteLine($"SignalR broadcast error in ProcessPayment: {hubEx.Message}");
            }
            return Ok(response);
        }
        catch (InvalidOperationException ex) when (ex.Message.Contains("thanh toán trước đó"))
        {
            return Conflict(new { message = ex.Message });
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (DbUpdateConcurrencyException)
        {
            return Conflict(new { message = "Đơn hàng đã được cập nhật bởi một nhân viên khác. Vui lòng tải lại trang." });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "Đã xảy ra lỗi hệ thống: " + ex.Message });
        }
    }

    [HttpPost("{id}/manual-discount")]
    [Authorize(Roles = "Owner,ShiftLeader,Cashier")]
    public async Task<IActionResult> ApplyManualDiscount(int id, [FromBody] ManualDiscountRequest request)
    {
        try
        {
            var response = await _orderService.ApplyManualDiscountAsync(id, request);
            return Ok(response);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (UnauthorizedAccessException ex)
        {
            return StatusCode(403, new { message = ex.Message });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (DbUpdateConcurrencyException)
        {
            return Conflict(new { message = "Đơn hàng đã được cập nhật bởi một nhân viên khác. Vui lòng tải lại trang." });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "Đã xảy ra lỗi hệ thống: " + ex.Message });
        }
    }

    [HttpGet("{id}/vietqr")]
    [Authorize]
    public async Task<IActionResult> GetOrderVietQr(int id, [FromQuery] decimal? amount)
    {
        var order = await _orderRepository.GetByIdAsync(id);
        if (order == null)
        {
            return NotFound(new { message = "Không tìm thấy đơn hàng." });
        }

        var qrAmount = amount ?? order.TotalAmount;
        var qrUrl = _vietQrService.GenerateQrCodeUrl(order.OrderCode, qrAmount);

        return Ok(new { qrUrl });
    }

    private int GetCurrentStaffId()
    {
        var subClaim = User?.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value 
            ?? User?.FindFirst("sub")?.Value;
        
        if (int.TryParse(subClaim, out int staffId))
        {
            return staffId;
        }
        return 0;
    }

    [HttpPut("{id}/status")]
    [Authorize(Roles = "Owner,ShiftLeader,Cashier,Barista,PastryStaff")]
    public async Task<IActionResult> UpdateStatus(int id, [FromBody] OrderStatusUpdateRequest request)
    {
        if (!Enum.TryParse<CafePOS.Domain.Enums.OrderStatus>(request.Status, true, out var status))
        {
            return BadRequest(new { message = "Trạng thái đơn hàng không hợp lệ." });
        }

        try
        {
            var response = await _orderService.UpdateOrderStatusAsync(id, status);
            try
            {
                // Broadcast the status update to everyone
                await _hubContext.Clients.All.SendAsync("OrderStatusChanged", id, request.Status);
            }
            catch (Exception hubEx)
            {
                Console.WriteLine($"SignalR broadcast error in UpdateStatus: {hubEx.Message}");
            }
            return Ok(response);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (DbUpdateConcurrencyException)
        {
            return Conflict(new { message = "Đơn hàng đã được cập nhật bởi một nhân viên khác. Vui lòng tải lại trang." });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "Đã xảy ra lỗi hệ thống: " + ex.Message });
        }
    }

    [HttpPut("{id}/station-status")]
    [Authorize(Roles = "Owner,ShiftLeader,Barista,PastryStaff")]
    public async Task<IActionResult> UpdateStationStatus(int id, [FromBody] StationStatusUpdateRequest request)
    {
        try
        {
            var response = await _orderService.UpdateStationPrepStatusAsync(id, request.Station, request.Status);
            try
            {
                // Broadcast the global order status changes to everyone
                await _hubContext.Clients.All.SendAsync("OrderStatusChanged", id, response.Status);
            }
            catch (Exception hubEx)
            {
                Console.WriteLine($"SignalR broadcast error in UpdateStationStatus: {hubEx.Message}");
            }
            return Ok(response);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (DbUpdateConcurrencyException)
        {
            return Conflict(new { message = "Đơn hàng đã được cập nhật bởi một nhân viên khác. Vui lòng tải lại trang." });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "Đã xảy ra lỗi hệ thống: " + ex.Message });
        }
    }
}

public class OrderStatusUpdateRequest
{
    public string Status { get; set; } = string.Empty;
}

public class StationStatusUpdateRequest
{
    public string Station { get; set; } = string.Empty; // "Bar" or "Pastry"
    public string Status { get; set; } = string.Empty;  // "Preparing" or "Completed"
}
