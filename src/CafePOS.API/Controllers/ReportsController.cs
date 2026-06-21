using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using CafePOS.Infrastructure.Data;
using CafePOS.Domain.Enums;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace CafePOS.API.Controllers;

[ApiController]
[Route("api/v1/[controller]")]
public class ReportsController : ControllerBase
{
    private readonly AppDbContext _context;

    public ReportsController(AppDbContext context)
    {
        _context = context;
    }

    [HttpGet("dashboard-stats")]
    public async Task<IActionResult> GetDashboardStats()
    {
        // 1. Core stats
        var completedOrders = _context.Orders.Where(o => o.Status == OrderStatus.Completed);
        var totalRevenue = await completedOrders.SumAsync(o => o.TotalAmount);
        var totalOrders = await _context.Orders.CountAsync();
        var activeStaffCount = await _context.Staffs.CountAsync(s => s.Status == "Active");
        var lowStockIngredientsCount = await _context.Ingredients.CountAsync(i => i.CurrentQuantity <= i.MinAlertQuantity);

        // 2. Weekly revenue (last 7 days)
        var now = DateTime.UtcNow.Date;
        var startDate = now.AddDays(-6);
        
        var ordersInWeek = await _context.Orders
            .Where(o => o.Status == OrderStatus.Completed && o.CreatedAt >= startDate)
            .ToListAsync();

        var weeklyRevenue = Enumerable.Range(0, 7)
            .Select(i => startDate.AddDays(i))
            .Select(date => new
            {
                Date = date.ToString("dd/MM"),
                DayOfWeek = date.ToString("ddd"),
                Revenue = ordersInWeek
                    .Where(o => o.CreatedAt.Date == date)
                    .Sum(o => o.TotalAmount)
            })
            .ToList();

        // 3. Recent orders (top 5)
        var recentOrders = await _context.Orders
            .OrderByDescending(o => o.CreatedAt)
            .Take(5)
            .Select(o => new
            {
                o.Id,
                o.OrderCode,
                o.CustomerName,
                o.TotalAmount,
                Status = o.Status.ToString(),
                o.CreatedAt
            })
            .ToListAsync();

        return Ok(new
        {
            success = true,
            data = new
            {
                totalRevenue,
                totalOrders,
                activeStaffCount,
                lowStockIngredientsCount,
                weeklyRevenue,
                recentOrders
            }
        });
    }
}
