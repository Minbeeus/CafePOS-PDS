using System.Threading.Tasks;
using CafePOS.Application.DTOs;
using CafePOS.Application.Interfaces;
using CafePOS.Application.Services;
using CafePOS.Domain.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CafePOS.API.Controllers;

[ApiController]
[Route("api/v1/[controller]")]
public class AuthController : ControllerBase
{
    private readonly AuthService _authService;
    private readonly IStaffRepository _staffRepository;

    public AuthController(AuthService authService, IStaffRepository staffRepository)
    {
        _authService = authService;
        _staffRepository = staffRepository;
    }

    [HttpPost("staff/login")]
    [AllowAnonymous]
    public async Task<IActionResult> StaffLogin([FromBody] StaffLoginRequest request)
    {
        var response = await _authService.LoginStaffAsync(request);
        if (response == null)
        {
            return Unauthorized(new { message = "Tên đăng nhập hoặc mật khẩu không chính xác." });
        }

        return Ok(response);
    }

    [HttpPost("pos/login")]
    [AllowAnonymous]
    public async Task<IActionResult> PosLogin([FromBody] PosLoginRequest request)
    {
        var response = await _authService.LoginPosAsync(request);
        if (response == null)
        {
            return Unauthorized(new { message = "Mã POS không chính xác hoặc nhân viên đã bị khóa." });
        }

        return Ok(response);
    }

    [HttpPost("staff/verify-code")]
    [Authorize] // Requires cashier or staff authentication
    public async Task<IActionResult> VerifyCode([FromBody] VerifyCodeRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.PosCode))
        {
            return BadRequest(new { message = "Mã POS không được trống." });
        }

        var staff = await _staffRepository.GetByPosCodeAsync(request.PosCode);
        if (staff == null)
        {
            return Ok(new { isValid = false, message = "Mã xác thực không đúng." });
        }

        // Check if the staff role matches the required role (Owner has bypass)
        bool hasRequiredRole = staff.Role == StaffRole.Owner || staff.Role.ToString() == request.RequiredRole;
        if (!hasRequiredRole)
        {
            return Ok(new { isValid = false, message = "Quyền hạn không đủ để xác nhận thao tác này." });
        }

        return Ok(new {
            isValid = true,
            staffId = staff.Id,
            staffName = staff.FullName,
            role = staff.Role.ToString()
        });
    }

    [HttpPost("customer/login")]
    [AllowAnonymous]
    public async Task<IActionResult> CustomerLogin([FromBody] CustomerLoginRequest request)
    {
        var response = await _authService.LoginCustomerAsync(request);
        if (response == null)
        {
            return Unauthorized(new { message = "Số điện thoại hoặc mật khẩu không chính xác." });
        }

        return Ok(response);
    }

    [HttpPost("customer/register")]
    [AllowAnonymous]
    public async Task<IActionResult> CustomerRegister([FromBody] CustomerRegisterRequest request)
    {
        try
        {
            var response = await _authService.RegisterCustomerAsync(request);
            if (response == null)
            {
                return BadRequest(new { message = "Không thể đăng ký tài khoản. Vui lòng kiểm tra lại thông tin." });
            }

            return Ok(response);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpPost("customer/google-login")]
    [AllowAnonymous]
    public async Task<IActionResult> CustomerGoogleLogin([FromBody] GoogleLoginRequest request)
    {
        var response = await _authService.LoginWithGoogleAsync(request);
        if (response == null)
        {
            return Unauthorized(new { message = "Xác thực tài khoản Google không thành công." });
        }

        return Ok(response);
    }
}

public class VerifyCodeRequest
{
    public string PosCode { get; set; } = string.Empty;
    public string RequiredRole { get; set; } = string.Empty;
}
