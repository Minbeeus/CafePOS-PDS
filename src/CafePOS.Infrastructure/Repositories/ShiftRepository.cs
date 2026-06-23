using CafePOS.Application.Interfaces.Repositories;
using CafePOS.Domain.Entities;
using CafePOS.Infrastructure.Data;

using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;

namespace CafePOS.Infrastructure.Repositories;

public class ShiftRepository : IShiftRepository
{
    private readonly AppDbContext _context;

    public ShiftRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<Shift?> GetActiveShiftAsync()
    {
        return await _context.Shifts.FirstOrDefaultAsync(s => s.Status == "Open");
    }

    public async Task<Shift?> GetActiveShiftWithOpenedByAsync()
    {
        return await _context.Shifts
            .Include(s => s.OpenedBy)
            .AsNoTracking()
            .FirstOrDefaultAsync(s => s.Status == "Open");
    }

    public async Task<Shift?> GetByIdAsync(int id)
    {
        return await _context.Shifts.FindAsync(id);
    }

    public async Task AddAsync(Shift shift)
    {
        _context.Shifts.Add(shift);
        await _context.SaveChangesAsync();
    }

    public async Task UpdateAsync(Shift shift)
    {
        _context.Shifts.Update(shift);
        await _context.SaveChangesAsync();
    }
}
