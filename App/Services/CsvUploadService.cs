using BlazorApp.Models.Dtos;

namespace BlazorApp.Services;

public class CsvUploadService : ICsvUploadService
{
    private readonly OctopusService _octopus;

    public CsvUploadService(OctopusService octopus)
    {
        _octopus = octopus;
    }

    public async Task<CsvImportResult> UploadCsvAsync(Stream stream, string fileName)
    {
        var (rows, skipped, warnings) = _octopus.ParseCsv(stream);
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
}
