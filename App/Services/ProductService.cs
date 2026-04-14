using BlazorApp.Models;

namespace BlazorApp.Services;

public class ProductService : IProductService
{
    public Task<List<ProductCategory>> GetCategoriesAsync()
    {
        // TODO: Replace with real HTTP call to backend API
        var categories = new List<ProductCategory>
        {
            new()
            {
                Name = "GAVEKASSER",
                Layout = TableLayout.Gavekasser,
                Products =
                [
                    Row(new() { OctopusId = "G1", WebId = "g1", WebTitle = "", OctopusTitle = "", Available = 0, PdfTitle = "Klosterbryg Gaveæske 5 x 50 cl", KegCollar = 3, PricePrUnit = 130 }),
                    Row(new() { OctopusId = "G2", WebId = "g2", WebTitle = "", OctopusTitle = "", Available = 0, PdfTitle = "Klosterbryg Gaveæske 3 x 50 cl", KegCollar = 5, PricePrUnit = 80 }),
                    Row(new() { OctopusId = "G3", WebId = "g3", WebTitle = "", OctopusTitle = "", Available = 0, PdfTitle = "Klosterbryg Gaveæske 6 x 33 cl", KegCollar = 4, PricePrUnit = 107 }),
                    Row(new() { OctopusId = "G4", WebId = "g4", WebTitle = "", OctopusTitle = "", Available = 0, PdfTitle = "Klosterbryg Pallegavekassen i træ med 6 x 50 cl", PricePrUnit = 168 }),
                    Row(new() { OctopusId = "G5", WebId = "g5", WebTitle = "", OctopusTitle = "", Available = 0, PdfTitle = "Klosterbryg ØLSTANG i træ med 10 x 50 cl", PricePrUnit = 268.5 }),
                ]
            },
            new()
            {
                Name = "KLOSTERBRYGGERIET - DANMARK",
                Layout = TableLayout.Standard,
                Products =
                [
                    Row(new() { OctopusId = "K1", WebId = "k1", WebTitle = "", OctopusTitle = "", Available = 50, PdfTitle = "Klosterbryggeriet Kloster Stout - Økologisk", KegCollar = 24, Str = 0.33, Alcohol = 8.0, PricePrUnit = 16.5 }),
                    Row(new() { OctopusId = "K2", WebId = "k2", WebTitle = "", OctopusTitle = "", Available = 30, PdfTitle = "Klosterbryggeriet Biere de Garde - Økologisk", KegCollar = 24, Str = 0.33, Alcohol = 9.0, PricePrUnit = 16.5 }),
                    Row(new() { OctopusId = "K3", WebId = "k3", WebTitle = "", OctopusTitle = "", Available = 20, PdfTitle = "Klosterbryggeriet Klosterhumle - Økologisk", KegCollar = 24, Str = 0.33, Alcohol = 6.0, PricePrUnit = 16.5 }),
                    Row(new() { OctopusId = "K4", WebId = "k4", WebTitle = "", OctopusTitle = "", Available = 15, PdfTitle = "Klosterbryggeriet Non PaleAle alkoholfri Øko.", KegCollar = 24, Str = 0.33, Alcohol = 0.5, PricePrUnit = 15.85 }),
                    Row(new() { OctopusId = "K5", WebId = "k5", WebTitle = "", OctopusTitle = "", Available = 40, PdfTitle = "Klosterbryggeriet VIKING - Økologisk", KegCollar = 15, Str = 0.5, Alcohol = 6.0, PricePrUnit = 25.35 }),
                    Row(new() { OctopusId = "K6", WebId = "k6", WebTitle = "", OctopusTitle = "", Available = 25, PdfTitle = "Klosterbryggeriet Hamborgøl Triple Bock", KegCollar = 15, Str = 0.5, Alcohol = 8.0, PricePrUnit = 23.85 }),
                    Row(new() { OctopusId = "K7", WebId = "k7", WebTitle = "", OctopusTitle = "", Available = 12, PdfTitle = "Klosterbryggeriet Øllikør - Varm øl", KegCollar = 12, Str = 0.35, Alcohol = 18.0, PricePrUnit = 52 }),
                ]
            },
            new()
            {
                Name = "MOSTERS økologiske saft - DANMARK",
                Layout = TableLayout.Saft,
                Products =
                [
                    Row(new() { OctopusId = "M1", WebId = "m1", WebTitle = "", OctopusTitle = "", Available = 80, PdfTitle = "MOSTERS Æble Økologisk", KegCollar = 20, Str = 0.25, PricePrUnit = 11 }),
                    Row(new() { OctopusId = "M2", WebId = "m2", WebTitle = "", OctopusTitle = "", Available = 60, PdfTitle = "MOSTERS Hyldeblomst Økologisk", KegCollar = 20, Str = 0.25, PricePrUnit = 10.2 }),
                    Row(new() { OctopusId = "M3", WebId = "m3", WebTitle = "", OctopusTitle = "", Available = 45, PdfTitle = "MOSTERS Solbær Økologisk", KegCollar = 20, Str = 0.25, PricePrUnit = 10.8 }),
                    Row(new() { OctopusId = "M4", WebId = "m4", WebTitle = "", OctopusTitle = "", Available = 55, PdfTitle = "MOSTERS Hindbær Økologisk", KegCollar = 20, Str = 0.25, PricePrUnit = 11.4 }),
                    Row(new() { OctopusId = "M5", WebId = "m5", WebTitle = "", OctopusTitle = "", Available = 70, PdfTitle = "MOSTERS Lemon / Ingefær Økologisk", KegCollar = 20, Str = 0.25, PricePrUnit = 9.85 }),
                ]
            },
            new()
            {
                Name = "BOLSKOV CIDER & MOST",
                Layout = TableLayout.Standard,
                Products =
                [
                    Row(new() { OctopusId = "B1", WebId = "b1", WebTitle = "", OctopusTitle = "", Available = 10, PdfTitle = "Bolskov CIDER Blend 1, årgang 2024", KegCollar = 6, Str = 0.75, Alcohol = 6.2, PricePrUnit = 80 }),
                    Row(new() { OctopusId = "B2", WebId = "b2", WebTitle = "", OctopusTitle = "", Available = 8,  PdfTitle = "Bolskov CIDER Blend 5, årgang 2024", KegCollar = 6, Str = 0.75, Alcohol = 6.0, PricePrUnit = 80 }),
                    Row(new() { OctopusId = "B3", WebId = "b3", WebTitle = "", OctopusTitle = "", Available = 6,  PdfTitle = "Bolskov CIDER Blend 7, årgang 2024", KegCollar = 6, Str = 0.75, Alcohol = 6.0, PricePrUnit = 80 }),
                ]
            },
            new()
            {
                Name = "AMARCORD - ITALIEN",
                Layout = TableLayout.Standard,
                Products =
                [
                    Row(new() { OctopusId = "A1", WebId = "a1", WebTitle = "", OctopusTitle = "", Available = 40, PdfTitle = "Amarcord Gradisca Lager 33 cl.", KegCollar = 24, Str = 0.33, Alcohol = 5.2, PricePrUnit = 17.25 }),
                    Row(new() { OctopusId = "A2", WebId = "a2", WebTitle = "", OctopusTitle = "", Available = 35, PdfTitle = "Amarcord Volpina Red Ale 33 cl.", KegCollar = 24, Str = 0.33, Alcohol = 6.5, PricePrUnit = 17.25 }),
                    Row(new() { OctopusId = "A3", WebId = "a3", WebTitle = "", OctopusTitle = "", Available = 20, PdfTitle = "Amarcord Tabachéra Strong Amber Ale 33cl.", KegCollar = 24, Str = 0.33, Alcohol = 9.0, PricePrUnit = 17.25 }),
                    Row(new() { OctopusId = "A4", WebId = "a4", WebTitle = "", OctopusTitle = "", Available = 18, PdfTitle = "Amarcord Gradisca Lager 50 cl.", KegCollar = 12, Str = 0.5, Alcohol = 5.2, PricePrUnit = 26.25 }),
                    Row(new() { OctopusId = "A5", WebId = "a5", WebTitle = "", OctopusTitle = "", Available = 15, PdfTitle = "Amarcord Riserva \"champagne-øl\"", KegCollar = 6, Str = 0.75, Alcohol = 10.0, PricePrUnit = 89 }),
                ]
            },
            new()
            {
                Name = "GALVANINA CENTURY LINE - ITALIEN",
                Layout = TableLayout.Standard,
                Products =
                [
                    Row(new() { OctopusId = "GAL1", WebId = "gal1", WebTitle = "", OctopusTitle = "", Available = 100, PdfTitle = "Galvanina Orange Økologisk", KegCollar = 12, Str = 0.35, PricePrUnit = 12.25 }),
                    Row(new() { OctopusId = "GAL2", WebId = "gal2", WebTitle = "", OctopusTitle = "", Available = 90,  PdfTitle = "Galvanina Lemon Økologisk", KegCollar = 12, Str = 0.35, PricePrUnit = 12.25 }),
                    Row(new() { OctopusId = "GAL3", WebId = "gal3", WebTitle = "", OctopusTitle = "", Available = 85,  PdfTitle = "Galvanina Bio Cola Økologisk", KegCollar = 12, Str = 0.35, PricePrUnit = 12.25 }),
                    Row(new() { OctopusId = "GAL4", WebId = "gal4", WebTitle = "", OctopusTitle = "", Available = 75,  PdfTitle = "Galvanina Tonic Økologisk", KegCollar = 12, Str = 0.35, PricePrUnit = 12.25 }),
                ]
            },
            new()
            {
                Name = "3 Monts Biere de Flandern - FRANKRIG",
                Layout = TableLayout.Standard,
                Products =
                [
                    Row(new() { OctopusId = "3M1", WebId = "3m1", WebTitle = "", OctopusTitle = "", Available = 20, PdfTitle = "3monts original - golden ale, 33 cl", KegCollar = 24, Str = 0.33, Alcohol = 8.5, PricePrUnit = 18 }),
                    Row(new() { OctopusId = "3M2", WebId = "3m2", WebTitle = "", OctopusTitle = "", Available = 15, PdfTitle = "3monts Saison, Season Ale 33 cl", KegCollar = 24, Str = 0.33, Alcohol = 6.5, PricePrUnit = 18 }),
                    Row(new() { OctopusId = "3M3", WebId = "3m3", WebTitle = "", OctopusTitle = "", Available = 12, PdfTitle = "3monts Curiée, Amber Ale, 33 cl", KegCollar = 24, Str = 0.33, Alcohol = 7.5, PricePrUnit = 18 }),
                ]
            },
            new()
            {
                Name = "HET ANKER BROUWERIJ - BELGIEN",
                Layout = TableLayout.Standard,
                Products =
                [
                    Row(new() { OctopusId = "GC1", WebId = "gc1", WebTitle = "", OctopusTitle = "", Available = 50, PdfTitle = "Gouden Carolus Ambrio", KegCollar = 24, Str = 0.33, Alcohol = 8.0, PricePrUnit = 17.75 }),
                    Row(new() { OctopusId = "GC2", WebId = "gc2", WebTitle = "", OctopusTitle = "", Available = 45, PdfTitle = "Gouden Carolus Classic", KegCollar = 24, Str = 0.33, Alcohol = 8.5, PricePrUnit = 17.75 }),
                    Row(new() { OctopusId = "GC3", WebId = "gc3", WebTitle = "", OctopusTitle = "", Available = 40, PdfTitle = "Gouden Carolus Triple", KegCollar = 24, Str = 0.33, Alcohol = 9.0, PricePrUnit = 17.75 }),
                    Row(new() { OctopusId = "GC4", WebId = "gc4", WebTitle = "", OctopusTitle = "", Available = 30, PdfTitle = "Gouden Carolus Christmas 33 cl", KegCollar = 24, Str = 0.33, Alcohol = 10.5, PricePrUnit = 17.95 }),
                ]
            },
        };

        return Task.FromResult(categories);
    }

    private static ProductRow Row(Product p) => new() { Product = p };
}
