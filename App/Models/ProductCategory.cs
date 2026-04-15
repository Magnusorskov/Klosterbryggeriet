namespace BlazorApp.Models;

public enum TableLayout { Standard, Gavekasser, Saft }

public class ProductCategory
{
    public string Name { get; set; } = "";
    public TableLayout Layout { get; set; } = TableLayout.Standard;
    public List<ProductRow> Products { get; set; } = [];
}
