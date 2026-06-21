using CafePOS.Domain.Enums;

namespace CafePOS.Domain.Entities;

public class Staff
{
    public int Id { get; set; }
    public string FullName { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string PosCode { get; set; } = string.Empty; // Mã đăng nhập POS - bcrypt hashed
    public string PasswordHash { get; set; } = string.Empty; // Cho Admin Dashboard
    public StaffRole Role { get; set; }
    public decimal BaseSalary { get; set; }
    public string Status { get; set; } = string.Empty; // Active, Inactive
    public DateTime CreatedAt { get; set; }
}
