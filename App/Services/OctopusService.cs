using System.Globalization;
using BlazorApp.Data;
using BlazorApp.Models;
using BlazorApp.Models.Dtos;
using Microsoft.EntityFrameworkCore;

namespace BlazorApp.Services;

public class OctopusService
{
    private readonly IDbContextFactory<AppDbContext> _contextFactory;
    private readonly LoggerService _logger;
    private static readonly CultureInfo DaCulture = new("da-DK");

    public OctopusService(IDbContextFactory<AppDbContext> contextFactory, LoggerService logger)
    {
        _contextFactory = contextFactory;
        _logger = logger;
    }

    public record OctopusCsvRow(int OctopusId, string OctopusTitle, int Available);

    public record PartitionedRows(
        List<OctopusCsvRow> ExistingProducts,
        List<OctopusCsvRow> ExistingDraftBeers,
        List<OctopusCsvRow> Fresh);

    public async Task<(List<OctopusCsvRow> Rows, int Skipped, List<string> Warnings)> ParseCsv(Stream fileStream)
    {
        using var reader = new StreamReader(fileStream);
        var content = await reader.ReadToEndAsync();
        var lines = content.Split('\n', StringSplitOptions.RemoveEmptyEntries);

        var byId = new Dictionary<int, OctopusCsvRow>();
        var warnings = new List<string>();
        var skipped = 0;

        foreach (var line in lines.Skip(1))
        {
            var columns = line.Split(';');
            if (columns.Length < 20)
            {
                skipped++;
                continue;
            }

            if (!int.TryParse(columns[0], NumberStyles.Integer, DaCulture, out var octopusId) ||
                !int.TryParse(columns[19], NumberStyles.Integer, DaCulture, out var available))
            {
                skipped++;
                continue;
            }

            var title = columns[1].Trim();
            var row = new OctopusCsvRow(octopusId, title, available);

            if (byId.ContainsKey(octopusId))
            {
                warnings.Add($"Duplicate OctopusId {octopusId}: using last row (Available={available})");
            }
            byId[octopusId] = row;
        }

        return (byId.Values.ToList(), skipped, warnings);
    }

    // Splits CSV rows into three buckets: existing products, existing draft
    // beers, and fresh (unknown to either table). OctopusId is unique across
    // the catalog so a row never lands in more than one bucket.
    public async Task<PartitionedRows> PartitionByExistence(IReadOnlyList<OctopusCsvRow> rows)
    {
        await using var db = _contextFactory.CreateDbContext();
        var ids = rows.Select(r => r.OctopusId).ToList();

        var productIds = (await db.Products
            .Where(p => ids.Contains(p.OctopusId))
            .Select(p => p.OctopusId)
            .ToListAsync()).ToHashSet();

        var draftBeerIds = (await db.DraftBeers
            .Where(b => ids.Contains(b.OctopusId))
            .Select(b => b.OctopusId)
            .ToListAsync()).ToHashSet();

        var existingProducts = rows.Where(r => productIds.Contains(r.OctopusId)).ToList();
        var existingDraftBeers = rows.Where(r => draftBeerIds.Contains(r.OctopusId)).ToList();
        var fresh = rows.Where(r =>
            !productIds.Contains(r.OctopusId) && !draftBeerIds.Contains(r.OctopusId)).ToList();

        return new PartitionedRows(existingProducts, existingDraftBeers, fresh);
    }

