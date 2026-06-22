using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using CafePOS.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;

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

    [HttpGet("lookup")]
    [Authorize(Roles = "Owner,ShiftLeader,Cashier")]
    public async Task<IActionResult> Lookup([FromQuery] string phone)
    {
        if (string.IsNullOrWhiteSpace(phone))
        {
            return BadRequest(new { success = false, message = "Số điện thoại không được để trống." });
        }

        var customer = await _customerRepository.GetByPhoneAsync(phone);
        if (customer == null)
        {
            return NotFound(new { success = false, message = "Không tìm thấy khách hàng với số điện thoại này." });
        }

        return Ok(new
        {
            success = true,
            data = new
            {
                customer.Id,
                customer.FullName,
                customer.Phone,
                customer.Email,
                customer.TotalSpend,
                LoyaltyTier = customer.LoyaltyTier.ToString(),
                customer.CurrentPoints
            }
        });
    }
}
