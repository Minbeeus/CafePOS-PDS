using CafePOS.Domain.Entities;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace CafePOS.Application.Interfaces.Repositories;

public interface IOrderRepository
{
    Task<Order?> GetByIdAsync(int id);
    Task<Order?> GetWithDetailsByIdAsync(int id);
    Task AddAsync(Order order);
    Task UpdateAsync(Order order);
    Task<List<Order>> GetActiveOrdersAsync();
    Task<(List<Order> items, int totalItems)> GetPagedOrdersAsync(int page, int pageSize, string? status, string? date);
    Task<string> GenerateOrderCodeAsync();
    Task AddPointTransactionAsync(PointTransaction tx);
    Task AddAuditLogAsync(AuditLog log);
    Task<List<Order>> GetCompletedOrdersSinceAsync(DateTime since);
}
