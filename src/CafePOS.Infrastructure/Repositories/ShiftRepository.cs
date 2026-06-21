using CafePOS.Application.Interfaces.Repositories;
using CafePOS.Domain.Entities;
using CafePOS.Infrastructure.Data;

namespace CafePOS.Infrastructure.Repositories;

public class ShiftRepository : IShiftRepository
{
    private readonly AppDbContext _context;

    public ShiftRepository(AppDbContext context)
    {
        _context = context;
    }
}
