using CafePOS.Application.Interfaces.Repositories;
using CafePOS.Domain.Entities;
using CafePOS.Infrastructure.Data;

namespace CafePOS.Infrastructure.Repositories;

public class PaymentRepository : IPaymentRepository
{
    private readonly AppDbContext _context;

    public PaymentRepository(AppDbContext context)
    {
        _context = context;
    }
}
