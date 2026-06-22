using System.Collections.Generic;
using System.Threading.Tasks;
using CafePOS.Domain.Entities;

namespace CafePOS.Application.Interfaces.Repositories;

public interface IProductRepository
{
    Task<Product?> GetWithSizesAndToppingsByIdAsync(int id);
    Task<List<Product>> GetAllActiveWithSizesAndToppingsAsync();
    Task<PointProduct?> GetActivePointProductByProductIdAsync(int productId);
}
