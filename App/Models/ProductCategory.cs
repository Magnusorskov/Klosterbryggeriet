namespace BlazorApp.Models;

public class ProductCategory
{
    public string Name { get; set; } = "";
    public string PriceLabel { get; set; } = "pr. enhed";
    public List<CategoryColumn> Columns { get; set; } = [];
    public List<ProductRow> Products { get; set; } = [];
}