namespace BlazorApp.Models;

/// <summary>
/// Wraps a DraftBeer with UI-only display state (not persisted to the database).
/// </summary>
public class DraftBeerRow
{
    public DraftBeer Beer { get; set; } = null!;
    public string? Color { get; set; }
}