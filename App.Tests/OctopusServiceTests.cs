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
        var expected = new List<Product>
        {
            new Product
            {
                Varenr = "13900",
                Varetekst = "4x50 cl Amarcord i gavekasse",
                Tekst2 = "",
                Beholdning = -24,
                Disponibel = -24
            },
            new Product
            {
                Varenr = "14500",
                Varetekst = "Pallegavekassen i træ",
                Tekst2 = "med 10 x 50 cl Klosterbryg",
                Beholdning = -1,
                Disponibel = -1
            },
            new Product
            {
                Varenr = "14520",
                Varetekst = "Trækasse med 6 x 50 cl",
                Tekst2 = "Klosterbryg",
                Beholdning = -7,
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
}