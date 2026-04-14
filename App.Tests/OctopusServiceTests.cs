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

    // [Fact]
    // public void OctopusCsvToEntities_WithValidInput_ReturnsExpectedResult()
    // {
    //
    // }
    //TODO: test at uploaded octopus csv opdaterer database (integrations test)
    
    //TODO: Lav test for kategoriserings algoritme samt selve algoritmen i OctopusService

}