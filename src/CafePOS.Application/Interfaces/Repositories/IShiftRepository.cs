using System.Threading.Tasks;
using CafePOS.Domain.Entities;

namespace CafePOS.Application.Interfaces.Repositories;

public interface IShiftRepository
{
    Task<Shift?> GetActiveShiftAsync();
}