    public async Task<List<ProductChange>> ApplyUpdatesToExisting(IReadOnlyList<OctopusCsvRow> rows)
    {
        if (rows.Count == 0) return [];

        await using var db = _contextFactory.CreateDbContext();
        var ids = rows.Select(r => r.OctopusId).ToList();
        var dbProducts = await db.Products
            .Where(p => ids.Contains(p.OctopusId))
            .ToListAsync();

        var changes = new List<ProductChange>();
        var statusFlips = new List<(Product Product, ProductStatus Previous, ProductStatus New)>();

        foreach (var dbProduct in dbProducts)
        {
            var row = rows.First(r => r.OctopusId == dbProduct.OctopusId);
            var previousAvailable = dbProduct.Available;
            var previousStatus = dbProduct.Status;
            var newStatus = Product.StatusFor(row.Available);

            if (previousStatus != newStatus)
            {
                statusFlips.Add((dbProduct, previousStatus, newStatus));
            }

            dbProduct.Available = row.Available;

            changes.Add(new ProductChange
            {
                OctopusId = dbProduct.OctopusId,
                OctopusTitle = dbProduct.OctopusTitle,
                PreviousAvailable = previousAvailable,
                NewAvailable = row.Available,
                PreviousStatus = previousStatus,
                NewStatus = newStatus,
                Kind = PendingProductKind.RegularProduct,
            });
        }

        await db.SaveChangesAsync();

        foreach (var (product, previous, next) in statusFlips)
        {
            await _logger.LogProductChange(product, previous, next);
        }

        return changes;
    }

    public async Task<List<ProductChange>> ApplyUpdatesToExistingDraftBeers(IReadOnlyList<OctopusCsvRow> rows)
    {
        if (rows.Count == 0) return [];

        await using var db = _contextFactory.CreateDbContext();
        var ids = rows.Select(r => r.OctopusId).ToList();
        var dbBeers = await db.DraftBeers
            .Where(b => ids.Contains(b.OctopusId))
            .ToListAsync();

        var changes = new List<ProductChange>();
        var statusFlips = new List<(DraftBeer Beer, ProductStatus Previous, ProductStatus New)>();

        foreach (var dbBeer in dbBeers)
        {
            var row = rows.First(r => r.OctopusId == dbBeer.OctopusId);
            var previousAvailable = dbBeer.Available;
            var previousStatus = DraftBeer.StatusFor(previousAvailable);
            var newStatus = DraftBeer.StatusFor(row.Available);

            if (previousStatus != newStatus)
            {
                statusFlips.Add((dbBeer, previousStatus, newStatus));
            }

            dbBeer.Available = row.Available;

            changes.Add(new ProductChange
            {
                OctopusId = dbBeer.OctopusId,
                OctopusTitle = dbBeer.OctopusTitle,
                PreviousAvailable = previousAvailable,
                NewAvailable = row.Available,
                PreviousStatus = previousStatus,
                NewStatus = newStatus,
                Kind = PendingProductKind.DraftBeer,
            });
        }

        await db.SaveChangesAsync();

        foreach (var (beer, previous, next) in statusFlips)
        {
            await _logger.LogDraftBeerChange(beer, previous, next);
        }

        return changes;
    }

    public async Task<List<ProductCreated>> CreateMissingProducts(IReadOnlyList<OctopusCsvRow> rows)
    {
        if (rows.Count == 0) return [];

        await using var db = _contextFactory.CreateDbContext();
        var created = new List<ProductCreated>();
        var newProducts = new List<Product>();

        foreach (var row in rows)
        {
            var product = new Product
            {
                OctopusId = row.OctopusId,
                OctopusTitle = row.OctopusTitle,
                Available = row.Available,
                InUse = false,
                WebId = 0,
                WebTitle = "",
                PdfTitle = "",
                Category = "",
            };
            newProducts.Add(product);

            created.Add(new ProductCreated
            {
                OctopusId = row.OctopusId,
                OctopusTitle = row.OctopusTitle,
                Available = row.Available,
                Kind = PendingProductKind.RegularProduct,
            });
        }

        await db.Products.AddRangeAsync(newProducts);
        await db.SaveChangesAsync();

        foreach (var product in newProducts)
        {
            await _logger.LogProductCreated(product);
        }

        return created;
    }

    public async Task UpdateAvailableFromOctopusCsv(Stream fileStream)
    {
        var (rows, _, _) = await ParseCsv(fileStream);
        var partitioned = await PartitionByExistence(rows);
        await ApplyUpdatesToExisting(partitioned.ExistingProducts);
        await ApplyUpdatesToExistingDraftBeers(partitioned.ExistingDraftBeers);
    }

