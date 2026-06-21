using System.Threading.Tasks;
using CafePOS.Domain.Entities;

namespace CafePOS.Application.Interfaces;

public interface IStaffRepository
{
    Task<Staff?> GetByIdAsync(int id);
    Task<Staff?> GetByUsernameAsync(string username); // Checks Email or Phone or FullName
    Task<Staff?> GetByPosCodeAsync(string posCode);   // Verifies Bcrypt-hashed PosCode in memory
    Task UpdateAsync(Staff staff);
}
