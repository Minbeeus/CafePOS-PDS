using System.Threading.Tasks;
using CafePOS.Application.Interfaces;
using CafePOS.Domain.Entities;
using CafePOS.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace CafePOS.Infrastructure.Repositories;

public class CustomerRepository : ICustomerRepository
{
    private readonly AppDbContext _context;

    public CustomerRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<Customer?> GetByIdAsync(int id)
    {
        return await _context.Customers.FindAsync(id);
    }

    public async Task<Customer?> GetByPhoneAsync(string phone)
    {
        return await _context.Customers
            .FirstOrDefaultAsync(c => c.Phone == phone);
    }

    public async Task<Customer?> GetByEmailAsync(string email)
    {
        return await _context.Customers
            .FirstOrDefaultAsync(c => c.Email == email);
    }

    public async Task<Customer?> GetByGoogleIdAsync(string googleId)
    {
        return await _context.Customers
            .FirstOrDefaultAsync(c => c.GoogleId == googleId);
    }

    public async Task AddAsync(Customer customer)
    {
        await _context.Customers.AddAsync(customer);
        await _context.SaveChangesAsync();
    }

    public async Task UpdateAsync(Customer customer)
    {
        _context.Customers.Update(customer);
        await _context.SaveChangesAsync();
    }

    public async Task<(List<Customer> items, int totalItems)> GetPagedCustomersAsync(int page, int pageSize, string? keyword)
    {
        var query = _context.Customers.AsNoTracking().AsQueryable();

        if (!string.IsNullOrWhiteSpace(keyword))
        {
            query = query.Where(c => c.FullName.Contains(keyword) || 
                                     c.Phone.Contains(keyword) || 
                                     c.Email.Contains(keyword));
        }

        var totalItems = await query.CountAsync();
        var items = await query
            .OrderBy(c => c.FullName)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return (items, totalItems);
    }
}
