using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using CafePOS.Infrastructure.Data;
using CafePOS.Domain.Entities;
using System.Threading.Tasks;
using System.Linq;
using System.IO;
using Microsoft.AspNetCore.Http;
using System.Collections.Generic;
using System;
using Microsoft.AspNetCore.Authorization;

namespace CafePOS.API.Controllers;

[ApiController]
[Route("api/v1/[controller]")]
public class ProductsController : ControllerBase
{
    private readonly AppDbContext _context;

    public ProductsController(AppDbContext context)
    {
        _context = context;
    }

    // ========================================================
    // I. PRODUCTS CRUD
    // ========================================================

    [HttpGet]
    public async Task<IActionResult> GetProducts()
    {
        var products = await _context.Products
            .Include(p => p.Category)
            .Include(p => p.Sizes)
            .Include(p => p.Toppings)
            .OrderBy(p => p.Name)
            .Select(p => new
            {
                p.Id,
                p.CategoryId,
                p.Name,
                p.Description,
                p.BasePrice,
                p.ImageUrl,
                p.Status,
                p.HasSizeOption,
                p.HasSugarOption,
                p.HasIceOption,
                CategoryName = p.Category != null ? p.Category.Name : "Chưa phân loại",
                Sizes = p.Sizes.Select(s => new
                {
                    s.Id,
                    s.SizeLabel,
                    s.PriceModifier,
                    s.IsDefault
                }).ToList(),
                Toppings = p.Toppings.Select(t => new
                {
                    t.Id,
                    t.Name,
                    t.Price
                }).ToList()
            })
            .ToListAsync();

        return Ok(new { success = true, data = products });
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetProductById(int id)
    {
        var p = await _context.Products
            .Include(p => p.Category)
            .Include(p => p.Sizes)
            .Include(p => p.Toppings)
            .FirstOrDefaultAsync(p => p.Id == id);

        if (p == null)
        {
            return NotFound(new { success = false, message = "Không tìm thấy sản phẩm." });
        }

        var result = new
        {
            p.Id,
            p.CategoryId,
            p.Name,
            p.Description,
            p.BasePrice,
            p.ImageUrl,
            p.Status,
            p.HasSizeOption,
            p.HasSugarOption,
            p.HasIceOption,
            CategoryName = p.Category != null ? p.Category.Name : "Chưa phân loại",
            Sizes = p.Sizes.Select(s => new
            {
                s.Id,
                s.SizeLabel,
                s.PriceModifier,
                s.IsDefault
            }).ToList(),
            Toppings = p.Toppings.Select(t => new
            {
                t.Id,
                t.Name,
                t.Price
            }).ToList()
        };

        return Ok(new { success = true, data = result });
    }

    [HttpPost]
    [Authorize(Roles = "Owner")]
    public async Task<IActionResult> CreateProduct([FromBody] ProductCreateRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Name))
        {
            return BadRequest(new { success = false, message = "Tên sản phẩm không được để trống." });
        }

        var product = new Product
        {
            CategoryId = request.CategoryId,
            Name = request.Name,
            Description = request.Description ?? "",
            BasePrice = request.BasePrice,
            ImageUrl = request.ImageUrl ?? "",
            Status = request.Status ?? "Active",
            HasSizeOption = request.HasSizeOption,
            HasSugarOption = request.HasSugarOption,
            HasIceOption = request.HasIceOption,
            CreatedAt = DateTime.UtcNow
        };

        if (request.ToppingIds != null && request.ToppingIds.Any())
        {
            var toppings = await _context.Toppings
                .Where(t => request.ToppingIds.Contains(t.Id))
                .ToListAsync();
            product.Toppings = toppings;
        }

        _context.Products.Add(product);
        await _context.SaveChangesAsync(); // Generates productId

