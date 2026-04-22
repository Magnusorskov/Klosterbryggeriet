using BlazorApp.Models;
using BlazorApp.Services;

namespace App.Tests;

public class ProductServiceIntegrationTest : IClassFixture<DatabaseFixture>, IAsyncLifetime
{
    private readonly DatabaseFixture _fixture;

    public ProductServiceIntegrationTest(DatabaseFixture fixture)
    {
        _fixture = fixture;
    }

    public Task InitializeAsync() => Task.CompletedTask;

    public async Task DisposeAsync()
    {
        await using var db = _fixture.CreateDbContext();
        db.Products.RemoveRange(db.Products);
        await db.SaveChangesAsync();
    }

    [Fact]
    public async Task MapProductsByCategory_GroupsProductsByCategory()
    {
        // Arrange
        var beer = CreateProduct(1, "Beer");
        var beer2 = CreateProduct(2 ,"Beer");
        var wine = CreateProduct(3, "Wine");

        await using (var db = _fixture.CreateDbContext())
        {
            await db.Products.AddRangeAsync(beer, beer2, wine);
            await db.SaveChangesAsync();
        }

        // Act
        Dictionary<string, List<Product>> result;
        await using (var db = _fixture.CreateDbContext())
        {
            var service = new ProductService(db);
            result = await service.MapProductsByCategory();
        }

        // Assert
        Assert.Equal(2, result.Count);
        Assert.Equal(2, result["Beer"].Count);
        Assert.Single(result["Wine"]);
    }

    [Fact]
    public async Task MapProductsByCategory_EmptyDatabase_ReturnsEmptyDictionary()
    {
        // Act
        Dictionary<string, List<Product>> result;
        await using (var db = _fixture.CreateDbContext())
        {
            var service = new ProductService(db);
            result = await service.MapProductsByCategory();
        }

        // Assert
        Assert.Empty(result);
    }

    [Fact]
    public async Task MapProductsByCategory_SingleCategory_ReturnsSingleEntry()
    {
        // Arrange
        var p1 = CreateProduct(10, "Soda");
        var p2 = CreateProduct(11, "Soda");

        await using (var db = _fixture.CreateDbContext())
        {
            await db.Products.AddRangeAsync(p1, p2);
            await db.SaveChangesAsync();
        }

        // Act
        Dictionary<string, List<Product>> result;
        await using (var db = _fixture.CreateDbContext())
        {
            var service = new ProductService(db);
            result = await service.MapProductsByCategory();
        }

        // Assert
        Assert.Single(result);
        Assert.Equal(2, result["Soda"].Count);
    }

    [Fact]
    public async Task MapProductsByCategory_ExcludesProductsWhereInUseIsFalse()
    {
        // Arrange
        var shown = CreateProduct(20, "Beer");
        var hidden = CreateProduct(21, "Beer");
        hidden.InUse = false;

        await using (var db = _fixture.CreateDbContext())
        {
            await db.Products.AddRangeAsync(shown, hidden);
            await db.SaveChangesAsync();
        }

        // Act
        Dictionary<string, List<Product>> result;
        await using (var db = _fixture.CreateDbContext())
        {
            var service = new ProductService(db);
            result = await service.MapProductsByCategory();
        }

        // Assert
        Assert.Single(result["Beer"]);
        Assert.Equal(20, result["Beer"][0].OctopusId);
    }

    private static Product CreateProduct(int octopusId, string category)
    {
        return new Product
        {
            OctopusId = octopusId,
            WebId = 0,
            WebTitle = "empty",
            PdfTitle = "empty",
            OctopusTitle = "empty",
            Available = 0,
            KegCollar = 0,
            Str = 0.0,
            Alcohol = 0.0,
            PricePrUnit = 0.0,
            Category = category
        };
    }
}