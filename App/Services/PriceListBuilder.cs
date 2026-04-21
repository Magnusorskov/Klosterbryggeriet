using System.Text.RegularExpressions;
using BlazorApp.Models;

namespace BlazorApp.Services;

public class PriceListBuilder(ProductService productService) : IPriceListBuilder
{
    public async Task<List<ProductCategory>> GetCategoriesAsync()
    {
        var raw = await GetProductsByCategoryAsync();
        var map = raw.ToDictionary(kvp => Normalize(kvp.Key), kvp => kvp.Value);

        return
        [
            new() { Name = "GAVEKASSER",                                Layout = TableLayout.Gavekasser, Products = Rows(map, "GAVEKASSER",                                TableLayout.Gavekasser) },
            new() { Name = "KLOSTERBRYGGERIET - DANMARK",               Layout = TableLayout.Standard,   Products = Rows(map, "KLOSTERBRYGGERIET - DANMARK",               TableLayout.Standard) },
            new() { Name = "MOSTERS økologiske saft - DANMARK",         Layout = TableLayout.Saft,       Products = Rows(map, "MOSTERS økologiske saft - DANMARK",         TableLayout.Saft) },
            new() { Name = "BOLSKOV CIDER & MOST",                      Layout = TableLayout.Standard,   Products = Rows(map, "BOLSKOV CIDER & MOST",                      TableLayout.Standard) },
            new() { Name = "ARDÉCHE likører - FRANKRIG",                Layout = TableLayout.Standard,   Products = Rows(map, "ARDÉCHE likører - FRANKRIG",                TableLayout.Standard) },
            new() { Name = "3 Monts Biere de Flandern - FRANKRIG",      Layout = TableLayout.Standard,   Products = Rows(map, "3 Monts Biere de Flandern - FRANKRIG",      TableLayout.Standard) },
            new() { Name = "AMARCORD - ITALIEN",                        Layout = TableLayout.Standard,   Products = Rows(map, "AMARCORD - ITALIEN",                        TableLayout.Standard) },
            new() { Name = "GALVANINA CENTURY LINE - ITALIEN",          Layout = TableLayout.Standard,   Products = Rows(map, "GALVANINA CENTURY LINE - ITALIEN",          TableLayout.Standard) },
            new() { Name = "RIVIERA GIN - Premium Elitist - Italien",   Layout = TableLayout.Standard,   Products = Rows(map, "RIVIERA GIN - Premium Elitist - Italien",   TableLayout.Standard) },
            new() { Name = "CANEDIGUERRA - Italien",                    Layout = TableLayout.Standard,   Products = Rows(map, "CANEDIGUERRA - Italien",                    TableLayout.Standard) },
            new() { Name = "PRIMATOR - TJEKKIET",                       Layout = TableLayout.Standard,   Products = Rows(map, "PRIMATOR - TJEKKIET",                       TableLayout.Standard) },
            new() { Name = "AYINGER - TYSKLAND",                        Layout = TableLayout.Standard,   Products = Rows(map, "AYINGER - TYSKLAND",                        TableLayout.Standard) },
            new() { Name = "WESTVLETEREN - BELGIEN",                    Layout = TableLayout.Standard,   Products = Rows(map, "WESTVLETEREN - BELGIEN",                    TableLayout.Standard) },
            new() { Name = "HET ANKER BROUWERIJ - BELGIEN",             Layout = TableLayout.Standard,   Products = Rows(map, "HET ANKER BROUWERIJ - BELGIEN",             TableLayout.Standard) },
            new() { Name = "HET ANKER STOKERIJ - BELGIEN",              Layout = TableLayout.Standard,   Products = Rows(map, "HET ANKER STOKERIJ - BELGIEN",              TableLayout.Standard) },
            new() { Name = "BATTELIEK - BELGIEN",                       Layout = TableLayout.Standard,   Products = Rows(map, "BATTELIEK - BELGIEN",                       TableLayout.Standard) },
            new() { Name = "SINT BERNARDUS - BELGIEN",                  Layout = TableLayout.Standard,   Products = Rows(map, "SINT BERNARDUS - BELGIEN",                  TableLayout.Standard) },
            new() { Name = "BOON - BELGIEN",                            Layout = TableLayout.Standard,   Products = Rows(map, "BOON - BELGIEN",                            TableLayout.Standard) },
            new() { Name = "DUBUISSON - BELGIEN",                       Layout = TableLayout.Standard,   Products = Rows(map, "DUBUISSON - BELGIEN",                       TableLayout.Standard) },
            new() { Name = "MARTHA - BELGIEN",                          Layout = TableLayout.Standard,   Products = Rows(map, "MARTHA - BELGIEN",                          TableLayout.Standard) },
            new() { Name = "7-ZONDERN - BELGIEN",                       Layout = TableLayout.Standard,   Products = Rows(map, "7-ZONDERN - BELGIEN",                       TableLayout.Standard) },
            new() { Name = "TER DOLEN - BELGIEN",                       Layout = TableLayout.Standard,   Products = Rows(map, "TER DOLEN - BELGIEN",                       TableLayout.Standard) },
            new() { Name = "STADSHAVEN - HOLLAND",                      Layout = TableLayout.Standard,   Products = Rows(map, "STADSHAVEN - HOLLAND",                      TableLayout.Standard) },
            new() { Name = "VAN STEENBERGE - BELGIEN",                  Layout = TableLayout.Standard,   Products = Rows(map, "VAN STEENBERGE - BELGIEN",                  TableLayout.Standard) },
            new() { Name = "ORVAL TRAPPIST - BELGIEN",                  Layout = TableLayout.Standard,   Products = Rows(map, "ORVAL TRAPPIST - BELGIEN",                  TableLayout.Standard) },
            new() { Name = "ACHEL TRAPPIST - BELGIEN",                  Layout = TableLayout.Standard,   Products = Rows(map, "ACHEL TRAPPIST - BELGIEN",                  TableLayout.Standard) },
            new() { Name = "ROCHEFORT TRAPPIST - BELGIEN",              Layout = TableLayout.Standard,   Products = Rows(map, "ROCHEFORT TRAPPIST - BELGIEN",              TableLayout.Standard) },
            new() { Name = "WESTMALLE TRAPPIST - BELGIEN",              Layout = TableLayout.Standard,   Products = Rows(map, "WESTMALLE TRAPPIST - BELGIEN",              TableLayout.Standard) },
            new() { Name = "CHOUFFE - BELGIEN",                         Layout = TableLayout.Standard,   Products = Rows(map, "CHOUFFE - BELGIEN",                         TableLayout.Standard) },
            new() { Name = "EGGENBERG - ØSTRIG",                        Layout = TableLayout.Standard,   Products = Rows(map, "EGGENBERG - ØSTRIG",                        TableLayout.Standard) },
        ];
    }

    private Task<Dictionary<string, List<Product>>> GetProductsByCategoryAsync()
    {
        return productService.MapProductsByCategory();
    }

    private static List<ProductRow> Rows(Dictionary<string, List<Product>> map, string category, TableLayout layout) =>
        map.GetValueOrDefault(Normalize(category), []).Select(p => new ProductRow
        {
            Product = p,
            Status = layout != TableLayout.Fadøl && p.Available < 30 ? "Udsolgt" : ""
        }).ToList();

    // Trims, collapses internal whitespace (catches "LINE- " vs "LINE - "), and uppercases.
    private static string Normalize(string s) =>
        Regex.Replace(s.Trim(), @"\s*-\s*", " - ").ToUpperInvariant();
}
