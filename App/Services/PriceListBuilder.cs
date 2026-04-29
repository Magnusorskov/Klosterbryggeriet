using System.Text.RegularExpressions;
using BlazorApp.Models;

namespace BlazorApp.Services;

public class PriceListBuilder(ProductService productService, CategoryService categoryService) : IPriceListBuilder
{
    public async Task<List<ProductCategory>> GetCategoriesAsync()
    {
        var dbCategories = await categoryService.GetAllAsync();
        var productsByCategory = await productService.MapProductsByCategory();

        // Normalize all product category keys for lookup
        var normalizedProducts = productsByCategory
            .ToDictionary(kvp => Normalize(kvp.Key), kvp => kvp.Value);

        return dbCategories
            .Select(cat => new ProductCategory
            {
                Name       = cat.Name,
                PriceLabel = cat.PriceLabel,
                Columns    = cat.Columns.OrderBy(c => c.SortOrder).ToList(),
                Products   = normalizedProducts.TryGetValue(Normalize(cat.Name), out var products)
                    ? products.Select(p => new ProductRow
                    {
                        Product = p,
                        Status  = p.Status == ProductStatus.SoldOut ? "Udsolgt" : ""
                    }).ToList()
                    : []
            })
            .Where(cat => cat.Products.Count > 0)
            .ToList();
    }

    // Trims, collapses whitespace around '-', uppercases — matches both DB names and Product.Category strings
    private static string Normalize(string s) =>
        Regex.Replace(s.Trim(), @"\s*-\s*", " - ").ToUpperInvariant();
}