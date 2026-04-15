using BlazorApp.Data;
using BlazorApp.Models;
using BlazorApp.Services;
using Microsoft.EntityFrameworkCore;

namespace App.Tests;

public class LoggerServiceIntegrationTest : IClassFixture<DatabaseFixture>, IAsyncLifetime
{
    private readonly DatabaseFixture _fixture;

    public LoggerServiceIntegrationTest(DatabaseFixture fixture)
    {
        _fixture = fixture;
    }
    
    public Task InitializeAsync() => Task.CompletedTask;
    
    public async Task DisposeAsync()
    {
        // Clean up data between tests so they stay isolated
        await using var db = _fixture.CreateDbContext();
        db.LogEntries.RemoveRange(db.LogEntries);
        db.Products.RemoveRange(db.Products);
        await db.SaveChangesAsync();
    }

    private async Task<Product> TriggerLogEvent_ProductNotChanged()
    {
        // Arrange
        var product = new Product
        {
            OctopusId = 14500,
            WebId = 0, // default value since not relevant for test case
            WebTitle = "empty", // default value since not relevant for test case
            PdfTitle = "empty", // default value since not relevant for test case
            OctopusTitle = "empty", // default value since not relevant for test case
            Available = -500,
            KegCollar = 0, // default value since not relevant for test case
            Str = 0.0, // default value since not relevant for test case
            Alcohol = 0.0, // default value since not relevant for test case
            PricePrUnit = 0.0, // default value since not relevant for test case
            Category = "undefined", // default value since not relevant for test case
            VariantId1 = 0, // default value since not relevant for test case
            VariantId2 = 0 // default value since not relevant for test case
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

        return product;
    }
    private async Task<Product> TriggerLogEvent_ProductChanged()
    {
        // Arrange
        var product = new Product
        {
            OctopusId = 14500,
            WebId = 0, // default value since not relevant for test case
            WebTitle = "empty", // default value since not relevant for test case
            PdfTitle = "empty", // default value since not relevant for test case
            OctopusTitle = "empty", // default value since not relevant for test case
            Available = -1,
            KegCollar = 0, // default value since not relevant for test case
            Str = 0.0, // default value since not relevant for test case
            Alcohol = 0.0, // default value since not relevant for test case
            PricePrUnit = 0.0, // default value since not relevant for test case
            Category = "undefined", // default value since not relevant for test case
            VariantId1 = 0, // default value since not relevant for test case
            VariantId2 = 0 // default value since not relevant for test case
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

        return product;
    }
    
    [Fact]
    public async Task LogProductChange_Default()
    {
        Product product = await TriggerLogEvent_ProductChanged();

        await using (var db = _fixture.CreateDbContext())
        {
            var foundEntries = await db.LogEntries.ToListAsync();
            Assert.Single(foundEntries);
            
            var entry = foundEntries[0];
            Assert.Equal(product.OctopusId, entry.OctopusId);
            Assert.Equal("int", entry.ValueType);
            Assert.Equal("Available",entry.ColumnName);
            Assert.Equal(product.PdfTitle, entry.ProductName);
            Assert.Equal(product.Available.ToString(), entry.PreviousValue);
            Assert.Equal("-500", entry.NewValue);
            
            
        }
    }

    [Fact]
    public async Task GetLogEntries_Default()
    {
        Product product = await TriggerLogEvent_ProductChanged(); 
        await using (var db = _fixture.CreateDbContext())
        {
            var service = new LoggerService(db);
            var foundEntries = await service.GetLogEntries();
            Assert.Single(foundEntries);
            Assert.Equal(product.OctopusId, foundEntries[0].OctopusId);
            
        }
        
    }

    [Fact]
    public async Task GetLogEntries_NoChangesResultsInNoLogs()
    {
        Product product = await TriggerLogEvent_ProductNotChanged(); 
        await using (var db = _fixture.CreateDbContext())
        {
            var service = new LoggerService(db);
            var foundEntries = await service.GetLogEntries();
            Assert.Empty(foundEntries);
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