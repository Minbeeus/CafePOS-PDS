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
            .Include(p => p.Category)
            .Include(p => p.Sizes)
            .Include(p => p.Toppings)
            .FirstOrDefaultAsync(p => p.Id == id);
    }

    public async Task<List<Product>> GetAllActiveWithSizesAndToppingsAsync()
    {
        return await _context.Products
            .Include(p => p.Sizes)
            .Include(p => p.Toppings)
            .AsNoTracking()
            .Where(p => p.Status == "Active")
            .ToListAsync();
    }

    public async Task<PointProduct?> GetActivePointProductByProductIdAsync(int productId)
    {
        return await _context.PointProducts
            .AsNoTracking()
            .FirstOrDefaultAsync(pp => pp.LinkedProductId == productId && pp.IsActive);
    }

    public async Task<Product?> GetByIdAsync(int id)
    {
        return await _context.Products.FindAsync(id);
    }

    public async Task AddAsync(Product product)
    {
        _context.Products.Add(product);
        await _context.SaveChangesAsync();
    }

    public async Task UpdateAsync(Product product)
    {
        _context.Products.Update(product);
        await _context.SaveChangesAsync();
    }

    public async Task UpdateProductWithDetailsAsync(Product product, List<ProductSize> newSizes, List<Topping> newToppings)
    {
        _context.ProductSizes.RemoveRange(product.Sizes);
        product.Sizes = newSizes;

        product.Toppings.Clear();
        foreach (var topping in newToppings)
        {
            product.Toppings.Add(topping);
        }

        await _context.SaveChangesAsync();
    }

    public async Task DeleteAsync(Product product)
    {
        _context.Products.Remove(product);
        await _context.SaveChangesAsync();
    }

    public async Task<(List<Product> items, int totalItems)> GetPagedProductsAsync(int page, int pageSize, int? categoryId, string? status, string? keyword)
    {
        var query = _context.Products
            .Include(p => p.Category)
            .Include(p => p.Sizes)
            .Include(p => p.Toppings)
            .AsNoTracking()
            .AsQueryable();

        if (categoryId.HasValue)
        {
            query = query.Where(p => p.CategoryId == categoryId.Value);
        }

        if (!string.IsNullOrWhiteSpace(status))
        {
            query = query.Where(p => p.Status == status);
        }

        if (!string.IsNullOrWhiteSpace(keyword))
        {
            query = query.Where(p => p.Name.Contains(keyword) || (p.Description != null && p.Description.Contains(keyword)));
        }

        var totalItems = await query.CountAsync();
        var items = await query
            .OrderBy(p => p.Name)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return (items, totalItems);
    }

    public async Task<List<Category>> GetCategoriesAsync()
    {
        return await _context.Categories
            .Include(c => c.Products)
            .AsNoTracking()
            .OrderBy(c => c.DisplayOrder)
            .ToListAsync();
    }

    public async Task<Category?> GetCategoryByIdAsync(int id)
    {
        return await _context.Categories
            .Include(c => c.Products)
            .FirstOrDefaultAsync(c => c.Id == id);
    }

    public async Task AddCategoryAsync(Category category)
    {
        _context.Categories.Add(category);
        await _context.SaveChangesAsync();
    }

    public async Task UpdateCategoryAsync(Category category)
    {
        _context.Categories.Update(category);
        await _context.SaveChangesAsync();
    }

    public async Task DeleteCategoryAsync(Category category)
    {
        _context.Categories.Remove(category);
        await _context.SaveChangesAsync();
    }

    public async Task<List<Topping>> GetToppingsAsync()
    {
        return await _context.Toppings
            .AsNoTracking()
            .OrderBy(t => t.Name)
            .ToListAsync();
    }

    public async Task<Topping?> GetToppingByIdAsync(int id)
    {
        return await _context.Toppings.FindAsync(id);
    }

    public async Task AddToppingAsync(Topping topping)
    {
        _context.Toppings.Add(topping);
        await _context.SaveChangesAsync();
    }

    public async Task UpdateToppingAsync(Topping topping)
    {
        _context.Toppings.Update(topping);
        await _context.SaveChangesAsync();
    }

    public async Task DeleteToppingAsync(Topping topping)
    {
        _context.Toppings.Remove(topping);
        await _context.SaveChangesAsync();
    }
}
