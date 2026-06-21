using System;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using CafePOS.Application.Interfaces;
using CafePOS.Domain.Entities;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;

namespace CafePOS.Infrastructure.Authentication;

public class JwtProvider : IJwtProvider
{
    private readonly IConfiguration _configuration;

    public JwtProvider(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    public string GenerateToken(Staff staff)
    {
        var secretKey = _configuration["JwtSettings:Secret"] ?? throw new InvalidOperationException("JWT Secret key is not configured.");
        var issuer = _configuration["JwtSettings:Issuer"] ?? "CafePOS";
        var audience = _configuration["JwtSettings:Audience"] ?? "CafePOS";
        var expiryMinutesStr = _configuration["JwtSettings:ExpiryMinutes"] ?? "1440";
        var expiryMinutes = double.TryParse(expiryMinutesStr, out var minutes) ? minutes : 1440;

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var claims = new[]
        {
            new Claim(JwtRegisteredClaimNames.Sub, staff.Id.ToString()),
            new Claim(JwtRegisteredClaimNames.Email, staff.Email),
            new Claim("name", staff.FullName),
            new Claim("role", staff.Role.ToString()),
            new Claim("posCodeHashed", staff.PosCode)
        };

        var token = new JwtSecurityToken(
            issuer: issuer,
            audience: audience,
            claims: claims,
            expires: DateTime.UtcNow.AddMinutes(expiryMinutes),
            signingCredentials: credentials);

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    public string GenerateToken(Customer customer)
    {
        var secretKey = _configuration["JwtSettings:Secret"] ?? throw new InvalidOperationException("JWT Secret key is not configured.");
        var issuer = _configuration["JwtSettings:Issuer"] ?? "CafePOS";
        var audience = _configuration["JwtSettings:Audience"] ?? "CafePOS";
        var expiryMinutesStr = _configuration["JwtSettings:ExpiryMinutes"] ?? "1440";
        var expiryMinutes = double.TryParse(expiryMinutesStr, out var minutes) ? minutes : 1440;

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var claims = new[]
        {
            new Claim(JwtRegisteredClaimNames.Sub, customer.Id.ToString()),
            new Claim(JwtRegisteredClaimNames.Email, customer.Email ?? string.Empty),
            new Claim("name", customer.FullName),
            new Claim("role", "Customer"),
            new Claim("phone", customer.Phone ?? string.Empty),
            new Claim("googleId", customer.GoogleId ?? string.Empty)
        };

        var token = new JwtSecurityToken(
            issuer: issuer,
            audience: audience,
            claims: claims,
            expires: DateTime.UtcNow.AddMinutes(expiryMinutes),
            signingCredentials: credentials);

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}
