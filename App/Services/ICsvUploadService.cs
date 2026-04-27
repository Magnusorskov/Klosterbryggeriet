namespace BlazorApp.Services;

public interface ICsvUploadService
{
    Task<CsvUploadResult> UploadCsvAsync(Stream stream, string fileName);
}
