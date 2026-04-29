using BlazorApp.Data;
using BlazorApp.Models;
using BlazorApp.Models.Dtos;
using Microsoft.EntityFrameworkCore;

namespace BlazorApp.Services;

public class CsvUploadService : ICsvUploadService
{
    private readonly OctopusService _octopus;
    private readonly IHostedShopService _shop;
    private readonly IDbContextFactory<AppDbContext> _contextFactory;
    private readonly bool _pushToHostedShopEnabled;

    public CsvUploadService(
        OctopusService octopus,
        IHostedShopService shop,
        IDbContextFactory<AppDbContext> contextFactory,
        bool pushToHostedShopEnabled = true)
    {
        _octopus = octopus;
        _shop = shop;
        _contextFactory = contextFactory;
        _pushToHostedShopEnabled = pushToHostedShopEnabled;
    }

    // Legacy one-shot path: parses, applies updates, and creates products in a
    // single call. Still used by direct/non-interactive callers and tests.
    public async Task<CsvImportResult> UploadCsvAsync(Stream stream, string fileName)
    {
        var (rows, skipped, warnings) = await _octopus.ParseCsv(stream);
        var (existing, fresh) = await _octopus.PartitionByExistence(rows);
        var updated = await _octopus.ApplyUpdatesToExisting(existing);
        var created = await _octopus.CreateMissingProducts(fresh);

        var result = new CsvImportResult
        {
            FileName = fileName,
            RowsParsed = rows.Count,
            RowsSkipped = skipped,
            Updated = updated,
            Created = created,
            Warnings = warnings,
        };

        await PushAffectedToShopAsync(updated, created, result);
        return result;
    }

    // Step 1 of the interactive flow: parse the CSV, diff against the DB,
    // and produce an editable preview without writing to the database.
    public async Task<CsvImportPreview> BuildPreviewAsync(Stream stream, string fileName)
    {
        var (rows, skipped, warnings) = await _octopus.ParseCsv(stream);
        var (existing, fresh) = await _octopus.PartitionByExistence(rows);
        var pendingUpdates = await _octopus.BuildPendingUpdatesAsync(existing);

        var pendingNew = fresh.Select(r => new PendingNewProduct
        {
            Product = new Product
            {
                OctopusId = r.OctopusId,
                OctopusTitle = r.OctopusTitle,
                Available = r.Available,
                InUse = false,
                WebId = 0,
                WebTitle = "",
                PdfTitle = "",
                Category = "",
            },
        }).ToList();

        return new CsvImportPreview
        {
            FileName = fileName,
            RowsParsed = rows.Count,
            RowsSkipped = skipped,
            Warnings = warnings,
            PendingUpdates = pendingUpdates,
            PendingNewProducts = pendingNew,
        };
    }

    // Step 2 of the interactive flow: commit the previewed updates and
    // user-edited new products. Caller is expected to have validated rows.
    public async Task<CsvImportResult> CommitPreviewAsync(CsvImportPreview preview)
    {
        var existingRows = preview.PendingUpdates
            .Select(u => new OctopusService.OctopusCsvRow(u.OctopusId, u.OctopusTitle, u.NewAvailable))
            .ToList();
        var updated = await _octopus.ApplyUpdatesToExisting(existingRows);
        var created = await _octopus.CommitNewProductsAsync(
            preview.PendingNewProducts.Select(p => p.Product).ToList());

        var result = new CsvImportResult
        {
            FileName = preview.FileName,
            RowsParsed = preview.RowsParsed,
            RowsSkipped = preview.RowsSkipped,
            Updated = updated,
            Created = created,
            Warnings = preview.Warnings,
        };

        await PushAffectedToShopAsync(updated, created, result);
        return result;
    }

    // After DB writes succeed, push the new stock for every affected product
    // (updated or newly created) that is InUse and has a webshop variant id.
    private async Task PushAffectedToShopAsync(
        IReadOnlyList<ProductChange> updated,
        IReadOnlyList<ProductCreated> created,
        CsvImportResult result)
    {
        if (!_pushToHostedShopEnabled) return;

        var ids = updated.Select(u => u.OctopusId)
            .Concat(created.Select(c => c.OctopusId))
            .ToList();
        if (ids.Count == 0) return;

        await using var db = _contextFactory.CreateDbContext();
        var products = await db.Products
            .Where(p => ids.Contains(p.OctopusId) && p.InUse && p.VariantId1 > 0)
            .ToListAsync();

        foreach (var product in products)
        {
            await PushVariantAsync(product.VariantId1, product.Available, result);
            if (product.VariantId2 > 0)
            {
                await PushVariantAsync(product.VariantId2, product.Available, result);
            }
        }
    }

    private async Task PushVariantAsync(int variantId, int antal, CsvImportResult result)
    {
        result.PushAttempted++;
        var ok = await _shop.OpdaterLager(variantId, antal);
        if (ok)
        {
            result.PushSucceeded++;
        }
        else
        {
            result.PushFailed++;
            result.FailedVariantIds.Add(variantId);
        }
    }
}
