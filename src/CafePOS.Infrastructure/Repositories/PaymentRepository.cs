using CafePOS.Application.Interfaces.Repositories;
using CafePOS.Domain.Entities;
using CafePOS.Domain.Enums;
using CafePOS.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CafePOS.Infrastructure.Repositories;

public class PaymentRepository : IPaymentRepository
{
    private readonly AppDbContext _context;

    public PaymentRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task AddAsync(Payment payment)
    {
        _context.Payments.Add(payment);
        await _context.SaveChangesAsync();
    }

    public async Task<bool> HasCompletedPaymentForOrderAsync(int orderId)
    {
        return await _context.Payments
            .AnyAsync(p => p.OrderId == orderId && p.Status == PaymentStatus.Completed);
    }

    public async Task<(List<Payment> items, int totalItems)> GetPagedPaymentsAsync(int page, int pageSize, DateTime? startDate, string? method)
    {
        var query = _context.Payments
            .Include(p => p.Order)
            .Include(p => p.CreatedByStaff)
            .AsQueryable();

        if (startDate.HasValue)
        {
            var startDateTime = startDate.Value.Date;
            var endDateTime = startDateTime.AddDays(1);
            query = query.Where(p => p.PaidAt >= startDateTime && p.PaidAt < endDateTime);
        }

        if (!string.IsNullOrWhiteSpace(method))
        {
            PaymentMethod paymentMethod;
            if (method.Equals("Transfer", StringComparison.OrdinalIgnoreCase))
            {
                paymentMethod = PaymentMethod.VietQr;
            }
            else if (Enum.TryParse(method, true, out paymentMethod))
            {
                query = query.Where(p => p.Method == paymentMethod);
            }
        }

        var totalItems = await query.CountAsync();
        var items = await query
            .OrderByDescending(p => p.PaidAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return (items, totalItems);
    }
}
