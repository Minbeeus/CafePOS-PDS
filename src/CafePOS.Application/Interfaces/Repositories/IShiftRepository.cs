using System.Threading.Tasks;
using CafePOS.Domain.Entities;

namespace CafePOS.Application.Interfaces.Repositories;

public interface IShiftRepository
{
    Task<Shift?> GetActiveShiftAsync();
    Task<Shift?> GetActiveShiftWithOpenedByAsync();
    Task<Shift?> GetByIdAsync(int id);
    Task AddAsync(Shift shift);
    Task UpdateAsync(Shift shift);
}
