using System.ComponentModel.DataAnnotations;

namespace BlazorApp.Models;

public class Category
{
    [Key]
    public int Id { get; set; }

    public required string Name { get; set; }

    // Label shown in the Price column header, e.g. "pr. flaske", "pr. enhed"
    public required string PriceLabel { get; set; }

    public int SortOrder { get; set; }

    public ICollection<CategoryColumn> Columns { get; set; } = [];
}