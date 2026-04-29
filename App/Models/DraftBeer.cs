using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BlazorApp.Models;

public class DraftBeer
{
    [Key]
    public required int OctopusId { get; set; }

    public required int WebId { get; set; }

    public required string WebTitle { get; set; }

    public required string PdfTitle { get; set; }

    public required string OctopusTitle { get; set; }

    public required int Available { get; set; }

    // Keg size in liters (e.g. 20, 30)
    public double Str { get; set; } = 0.0;

    public double Alcohol { get; set; } = 0.0;

    // Price pr. liter
    public double PricePrUnit { get; set; } = 0.0;

    public required string Category { get; set; }

    public int VariantId1 { get; set; }
    public int VariantId2 { get; set; }

    public bool InUse { get; set; } = true;

    // Kobling type: "S", "A", "Key keg"
    public string Kobling { get; set; } = "";

    // Country of origin (e.g. "Danmark", "Belgien")
    public string Land { get; set; } = "";

    // Anker price = keg size in liters * price pr. liter (UI only)
    [NotMapped]
    public double AnkerPrice => Str * PricePrUnit;

    [NotMapped]
    public string Status => Available <= 0 ? "Udsolgt" : "";
}