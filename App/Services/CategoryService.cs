using BlazorApp.Data;
using BlazorApp.Models;
using Microsoft.EntityFrameworkCore;

namespace BlazorApp.Services;

public class CategoryService(AppDbContext db)
{
    public async Task<List<Category>> GetAllAsync() =>
        await db.Categories
            .Include(c => c.Columns.OrderBy(col => col.SortOrder))
            .OrderBy(c => c.SortOrder)
            .ToListAsync();

    public async Task<Category?> GetByIdAsync(int id) =>
        await db.Categories
            .Include(c => c.Columns.OrderBy(col => col.SortOrder))
            .FirstOrDefaultAsync(c => c.Id == id);

    public async Task<Category> CreateAsync(Category category)
    {
        // New categories go to the bottom
        var maxOrder = await db.Categories.AnyAsync()
            ? await db.Categories.MaxAsync(c => c.SortOrder)
            : 0;
        category.SortOrder = maxOrder + 1;

        db.Categories.Add(category);
        await db.SaveChangesAsync();
        return category;
    }

    public async Task UpdateAsync(Category category)
    {
        var existing = await db.Categories
            .Include(c => c.Columns)
            .FirstOrDefaultAsync(c => c.Id == category.Id)
            ?? throw new InvalidOperationException($"Category {category.Id} not found");

        existing.Name       = category.Name;
        existing.PriceLabel = category.PriceLabel;

        // Replace columns
        db.CategoryColumns.RemoveRange(existing.Columns);
        existing.Columns = category.Columns;

        await db.SaveChangesAsync();
    }

    // Persists a new order given an ordered list of category ids
    public async Task SaveOrderAsync(List<int> orderedIds)
    {
        var categories = await db.Categories.ToListAsync();
        for (int i = 0; i < orderedIds.Count; i++)
        {
            var cat = categories.FirstOrDefault(c => c.Id == orderedIds[i]);
            if (cat != null) cat.SortOrder = i + 1;
        }
        await db.SaveChangesAsync();
    }

    public async Task DeleteAsync(int id)
    {
        var category = await db.Categories.FindAsync(id)
            ?? throw new InvalidOperationException($"Category {id} not found");

        db.Categories.Remove(category);
        await db.SaveChangesAsync();
    }

    public static List<(string FieldName, string DisplayLabel)> AvailableFields =>
    [
        ("KegCollar", "Kasse/Kolli"),
        ("Str",       "Str."),
        ("Alcohol",   "Alkohol%"),
    ];
}