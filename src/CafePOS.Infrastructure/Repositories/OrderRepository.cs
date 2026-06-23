using CafePOS.Application.Interfaces.Repositories;
using CafePOS.Domain.Entities;
using CafePOS.Infrastructure.Data;

using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CafePOS.Infrastructure.Repositories;

public class OrderRepository : IOrderRepository
{
    private readonly AppDbContext _context;

    public OrderRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<Order?> GetByIdAsync(int id)
    {
        return await _context.Orders.FindAsync(id);
    }

    public async Task<Order?> GetWithDetailsByIdAsync(int id)
    {
        return await _context.Orders
            .Include(o => o.OrderItems)
                .ThenInclude(oi => oi.Toppings)
                    .ThenInclude(oit => oit.Topping)
            .Include(o => o.OrderItems)
                .ThenInclude(oi => oi.Product)
            .Include(o => o.Customer)
            .Include(o => o.Staff)
            .Include(o => o.Discounts)
            .FirstOrDefaultAsync(o => o.Id == id);
    }

    public async Task AddAsync(Order order)
    {
        _context.Orders.Add(order);
        await _context.SaveChangesAsync();
    }

    public async Task UpdateAsync(Order order)
    {
        _context.Orders.Update(order);
        await _context.SaveChangesAsync();
    }

    public async Task<List<Order>> GetActiveOrdersAsync()
    {
        var today = DateTime.UtcNow.Date;
        return await _context.Orders
            .Include(o => o.OrderItems)
                .ThenInclude(oi => oi.Toppings)
                    .ThenInclude(oit => oit.Topping)
            .Include(o => o.OrderItems)
                .ThenInclude(oi => oi.Product)
            .Include(o => o.Customer)
            .AsNoTracking()
            .Where(o => o.CreatedAt >= today)
            .OrderByDescending(o => o.CreatedAt)
            .ToListAsync();
    }

    public async Task<(List<Order> items, int totalItems)> GetPagedOrdersAsync(int page, int pageSize, string? status, string? date)
    {
        var query = _context.Orders
            .Include(o => o.OrderItems)
                .ThenInclude(oi => oi.Toppings)
                    .ThenInclude(oit => oit.Topping)
            .Include(o => o.OrderItems)
                .ThenInclude(oi => oi.Product)
            .Include(o => o.Customer)
            .AsNoTracking()
            .AsQueryable();

        if (date == "today")
        {
            var today = DateTime.UtcNow.Date;
            query = query.Where(o => o.CreatedAt >= today);
        }

        if (!string.IsNullOrWhiteSpace(status))
        {
            var statuses = status.Split(',').Select(s => s.Trim()).ToList();
            var enumStatuses = statuses
                .Select(s => Enum.TryParse<CafePOS.Domain.Enums.OrderStatus>(s, true, out var res) ? (CafePOS.Domain.Enums.OrderStatus?)res : null)
                .Where(s => s.HasValue)
                .Select(s => s!.Value)
                .ToList();
            
            if (enumStatuses.Any())
            {
                query = query.Where(o => enumStatuses.Contains(o.Status));
            }
        }

        var totalItems = await query.CountAsync();
        var items = await query
            .OrderByDescending(o => o.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return (items, totalItems);
    }

    public async Task<string> GenerateOrderCodeAsync()
    {
        var dateStr = DateTime.UtcNow.ToString("yyMMdd");
        var prefix = $"CF{dateStr}";
        var orderCodesToday = await _context.Orders
            .Where(o => o.OrderCode.StartsWith(prefix))
            .Select(o => o.OrderCode)
            .ToListAsync();

        int nextSeq = 1;
        if (orderCodesToday.Any())
        {
            var seqs = orderCodesToday
                .Select(c => c.Length >= 10 ? c.Substring(8) : "0")
                .Select(s => int.TryParse(s, out var val) ? val : 0);
            nextSeq = seqs.Max() + 1;
        }
        return $"{prefix}{nextSeq:D4}";
    }

    public async Task AddPointTransactionAsync(PointTransaction tx)
    {
        _context.PointTransactions.Add(tx);
        await _context.SaveChangesAsync();
    }

    public async Task AddAuditLogAsync(AuditLog log)
    {
        _context.AuditLogs.Add(log);
        await _context.SaveChangesAsync();
    }

    public async Task<List<Order>> GetCompletedOrdersSinceAsync(DateTime since)
    {
        return await _context.Orders
            .AsNoTracking()
            .Where(o => o.CreatedAt >= since && o.Status == CafePOS.Domain.Enums.OrderStatus.Completed)
            .ToListAsync();
    }
}
