using BlazorApp.Models;
using BlazorApp.Models.Dtos;

namespace BlazorApp.Services;

public class CsvUploadService : ICsvUploadService
{
    private readonly OctopusService _octopus;

    public CsvUploadService(OctopusService octopus)
    {
        _octopus = octopus;
    }

    // Legacy one-shot path: parses, applies updates, and creates products in a
    // single call. Still used by direct/non-interactive callers and tests.
    public async Task<CsvImportResult> UploadCsvAsync(Stream stream, string fileName)
    {
        var (rows, skipped, warnings) = await _octopus.ParseCsv(stream);
        var (existing, fresh) = await _octopus.PartitionByExistence(rows);
        var updated = await _octopus.ApplyUpdatesToExisting(existing);
        var created = await _octopus.CreateMissingProducts(fresh);

        return new CsvImportResult
        {
            FileName = fileName,
            RowsParsed = rows.Count,
            RowsSkipped = skipped,
            Updated = updated,
            Created = created,
            Warnings = warnings,
        };
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

        return new CsvImportResult
        {
            FileName = preview.FileName,
            RowsParsed = preview.RowsParsed,
            RowsSkipped = preview.RowsSkipped,
            Updated = updated,
            Created = created,
            Warnings = preview.Warnings,
        };
    }
}
