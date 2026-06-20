using System;
using System.Threading.Tasks;
using CafePOS.Application.DTOs;
using CafePOS.Application.Interfaces;
using CafePOS.Domain.Entities;

namespace CafePOS.Application.Services;

public class AuthService
{
    private readonly IStaffRepository _staffRepository;
    private readonly ICustomerRepository _customerRepository;
    private readonly IJwtProvider _jwtProvider;

    public AuthService(
        IStaffRepository staffRepository, 
        ICustomerRepository customerRepository, 
        IJwtProvider jwtProvider)
    {
        _staffRepository = staffRepository;
        _customerRepository = customerRepository;
        _jwtProvider = jwtProvider;
    }

    public async Task<AuthResponse?> LoginStaffAsync(StaffLoginRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Username) || string.IsNullOrWhiteSpace(request.Password))
        {
            return null;
        }

        var staff = await _staffRepository.GetByUsernameAsync(request.Username);
        if (staff == null)
        {
            return null;
        }

        // Verify password
        if (string.IsNullOrEmpty(staff.PasswordHash) || !BCrypt.Net.BCrypt.Verify(request.Password, staff.PasswordHash))
        {
            return null;
        }

        // Generate token
        var token = _jwtProvider.GenerateToken(staff);

        return new AuthResponse
        {
            Token = token,
            Staff = new StaffDto
            {
                Id = staff.Id,
                FullName = staff.FullName,
                Email = staff.Email,
                Role = staff.Role.ToString()
            }
        };
    }

    public async Task<AuthResponse?> LoginPosAsync(PosLoginRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.PosCode))
        {
            return null;
        }

        var staff = await _staffRepository.GetByPosCodeAsync(request.PosCode);
        if (staff == null)
        {
            return null;
        }

        // Generate token
        var token = _jwtProvider.GenerateToken(staff);

        return new AuthResponse
        {
            Token = token,
            Staff = new StaffDto
            {
                Id = staff.Id,
                FullName = staff.FullName,
                Email = staff.Email,
                Role = staff.Role.ToString()
            }
        };
    }

    public async Task<CustomerAuthResponse?> RegisterCustomerAsync(CustomerRegisterRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Phone) || string.IsNullOrWhiteSpace(request.Password) || string.IsNullOrWhiteSpace(request.FullName))
        {
            return null;
        }

        // Check if phone already registered
        var existing = await _customerRepository.GetByPhoneAsync(request.Phone);
        if (existing != null)
        {
            throw new InvalidOperationException("Số điện thoại này đã được đăng ký.");
        }

        // Hash password
        var passwordHash = BCrypt.Net.BCrypt.HashPassword(request.Password);

        var customer = new Customer
        {
            FullName = request.FullName,
            Phone = request.Phone,
            Email = request.Email,
            PasswordHash = passwordHash,
            GoogleId = null,
            TotalSpend = 0,
            LoyaltyTier = CafePOS.Domain.Enums.LoyaltyTier.None,
            CurrentPoints = 0,
            PointsResetAt = DateTime.UtcNow.AddMonths(2),
            CreatedAt = DateTime.UtcNow
        };

        await _customerRepository.AddAsync(customer);

        var token = _jwtProvider.GenerateToken(customer);

        return new CustomerAuthResponse
        {
            Token = token,
            Customer = new CustomerDto
            {
                Id = customer.Id,
                FullName = customer.FullName,
                Phone = customer.Phone ?? string.Empty,
                Email = customer.Email ?? string.Empty,
                LoyaltyTier = customer.LoyaltyTier.ToString(),
                CurrentPoints = customer.CurrentPoints
            }
        };
    }

    public async Task<CustomerAuthResponse?> LoginCustomerAsync(CustomerLoginRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Phone) || string.IsNullOrWhiteSpace(request.Password))
        {
            return null;
        }

        var customer = await _customerRepository.GetByPhoneAsync(request.Phone);
        if (customer == null || string.IsNullOrEmpty(customer.PasswordHash))
        {
            return null;
        }

        // Verify password
        if (!BCrypt.Net.BCrypt.Verify(request.Password, customer.PasswordHash))
        {
            return null;
        }

        var token = _jwtProvider.GenerateToken(customer);

        return new CustomerAuthResponse
        {
            Token = token,
            Customer = new CustomerDto
            {
                Id = customer.Id,
                FullName = customer.FullName,
                Phone = customer.Phone ?? string.Empty,
                Email = customer.Email ?? string.Empty,
                LoyaltyTier = customer.LoyaltyTier.ToString(),
                CurrentPoints = customer.CurrentPoints
            }
        };
    }

    public async Task<CustomerAuthResponse?> LoginWithGoogleAsync(GoogleLoginRequest request)
    {
        string email = "";
        string fullName = "";
        string googleId = "";

        if (request.IsDemo)
        {
            email = request.DemoEmail ?? "demo@gmail.com";
            fullName = request.DemoName ?? "Demo Google User";
            googleId = request.DemoGoogleId ?? "google-demo-id-12345";
        }
        else
        {
            if (string.IsNullOrWhiteSpace(request.IdToken))
            {
                return null;
            }

            try
            {
                // Validate Google IdToken via HTTP request info endpoint
                using var client = new System.Net.Http.HttpClient();
                var url = $"https://oauth2.googleapis.com/tokeninfo?id_token={request.IdToken}";
                var response = await client.GetAsync(url);
                if (!response.IsSuccessStatusCode)
                {
                    return null;
                }

                var content = await response.Content.ReadAsStringAsync();
                using var jsonDoc = System.Text.Json.JsonDocument.Parse(content);
                var root = jsonDoc.RootElement;

                if (!root.TryGetProperty("sub", out var subProp) || 
                    !root.TryGetProperty("email", out var emailProp))
                {
                    return null;
                }

                googleId = subProp.GetString() ?? "";
                email = emailProp.GetString() ?? "";
                
                if (root.TryGetProperty("name", out var nameProp))
                {
                    fullName = nameProp.GetString() ?? "";
                }
                else
                {
                    fullName = email.Split('@')[0];
                }
            }
            catch
            {
                return null;
            }
        }

        if (string.IsNullOrEmpty(googleId) || string.IsNullOrEmpty(email))
        {
            return null;
        }

        // Try getting customer by GoogleId first
        var customer = await _customerRepository.GetByGoogleIdAsync(googleId);
        
        // If not found by GoogleId, try by Email
        if (customer == null)
        {
            customer = await _customerRepository.GetByEmailAsync(email);
            if (customer != null)
            {
                // Link GoogleId to existing customer
                customer.GoogleId = googleId;
                await _customerRepository.UpdateAsync(customer);
            }
        }

        // If still not found, register new customer
        if (customer == null)
        {
            customer = new Customer
            {
                FullName = fullName,
                Email = email,
                GoogleId = googleId,
                Phone = "", // Initially empty, can be updated later
                PasswordHash = "", // No password for Google account
                TotalSpend = 0,
                LoyaltyTier = CafePOS.Domain.Enums.LoyaltyTier.None,
                CurrentPoints = 0,
                PointsResetAt = DateTime.UtcNow.AddMonths(2),
                CreatedAt = DateTime.UtcNow
            };
            await _customerRepository.AddAsync(customer);
        }

        var token = _jwtProvider.GenerateToken(customer);

        return new CustomerAuthResponse
        {
            Token = token,
            Customer = new CustomerDto
            {
                Id = customer.Id,
                FullName = customer.FullName,
                Phone = customer.Phone ?? string.Empty,
                Email = customer.Email ?? string.Empty,
                LoyaltyTier = customer.LoyaltyTier.ToString(),
                CurrentPoints = customer.CurrentPoints
            }
        };
    }
}
