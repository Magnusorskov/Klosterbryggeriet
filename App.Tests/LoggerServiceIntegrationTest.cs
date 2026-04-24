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
        await using var db = _fixture.CreateDbContext();
        db.LogEntries.RemoveRange(db.LogEntries);
        db.Products.RemoveRange(db.Products);
        await db.SaveChangesAsync();
    }

    private static Product BuildProduct(int octopusId, int available, string pdfTitle = "empty") => new()
    {
        OctopusId = octopusId,
        WebId = 0,
        WebTitle = "empty",
        PdfTitle = pdfTitle,
        OctopusTitle = "empty",
        Available = available,
        KegCollar = 0,
        Str = 0.0,
        Alcohol = 0.0,
        PricePrUnit = 0.0,
        Category = "undefined",
        VariantId1 = 0,
        VariantId2 = 0
    };

    private async Task SeedProduct(Product product)
    {
        await using var db = _fixture.CreateDbContext();
        await db.Products.AddAsync(product);
        await db.SaveChangesAsync();
    }


    [Fact]
    public async Task LogProductChange_WritesEntry_WithCorrectFields()
    {
        var product = BuildProduct(14500, 100, pdfTitle: "Klosterbryg");
        await SeedProduct(product);

        await using (var db = _fixture.CreateDbContext())
        {
            var service = new LoggerService(db);
            await service.LogProductChange(product, ProductStatus.Available, ProductStatus.SoldOut);
        }

        await using (var db = _fixture.CreateDbContext())
        {
            var entries = await db.LogEntries.ToListAsync();
            Assert.Single(entries);
            var entry = entries[0];
            Assert.Equal(14500, entry.OctopusId);
            Assert.Equal("Klosterbryg", entry.ProductName);
            Assert.Equal(ProductStatus.Available, entry.PreviousStatus);
            Assert.Equal(ProductStatus.SoldOut, entry.NewStatus);
        }
    }

    [Fact]
    public async Task GetLogEntries_ReturnsAllLoggedEntries()
    {
        var product = BuildProduct(14500, 100);
        await SeedProduct(product);

        await using (var db = _fixture.CreateDbContext())
        {
            var service = new LoggerService(db);
            await service.LogProductChange(product, ProductStatus.Available, ProductStatus.SoldOut);

            var entries = await service.GetLogEntries();
            Assert.Single(entries);
            Assert.Equal(14500, entries[0].OctopusId);
        }
    }


    [Fact]
    public async Task OctopusCsv_AvailableToSoldOut_LogsOneTransition()
    {
        // DB has Available=100 (status = Available). CSV sets Available=-500 (status = SoldOut).
        var product = BuildProduct(14500, 100);
        await SeedProduct(product);

        await using (var db = _fixture.CreateDbContext())
        {
            var service = new OctopusService(db, new LoggerService(db));
            await service.UpdateAvailableFromOctopusCsv(GetFileFromPath("TestData/OctopusTestData.csv"));
        }

        await using (var db = _fixture.CreateDbContext())
        {
            var entries = await db.LogEntries.ToListAsync();
            Assert.Single(entries);
            Assert.Equal(14500, entries[0].OctopusId);
            Assert.Equal(ProductStatus.Available, entries[0].PreviousStatus);
            Assert.Equal(ProductStatus.SoldOut, entries[0].NewStatus);
        }
    }

    [Fact]
    public async Task OctopusCsv_StaysSoldOut_LogsNothing()
    {
        var product = BuildProduct(14500, -1);
        await SeedProduct(product);

        await using (var db = _fixture.CreateDbContext())
        {
            var service = new OctopusService(db, new LoggerService(db));
            await service.UpdateAvailableFromOctopusCsv(GetFileFromPath("TestData/OctopusTestData.csv"));
        }

        await using (var db = _fixture.CreateDbContext())
        {
            var entries = await db.LogEntries.ToListAsync();
            Assert.Empty(entries);
        }
    }

    [Fact]
    public async Task OctopusCsv_NoMatchingProductInDb_LogsNothing()
    {
        // Product in DB has OctopusId 100, CSV only references 14500.
        var product = BuildProduct(100, 50);
        await SeedProduct(product);

        await using (var db = _fixture.CreateDbContext())
        {
            var service = new OctopusService(db, new LoggerService(db));
            await service.UpdateAvailableFromOctopusCsv(GetFileFromPath("TestData/OctopusTestData.csv"));
        }

        await using (var db = _fixture.CreateDbContext())
        {
            var entries = await db.LogEntries.ToListAsync();
            Assert.Empty(entries);
        }
    }

    private FileStream GetFileFromPath(string filePath) => File.OpenRead(filePath);
}
