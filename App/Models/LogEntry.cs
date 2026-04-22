using System.ComponentModel.DataAnnotations;

namespace BlazorApp.Models;

public class LogEntry
{
    [Key]
    public int Id { get; set; }
    public DateTime DateChanged { get; set; } = DateTime.Now;
    public required int OctopusId { get; set; }
    public required string ProductName { get; set; }
    public required ProductStatus PreviousStatus { get; set; }
    public required ProductStatus NewStatus { get; set; }
}
