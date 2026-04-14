namespace BlazorApp.Models;

/// <summary>
/// Wraps a Product with UI-only display state (not persisted to the database).
/// </summary>
public class ProductRow
{
    public Product Product { get; set; } = null!;
    public string Status { get; set; } = "";
    public string? Color { get; set; }  // null | "green" | "yellow" | "red"
    public string Notes { get; set; } = ""; // gavekasser kolli description
}
