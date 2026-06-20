using CafePOS.Domain.Entities;

namespace CafePOS.Application.Interfaces;

public interface IJwtProvider
{
    string GenerateToken(Staff staff);
    string GenerateToken(Customer customer);
}
