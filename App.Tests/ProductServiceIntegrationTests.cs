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

    // Verify UpdateAsync actually persists every manual field + the InUse flip.
    // US 6.2 depends on this path: freshly created CSV products (InUse=false, blank
    // manual fields) must end up fully reviewed after the admin saves in the modal.
    [Fact]
    public async Task UpdateAsync_PersistsManualFieldsAndInUseFlip()
    {
        var seed = new Product
        {
            OctopusId = 99001,
            WebId = 0,
            WebTitle = "",
            PdfTitle = "",
            OctopusTitle = "Ny øl",
            Available = 42,
            KegCollar = null,
            Str = 0.0,
            Alcohol = 0.0,
            PricePrUnit = 0.0,
            Category = "",
            VariantId1 = 0,
            VariantId2 = 0,
            InUse = false,
            HalfKolli = false,
        };

        await using (var db = _fixture.CreateDbContext())
        {
            await db.Products.AddAsync(seed);
            await db.SaveChangesAsync();
        }

        var update = new Product
        {
            OctopusId = 99001,
            WebId = 0,
            WebTitle = "",
            PdfTitle = "Special øl 33cl",
            OctopusTitle = "Ny øl",
            Available = 42,
            KegCollar = 24,
            Str = 0.33,
            Alcohol = 6.5,
            PricePrUnit = 18.75,
            Category = "Specialøl",
            VariantId1 = 0,
            VariantId2 = 0,
            InUse = true,
            HalfKolli = false,
        };

        await using (var db = _fixture.CreateDbContext())
        {
            var service = new ProductService(db);
            await service.UpdateAsync(update);
        }

        await using (var db = _fixture.CreateDbContext())
        {
            var saved = await db.Products.FindAsync(99001);
            Assert.NotNull(saved);
            Assert.True(saved.InUse);
            Assert.Equal("Special øl 33cl", saved.PdfTitle);
            Assert.Equal("Specialøl", saved.Category);
            Assert.Equal(24, saved.KegCollar);
            Assert.Equal(0.33, saved.Str);
            Assert.Equal(6.5, saved.Alcohol);
            Assert.Equal(18.75, saved.PricePrUnit);
        }
    }

    // Toggling InUse alone must persist — PDF visibility hinges on this bit.
    [Fact]
    public async Task UpdateAsync_InUseFlipOnly_Persists()
    {
        var seed = CreateProduct(77, "Beer");
        seed.InUse = false;

        await using (var db = _fixture.CreateDbContext())
        {
            await db.Products.AddAsync(seed);
            await db.SaveChangesAsync();
        }

        Product fetched;
        await using (var db = _fixture.CreateDbContext())
        {
            fetched = (await db.Products.FindAsync(77))!;
        }

        fetched.InUse = true;
        await using (var db = _fixture.CreateDbContext())
        {
            var service = new ProductService(db);
            await service.UpdateAsync(fetched);
        }

        await using (var db = _fixture.CreateDbContext())
        {
            var saved = await db.Products.FindAsync(77);
            Assert.NotNull(saved);
            Assert.True(saved.InUse);
        }
    }

    // Calling UpdateAsync for a non-existent OctopusId is a silent no-op — no row created.
    [Fact]
    public async Task UpdateAsync_UnknownOctopusId_NoOp()
    {
        var phantom = CreateProduct(404, "Ghost");

        await using (var db = _fixture.CreateDbContext())
        {
            var service = new ProductService(db);
            await service.UpdateAsync(phantom);
        }

        await using (var db = _fixture.CreateDbContext())
        {
            var saved = await db.Products.FindAsync(404);
            Assert.Null(saved);
        }
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