namespace BlazorApp.Models;

public class Product
{
    // Tidligere VareNr
    public required string OctopusId { get; set; }
    
    // PRODUCT_ID i title
    public required string WebId { get; set; }
    
    // TITLE_DK from Henrik csv
    public required string WebTitle { get; set; }
    
    public required string PdfTitle { get; set; }
    
    // Tidligere VareTekst
    public required string OctopusTitle { get; set; }
    
    // Disponibel
    public required decimal Available { get; set; }
    
    // Kasse kolli
    public int? KegCollar { get; set; } = 0;

    public double Str { get; set; } = 0.0;
    
    public double Alcohol { get; set; } = 0.0;
    
    public double PricePrUnit { get; set; } = 0.0;
    
    public String Category { get; set; }
    

    private const double TOLERANCE = 1e-6; // 0.000001 
    protected bool Equals(Product other)
    {
        return OctopusId == other.OctopusId
               && WebId == other.WebId
               && WebTitle == other.WebTitle
               && PdfTitle == other.PdfTitle
               && OctopusTitle == other.OctopusTitle
               && Available == other.Available
               && KegCollar == other.KegCollar
               && Math.Abs(Str - other.Str) < TOLERANCE
               && Math.Abs(Alcohol - other.Alcohol) < TOLERANCE
               && Math.Abs(PricePrUnit - other.PricePrUnit) < TOLERANCE
               && Category == other.Category;
    }
    

    public override bool Equals(object? obj)
    {
        if (obj is null) return false;
        if (ReferenceEquals(this, obj)) return true;
        if (obj.GetType() != GetType()) return false;
        return Equals((Product)obj);
    }

    public override int GetHashCode()
    {
        var hashCode = new HashCode();
        hashCode.Add(OctopusId);
        hashCode.Add(WebId);
        hashCode.Add(WebTitle);
        hashCode.Add(PdfTitle);
        hashCode.Add(OctopusTitle);
        hashCode.Add(Available);
        hashCode.Add(KegCollar);
        hashCode.Add(Str);
        hashCode.Add(Alcohol);
        hashCode.Add(PricePrUnit);
        hashCode.Add(Category);
        return hashCode.ToHashCode();
    }
}