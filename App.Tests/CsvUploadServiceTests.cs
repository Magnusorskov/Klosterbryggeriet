using System.Text;
using BlazorApp.Models;
using BlazorApp.Models.Dtos;
using BlazorApp.Services;
using Microsoft.EntityFrameworkCore;

namespace App.Tests;

public class CsvUploadServiceTests : IClassFixture<DatabaseFixture>, IAsyncLifetime
{
    private readonly DatabaseFixture _fixture;

    public CsvUploadServiceTests(DatabaseFixture fixture)
    {
        _fixture = fixture;
    }

    public Task InitializeAsync() => Task.CompletedTask;

    public async Task DisposeAsync()
    {
        await using var db = _fixture.CreateDbContext();
        db.Products.RemoveRange(db.Products);
        db.DraftBeers.RemoveRange(db.DraftBeers);
        db.LogEntries.RemoveRange(db.LogEntries);
        await db.SaveChangesAsync();
    }

    [Fact]
    public async Task UploadCsvAsync_PushesEligibleProducts()
    {
        await SeedProducts(
            MakeProduct(101, variantId1: 9001, available: -1, inUse: true),
            MakeProduct(102, variantId1: 9002, available: -1, inUse: true),
            MakeProduct(103, variantId1: 0, available: -1, inUse: true));

        var csv = BuildCsv((101, 50), (102, 75), (103, 10));
        var fake = new FakeHostedShopService();

        CsvImportResult result;
        await using (var db = _fixture.CreateDbContext())
        {
            var sut = new CsvUploadService(new OctopusService(_fixture, new LoggerService(_fixture)), fake, _fixture);
            result = await sut.UploadCsvAsync(StreamFor(csv), "test.csv");
        }

        Assert.Equal(3, result.Updated.Count);
        Assert.Equal(2, result.PushAttempted);
        Assert.Equal(2, result.PushSucceeded);
        Assert.Equal(0, result.PushFailed);
        Assert.Empty(result.FailedVariantIds);
        Assert.Equal(2, fake.Calls.Count);
        Assert.Contains(fake.Calls, c => c.variantId == 9001 && c.antal == 50);
        Assert.Contains(fake.Calls, c => c.variantId == 9002 && c.antal == 75);
    }

    [Fact]
    public async Task UploadCsvAsync_RecordsFailures()
    {
        await SeedProducts(
            MakeProduct(201, variantId1: 8001, available: -1, inUse: true),
            MakeProduct(202, variantId1: 8002, available: -1, inUse: true));

        var csv = BuildCsv((201, 5), (202, 6));
        var fake = new FakeHostedShopService { FailingVariantIds = { 8002 } };

        CsvImportResult result;
        await using (var db = _fixture.CreateDbContext())
        {
            var sut = new CsvUploadService(new OctopusService(_fixture, new LoggerService(_fixture)), fake, _fixture);
            result = await sut.UploadCsvAsync(StreamFor(csv), "test.csv");
        }

        Assert.Equal(2, result.PushAttempted);
        Assert.Equal(1, result.PushSucceeded);
        Assert.Equal(1, result.PushFailed);
        Assert.Equal(new[] { 8002 }, result.FailedVariantIds);
    }

    [Fact]
    public async Task UploadCsvAsync_SkipsInUseFalse()
    {
        await SeedProducts(
            MakeProduct(301, variantId1: 7001, available: -1, inUse: false));

        var csv = BuildCsv((301, 42));
        var fake = new FakeHostedShopService();

        CsvImportResult result;
        await using (var db = _fixture.CreateDbContext())
        {
            var sut = new CsvUploadService(new OctopusService(_fixture, new LoggerService(_fixture)), fake, _fixture);
            result = await sut.UploadCsvAsync(StreamFor(csv), "test.csv");
        }

        Assert.Equal(1, result.Updated.Count);
        Assert.Equal(0, result.PushAttempted);
        Assert.Empty(fake.Calls);
    }

    [Fact]
    public async Task UploadCsvAsync_SkipsZeroVariantId()
    {
        await SeedProducts(
            MakeProduct(401, variantId1: 0, available: -1, inUse: true));

        var csv = BuildCsv((401, 99));
        var fake = new FakeHostedShopService();

        CsvImportResult result;
        await using (var db = _fixture.CreateDbContext())
        {
            var sut = new CsvUploadService(new OctopusService(_fixture, new LoggerService(_fixture)), fake, _fixture);
            result = await sut.UploadCsvAsync(StreamFor(csv), "test.csv");
        }

        Assert.Equal(1, result.Updated.Count);
        Assert.Equal(0, result.PushAttempted);
        Assert.Empty(fake.Calls);
    }

    [Fact]
    public async Task UploadCsvAsync_PushesBothVariantsWhenVariantId2Set()
    {
        await SeedProducts(
            MakeProduct(501, variantId1: 6001, variantId2: 6002, available: -1, inUse: true));

        var csv = BuildCsv((501, 12));
        var fake = new FakeHostedShopService();

        CsvImportResult result;
        await using (var db = _fixture.CreateDbContext())
        {
            var sut = new CsvUploadService(new OctopusService(_fixture, new LoggerService(_fixture)), fake, _fixture);
            result = await sut.UploadCsvAsync(StreamFor(csv), "test.csv");
        }

        Assert.Equal(2, result.PushAttempted);
        Assert.Equal(2, result.PushSucceeded);
        Assert.Contains(fake.Calls, c => c.variantId == 6001 && c.antal == 12);
        Assert.Contains(fake.Calls, c => c.variantId == 6002 && c.antal == 12);
    }