    // Build the diff vs. current DB state for a CSV's existing rows without
    // committing anything. Used by the preview-then-commit upload flow.
    public async Task<List<PendingUpdate>> BuildPendingUpdatesAsync(IReadOnlyList<OctopusCsvRow> existingRows)
    {
        if (existingRows.Count == 0) return [];

        await using var db = _contextFactory.CreateDbContext();
        var ids = existingRows.Select(r => r.OctopusId).ToList();
        var dbProducts = await db.Products
            .Where(p => ids.Contains(p.OctopusId))
            .ToListAsync();

        var updates = new List<PendingUpdate>();
        foreach (var dbProduct in dbProducts)
        {
            var row = existingRows.First(r => r.OctopusId == dbProduct.OctopusId);
            if (row.Available == dbProduct.Available) continue;
            var newStatus = Product.StatusFor(row.Available);
            updates.Add(new PendingUpdate
            {
                OctopusId = dbProduct.OctopusId,
                OctopusTitle = dbProduct.OctopusTitle,
                PreviousAvailable = dbProduct.Available,
                NewAvailable = row.Available,
                PreviousStatus = dbProduct.Status,
                NewStatus = newStatus,
                Kind = PendingProductKind.RegularProduct,
            });
        }
        return updates;
    }

    public async Task<List<PendingUpdate>> BuildPendingDraftBeerUpdatesAsync(IReadOnlyList<OctopusCsvRow> existingRows)
    {
        if (existingRows.Count == 0) return [];

        await using var db = _contextFactory.CreateDbContext();
        var ids = existingRows.Select(r => r.OctopusId).ToList();
        var dbBeers = await db.DraftBeers
            .Where(b => ids.Contains(b.OctopusId))
            .ToListAsync();

        var updates = new List<PendingUpdate>();
        foreach (var dbBeer in dbBeers)
        {
            var row = existingRows.First(r => r.OctopusId == dbBeer.OctopusId);
            if (row.Available == dbBeer.Available) continue;
            var previousStatus = DraftBeer.StatusFor(dbBeer.Available);
            var newStatus = DraftBeer.StatusFor(row.Available);
            updates.Add(new PendingUpdate
            {
                OctopusId = dbBeer.OctopusId,
                OctopusTitle = dbBeer.OctopusTitle,
                PreviousAvailable = dbBeer.Available,
                NewAvailable = row.Available,
                PreviousStatus = previousStatus,
                NewStatus = newStatus,
                Kind = PendingProductKind.DraftBeer,
            });
        }
        return updates;
    }

    // Persists user-edited new products and logs each creation.
    public async Task<List<ProductCreated>> CommitNewProductsAsync(IReadOnlyList<Product> products)
    {
        if (products.Count == 0) return [];

        await using var db = _contextFactory.CreateDbContext();
        await db.Products.AddRangeAsync(products);
        await db.SaveChangesAsync();

        foreach (var product in products)
        {
            await _logger.LogProductCreated(product);
        }

        return products.Select(p => new ProductCreated
        {
            OctopusId = p.OctopusId,
            OctopusTitle = p.OctopusTitle,
            Available = p.Available,
            Kind = PendingProductKind.RegularProduct,
        }).ToList();
    }

    public async Task<List<ProductCreated>> CommitNewDraftBeersAsync(IReadOnlyList<DraftBeer> beers)
    {
        if (beers.Count == 0) return [];

        await using var db = _contextFactory.CreateDbContext();
        await db.DraftBeers.AddRangeAsync(beers);
        await db.SaveChangesAsync();

        foreach (var beer in beers)
        {
            await _logger.LogDraftBeerCreated(beer);
        }

        return beers.Select(b => new ProductCreated
        {
            OctopusId = b.OctopusId,
            OctopusTitle = b.OctopusTitle,
            Available = b.Available,
            Kind = PendingProductKind.DraftBeer,
        }).ToList();
    }
}
