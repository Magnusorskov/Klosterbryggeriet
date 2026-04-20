using System.ComponentModel.DataAnnotations;

namespace BlazorApp.Models;

public class LogEntry
{
    [Key]
    public int Id { get; set; }
    public DateTime DateChanged { get; set; } = DateTime.Now;
    public required string ColumnName { get; set; }
    public required string ValueType { get; set; }
    public required int OctopusId { get; set; }
    public required string ProductName { get; set; }
    public required string PreviousValue { get; set; }
    public required string NewValue { get; set; }
    public bool Approved { get; set; }
    
    public LogEntry()
    {
    }
}

