using CafePOS.Application.Interfaces.Repositories;
using CafePOS.Domain.Entities;
using CafePOS.Infrastructure.Data;

namespace CafePOS.Infrastructure.Repositories;

public class ProductRepository : IProductRepository
{
    private readonly AppDbContext _context;

    public ProductRepository(AppDbContext context)
    {
        _context = context;
    }
}
