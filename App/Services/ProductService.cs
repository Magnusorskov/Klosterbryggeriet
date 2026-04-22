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

    public async Task UpdateAsync(Product update)
    {
        var existing = await _db.Products.FindAsync(update.OctopusId);
        if (existing == null) return;

        existing.WebId        = update.WebId;
        existing.WebTitle     = update.WebTitle;
        existing.PdfTitle     = update.PdfTitle;
        existing.OctopusTitle = update.OctopusTitle;
        existing.Available    = update.Available;
        existing.KegCollar    = update.KegCollar;
        existing.Str          = update.Str;
        existing.Alcohol      = update.Alcohol;
        existing.PricePrUnit  = update.PricePrUnit;
        existing.Category     = update.Category;
        existing.VariantId1   = update.VariantId1;
        existing.VariantId2   = update.VariantId2;

        await _db.SaveChangesAsync();
    }
}