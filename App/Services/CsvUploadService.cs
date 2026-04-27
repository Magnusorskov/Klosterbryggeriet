using BlazorApp.Data;
using Microsoft.EntityFrameworkCore;

namespace BlazorApp.Services;

public record CsvUploadResult(
    int DbProductsUpdated,
    int PushAttempted,
    int PushSucceeded,
    int PushFailed,
    IReadOnlyList<int> FailedVariantIds);

public class CsvUploadService : ICsvUploadService
{
    private readonly OctopusService _octopus;
    private readonly IHostedShopService _shop;
    private readonly AppDbContext _db;

    public CsvUploadService(OctopusService octopus, IHostedShopService shop, AppDbContext db)
    {
        _octopus = octopus;
        _shop = shop;
        _db = db;
    }

    public async Task<CsvUploadResult> UploadCsvAsync(Stream stream, string fileName)
    {
        var affectedIds = await _octopus.UpdateAvailableFromOctopusCsvAsync(stream);

        var products = await _db.Products
            .Where(p => affectedIds.Contains(p.OctopusId) && p.InUse && p.VariantId1 > 0)
            .ToListAsync();

        var pushAttempted = 0;
        var pushSucceeded = 0;
        var pushFailed = 0;
        var failedVariantIds = new List<int>();

        foreach (var product in products)
        {
            await PushVariant(product.VariantId1, product.Available);

            if (product.VariantId2 > 0)
            {
                await PushVariant(product.VariantId2, product.Available);
            }
        }

        return new CsvUploadResult(
            DbProductsUpdated: affectedIds.Count,
            PushAttempted: pushAttempted,
            PushSucceeded: pushSucceeded,
            PushFailed: pushFailed,
            FailedVariantIds: failedVariantIds);

        async Task PushVariant(int variantId, int antal)
        {
            pushAttempted++;
            var ok = await _shop.OpdaterLager(variantId, antal);
            if (ok) pushSucceeded++;
            else
            {
                pushFailed++;
                failedVariantIds.Add(variantId);
            }
        }
    }
}
