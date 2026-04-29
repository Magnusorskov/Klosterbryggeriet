namespace BlazorApp.Models.Dtos;

public class PendingUpdate
{
    public required int OctopusId { get; set; }
    public required string OctopusTitle { get; set; }
    public required int PreviousAvailable { get; set; }
    public required int NewAvailable { get; set; }
    public required ProductStatus PreviousStatus { get; set; }
    public required ProductStatus NewStatus { get; set; }
    public bool StatusFlipped => PreviousStatus != NewStatus;
}
