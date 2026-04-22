using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BlazorApp.Models;

public class Product
{
    public const int SoldOutThreshold = 30;

    [NotMapped]
    public ProductStatus Status => StatusFor(Available);

    public static ProductStatus StatusFor(int available)
        => available <= SoldOutThreshold ? ProductStatus.SoldOut : ProductStatus.Available;

    // Tidligere VareNr
    [Key]
    public required int OctopusId { get; set; }
    
    // PRODUCT_ID i title
    public required int WebId { get; set; }
    
    // TITLE_DK from Henrik csv
    public required string WebTitle { get; set; }
    
    public required string PdfTitle { get; set; }
    
    // Tidligere VareTekst
    public required string OctopusTitle { get; set; }
    
    // Disponibel
    public required int Available { get; set; }
    
    // Kasse kolli
    public int? KegCollar { get; set; } = 0;

    public double Str { get; set; } = 0.0;
    
    public double Alcohol { get; set; } = 0.0;
    
    public double PricePrUnit { get; set; } = 0.0;
    
    public required string Category { get; set; }

    public int VariantId1 { get; set; }
    public int VariantId2 { get; set; }

    // Controls whether the product is shown on the generated PDF and
    // considered when updating the website. Defaults to true.
    public bool InUse { get; set; } = true;

    // Half-kolli availability: when true, the PDF appends "*" to Kasse/Kolli
    // and the price list explains that * means a half-kasse can be ordered.
    public bool HalfKolli { get; set; } = false;


    // private const double TOLERANCE = 1e-6; // 0.000001
    // protected bool Equals(Product other)
    // {
    //     return OctopusId == other.OctopusId
    //            && WebId == other.WebId
    //            && WebTitle == other.WebTitle
    //            && PdfTitle == other.PdfTitle
    //            && OctopusTitle == other.OctopusTitle
    //            && Available == other.Available
    //            && KegCollar == other.KegCollar
    //            && Math.Abs(Str - other.Str) < TOLERANCE
    //            && Math.Abs(Alcohol - other.Alcohol) < TOLERANCE
    //            && Math.Abs(PricePrUnit - other.PricePrUnit) < TOLERANCE
    //            && Category == other.Category;
    // }
    //
    //
    // public override bool Equals(object? obj)
    // {
    //     if (obj is null) return false;
    //     if (ReferenceEquals(this, obj)) return true;
    //     if (obj.GetType() != GetType()) return false;
    //     return Equals((Product)obj);
    // }
    //
    // public override int GetHashCode()
    // {
    //     var hashCode = new HashCode();
    //     hashCode.Add(OctopusId);
    //     hashCode.Add(WebId);
    //     hashCode.Add(WebTitle);
    //     hashCode.Add(PdfTitle);
    //     hashCode.Add(OctopusTitle);
    //     hashCode.Add(Available);
    //     hashCode.Add(KegCollar);
    //     hashCode.Add(Str);
    //     hashCode.Add(Alcohol);
    //     hashCode.Add(PricePrUnit);
    //     hashCode.Add(Category);
    //     return hashCode.ToHashCode();
    // }
}