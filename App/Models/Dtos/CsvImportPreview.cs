namespace BlazorApp.Models.Dtos;

public class CsvImportPreview
{
    public DateTime UploadedAt { get; set; } = DateTime.Now;
    public string FileName { get; set; } = "";
    public int RowsParsed { get; set; }
    public int RowsSkipped { get; set; }
    public List<string> Warnings { get; set; } = [];
    public List<PendingUpdate> PendingUpdates { get; set; } = [];
    public List<PendingNewProduct> PendingNewProducts { get; set; } = [];

    public int StatusFlipCount => PendingUpdates.Count(u => u.StatusFlipped);
}
