using BlazorApp.Services;

namespace App.Tests;

public class OctopusServiceTests
{
    private readonly OctopusService _service = new();

    [Fact]
    public void OctopusCsvToEntities_WithValidInput_Works()
    {
        // Næste gang skal vi sige på forhånd, hvordan vores objekter af klassen 
        // "vare" skal se ud, og derefter indlæse og lave de objekter gennem 
        // metoden og matche dem op med hinanden.
        var filePath = "TestData/OctopusTestData.csv"; 
        var result = _service.OctopusCsvToEntities(filePath);
    }
}