    [Fact]
    public async Task UploadCsvAsync_EmptyCsv_NoSoapCalls()
    {
        var fake = new FakeHostedShopService();
        var csv = "header\n";

        CsvImportResult result;
        await using (var db = _fixture.CreateDbContext())
        {
            var sut = new CsvUploadService(new OctopusService(_fixture, new LoggerService(_fixture)), fake, _fixture);
            result = await sut.UploadCsvAsync(StreamFor(csv), "empty.csv");
        }

        Assert.Equal(0, result.Updated.Count);
        Assert.Equal(0, result.PushAttempted);
        Assert.Empty(fake.Calls);
    }

    // Draft beer: a row marked as DraftBeer in the preview commits to
    // DraftBeers (not Products) with Category forced to "FADØL" and the
    // user-supplied Kobling/Land carried through.
    [Fact]
    public async Task CommitPreviewAsync_DraftBeerRow_LandsInDraftBeersTable()
    {
        var preview = new CsvImportPreview
        {
            FileName = "test.csv",
            RowsParsed = 1,
            PendingNewProducts =
            {
                new PendingNewProduct
                {
                    Kind = PendingProductKind.DraftBeer,
                    Kobling = "Key keg",
                    Land = "Belgien",
                    Product = new Product
                    {
                        OctopusId = 8800,
                        WebId = 0,
                        WebTitle = "",
                        PdfTitle = "Westmalle Tripel",
                        OctopusTitle = "Westmalle Tripel",
                        Available = 4,
                        Category = "",
                        InUse = true,
                        Str = 20,
                        Alcohol = 9.5,
                        PricePrUnit = 80,
                    },
                }
            }
        };

        var fake = new FakeHostedShopService();
        CsvImportResult result;
        await using (var db = _fixture.CreateDbContext())
        {
            var sut = new CsvUploadService(new OctopusService(_fixture, new LoggerService(_fixture)), fake, _fixture);
            result = await sut.CommitPreviewAsync(preview);
        }

        Assert.Single(result.Created);
        Assert.Equal(PendingProductKind.DraftBeer, result.Created[0].Kind);

        await using var assertDb = _fixture.CreateDbContext();
        Assert.False(await assertDb.Products.AnyAsync(p => p.OctopusId == 8800));
        var stored = await assertDb.DraftBeers.FindAsync(8800);
        Assert.NotNull(stored);
        Assert.Equal("FADØL", stored.Category);
        Assert.Equal("Key keg", stored.Kobling);
        Assert.Equal("Belgien", stored.Land);
        Assert.Equal(20, stored.Str);
        Assert.Equal(80, stored.PricePrUnit);
        Assert.True(stored.InUse);
    }

    // Draft beer: webshop push covers DraftBeers too. A draft beer with a
    // VariantId1 should fire the same webshop call as a regular product.
    [Fact]
    public async Task CommitPreviewAsync_DraftBeerWithVariant_PushesToWebshop()
    {
        var preview = new CsvImportPreview
        {
            FileName = "test.csv",
            PendingNewProducts =
            {
                new PendingNewProduct
                {
                    Kind = PendingProductKind.DraftBeer,
                    Kobling = "S",
                    Land = "Danmark",
                    Product = new Product
                    {
                        OctopusId = 9001,
                        WebId = 1234,
                        WebTitle = "",
                        PdfTitle = "Klosterbryg Pils",
                        OctopusTitle = "Klosterbryg Pils",
                        Available = 12,
                        Category = "",
                        InUse = true,
                        Str = 20,
                        PricePrUnit = 50,
                        VariantId1 = 4242,
                    },
                }
            }
        };

        var fake = new FakeHostedShopService();
        CsvImportResult result;
        await using (var db = _fixture.CreateDbContext())
        {
            var sut = new CsvUploadService(new OctopusService(_fixture, new LoggerService(_fixture)), fake, _fixture);
            result = await sut.CommitPreviewAsync(preview);
        }

        Assert.Equal(1, result.PushAttempted);
        Assert.Equal(1, result.PushSucceeded);
        Assert.Contains(fake.Calls, c => c.variantId == 4242 && c.antal == 12);
    }

    private async Task SeedProducts(params Product[] products)
    {
        await using var db = _fixture.CreateDbContext();
        await db.Products.AddRangeAsync(products);
        await db.SaveChangesAsync();
    }

    private static Product MakeProduct(int octopusId, int variantId1, int available, bool inUse, int variantId2 = 0)
        => new()
        {
            OctopusId = octopusId,
            WebId = 0,
            WebTitle = "x",
            PdfTitle = "x",
            OctopusTitle = "x",
            Available = available,
            Category = "undefined",
            VariantId1 = variantId1,
            VariantId2 = variantId2,
            InUse = inUse,
        };

    private static string BuildCsv(params (int octopusId, int available)[] rows)
    {
        var sb = new StringBuilder();
        sb.AppendLine("header;a;b;c;d;e;f;g;h;i;j;k;l;m;n;o;p;q;r;s");
        foreach (var (id, available) in rows)
        {
            sb.Append(id);
            for (var i = 1; i < 19; i++) sb.Append(";.");
            sb.Append(';').Append(available).Append('\n');
        }
        return sb.ToString();
    }

    private static MemoryStream StreamFor(string content)
        => new(Encoding.UTF8.GetBytes(content));

    private class FakeHostedShopService : IHostedShopService
    {
        public List<(int variantId, int antal, int lagerId)> Calls { get; } = new();
        public HashSet<int> FailingVariantIds { get; } = new();

        public Task<bool> OpdaterLager(int variantId, int antal, int lagerId = 1)
        {
            Calls.Add((variantId, antal, lagerId));
            return Task.FromResult(!FailingVariantIds.Contains(variantId));
        }
    }
}
