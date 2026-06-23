using System.Collections.Generic;
using System.Threading.Tasks;
using CafePOS.Domain.Entities;

namespace CafePOS.Application.Interfaces;

public interface ICustomerRepository
{
    Task<Customer?> GetByIdAsync(int id);
    Task<Customer?> GetByPhoneAsync(string phone);
    Task<Customer?> GetByEmailAsync(string email);
    Task<Customer?> GetByGoogleIdAsync(string googleId);
    Task AddAsync(Customer customer);
    Task UpdateAsync(Customer customer);
    Task<(List<Customer> items, int totalItems)> GetPagedCustomersAsync(int page, int pageSize, string? keyword);
}
