using BlazorApp.Data;
using BlazorApp.Models;
using BlazorApp.Services;
using Microsoft.EntityFrameworkCore;

namespace App.Tests;

public class OctopusServiceIntegrationTest : IClassFixture<DatabaseFixture>, IAsyncLifetime
{
    private readonly DatabaseFixture _fixture;

    public OctopusServiceIntegrationTest(DatabaseFixture fixture)
    {
        _fixture = fixture;
    }

    public Task InitializeAsync() => Task.CompletedTask;

    public async Task DisposeAsync()
    {
        // Clean up data between tests so they stay isolated
        await using var db = _fixture.CreateDbContext();
        db.Products.RemoveRange(db.Products);
        await db.SaveChangesAsync();
    }

    [Fact]
    public async Task UpdateAvailableFromOctopusCsv_Default()
    {
        // Arrange
        var product = new Product
        {
            OctopusId = "14500",
            WebId = "0", // default value since not relevant for test case
            WebTitle = "empty", // default value since not relevant for test case
            PdfTitle = "empty", // default value since not relevant for test case
            OctopusTitle = "empty", // default value since not relevant for test case
            Available = -1,
            KegCollar = 0, // default value since not relevant for test case
            Str = 0.0, // default value since not relevant for test case
            Alcohol = 0.0, // default value since not relevant for test case
            PricePrUnit = 0.0, // default value since not relevant for test case
            Category = "undefined" // default value since not relevant for test case
        };

        await using (var db = _fixture.CreateDbContext())
        {
            await db.Products.AddAsync(product);
            await db.SaveChangesAsync();
        }

        var testFile = GetFileFromPath("TestData/OctopusTestData.csv");
        await using (var db = _fixture.CreateDbContext())
        {
            var service = new OctopusService(db);
            await service.UpdateAvailableFromOctopusCsv(testFile);
        }

        await using (var db = _fixture.CreateDbContext())
        {
            var saved = await db.Products.FindAsync("14500");
            Assert.NotNull(saved);

            var previousValue = product.Available;
            Assert.NotEqual(saved.Available, previousValue);
            Assert.Equal(-500, saved.Available);
        }
    }

    [Fact]
    public async Task UpdateAvailableFromOctopusCsv_ExcludedProductNotChanged()
    {
        // Arrange
        var product = new Product
        {
            OctopusId = "100",
            WebId = "0", // default value since not relevant for test case
            WebTitle = "empty", // default value since not relevant for test case
            PdfTitle = "empty", // default value since not relevant for test case
            OctopusTitle = "empty", // default value since not relevant for test case
            Available = -1,
            KegCollar = 0, // default value since not relevant for test case
            Str = 0.0, // default value since not relevant for test case
            Alcohol = 0.0, // default value since not relevant for test case
            PricePrUnit = 0.0, // default value since not relevant for test case
            Category = "undefined" // default value since not relevant for test case
        };

        await using (var db = _fixture.CreateDbContext())
        {
            await db.Products.AddAsync(product);
            await db.SaveChangesAsync();
        }

        var testFile = GetFileFromPath("TestData/OctopusTestData.csv");
        await using (var db = _fixture.CreateDbContext())
        {
            var service = new OctopusService(db);
            await service.UpdateAvailableFromOctopusCsv(testFile);
        }

        await using (var db = _fixture.CreateDbContext())
        {
            var saved = await db.Products.FindAsync("100");
            Assert.NotNull(saved);

            var previousValue = product.Available;
            Assert.Equal(saved.Available, previousValue);
            Assert.NotEqual(-500, saved.Available);
        }
    }


    /**
    * Helper method for mocking the frontend passing a file to the
    * OctopusCsvToEntities method
    */
    private FileStream GetFileFromPath(string filePath)
    {
        return File.OpenRead(filePath);
    }

}