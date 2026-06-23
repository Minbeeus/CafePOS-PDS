using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using CafePOS.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using System.Collections.Generic;
using System;
using System.Linq;

namespace CafePOS.API.Controllers;

[ApiController]
[Route("api/v1/[controller]")]
public class CustomersController : ControllerBase
{
    private readonly ICustomerRepository _customerRepository;

    public CustomersController(ICustomerRepository customerRepository)
    {
        _customerRepository = customerRepository;
    }

    [HttpGet]
    [Authorize(Roles = "Owner,ShiftLeader,Cashier")]
    public async Task<IActionResult> GetCustomers(
        [FromQuery] string? keyword = null,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20)
    {
        if (page < 1) page = 1;
        if (pageSize < 1) pageSize = 10;
        if (pageSize > 1000) pageSize = 1000;

        var (items, totalItems) = await _customerRepository.GetPagedCustomersAsync(page, pageSize, keyword);
        var totalPages = (int)Math.Ceiling((double)totalItems / pageSize);

        var mapped = items.Select(c => new
        {
            c.Id,
            c.FullName,
            c.Phone,
            c.Email,
            c.TotalSpend,
            LoyaltyTier = c.LoyaltyTier.ToString(),
            c.CurrentPoints
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

    [HttpGet("lookup")]
    [Authorize(Roles = "Owner,ShiftLeader,Cashier")]
    public async Task<IActionResult> Lookup([FromQuery] string phone)
    {
        if (string.IsNullOrWhiteSpace(phone))
        {
            return BadRequest(new { message = "Số điện thoại không được để trống." });
        }

        var customer = await _customerRepository.GetByPhoneAsync(phone);
        if (customer == null)
        {
            return NotFound(new { message = "Không tìm thấy khách hàng với số điện thoại này." });
        }

        return Ok(new
        {
            customer.Id,
            customer.FullName,
            customer.Phone,
            customer.Email,
            customer.TotalSpend,
            LoyaltyTier = customer.LoyaltyTier.ToString(),
            customer.CurrentPoints
        });
    }
}
