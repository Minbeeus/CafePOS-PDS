using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using CafePOS.Domain.Entities;

namespace CafePOS.Application.Interfaces.Repositories;

public interface IPaymentRepository
{
    Task AddAsync(Payment payment);
    Task<bool> HasCompletedPaymentForOrderAsync(int orderId);
    Task<(List<Payment> items, int totalItems)> GetPagedPaymentsAsync(int page, int pageSize, DateTime? startDate, string? method);
}
