using CafePOS.Application.Interfaces.Repositories;
using CafePOS.Domain.Entities;
using CafePOS.Infrastructure.Data;

namespace CafePOS.Infrastructure.Repositories;

public class OrderRepository : IOrderRepository
{
    private readonly AppDbContext _context;

    public OrderRepository(AppDbContext context)
    {
        _context = context;
    }
}
