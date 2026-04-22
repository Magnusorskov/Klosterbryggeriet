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

    public Task<List<Product>> ListInUseProducts()
    {
        return _db.Products.Where(p => p.InUse).ToListAsync();
    }

    public async Task<Dictionary<string, List<Product>>> MapProductsByCategory() {
        var products = await ListInUseProducts();
        return products.GroupBy(p => p.Category)
            .ToDictionary(g => g.Key, g => g.ToList());
    }
}