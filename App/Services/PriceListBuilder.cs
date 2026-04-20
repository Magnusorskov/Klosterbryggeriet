using BlazorApp.Models;

namespace BlazorApp.Services;

public class PriceListBuilder(ProductService productService) : IPriceListBuilder
{
    public async Task<List<ProductCategory>> GetCategoriesAsync()
    {
        var map = await GetProductsByCategoryAsync();

        return
        [
            new() { Name = "GAVEKASSER",                           Layout = TableLayout.Gavekasser, Products = Rows(map, "GAVEKASSER",                           TableLayout.Gavekasser) },
            new() { Name = "KLOSTERBRYGGERIET - DANMARK",          Layout = TableLayout.Standard,   Products = Rows(map, "KLOSTERBRYGGERIET - DANMARK",          TableLayout.Standard) },
            new() { Name = "MOSTERS økologiske saft - DANMARK",    Layout = TableLayout.Saft,       Products = Rows(map, "MOSTERS økologiske saft - DANMARK",    TableLayout.Saft) },
            new() { Name = "BOLSKOV CIDER & MOST",                 Layout = TableLayout.Standard,   Products = Rows(map, "BOLSKOV CIDER & MOST",                 TableLayout.Standard) },
            new() { Name = "AMARCORD - ITALIEN",                   Layout = TableLayout.Standard,   Products = Rows(map, "AMARCORD - ITALIEN",                   TableLayout.Standard) },
            new() { Name = "GALVANINA CENTURY LINE - ITALIEN",     Layout = TableLayout.Standard,   Products = Rows(map, "GALVANINA CENTURY LINE - ITALIEN",     TableLayout.Standard) },
            new() { Name = "3 Monts Biere de Flandern - FRANKRIG", Layout = TableLayout.Standard,   Products = Rows(map, "3 Monts Biere de Flandern - FRANKRIG", TableLayout.Standard) },
            new() { Name = "HET ANKER BROUWERIJ - BELGIEN",        Layout = TableLayout.Standard,   Products = Rows(map, "HET ANKER BROUWERIJ - BELGIEN",        TableLayout.Standard) },
        ];
    }

    private Task<Dictionary<string, List<Product>>> GetProductsByCategoryAsync()
    {
        return productService.MapProductsByCategory();
    }

    private static List<ProductRow> Rows(Dictionary<string, List<Product>> map, string category, TableLayout layout) =>
        map.GetValueOrDefault(category, []).Select(p => new ProductRow
        {
            Product = p,
            Status = layout != TableLayout.Fadøl && p.Available < 30 ? "Udsolgt" : ""
        }).ToList();
}
