namespace BlazorApp.Services;

public interface ICsvUploadService
{
    Task UploadCsvAsync(Stream stream, string fileName);
}
