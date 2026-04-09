namespace BlazorApp.Models;

public class Product
{
    public required string Varenr { get; set; }
    public required string Varetekst { get; set; }
    public string? Tekst2 { get; set; }
    public decimal Beholdning { get; set; }
    public required decimal Disponibel { get; set; }

    public int? KasseKolli { get; set; } = 0;

    public double Str { get; set; } = 0.0;
    
    public double Alkohol { get; set; } = 0.0;
    
    public double PrisPrEnhed { get; set; } = 0.0;
    

    private const double TOLERANCE = 1e-6; // 0.000001 
    protected bool Equals(Product other)
    {
        return Varenr == other.Varenr 
               && Varetekst == other.Varetekst
               && Tekst2 == other.Tekst2
               && Beholdning == other.Beholdning
               && Disponibel == other.Disponibel
               && KasseKolli == other.KasseKolli
               && Math.Abs(Str - other.Str) < TOLERANCE
               && Math.Abs(Alkohol - other.Alkohol) < TOLERANCE
               && Math.Abs(PrisPrEnhed - other.PrisPrEnhed) < TOLERANCE;
        
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
        hashCode.Add(Varenr);
        hashCode.Add(Varetekst);
        hashCode.Add(Tekst2);
        hashCode.Add(Beholdning);
        hashCode.Add(Disponibel);
        hashCode.Add(KasseKolli);
        hashCode.Add(Str);
        hashCode.Add(Alkohol);
        hashCode.Add(PrisPrEnhed);
        return hashCode.ToHashCode();
    }
}