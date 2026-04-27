using BlazorApp.Models.Dtos;

namespace BlazorApp.Services;

public interface ICsvUploadService
{
    Task<CsvImportResult> UploadCsvAsync(Stream stream, string fileName);
}
