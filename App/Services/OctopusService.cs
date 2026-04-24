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

    public async Task<(List<OctopusCsvRow> Existing, List<OctopusCsvRow> New)>
        PartitionByExistence(IReadOnlyList<OctopusCsvRow> rows)
    {
        await using var db = _contextFactory.CreateDbContext();
        var ids = rows.Select(r => r.OctopusId).ToList();
        var existingIds = (await db.Products
            .Where(p => ids.Contains(p.OctopusId))
            .Select(p => p.OctopusId)
            .ToListAsync()).ToHashSet();

        var existing = rows.Where(r => existingIds.Contains(r.OctopusId)).ToList();
        var fresh = rows.Where(r => !existingIds.Contains(r.OctopusId)).ToList();
        return (existing, fresh);
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
            });
        }

        await db.SaveChangesAsync();

        foreach (var (product, previous, next) in statusFlips)
        {
            await _logger.LogProductChange(product, previous, next);
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
        var (existing, _) = await PartitionByExistence(rows);
        await ApplyUpdatesToExisting(existing);
    }
}
