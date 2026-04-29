using BlazorApp.Models;
using BlazorApp.Models.Dtos;

namespace BlazorApp.Services;

public interface ICsvUploadService
{
    Task<CsvImportResult> UploadCsvAsync(Stream stream, string fileName);
    Task<CsvImportPreview> BuildPreviewAsync(Stream stream, string fileName);
    Task<CsvImportResult> CommitPreviewAsync(CsvImportPreview preview);
}
