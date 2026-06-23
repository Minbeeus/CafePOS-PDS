using Microsoft.AspNetCore.Mvc;
using CafePOS.Domain.Entities;
using CafePOS.Application.Interfaces.Repositories;
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
    private readonly IProductRepository _productRepository;

    public ProductsController(IProductRepository productRepository)
    {
        _productRepository = productRepository;
    }

    // ========================================================
    // I. PRODUCTS CRUD & PAGINATION
    // ========================================================

    [HttpGet]
    public async Task<IActionResult> GetProducts(
        [FromQuery] int? categoryId = null,
        [FromQuery] string? status = null,
        [FromQuery] string? keyword = null,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20)
    {
        if (page < 1) page = 1;
        if (pageSize < 1) pageSize = 10;
        if (pageSize > 1000) pageSize = 1000;

        var (products, totalItems) = await _productRepository.GetPagedProductsAsync(page, pageSize, categoryId, status, keyword);
        var totalPages = (int)Math.Ceiling((double)totalItems / pageSize);

        var mapped = products.Select(p => new
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
        }).ToList();

        return Ok(new
        {
            items = mapped,
            page,
            pageSize,
            totalItems,
            totalPages
        });
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetProductById(int id)
    {
        var p = await _productRepository.GetWithSizesAndToppingsByIdAsync(id);

        if (p == null)
        {
            return NotFound(new { message = "Không tìm thấy sản phẩm." });
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

        return Ok(result);
    }

    [HttpPost]
    [Authorize(Roles = "Owner")]
    public async Task<IActionResult> CreateProduct([FromBody] ProductCreateRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Name))
        {
            return BadRequest(new { message = "Tên sản phẩm không được để trống." });
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
            var toppings = new List<Topping>();
            foreach (var id in request.ToppingIds)
            {
                var topping = await _productRepository.GetToppingByIdAsync(id);
                if (topping != null) toppings.Add(topping);
            }
            product.Toppings = toppings;
        }

        if (request.HasSizeOption && request.Sizes != null && request.Sizes.Any())
        {
            foreach (var sizeReq in request.Sizes)
            {
                product.Sizes.Add(new ProductSize
                {
                    SizeLabel = sizeReq.SizeLabel,
                    PriceModifier = sizeReq.PriceModifier,
                    IsDefault = sizeReq.IsDefault
                });
            }
        }
        else
        {
            product.Sizes.Add(new ProductSize
            {
                SizeLabel = "Regular",
                PriceModifier = 0,
                IsDefault = true
            });
        }

        await _productRepository.AddAsync(product);

        return CreatedAtAction(nameof(GetProductById), new { id = product.Id }, product);
    }

    [HttpPut("{id}")]
    [Authorize(Roles = "Owner")]
    public async Task<IActionResult> UpdateProduct(int id, [FromBody] ProductCreateRequest request)
    {
        var product = await _productRepository.GetWithSizesAndToppingsByIdAsync(id);

        if (product == null)
        {
            return NotFound(new { message = "Không tìm thấy sản phẩm." });
        }

        if (string.IsNullOrWhiteSpace(request.Name))
        {
            return BadRequest(new { message = "Tên sản phẩm không được để trống." });
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

        var newSizes = new List<ProductSize>();
        if (request.HasSizeOption && request.Sizes != null && request.Sizes.Any())
        {
            foreach (var sizeReq in request.Sizes)
            {
                newSizes.Add(new ProductSize
                {
                    ProductId = product.Id,
                    SizeLabel = sizeReq.SizeLabel,
                    PriceModifier = sizeReq.PriceModifier,
                    IsDefault = sizeReq.IsDefault
                });
            }
        }
        else
        {
            newSizes.Add(new ProductSize
            {
                ProductId = product.Id,
                SizeLabel = "Regular",
                PriceModifier = 0,
                IsDefault = true
            });
        }

        var newToppings = new List<Topping>();
        if (request.ToppingIds != null && request.ToppingIds.Any())
        {
            foreach (var toppingId in request.ToppingIds)
            {
                var topping = await _productRepository.GetToppingByIdAsync(toppingId);
                if (topping != null) newToppings.Add(topping);
            }
        }

        await _productRepository.UpdateProductWithDetailsAsync(product, newSizes, newToppings);

        return Ok(product);
    }

    [HttpDelete("{id}")]
    [Authorize(Roles = "Owner")]
    public async Task<IActionResult> DeleteProduct(int id)
    {
        var product = await _productRepository.GetByIdAsync(id);
        if (product == null)
        {
            return NotFound(new { message = "Không tìm thấy sản phẩm." });
        }

        var staffId = GetCurrentStaffId();
        product.IsDeleted = true;
        product.DeletedAt = DateTime.UtcNow;
        product.DeletedBy = staffId;
        await _productRepository.UpdateAsync(product);

        return NoContent();
    }

    [HttpPatch("{id}/status")]
    [Authorize(Roles = "Owner,ShiftLeader")]
    public async Task<IActionResult> UpdateProductStatus(int id, [FromBody] ProductStatusUpdateRequest request)
    {
        var product = await _productRepository.GetByIdAsync(id);
        if (product == null)
        {
            return NotFound(new { message = "Không tìm thấy sản phẩm." });
        }

        if (string.IsNullOrWhiteSpace(request.Status))
        {
            return BadRequest(new { message = "Trạng thái không được để trống." });
        }

        product.Status = request.Status;
        await _productRepository.UpdateAsync(product);

        return Ok(product);
    }

    [HttpPost("upload-image")]
    [Authorize(Roles = "Owner,ShiftLeader")]
    public async Task<IActionResult> UploadImage(IFormFile file)
    {
        if (file == null || file.Length == 0)
        {
            return BadRequest(new { message = "Không tìm thấy file tải lên." });
        }

        var webRoot = Path.Combine(Directory.GetCurrentDirectory(), "..", "CafePOS.Web", "wwwroot");
        if (!Directory.Exists(webRoot))
        {
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
        return Ok(new { imageUrl = relativePath });
    }

    // ========================================================
    // II. CATEGORIES CRUD
    // ========================================================

    [HttpGet("categories")]
    public async Task<IActionResult> GetCategories()
    {
        var categories = await _productRepository.GetCategoriesAsync();
        var mapped = categories.Select(c => new
        {
            c.Id,
            c.Name,
            c.DisplayStation,
            c.DisplayOrder,
            c.IsActive,
            ProductsCount = c.Products.Count(p => !p.IsDeleted)
        }).ToList();

        return Ok(mapped);
    }

    [HttpPost("categories")]
    [Authorize(Roles = "Owner")]
    public async Task<IActionResult> CreateCategory([FromBody] CategoryRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Name))
        {
            return BadRequest(new { message = "Tên danh mục không được để trống." });
        }

        var categories = await _productRepository.GetCategoriesAsync();
        var displayOrder = categories.Any()
            ? categories.Max(c => c.DisplayOrder) + 1
            : 1;

        var category = new Category
        {
            Name = request.Name,
            DisplayStation = request.DisplayStation ?? "Bar",
            DisplayOrder = request.DisplayOrder ?? displayOrder,
            IsActive = request.IsActive ?? true
        };

        await _productRepository.AddCategoryAsync(category);

        return Created(string.Empty, category);
    }

    [HttpPut("categories/{id}")]
    [Authorize(Roles = "Owner")]
    public async Task<IActionResult> UpdateCategory(int id, [FromBody] CategoryRequest request)
    {
        var category = await _productRepository.GetCategoryByIdAsync(id);
        if (category == null)
        {
            return NotFound(new { message = "Không tìm thấy danh mục." });
        }

        if (string.IsNullOrWhiteSpace(request.Name))
        {
            return BadRequest(new { message = "Tên danh mục không được để trống." });
        }

        category.Name = request.Name;
        category.DisplayStation = request.DisplayStation ?? category.DisplayStation;
        category.DisplayOrder = request.DisplayOrder ?? category.DisplayOrder;
        category.IsActive = request.IsActive ?? category.IsActive;

        await _productRepository.UpdateCategoryAsync(category);

        return Ok(category);
    }

    [HttpDelete("categories/{id}")]
    [Authorize(Roles = "Owner")]
    public async Task<IActionResult> DeleteCategory(int id)
    {
        var category = await _productRepository.GetCategoryByIdAsync(id);
        if (category == null)
        {
            return NotFound(new { message = "Không tìm thấy danh mục." });
        }

        if (category.Products.Any(p => !p.IsDeleted))
        {
            return BadRequest(new { message = "Không thể xóa danh mục đã chứa sản phẩm." });
        }

        var staffId = GetCurrentStaffId();
        category.IsDeleted = true;
        category.DeletedAt = DateTime.UtcNow;
        category.DeletedBy = staffId;
        await _productRepository.UpdateCategoryAsync(category);

        return NoContent();
    }

    // ========================================================
    // III. TOPPINGS CRUD
    // ========================================================

    [HttpGet("toppings")]
    public async Task<IActionResult> GetToppings()
    {
        var toppings = await _productRepository.GetToppingsAsync();
        return Ok(toppings);
    }

    [HttpPost("toppings")]
    [Authorize(Roles = "Owner")]
    public async Task<IActionResult> CreateTopping([FromBody] ToppingRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Name))
        {
            return BadRequest(new { message = "Tên topping không được để trống." });
        }

        var topping = new Topping
        {
            Name = request.Name,
            Price = request.Price,
            IsActive = request.IsActive ?? true,
            CreatedAt = DateTime.UtcNow
        };

        await _productRepository.AddToppingAsync(topping);

        return Created(string.Empty, topping);
    }

    [HttpPut("toppings/{id}")]
    [Authorize(Roles = "Owner")]
    public async Task<IActionResult> UpdateTopping(int id, [FromBody] ToppingRequest request)
    {
        var topping = await _productRepository.GetToppingByIdAsync(id);
        if (topping == null)
        {
            return NotFound(new { message = "Không tìm thấy topping." });
        }

        if (string.IsNullOrWhiteSpace(request.Name))
        {
            return BadRequest(new { message = "Tên topping không được để trống." });
        }

        topping.Name = request.Name;
        topping.Price = request.Price;
        topping.IsActive = request.IsActive ?? topping.IsActive;

        await _productRepository.UpdateToppingAsync(topping);

        return Ok(topping);
    }

    [HttpDelete("toppings/{id}")]
    [Authorize(Roles = "Owner")]
    public async Task<IActionResult> DeleteTopping(int id)
    {
        var topping = await _productRepository.GetToppingByIdAsync(id);
        if (topping == null)
        {
            return NotFound(new { message = "Không tìm thấy topping." });
        }

        var staffId = GetCurrentStaffId();
        topping.IsDeleted = true;
        topping.DeletedAt = DateTime.UtcNow;
        topping.DeletedBy = staffId;
        await _productRepository.UpdateToppingAsync(topping);

        return NoContent();
    }

    private int GetCurrentStaffId()
    {
        var subClaim = User?.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value 
            ?? User?.FindFirst("sub")?.Value;
        
        if (int.TryParse(subClaim, out int staffId))
        {
            return staffId;
        }
        return 0;
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
