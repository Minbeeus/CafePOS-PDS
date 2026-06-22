using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using CafePOS.API.Controllers;
using CafePOS.Domain.Entities;
using CafePOS.Infrastructure.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace CafePOS.Tests.UnitTests;

public class MenuTests : IDisposable
{
    private readonly AppDbContext _context;
    private readonly ProductsController _controller;

    public MenuTests()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        _context = new AppDbContext(options);
        _context.Database.EnsureCreated();
        _controller = new ProductsController(_context);
    }

    public void Dispose()
    {
        _context.Database.EnsureDeleted();
        _context.Dispose();
    }

    // ========================================================
    // I. CATEGORIES TESTS
    // ========================================================

    [Fact]
    public async Task GetCategories_ReturnsAllCategories()
    {
        // Arrange
        _context.Categories.Add(new Category { Name = "Category 1", DisplayStation = "Bar", DisplayOrder = 1, IsActive = true });
        _context.Categories.Add(new Category { Name = "Category 2", DisplayStation = "Pastry", DisplayOrder = 2, IsActive = true });
        await _context.SaveChangesAsync();

        // Act
        var result = await _controller.GetCategories();

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        Assert.NotNull(okResult.Value);
        
        var json = JsonSerializer.Serialize(okResult.Value);
        using var doc = JsonDocument.Parse(json);
        var data = doc.RootElement.GetProperty("data");
        Assert.Equal(2, data.GetArrayLength());
    }

    [Fact]
    public async Task CreateCategory_SavesCategory()
    {
        // Arrange
        var request = new CategoryRequest
        {
            Name = "New Category",
            DisplayStation = "Bar",
            IsActive = true
        };

        // Act
        var result = await _controller.CreateCategory(request);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        var savedCategory = await _context.Categories.FirstOrDefaultAsync(c => c.Name == "New Category");
        Assert.NotNull(savedCategory);
        Assert.Equal("Bar", savedCategory.DisplayStation);
        Assert.Equal(1, savedCategory.DisplayOrder);
    }

    [Fact]
    public async Task CreateCategory_EmptyName_ReturnsBadRequest()
    {
        // Arrange
        var request = new CategoryRequest { Name = "" };

        // Act
        var result = await _controller.CreateCategory(request);

        // Assert
        Assert.IsType<BadRequestObjectResult>(result);
    }

    [Fact]
    public async Task UpdateCategory_UpdatesDetails()
    {
        // Arrange
        var category = new Category { Name = "Old Name", DisplayStation = "Bar", DisplayOrder = 1, IsActive = true };
        _context.Categories.Add(category);
        await _context.SaveChangesAsync();

        var request = new CategoryRequest
        {
            Name = "New Name",
            DisplayStation = "Pastry",
            DisplayOrder = 5,
            IsActive = false
        };

        // Act
        var result = await _controller.UpdateCategory(category.Id, request);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        var updated = await _context.Categories.FindAsync(category.Id);
        Assert.NotNull(updated);
        Assert.Equal("New Name", updated.Name);
        Assert.Equal("Pastry", updated.DisplayStation);
        Assert.Equal(5, updated.DisplayOrder);
        Assert.False(updated.IsActive);
    }

    [Fact]
    public async Task UpdateCategory_NotFound_ReturnsNotFound()
    {
        // Act
        var result = await _controller.UpdateCategory(999, new CategoryRequest { Name = "Name" });

        // Assert
        Assert.IsType<NotFoundObjectResult>(result);
    }

    [Fact]
    public async Task DeleteCategory_NoProducts_RemovesCategory()
    {
        // Arrange
        var category = new Category { Name = "Empty Category", DisplayStation = "Bar", IsActive = true };
        _context.Categories.Add(category);
        await _context.SaveChangesAsync();

        // Act
        var result = await _controller.DeleteCategory(category.Id);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        var deleted = await _context.Categories.FindAsync(category.Id);
        Assert.Null(deleted);
    }

    [Fact]
    public async Task DeleteCategory_HasProducts_ReturnsBadRequest()
    {
        // Arrange
        var category = new Category { Name = "Has Products", DisplayStation = "Bar", IsActive = true };
        _context.Categories.Add(category);
        await _context.SaveChangesAsync();

        var product = new Product { CategoryId = category.Id, Name = "Product", BasePrice = 10000, Status = "Active" };
        _context.Products.Add(product);
        await _context.SaveChangesAsync();

        // Act
        var result = await _controller.DeleteCategory(category.Id);

        // Assert
        Assert.IsType<BadRequestObjectResult>(result);
    }

    // ========================================================
    // II. TOPPINGS TESTS
    // ========================================================

    [Fact]
    public async Task GetToppings_ReturnsAllToppings()
    {
        // Arrange
        _context.Toppings.Add(new Topping { Name = "Topping 1", Price = 5000, IsActive = true });
        _context.Toppings.Add(new Topping { Name = "Topping 2", Price = 6000, IsActive = true });
        await _context.SaveChangesAsync();

        // Act
        var result = await _controller.GetToppings();

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        Assert.NotNull(okResult.Value);
        
        var json = JsonSerializer.Serialize(okResult.Value);
        using var doc = JsonDocument.Parse(json);
        var data = doc.RootElement.GetProperty("data");
        Assert.Equal(2, data.GetArrayLength());
    }

    [Fact]
    public async Task CreateTopping_SavesTopping()
    {
        // Arrange
        var request = new ToppingRequest { Name = "Pearl", Price = 5000, IsActive = true };

        // Act
        var result = await _controller.CreateTopping(request);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        var saved = await _context.Toppings.FirstOrDefaultAsync(t => t.Name == "Pearl");
        Assert.NotNull(saved);
        Assert.Equal(5000, saved.Price);
    }

    [Fact]
    public async Task CreateTopping_EmptyName_ReturnsBadRequest()
    {
        // Arrange
        var request = new ToppingRequest { Name = "", Price = 5000 };

        // Act
        var result = await _controller.CreateTopping(request);

        // Assert
        Assert.IsType<BadRequestObjectResult>(result);
    }

    [Fact]
    public async Task UpdateTopping_UpdatesDetails()
    {
        // Arrange
        var topping = new Topping { Name = "Old", Price = 3000, IsActive = true };
        _context.Toppings.Add(topping);
        await _context.SaveChangesAsync();

        var request = new ToppingRequest { Name = "New", Price = 4000, IsActive = false };

        // Act
        var result = await _controller.UpdateTopping(topping.Id, request);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        var updated = await _context.Toppings.FindAsync(topping.Id);
        Assert.NotNull(updated);
        Assert.Equal("New", updated.Name);
        Assert.Equal(4000, updated.Price);
        Assert.False(updated.IsActive);
    }

    [Fact]
    public async Task DeleteTopping_RemovesTopping()
    {
        // Arrange
        var topping = new Topping { Name = "Delete Me", Price = 1000, IsActive = true };
        _context.Toppings.Add(topping);
        await _context.SaveChangesAsync();

        // Act
        var result = await _controller.DeleteTopping(topping.Id);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        var deleted = await _context.Toppings.FindAsync(topping.Id);
        Assert.Null(deleted);
    }

    // ========================================================
    // III. PRODUCTS & SIZES TESTS
    // ========================================================

    [Fact]
    public async Task GetProducts_ReturnsProductsWithSizesAndToppings()
    {
        // Arrange
        var category = new Category { Name = "Coffee", DisplayStation = "Bar", IsActive = true };
        _context.Categories.Add(category);

        var topping = new Topping { Name = "Jelly", Price = 5000, IsActive = true };
        _context.Toppings.Add(topping);
        await _context.SaveChangesAsync();

        var product = new Product
        {
            CategoryId = category.Id,
            Name = "Black Coffee",
            BasePrice = 20000,
            Status = "Active"
        };
        product.Toppings.Add(topping);
        _context.Products.Add(product);
        await _context.SaveChangesAsync();

        var size = new ProductSize { ProductId = product.Id, SizeLabel = "L", PriceModifier = 5000, IsDefault = false };
        _context.ProductSizes.Add(size);
        await _context.SaveChangesAsync();

        // Act
        var result = await _controller.GetProducts();

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        Assert.NotNull(okResult.Value);
        
        var json = JsonSerializer.Serialize(okResult.Value);
        using var doc = JsonDocument.Parse(json);
        var data = doc.RootElement.GetProperty("data");
        Assert.Equal(1, data.GetArrayLength());
    }

    [Fact]
    public async Task GetProductById_ReturnsCorrectProduct()
    {
        // Arrange
        var category = new Category { Name = "Tea", DisplayStation = "Bar", IsActive = true };
        _context.Categories.Add(category);
        await _context.SaveChangesAsync();

        var product = new Product { CategoryId = category.Id, Name = "Tea", BasePrice = 15000, Status = "Active" };
        _context.Products.Add(product);
        await _context.SaveChangesAsync();

        // Act
        var result = await _controller.GetProductById(product.Id);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        Assert.NotNull(okResult.Value);
        
        var json = JsonSerializer.Serialize(okResult.Value);
        using var doc = JsonDocument.Parse(json);
        var data = doc.RootElement.GetProperty("data");
        Assert.Equal(product.Id, data.GetProperty("Id").GetInt32());
    }

    [Fact]
    public async Task GetProductById_NotFound_ReturnsNotFound()
    {
        // Act
        var result = await _controller.GetProductById(999);

        // Assert
        Assert.IsType<NotFoundObjectResult>(result);
    }

    [Fact]
    public async Task CreateProduct_WithSizesAndToppings_SavesSuccessfully()
    {
        // Arrange
        var category = new Category { Name = "Tea", DisplayStation = "Bar", IsActive = true };
        var topping1 = new Topping { Name = "Chia", Price = 4000, IsActive = true };
        _context.Categories.Add(category);
        _context.Toppings.Add(topping1);
        await _context.SaveChangesAsync();

        var request = new ProductCreateRequest
        {
            CategoryId = category.Id,
            Name = "Green Tea",
            BasePrice = 25000,
            HasSizeOption = true,
            Sizes = new List<SizeRequest>
            {
                new SizeRequest { SizeLabel = "M", PriceModifier = 0, IsDefault = true },
                new SizeRequest { SizeLabel = "L", PriceModifier = 5000, IsDefault = false }
            },
            ToppingIds = new List<int> { topping1.Id }
        };

        // Act
        var result = await _controller.CreateProduct(request);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        var saved = await _context.Products
            .Include(p => p.Sizes)
            .Include(p => p.Toppings)
            .FirstOrDefaultAsync(p => p.Name == "Green Tea");

        Assert.NotNull(saved);
        Assert.Equal(2, saved.Sizes.Count);
        Assert.Single(saved.Toppings);
        Assert.Equal("Chia", saved.Toppings.First().Name);
    }

    [Fact]
    public async Task CreateProduct_NoSizeOption_SavesWithDefaultSize()
    {
        // Arrange
        var category = new Category { Name = "Tea", DisplayStation = "Bar", IsActive = true };
        _context.Categories.Add(category);
        await _context.SaveChangesAsync();

        var request = new ProductCreateRequest
        {
            CategoryId = category.Id,
            Name = "Espresso",
            BasePrice = 30000,
            HasSizeOption = false
        };

        // Act
        var result = await _controller.CreateProduct(request);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        var saved = await _context.Products
            .Include(p => p.Sizes)
            .FirstOrDefaultAsync(p => p.Name == "Espresso");

        Assert.NotNull(saved);
        Assert.Single(saved.Sizes);
        Assert.Equal("Regular", saved.Sizes.First().SizeLabel);
        Assert.True(saved.Sizes.First().IsDefault);
    }

    [Fact]
    public async Task CreateProduct_EmptyName_ReturnsBadRequest()
    {
        // Arrange
        var request = new ProductCreateRequest { Name = "" };

        // Act
        var result = await _controller.CreateProduct(request);

        // Assert
        Assert.IsType<BadRequestObjectResult>(result);
    }

    [Fact]
    public async Task UpdateProduct_UpdatesProductSizesAndToppings()
    {
        // Arrange
        var category = new Category { Name = "Tea", DisplayStation = "Bar", IsActive = true };
        var topping1 = new Topping { Name = "Chia", Price = 4000, IsActive = true };
        var topping2 = new Topping { Name = "Aloevera", Price = 6000, IsActive = true };
        _context.Categories.Add(category);
        _context.Toppings.AddRange(topping1, topping2);
        await _context.SaveChangesAsync();

        var product = new Product { CategoryId = category.Id, Name = "Oolong", BasePrice = 20000, Status = "Active" };
        product.Toppings.Add(topping1);
        _context.Products.Add(product);
        await _context.SaveChangesAsync();

        var size = new ProductSize { ProductId = product.Id, SizeLabel = "S", PriceModifier = 0, IsDefault = true };
        _context.ProductSizes.Add(size);
        await _context.SaveChangesAsync();

        var request = new ProductCreateRequest
        {
            CategoryId = category.Id,
            Name = "New Oolong",
            BasePrice = 22000,
            HasSizeOption = true,
            Sizes = new List<SizeRequest>
            {
                new SizeRequest { SizeLabel = "L", PriceModifier = 6000, IsDefault = true }
            },
            ToppingIds = new List<int> { topping2.Id }
        };

        // Act
        var result = await _controller.UpdateProduct(product.Id, request);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        var updated = await _context.Products
            .Include(p => p.Sizes)
            .Include(p => p.Toppings)
            .FirstOrDefaultAsync(p => p.Id == product.Id);

        Assert.NotNull(updated);
        Assert.Equal("New Oolong", updated.Name);
        Assert.Equal(22000, updated.BasePrice);
        Assert.Single(updated.Sizes);
        Assert.Equal("L", updated.Sizes.First().SizeLabel);
        Assert.Single(updated.Toppings);
        Assert.Equal("Aloevera", updated.Toppings.First().Name);
    }

    [Fact]
    public async Task DeleteProduct_RemovesProduct()
    {
        // Arrange
        var category = new Category { Name = "Tea", DisplayStation = "Bar", IsActive = true };
        _context.Categories.Add(category);
        await _context.SaveChangesAsync();

        var product = new Product { CategoryId = category.Id, Name = "Delete Item", BasePrice = 10000, Status = "Active" };
        _context.Products.Add(product);
        await _context.SaveChangesAsync();

        // Act
        var result = await _controller.DeleteProduct(product.Id);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        var deleted = await _context.Products.FindAsync(product.Id);
        Assert.Null(deleted);
    }

    [Fact]
    public async Task UpdateProductStatus_UpdatesStatusSuccessfully()
    {
        // Arrange
        var category = new Category { Name = "Tea", DisplayStation = "Bar", IsActive = true };
        _context.Categories.Add(category);
        await _context.SaveChangesAsync();

        var product = new Product { CategoryId = category.Id, Name = "Status Item", BasePrice = 10000, Status = "Active" };
        _context.Products.Add(product);
        await _context.SaveChangesAsync();

        var request = new ProductStatusUpdateRequest { Status = "OutOfStock" };

        // Act
        var result = await _controller.UpdateProductStatus(product.Id, request);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        var updated = await _context.Products.FindAsync(product.Id);
        Assert.NotNull(updated);
        Assert.Equal("OutOfStock", updated.Status);
    }
}

