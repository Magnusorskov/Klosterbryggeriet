using BlazorApp.Models.Dtos;

namespace BlazorApp.Services;

// Scoped per-circuit holder so the import preview, status messages, and last
// committed receipt survive when the user navigates between pages.
public class CsvImportSession
{
    public CsvImportPreview? Preview { get; set; }
    public CsvImportResult? LastResult { get; set; }
    public string? ErrorMessage { get; set; }
    public string? SuccessMessage { get; set; }

    public void Reset()
    {
        Preview = null;
        LastResult = null;
        ErrorMessage = null;
        SuccessMessage = null;
    }
}
