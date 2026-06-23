using System.Collections.Generic;
using System.Threading.Tasks;
using CafePOS.Domain.Entities;

namespace CafePOS.Application.Interfaces.Repositories;

public interface IProductRepository
{
    Task<Product?> GetWithSizesAndToppingsByIdAsync(int id);
    Task<List<Product>> GetAllActiveWithSizesAndToppingsAsync();
    Task<PointProduct?> GetActivePointProductByProductIdAsync(int productId);
    Task<Product?> GetByIdAsync(int id);
    Task AddAsync(Product product);
    Task UpdateAsync(Product product);
    Task UpdateProductWithDetailsAsync(Product product, List<ProductSize> newSizes, List<Topping> newToppings);
    Task DeleteAsync(Product product);
    Task<(List<Product> items, int totalItems)> GetPagedProductsAsync(int page, int pageSize, int? categoryId, string? status, string? keyword);
    Task<List<Category>> GetCategoriesAsync();
    Task<Category?> GetCategoryByIdAsync(int id);
    Task AddCategoryAsync(Category category);
    Task UpdateCategoryAsync(Category category);
    Task DeleteCategoryAsync(Category category);
    Task<List<Topping>> GetToppingsAsync();
    Task<Topping?> GetToppingByIdAsync(int id);
    Task AddToppingAsync(Topping topping);
    Task UpdateToppingAsync(Topping topping);
    Task DeleteToppingAsync(Topping topping);
}
