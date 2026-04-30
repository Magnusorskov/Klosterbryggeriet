namespace BlazorApp.Models.Dtos;

public enum PendingProductKind
{
    RegularProduct,
    DraftBeer,
}

public class PendingNewProduct
{
    public required Product Product { get; set; }
    public List<string> Errors { get; set; } = [];
    public HashSet<string> FieldErrors { get; set; } = [];
    public bool CustomCategory { get; set; }

    public PendingProductKind Kind { get; set; } = PendingProductKind.RegularProduct;

    // Draft-beer-only fields. Ignored when Kind == RegularProduct.
    public string Kobling { get; set; } = "";
    public string Land { get; set; } = "";
    public bool CustomKobling { get; set; }

    public int KegCollarInput
    {
        get => Product.KegCollar ?? 0;
        set => Product.KegCollar = value == 0 ? null : value;
    }

    // Project the shared fields into a DraftBeer entity. Category is forced to
    // "FADØL" so draft beers slot under the FADØL section on the price list.
    public DraftBeer ToDraftBeer() => new()
    {
        OctopusId    = Product.OctopusId,
        WebId        = Product.WebId,
        WebTitle     = Product.WebTitle ?? "",
        PdfTitle     = Product.PdfTitle ?? "",
        OctopusTitle = Product.OctopusTitle ?? "",
        Available    = Product.Available,
        Str          = Product.Str,
        Alcohol      = Product.Alcohol,
        PricePrUnit  = Product.PricePrUnit,
        Category     = "FADØL",
        VariantId1   = Product.VariantId1,
        VariantId2   = Product.VariantId2,
        InUse        = Product.InUse,
        Kobling      = Kobling ?? "",
        Land         = Land ?? "",
    };
}
