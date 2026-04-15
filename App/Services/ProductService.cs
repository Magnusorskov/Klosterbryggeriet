using BlazorApp.Data;
using BlazorApp.Models;
using Microsoft.EntityFrameworkCore;

namespace BlazorApp.Services;

public class ProductService
{
    private readonly AppDbContext _db;

    public ProductService(AppDbContext db)
    {
        _db = db;
    }

    public Task<List<Product>> ListProducts()
    {
        return _db.Products.ToListAsync();
    }

    public async Task<Dictionary<string, List<Product>>> MapProductsByCategory() {
        var products = await ListProducts();
        return products.GroupBy(p => p.Category)
            .ToDictionary(g => g.Key, g => g.ToList());
    }
}