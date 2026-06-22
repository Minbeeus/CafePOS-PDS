using CafePOS.Application.Interfaces.Repositories;
using CafePOS.Domain.Entities;
using CafePOS.Infrastructure.Data;

using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;

namespace CafePOS.Infrastructure.Repositories;

public class VoucherRepository : IVoucherRepository
{
    private readonly AppDbContext _context;

    public VoucherRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<Voucher?> GetActiveByCodeAsync(string code)
    {
        return await _context.Vouchers.FirstOrDefaultAsync(v => v.Code == code && v.IsActive);
    }

    public async Task UpdateAsync(Voucher voucher)
    {
        _context.Vouchers.Update(voucher);
        await _context.SaveChangesAsync();
    }
}
