using CafePOS.Application.Interfaces.Repositories;
using CafePOS.Domain.Entities;
using CafePOS.Infrastructure.Data;

namespace CafePOS.Infrastructure.Repositories;

public class VoucherRepository : IVoucherRepository
{
    private readonly AppDbContext _context;

    public VoucherRepository(AppDbContext context)
    {
        _context = context;
    }
}
