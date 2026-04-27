using BlazorApp.Data;
using BlazorApp.Models;
using Microsoft.EntityFrameworkCore;

namespace BlazorApp.Services;

public class DraftBeerService(IDbContextFactory<AppDbContext> factory)
{
    public async Task<List<DraftBeer>> GetAllAsync()
    {
        await using var db = await factory.CreateDbContextAsync();
        return await db.DraftBeers
            .OrderBy(b => b.PdfTitle)
            .ToListAsync();
    }

    public async Task SaveAsync(DraftBeer beer)
    {
        await using var db = await factory.CreateDbContextAsync();
        var existing = await db.DraftBeers.FindAsync(beer.OctopusId)
            ?? throw new InvalidOperationException($"DraftBeer {beer.OctopusId} not found");

        existing.WebId        = beer.WebId;
        existing.WebTitle     = beer.WebTitle;
        existing.PdfTitle     = beer.PdfTitle;
        existing.OctopusTitle = beer.OctopusTitle;
        existing.Available    = beer.Available;
        existing.Str          = beer.Str;
        existing.Alcohol      = beer.Alcohol;
        existing.PricePrUnit  = beer.PricePrUnit;
        existing.Category     = beer.Category;
        existing.VariantId1   = beer.VariantId1;
        existing.VariantId2   = beer.VariantId2;
        existing.InUse        = beer.InUse;
        existing.Kobling      = beer.Kobling;
        existing.Land         = beer.Land;

        await db.SaveChangesAsync();
    }

    public async Task DeleteAsync(int octopusId)
    {
        await using var db = await factory.CreateDbContextAsync();
        var beer = await db.DraftBeers.FindAsync(octopusId);
        if (beer is null) return;
        db.DraftBeers.Remove(beer);
        await db.SaveChangesAsync();
    }

    public async Task AddAsync(DraftBeer beer)
    {
        await using var db = await factory.CreateDbContextAsync();
        db.DraftBeers.Add(beer);
        await db.SaveChangesAsync();
    }
}