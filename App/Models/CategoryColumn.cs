using System.ComponentModel.DataAnnotations;

namespace BlazorApp.Models;

public class CategoryColumn
{
    [Key]
    public int Id { get; set; }

    public int CategoryId { get; set; }
    public Category Category { get; set; } = null!;

    // Maps to a Product field name (e.g. "Alcohol", "Str", "KegCollar") or a free-text custom label
    public required string FieldName { get; set; }

    // Column header shown in the table, e.g. "Alkohol%", "Str.", "Kasse/Kolli"
    public required string DisplayLabel { get; set; }

    // Position between the fixed Name column and the fixed Price/Status columns
    public int SortOrder { get; set; }
}