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
}