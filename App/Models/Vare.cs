namespace BlazorApp.Models;

public class Vare
{
    public string Varenr { get; set; }
    public string Varetekst { get; set; }
    public string Tekst2 { get; set; }
    public string Lokation { get; set; }
    public string Leverandor { get; set; }
    public decimal Beholdning { get; set; }
    public decimal Prisfaktor { get; set; }
    public decimal Gennemsnit { get; set; }
    public decimal Lagervaerdi { get; set; }
    public decimal Genanskaffelse { get; set; }
    public decimal LagervaerdiGenanskaffelse { get; set; }
    public string Varegruppe { get; set; }
    public string Undergruppe { get; set; }
    public string Lagergruppe { get; set; }
    public DateTime? SidsteSalgsdato { get; set; }
    public DateTime? SidsteKobsdato { get; set; }
    public string Edbnr { get; set; }
    public string Enhed { get; set; }
    public string Type { get; set; }
    public decimal Disponibel { get; set; }

    protected bool Equals(Vare other)
    {
        return Varenr == other.Varenr && Varetekst == other.Varetekst && Tekst2 == other.Tekst2 && Lokation == other.Lokation && Leverandor == other.Leverandor && Beholdning == other.Beholdning && Prisfaktor == other.Prisfaktor && Gennemsnit == other.Gennemsnit && Lagervaerdi == other.Lagervaerdi && Genanskaffelse == other.Genanskaffelse && LagervaerdiGenanskaffelse == other.LagervaerdiGenanskaffelse && Varegruppe == other.Varegruppe && Undergruppe == other.Undergruppe && Lagergruppe == other.Lagergruppe && Nullable.Equals(SidsteSalgsdato, other.SidsteSalgsdato) && Nullable.Equals(SidsteKobsdato, other.SidsteKobsdato) && Edbnr == other.Edbnr && Enhed == other.Enhed && Type == other.Type && Disponibel == other.Disponibel;
    }

    public override bool Equals(object? obj)
    {
        if (obj is null) return false;
        if (ReferenceEquals(this, obj)) return true;
        if (obj.GetType() != GetType()) return false;
        return Equals((Vare)obj);
    }

    public override int GetHashCode()
    {
        var hashCode = new HashCode();
        hashCode.Add(Varenr);
        hashCode.Add(Varetekst);
        hashCode.Add(Tekst2);
        hashCode.Add(Lokation);
        hashCode.Add(Leverandor);
        hashCode.Add(Beholdning);
        hashCode.Add(Prisfaktor);
        hashCode.Add(Gennemsnit);
        hashCode.Add(Lagervaerdi);
        hashCode.Add(Genanskaffelse);
        hashCode.Add(LagervaerdiGenanskaffelse);
        hashCode.Add(Varegruppe);
        hashCode.Add(Undergruppe);
        hashCode.Add(Lagergruppe);
        hashCode.Add(SidsteSalgsdato);
        hashCode.Add(SidsteKobsdato);
        hashCode.Add(Edbnr);
        hashCode.Add(Enhed);
        hashCode.Add(Type);
        hashCode.Add(Disponibel);
        return hashCode.ToHashCode();
    }
}