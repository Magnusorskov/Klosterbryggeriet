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
    // Treats every fresh row as a regular Product — draft beers come in via
    // the interactive preview flow only.
    public async Task<CsvImportResult> UploadCsvAsync(Stream stream, string fileName)
    {
        var (rows, skipped, warnings) = await _octopus.ParseCsv(stream);
        var partitioned = await _octopus.PartitionByExistence(rows);

        var updatedProducts = await _octopus.ApplyUpdatesToExisting(partitioned.ExistingProducts);
        var updatedDraftBeers = await _octopus.ApplyUpdatesToExistingDraftBeers(partitioned.ExistingDraftBeers);
        var created = await _octopus.CreateMissingProducts(partitioned.Fresh);

        var updated = updatedProducts.Concat(updatedDraftBeers).ToList();
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
        var partitioned = await _octopus.PartitionByExistence(rows);

        var pendingProductUpdates = await _octopus.BuildPendingUpdatesAsync(partitioned.ExistingProducts);
        var pendingDraftBeerUpdates = await _octopus.BuildPendingDraftBeerUpdatesAsync(partitioned.ExistingDraftBeers);
        var pendingUpdates = pendingProductUpdates.Concat(pendingDraftBeerUpdates).ToList();

        var pendingNew = partitioned.Fresh.Select(r => new PendingNewProduct
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
        var productUpdateRows = preview.PendingUpdates
            .Where(u => u.Kind == PendingProductKind.RegularProduct)
            .Select(u => new OctopusService.OctopusCsvRow(u.OctopusId, u.OctopusTitle, u.NewAvailable))
            .ToList();
        var draftBeerUpdateRows = preview.PendingUpdates
            .Where(u => u.Kind == PendingProductKind.DraftBeer)
            .Select(u => new OctopusService.OctopusCsvRow(u.OctopusId, u.OctopusTitle, u.NewAvailable))
            .ToList();

        var updatedProducts = await _octopus.ApplyUpdatesToExisting(productUpdateRows);
        var updatedDraftBeers = await _octopus.ApplyUpdatesToExistingDraftBeers(draftBeerUpdateRows);

        var newProducts = preview.PendingNewProducts
            .Where(p => p.Kind == PendingProductKind.RegularProduct)
            .Select(p => p.Product)
            .ToList();
        var newDraftBeers = preview.PendingNewProducts
            .Where(p => p.Kind == PendingProductKind.DraftBeer)
            .Select(p => p.ToDraftBeer())
            .ToList();

        var createdProducts = await _octopus.CommitNewProductsAsync(newProducts);
        var createdDraftBeers = await _octopus.CommitNewDraftBeersAsync(newDraftBeers);

        var updated = updatedProducts.Concat(updatedDraftBeers).ToList();
        var created = createdProducts.Concat(createdDraftBeers).ToList();

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

    // After DB writes succeed, push the new stock for every affected row
    // (updated or newly created) that is InUse and has a webshop variant id.
    // Covers both Products and DraftBeers — fadøl with variants are still
    // part of the webshop catalog.
    private async Task PushAffectedToShopAsync(
        IReadOnlyList<ProductChange> updated,
        IReadOnlyList<ProductCreated> created,
        CsvImportResult result)
    {
        if (!_pushToHostedShopEnabled) return;

        var productIds = updated.Where(u => u.Kind == PendingProductKind.RegularProduct).Select(u => u.OctopusId)
            .Concat(created.Where(c => c.Kind == PendingProductKind.RegularProduct).Select(c => c.OctopusId))
            .ToList();
        var draftBeerIds = updated.Where(u => u.Kind == PendingProductKind.DraftBeer).Select(u => u.OctopusId)
            .Concat(created.Where(c => c.Kind == PendingProductKind.DraftBeer).Select(c => c.OctopusId))
            .ToList();

        if (productIds.Count == 0 && draftBeerIds.Count == 0) return;

        await using var db = _contextFactory.CreateDbContext();

        if (productIds.Count > 0)
        {
            var products = await db.Products
                .Where(p => productIds.Contains(p.OctopusId) && p.InUse && p.VariantId1 > 0)
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

        if (draftBeerIds.Count > 0)
        {
            var beers = await db.DraftBeers
                .Where(b => draftBeerIds.Contains(b.OctopusId) && b.InUse && b.VariantId1 > 0)
                .ToListAsync();

            foreach (var beer in beers)
            {
                await PushVariantAsync(beer.VariantId1, beer.Available, result);
                if (beer.VariantId2 > 0)
                {
                    await PushVariantAsync(beer.VariantId2, beer.Available, result);
                }
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
