using BlazorApp.Data;
using BlazorApp.Models;
using Microsoft.EntityFrameworkCore;

namespace BlazorApp.Services;

public class ProductService
{
    private readonly IDbContextFactory<AppDbContext> _contextFactory;

    public ProductService(IDbContextFactory<AppDbContext> contextFactory)
    {
        _contextFactory = contextFactory;
    }

    public async Task<List<Product>> ListProducts()
    {
        await using var db = _contextFactory.CreateDbContext();
        return await db.Products.ToListAsync();
    }

    public async Task<List<Product>> ListInUseProducts()
    {
        await using var db = _contextFactory.CreateDbContext();
        return await db.Products.Where(p => p.InUse).ToListAsync();
    }

    public async Task<Dictionary<string, List<Product>>> MapProductsByCategory() {
        var products = await ListInUseProducts();
        return products.GroupBy(p => p.Category)
            .ToDictionary(g => g.Key, g => g.ToList());
    }

    public async Task DeleteAsync(int octopusId)
    {
        await using var db = _contextFactory.CreateDbContext();
        var existing = await db.Products.FindAsync(octopusId);
        if (existing == null) return;
        db.Products.Remove(existing);
        await db.SaveChangesAsync();
    }

    public async Task UpdateAsync(Product update)
    {
        await using var db = _contextFactory.CreateDbContext();
        var existing = await db.Products.FindAsync(update.OctopusId);
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
        existing.InUse        = update.InUse;
        existing.HalfKolli    = update.HalfKolli;

        await db.SaveChangesAsync();
    }
}
