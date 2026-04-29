namespace BlazorApp.Models.Dtos;

public class CsvImportResult
{
    public DateTime ImportedAt { get; set; } = DateTime.Now;
    public string FileName { get; set; } = "";
    public int RowsParsed { get; set; }
    public int RowsSkipped { get; set; }
    public List<ProductChange> Updated { get; set; } = [];
    public List<ProductCreated> Created { get; set; } = [];
    public List<string> Warnings { get; set; } = [];

    public int PushAttempted { get; set; }
    public int PushSucceeded { get; set; }
    public int PushFailed { get; set; }
    public List<int> FailedVariantIds { get; set; } = [];

    public int StatusFlipCount => Updated.Count(u => u.StatusFlipped);
}
