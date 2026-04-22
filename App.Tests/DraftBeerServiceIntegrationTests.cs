using BlazorApp.Data;
using BlazorApp.Models;
using BlazorApp.Services;
using Microsoft.EntityFrameworkCore;

namespace App.Tests;

public class DraftBeerServiceIntegrationTests : IClassFixture<DatabaseFixture>, IAsyncLifetime
{
    private readonly DatabaseFixture _fixture;

    public DraftBeerServiceIntegrationTests(DatabaseFixture fixture)
    {
        _fixture = fixture;
    }

    public Task InitializeAsync() => Task.CompletedTask;

    public async Task DisposeAsync()
    {
        await using var db = _fixture.CreateDbContext();
        db.DraftBeers.RemoveRange(db.DraftBeers);
        await db.SaveChangesAsync();
    }

    [Fact]
    public async Task GetAllAsync_ReturnsDraftBeersOrderedByPdfTitle()
    {
        // Arrange
        await using (var db = _fixture.CreateDbContext())
        {
            await db.DraftBeers.AddRangeAsync(
                CreateBeer(octopusId: 1, pdfTitle: "Westmalle"),
                CreateBeer(octopusId: 2, pdfTitle: "Ayinger Urweisse"),
                CreateBeer(octopusId: 3, pdfTitle: "Gouden Carolus"));
            await db.SaveChangesAsync();
        }

        // Act
        List<DraftBeer> result;
        var service = new DraftBeerService(new FixtureDbContextFactory(_fixture));
        result = await service.GetAllAsync();

        // Assert
        Assert.Equal(3, result.Count);
        Assert.Equal("Ayinger Urweisse", result[0].PdfTitle);
        Assert.Equal("Gouden Carolus", result[1].PdfTitle);
        Assert.Equal("Westmalle", result[2].PdfTitle);
    }

    [Fact]
    public async Task GetAllAsync_EmptyDatabase_ReturnsEmptyList()
    {
        // Act
        var service = new DraftBeerService(new FixtureDbContextFactory(_fixture));
        var result = await service.GetAllAsync();

        // Assert
        Assert.Empty(result);
    }

    [Fact]
    public async Task AddAsync_AddsDraftBeer()
    {
        // Arrange
        var beer = CreateBeer(octopusId: 42, pdfTitle: "Ayinger Celebrator");

        // Act
        var service = new DraftBeerService(new FixtureDbContextFactory(_fixture));
        await service.AddAsync(beer);

        // Assert
        await using var assertDb = _fixture.CreateDbContext();
        var stored = await assertDb.DraftBeers.SingleAsync();
        Assert.Equal(42, stored.OctopusId);
        Assert.Equal("Ayinger Celebrator", stored.PdfTitle);
    }

    [Fact]
    public async Task SaveAsync_UpdatesAllFields()
    {
        // Arrange
        await using (var db = _fixture.CreateDbContext())
        {
            await db.DraftBeers.AddAsync(CreateBeer(octopusId: 10, pdfTitle: "Urweisse", kobling: "A", land: "Tyskland", str: 30, alcohol: 5.8, pricePrUnit: 24));
            await db.SaveChangesAsync();
        }

        // Act
        var updated = CreateBeer(octopusId: 10, pdfTitle: "Urweisse Opdateret", kobling: "S", land: "Belgien", str: 20, alcohol: 7.0, pricePrUnit: 38);
        var service = new DraftBeerService(new FixtureDbContextFactory(_fixture));
        await service.SaveAsync(updated);

        // Assert
        await using var assertDb = _fixture.CreateDbContext();
        var stored = await assertDb.DraftBeers.SingleAsync();
        Assert.Equal("Urweisse Opdateret", stored.PdfTitle);
        Assert.Equal("S", stored.Kobling);
        Assert.Equal("Belgien", stored.Land);
        Assert.Equal(20, stored.Str);
        Assert.Equal(7.0, stored.Alcohol);
        Assert.Equal(38, stored.PricePrUnit);
    }

    [Fact]
    public async Task SaveAsync_NotFound_Throws()
    {
        // Act & Assert
        var service = new DraftBeerService(new FixtureDbContextFactory(_fixture));
        await Assert.ThrowsAsync<InvalidOperationException>(
            () => service.SaveAsync(CreateBeer(octopusId: 9999, pdfTitle: "Findes ikke")));
    }

    [Fact]
    public async Task DeleteAsync_RemovesDraftBeer()
    {
        // Arrange
        await using (var db = _fixture.CreateDbContext())
        {
            await db.DraftBeers.AddAsync(CreateBeer(octopusId: 55, pdfTitle: "Til sletning"));
            await db.SaveChangesAsync();
        }

        // Act
        var service = new DraftBeerService(new FixtureDbContextFactory(_fixture));
        await service.DeleteAsync(55);

        // Assert
        await using var assertDb = _fixture.CreateDbContext();
        Assert.False(await assertDb.DraftBeers.AnyAsync(b => b.OctopusId == 55));
    }

    [Fact]
    public async Task DeleteAsync_NotFound_DoesNothing()
    {
        // Act & Assert (skal ikke kaste exception)
        var service = new DraftBeerService(new FixtureDbContextFactory(_fixture));
        await service.DeleteAsync(9999);
    }

    private static DraftBeer CreateBeer(
        int octopusId = 1,
        string pdfTitle = "Test Øl",
        string kobling = "S",
        string land = "Danmark",
        double str = 30,
        double alcohol = 5.0,
        double pricePrUnit = 25) => new()
    {
        OctopusId    = octopusId,
        WebId        = 0,
        WebTitle     = $"{pdfTitle} {str}L",
        PdfTitle     = pdfTitle,
        OctopusTitle = pdfTitle,
        Available    = 10,
        Str          = str,
        Alcohol      = alcohol,
        PricePrUnit  = pricePrUnit,
        Category     = "TEST",
        Kobling      = kobling,
        Land         = land
    };

    private sealed class FixtureDbContextFactory(DatabaseFixture fixture) : IDbContextFactory<AppDbContext>
    {
        public AppDbContext CreateDbContext() => fixture.CreateDbContext();
    }
}
