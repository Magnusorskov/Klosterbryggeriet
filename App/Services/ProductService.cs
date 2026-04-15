using BlazorApp.Models;

namespace BlazorApp.Services;

public class ProductService : IProductService
{
    public async Task<List<ProductCategory>> GetCategoriesAsync()
    {
        var map = await GetProductsByCategoryAsync();

        return
        [
            new()
                { Name = "GAVEKASSER", Layout = TableLayout.Gavekasser, Products = Rows(map, "GAVEKASSER") },
            new()
            {
                Name = "KLOSTERBRYGGERIET - DANMARK", Layout = TableLayout.Standard,
                Products = Rows(map, "KLOSTERBRYGGERIET - DANMARK")
            },
            new()
            {
                Name = "MOSTERS økologiske saft - DANMARK", Layout = TableLayout.Saft,
                Products = Rows(map, "MOSTERS økologiske saft - DANMARK")
            },
            new()
            {
                Name = "BOLSKOV CIDER & MOST", Layout = TableLayout.Standard,
                Products = Rows(map, "BOLSKOV CIDER & MOST")
            },
            new()
            {
                Name = "AMARCORD - ITALIEN", Layout = TableLayout.Standard, Products = Rows(map, "AMARCORD - ITALIEN")
            },
            new()
            {
                Name = "GALVANINA CENTURY LINE - ITALIEN", Layout = TableLayout.Standard,
                Products = Rows(map, "GALVANINA CENTURY LINE - ITALIEN")
            },
            new()
            {
                Name = "3 Monts Biere de Flandern - FRANKRIG", Layout = TableLayout.Standard,
                Products = Rows(map, "3 Monts Biere de Flandern - FRANKRIG")
            },
            new()
            {
                Name = "HET ANKER BROUWERIJ - BELGIEN", Layout = TableLayout.Standard,
                Products = Rows(map, "HET ANKER BROUWERIJ - BELGIEN")
            },
        ];
    }

    // TODO: Replace with real backend API call
    private Task<Dictionary<string, List<Product>>> GetProductsByCategoryAsync()
    {
        return Task.FromResult(new Dictionary<string, List<Product>>());
    }

    private static List<ProductRow> Rows(Dictionary<string, List<Product>> map, string category) =>
        map.GetValueOrDefault(category, []).Select(p => new ProductRow { Product = p }).ToList();
}