        // Add sizes
        if (request.HasSizeOption && request.Sizes != null && request.Sizes.Any())
        {
            foreach (var sizeReq in request.Sizes)
            {
                var size = new ProductSize
                {
                    ProductId = product.Id,
                    SizeLabel = sizeReq.SizeLabel,
                    PriceModifier = sizeReq.PriceModifier,
                    IsDefault = sizeReq.IsDefault
                };
                _context.ProductSizes.Add(size);
            }
        }
        else
        {
            // Default size if size option is disabled
            var defaultSize = new ProductSize
            {
                ProductId = product.Id,
                SizeLabel = "Regular",
                PriceModifier = 0,
                IsDefault = true
            };
            _context.ProductSizes.Add(defaultSize);
        }

        await _context.SaveChangesAsync();

        return Ok(new { success = true, data = product, message = "Tạo sản phẩm thành công" });
    }

    [HttpPut("{id}")]
    [Authorize(Roles = "Owner")]
    public async Task<IActionResult> UpdateProduct(int id, [FromBody] ProductCreateRequest request)
    {
        var product = await _context.Products
            .Include(p => p.Sizes)
            .Include(p => p.Toppings)
            .FirstOrDefaultAsync(p => p.Id == id);

        if (product == null)
        {
            return NotFound(new { success = false, message = "Không tìm thấy sản phẩm." });
        }

        if (string.IsNullOrWhiteSpace(request.Name))
        {
            return BadRequest(new { success = false, message = "Tên sản phẩm không được để trống." });
        }

        product.CategoryId = request.CategoryId;
        product.Name = request.Name;
        product.Description = request.Description ?? "";
        product.BasePrice = request.BasePrice;
        product.ImageUrl = request.ImageUrl ?? product.ImageUrl;
        product.Status = request.Status ?? product.Status;
        product.HasSizeOption = request.HasSizeOption;
        product.HasSugarOption = request.HasSugarOption;
        product.HasIceOption = request.HasIceOption;

        // Clear existing sizes
        _context.ProductSizes.RemoveRange(product.Sizes);

        // Add updated sizes
        if (request.HasSizeOption && request.Sizes != null && request.Sizes.Any())
        {
            foreach (var sizeReq in request.Sizes)
            {
                var size = new ProductSize
                {
                    ProductId = product.Id,
                    SizeLabel = sizeReq.SizeLabel,
                    PriceModifier = sizeReq.PriceModifier,
                    IsDefault = sizeReq.IsDefault
                };
                _context.ProductSizes.Add(size);
            }
        }
        else
        {
            var defaultSize = new ProductSize
            {
                ProductId = product.Id,
                SizeLabel = "Regular",
                PriceModifier = 0,
                IsDefault = true
            };
            _context.ProductSizes.Add(defaultSize);
        }

        // Update toppings
        product.Toppings.Clear();
        if (request.ToppingIds != null && request.ToppingIds.Any())
        {
            var toppings = await _context.Toppings
                .Where(t => request.ToppingIds.Contains(t.Id))
                .ToListAsync();
            product.Toppings = toppings;
        }

        await _context.SaveChangesAsync();

        return Ok(new { success = true, data = product, message = "Cập nhật sản phẩm thành công" });
    }

    [HttpDelete("{id}")]
    [Authorize(Roles = "Owner")]
    public async Task<IActionResult> DeleteProduct(int id)
    {
        var product = await _context.Products.FindAsync(id);
        if (product == null)
        {
            return NotFound(new { success = false, message = "Không tìm thấy sản phẩm." });
        }

        _context.Products.Remove(product);
        await _context.SaveChangesAsync();

        return Ok(new { success = true, message = "Xóa sản phẩm thành công" });
    }

    [HttpPatch("{id}/status")]
    [Authorize(Roles = "Owner,ShiftLeader")]
    public async Task<IActionResult> UpdateProductStatus(int id, [FromBody] ProductStatusUpdateRequest request)
    {
        var product = await _context.Products.FindAsync(id);
        if (product == null)
        {
            return NotFound(new { success = false, message = "Không tìm thấy sản phẩm." });
        }

        if (string.IsNullOrWhiteSpace(request.Status))
        {
            return BadRequest(new { success = false, message = "Trạng thái không được để trống." });
        }

        product.Status = request.Status;
        await _context.SaveChangesAsync();

        return Ok(new { success = true, data = product, message = "Cập nhật trạng thái thành công" });
    }

    [HttpPost("upload-image")]
    [Authorize(Roles = "Owner,ShiftLeader")]
    public async Task<IActionResult> UploadImage(IFormFile file)
    {
        if (file == null || file.Length == 0)
        {
            return BadRequest(new { success = false, message = "Không tìm thấy file tải lên." });
        }

        // Try to resolve path to CafePOS.Web wwwroot
        var webRoot = Path.Combine(Directory.GetCurrentDirectory(), "..", "CafePOS.Web", "wwwroot");
        if (!Directory.Exists(webRoot))
        {
            // Fallback to local wwwroot if executing directly inside Web folder
            webRoot = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot");
        }
        
        var folderPath = Path.Combine(webRoot, "images", "products");
        if (!Directory.Exists(folderPath))
        {
            Directory.CreateDirectory(folderPath);
        }

        var extension = Path.GetExtension(file.FileName);
        var filename = $"{Guid.NewGuid()}{extension}";
        var filePath = Path.Combine(folderPath, filename);

        using (var stream = new FileStream(filePath, FileMode.Create))
        {
            await file.CopyToAsync(stream);
        }

        var relativePath = $"/images/products/{filename}";
        return Ok(new { success = true, data = relativePath, message = "Tải ảnh lên thành công." });
    }

    // ========================================================
    // II. CATEGORIES CRUD
    // ========================================================

    [HttpGet("categories")]
    public async Task<IActionResult> GetCategories()
    {
        var categories = await _context.Categories
            .Include(c => c.Products)
            .OrderBy(c => c.DisplayOrder)
            .Select(c => new
            {
                c.Id,
                c.Name,
                c.DisplayStation,
                c.DisplayOrder,
                c.IsActive,
                ProductsCount = c.Products.Count
            })
            .ToListAsync();

        return Ok(new { success = true, data = categories });
    }

    [HttpPost("categories")]
    [Authorize(Roles = "Owner")]
    public async Task<IActionResult> CreateCategory([FromBody] CategoryRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Name))
        {
            return BadRequest(new { success = false, message = "Tên danh mục không được để trống." });
        }

        var displayOrder = await _context.Categories.AnyAsync()
            ? await _context.Categories.MaxAsync(c => c.DisplayOrder) + 1
            : 1;

        var category = new Category
        {
            Name = request.Name,
            DisplayStation = request.DisplayStation ?? "Bar",
            DisplayOrder = request.DisplayOrder ?? displayOrder,
            IsActive = request.IsActive ?? true
        };

        _context.Categories.Add(category);
        await _context.SaveChangesAsync();

        return Ok(new { success = true, data = category, message = "Tạo danh mục thành công" });
    }

    [HttpPut("categories/{id}")]
    [Authorize(Roles = "Owner")]
    public async Task<IActionResult> UpdateCategory(int id, [FromBody] CategoryRequest request)
    {
        var category = await _context.Categories.FindAsync(id);
        if (category == null)
        {
            return NotFound(new { success = false, message = "Không tìm thấy danh mục." });
        }

        if (string.IsNullOrWhiteSpace(request.Name))
        {
            return BadRequest(new { success = false, message = "Tên danh mục không được để trống." });
        }

        category.Name = request.Name;
        category.DisplayStation = request.DisplayStation ?? category.DisplayStation;
        category.DisplayOrder = request.DisplayOrder ?? category.DisplayOrder;
        category.IsActive = request.IsActive ?? category.IsActive;

        await _context.SaveChangesAsync();

        return Ok(new { success = true, data = category, message = "Cập nhật danh mục thành công" });
    }

    [HttpDelete("categories/{id}")]
    [Authorize(Roles = "Owner")]
    public async Task<IActionResult> DeleteCategory(int id)
    {
        var category = await _context.Categories
            .Include(c => c.Products)
            .FirstOrDefaultAsync(c => c.Id == id);

        if (category == null)
        {
            return NotFound(new { success = false, message = "Không tìm thấy danh mục." });
        }

        if (category.Products.Any())
        {
            return BadRequest(new { success = false, message = "Không thể xóa danh mục đã chứa sản phẩm." });
        }

        _context.Categories.Remove(category);
        await _context.SaveChangesAsync();

        return Ok(new { success = true, message = "Xóa danh mục thành công" });
    }

    // ========================================================
    // III. TOPPINGS CRUD
    // ========================================================

    [HttpGet("toppings")]
    public async Task<IActionResult> GetToppings()
    {
        var toppings = await _context.Toppings
            .OrderBy(t => t.Name)
            .ToListAsync();

        return Ok(new { success = true, data = toppings });
    }

    [HttpPost("toppings")]
    [Authorize(Roles = "Owner")]
    public async Task<IActionResult> CreateTopping([FromBody] ToppingRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Name))
        {
            return BadRequest(new { success = false, message = "Tên topping không được để trống." });
        }

        var topping = new Topping
        {
            Name = request.Name,
            Price = request.Price,
            IsActive = request.IsActive ?? true,
            CreatedAt = DateTime.UtcNow
        };

        _context.Toppings.Add(topping);
        await _context.SaveChangesAsync();

        return Ok(new { success = true, data = topping, message = "Tạo topping thành công" });
    }

    [HttpPut("toppings/{id}")]
    [Authorize(Roles = "Owner")]
    public async Task<IActionResult> UpdateTopping(int id, [FromBody] ToppingRequest request)
    {
        var topping = await _context.Toppings.FindAsync(id);
        if (topping == null)
        {
            return NotFound(new { success = false, message = "Không tìm thấy topping." });
        }

        if (string.IsNullOrWhiteSpace(request.Name))
        {
            return BadRequest(new { success = false, message = "Tên topping không được để trống." });
        }

        topping.Name = request.Name;
        topping.Price = request.Price;
        topping.IsActive = request.IsActive ?? topping.IsActive;

        await _context.SaveChangesAsync();

        return Ok(new { success = true, data = topping, message = "Cập nhật topping thành công" });
    }

    [HttpDelete("toppings/{id}")]
    [Authorize(Roles = "Owner")]
    public async Task<IActionResult> DeleteTopping(int id)
    {
        var topping = await _context.Toppings.FindAsync(id);
        if (topping == null)
        {
            return NotFound(new { success = false, message = "Không tìm thấy topping." });
        }

        _context.Toppings.Remove(topping);
        await _context.SaveChangesAsync();

        return Ok(new { success = true, message = "Xóa topping thành công" });
    }
}

// Request models

public class ProductCreateRequest
{
    public int CategoryId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public decimal BasePrice { get; set; }
    public string? ImageUrl { get; set; }
    public string? Status { get; set; }
    public bool HasSizeOption { get; set; }
    public bool HasSugarOption { get; set; }
    public bool HasIceOption { get; set; }
    public List<SizeRequest>? Sizes { get; set; }
    public List<int>? ToppingIds { get; set; }
}

public class SizeRequest
{
    public string SizeLabel { get; set; } = string.Empty;
    public decimal PriceModifier { get; set; }
    public bool IsDefault { get; set; }
}

public class CategoryRequest
{
    public string Name { get; set; } = string.Empty;
    public string? DisplayStation { get; set; }
    public int? DisplayOrder { get; set; }
    public bool? IsActive { get; set; }
}

public class ToppingRequest
{
    public string Name { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public bool? IsActive { get; set; }
}

public class ProductStatusUpdateRequest
{
    public string Status { get; set; } = string.Empty;
}
