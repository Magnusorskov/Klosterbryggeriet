using BlazorApp.Models;
using BlazorApp.Services;

namespace App.Tests;

public class OctopusServiceTests
{
    private readonly OctopusService _service = new();


    /**
     * Helper method for mocking the frontend passing a file to the
     * OctopusCsvToEntities method
     */
    private FileStream GetFileFromPath(string filePath)
    {
        return File.OpenRead(filePath);
    }

    [Fact]
    public void OctopusCsvToEntities_WithValidInput_ReturnsExpectedResult()
    {
        var expected = new List<Vare>
        {
            new Vare
            {
                Varenr = "13900",
                Varetekst = "4x50 cl Amarcord i gavekasse",
                Tekst2 = "",
                Lokation = "",
                Leverandor = "",
                Beholdning = -24,
                Prisfaktor = 1,
                Gennemsnit = 70,
                Lagervaerdi = -1680.00m,
                Genanskaffelse = 70,
                LagervaerdiGenanskaffelse = -1680.00m,
                Varegruppe = "80",
                Undergruppe = "10",
                Lagergruppe = "10",
                SidsteSalgsdato = new DateTime(2026, 1, 14),
                SidsteKobsdato = new DateTime(2021, 12, 7),
                Edbnr = "00000",
                Enhed = "STK",
                Type = "P",
                Disponibel = -24
            },
            new Vare
            {
                Varenr = "14500",
                Varetekst = "Pallegavekassen i træ",
                Tekst2 = "med 10 x 50 cl Klosterbryg",
                Lokation = "",
                Leverandor = "",
                Beholdning = -1,
                Prisfaktor = 1,
                Gennemsnit = 180,
                Lagervaerdi = -180,
                Genanskaffelse = 180,
                LagervaerdiGenanskaffelse = -180,
                Varegruppe = "80",
                Undergruppe = "10",
                Lagergruppe = "10",
                SidsteSalgsdato = new DateTime(2026, 1, 23),
                SidsteKobsdato = new DateTime(2021, 12, 7),
                Edbnr = "14500",
                Enhed = "STK",
                Type = "P",
                Disponibel = -1
            },
            new Vare
            {
                Varenr = "14520",
                Varetekst = "Trækasse med 6 x 50 cl",
                Tekst2 = "Klosterbryg",
                Lokation = "",
                Leverandor = "",
                Beholdning = -7,
                Prisfaktor = 1,
                Gennemsnit = 0,
                Lagervaerdi = 0,
                Genanskaffelse = 140,
                LagervaerdiGenanskaffelse = -980,
                Varegruppe = "80",
                Undergruppe = "10",
                Lagergruppe = "10",
                SidsteSalgsdato = new DateTime(2026, 2, 11),
                SidsteKobsdato = new DateTime(2021, 12, 7),
                Edbnr = "14520",
                Enhed = "STK",
                Type = "P",
                Disponibel = -7
            }
        };

        var filePath = "TestData/OctopusTestData.csv";
        var stream = GetFileFromPath(filePath);
        var result = _service.OctopusCsvToEntities(stream);

        Assert.Equal(expected, result);
    }

    [Fact]
    public void OctopusCsvToEntities_WithHeaderOnly_ReturnsEmptyList()
    {
        var stream = GetFileFromPath("TestData/OctopusTestDataHeaderOnly.csv");
        var result = _service.OctopusCsvToEntities(stream);

        Assert.Empty(result);
    }

    [Fact]
    public void OctopusCsvToEntities_WithMissingDates_ReturnsNullDates()
    {
        var stream = GetFileFromPath("TestData/OctopusTestDataMissingDates.csv");
        var result = _service.OctopusCsvToEntities(stream);

        Assert.Single(result);
        Assert.Null(result[0].SidsteSalgsdato);
        Assert.Null(result[0].SidsteKobsdato);
    }
}