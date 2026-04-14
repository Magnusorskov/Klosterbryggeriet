namespace BlazorApp.Services;

public class CsvUploadService : ICsvUploadService
{
    public async Task UploadCsvAsync(Stream stream, string fileName)
    {
        // TODO: Replace with real HTTP call to backend
        // e.g. POST multipart/form-data to /api/import/csv
        await Task.Delay(500); // simulate network latency
    }
}
