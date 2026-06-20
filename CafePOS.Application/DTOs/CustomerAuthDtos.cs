namespace CafePOS.Application.DTOs;

public class CustomerLoginRequest
{
    public string Phone { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
}

public class CustomerRegisterRequest
{
    public string FullName { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
}

public class GoogleLoginRequest
{
    public string IdToken { get; set; } = string.Empty;
    // For Demo Fallback mode:
    public string? DemoEmail { get; set; }
    public string? DemoName { get; set; }
    public string? DemoGoogleId { get; set; }
    public bool IsDemo { get; set; }
}

public class CustomerAuthResponse
{
    public string Token { get; set; } = string.Empty;
    public CustomerDto Customer { get; set; } = new();
}

public class CustomerDto
{
    public int Id { get; set; }
    public string FullName { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string LoyaltyTier { get; set; } = string.Empty;
    public int CurrentPoints { get; set; }
}
