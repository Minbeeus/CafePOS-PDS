using CafePOS.Application.Interfaces.Repositories;
using CafePOS.Domain.Entities;
using CafePOS.Infrastructure.Data;

using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CafePOS.Infrastructure.Repositories;

public class ProductRepository : IProductRepository
{
    private readonly AppDbContext _context;

    public ProductRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<Product?> GetWithSizesAndToppingsByIdAsync(int id)
    {
        return await _context.Products
            .Include(p => p.Sizes)
            .Include(p => p.Toppings)
            .FirstOrDefaultAsync(p => p.Id == id);
    }

    public async Task<List<Product>> GetAllActiveWithSizesAndToppingsAsync()
    {
        return await _context.Products
            .Include(p => p.Sizes)
            .Include(p => p.Toppings)
            .Where(p => p.Status == "Active")
            .ToListAsync();
    }

    public async Task<PointProduct?> GetActivePointProductByProductIdAsync(int productId)
    {
        return await _context.PointProducts
            .FirstOrDefaultAsync(pp => pp.LinkedProductId == productId && pp.IsActive);
    }
}
