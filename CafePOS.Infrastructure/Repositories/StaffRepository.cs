using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CafePOS.Application.Interfaces;
using CafePOS.Domain.Entities;
using CafePOS.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace CafePOS.Infrastructure.Repositories;

public class StaffRepository : IStaffRepository
{
    private readonly AppDbContext _context;

    public StaffRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<Staff?> GetByIdAsync(int id)
    {
        return await _context.Staffs.FindAsync(id);
    }

    public async Task<Staff?> GetByUsernameAsync(string username)
    {
        // Try matching Email first, then Phone
        return await _context.Staffs
            .FirstOrDefaultAsync(s => (s.Email == username || s.Phone == username) && s.Status == "Active");
    }

    public async Task<Staff?> GetByPosCodeAsync(string posCode)
    {
        // Since posCode is bcrypt-hashed in DB, we must fetch active staffs and verify in memory.
        var activeStaffs = await _context.Staffs
            .Where(s => s.Status == "Active")
            .ToListAsync();

        foreach (var staff in activeStaffs)
        {
            if (!string.IsNullOrEmpty(staff.PosCode) && BCrypt.Net.BCrypt.Verify(posCode, staff.PosCode))
            {
                return staff;
            }
        }

        return null;
    }

    public async Task UpdateAsync(Staff staff)
    {
        _context.Staffs.Update(staff);
        await _context.SaveChangesAsync();
    }
}
