using System.Text;
using BlazorApp.Models;
using BlazorApp.Models.Dtos;
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
        db.LogEntries.RemoveRange(db.LogEntries);
        db.Products.RemoveRange(db.Products);
        db.DraftBeers.RemoveRange(db.DraftBeers);
        await db.SaveChangesAsync();
    }

    [Fact]
    public async Task UpdateAvailableFromOctopusCsv_Default()
    {
        // Arrange
        var product = CreateSeedProduct(14500, available: -1);

        await using (var db = _fixture.CreateDbContext())
        {
            await db.Products.AddAsync(product);
            await db.SaveChangesAsync();
        }

        var testFile = GetFileFromPath("TestData/OctopusTestData.csv");
        await using (var db = _fixture.CreateDbContext())
        {
            var service = new OctopusService(_fixture, new LoggerService(_fixture));
            await service.UpdateAvailableFromOctopusCsv(testFile);
        }

        await using (var db = _fixture.CreateDbContext())
        {
            var saved = await db.Products.FindAsync(14500);
            Assert.NotNull(saved);
            Assert.Equal(-500, saved.Available);
        }
    }

    [Fact]
    public async Task UpdateAvailableFromOctopusCsv_ExcludedProductNotChanged()
    {
        // Arrange
        var product = CreateSeedProduct(100, available: -1);

        await using (var db = _fixture.CreateDbContext())
        {
            await db.Products.AddAsync(product);
            await db.SaveChangesAsync();
        }

        var testFile = GetFileFromPath("TestData/OctopusTestData.csv");
        await using (var db = _fixture.CreateDbContext())
        {
            var service = new OctopusService(_fixture, new LoggerService(_fixture));
            await service.UpdateAvailableFromOctopusCsv(testFile);
        }

        await using (var db = _fixture.CreateDbContext())
        {
            var saved = await db.Products.FindAsync(100);
            Assert.NotNull(saved);
            Assert.Equal(-1, saved.Available);
        }
    }

    // US 7.1: Unknown OctopusId in CSV creates a new product with InUse=false.
    [Fact]
    public async Task UploadCsv_UnknownOctopusId_CreatesProductWithInUseFalse()
    {
        var testFile = GetFileFromPath("TestData/OctopusTestData.csv");
        await using (var db = _fixture.CreateDbContext())
        {
            var uploader = new CsvUploadService(new OctopusService(_fixture, new LoggerService(_fixture)), new NoOpHostedShopService(), _fixture);
            await uploader.UploadCsvAsync(testFile, "octopus.csv");
        }

        await using (var db = _fixture.CreateDbContext())
        {
            var saved = await db.Products.FindAsync(14500);
            Assert.NotNull(saved);
            Assert.False(saved.InUse);
            Assert.Equal("Pallegavekassen i træ", saved.OctopusTitle);
            Assert.Equal(-500, saved.Available);
            Assert.Equal("", saved.WebTitle);
            Assert.Equal("", saved.PdfTitle);
            Assert.Equal("", saved.Category);
            Assert.Equal(0, saved.WebId);
        }
    }

    // US 7.1: Creation must be logged via LoggerService.
    [Fact]
    public async Task UploadCsv_UnknownOctopusId_LogsCreation()
    {
        var testFile = GetFileFromPath("TestData/OctopusTestData.csv");
        await using (var db = _fixture.CreateDbContext())
        {
            var uploader = new CsvUploadService(new OctopusService(_fixture, new LoggerService(_fixture)), new NoOpHostedShopService(), _fixture);
            await uploader.UploadCsvAsync(testFile, "octopus.csv");
        }

        await using (var db = _fixture.CreateDbContext())
        {
            var entries = db.LogEntries
                .Where(e => e.Kind == LogEntryKind.ProductCreated && e.OctopusId == 14500)
                .ToList();
            Assert.Single(entries);
            Assert.Equal("Pallegavekassen i træ", entries[0].ProductName);
        }
    }

    // CsvImportResult shape: updated + created populated side-by-side.
    [Fact]
    public async Task UploadCsv_ReturnsResultWithUpdatedAndCreated()
    {
        var existing = CreateSeedProduct(100, available: 500);

        await using (var db = _fixture.CreateDbContext())
        {
            await db.Products.AddAsync(existing);
            await db.SaveChangesAsync();
        }

        var csv = BuildCsv(
            (100, "Eksisterende øl", 120),
            (99001, "Ny specialøl", 42));

        BlazorApp.Models.Dtos.CsvImportResult result;
        await using (var db = _fixture.CreateDbContext())
        {
            var uploader = new CsvUploadService(new OctopusService(_fixture, new LoggerService(_fixture)), new NoOpHostedShopService(), _fixture);
            result = await uploader.UploadCsvAsync(csv, "test.csv");
        }

        Assert.Single(result.Updated);
        Assert.Single(result.Created);
        Assert.Equal(100, result.Updated[0].OctopusId);
        Assert.Equal(500, result.Updated[0].PreviousAvailable);
        Assert.Equal(120, result.Updated[0].NewAvailable);
        Assert.Equal(99001, result.Created[0].OctopusId);
        Assert.Equal("Ny specialøl", result.Created[0].OctopusTitle);
    }

    // CsvImportResult shape: status flip detected and logged.
    [Fact]
    public async Task UploadCsv_ExistingProductStatusFlip_LogsStatusChange()
    {
        var existing = CreateSeedProduct(14500, available: 500);

        await using (var db = _fixture.CreateDbContext())
        {
            await db.Products.AddAsync(existing);
            await db.SaveChangesAsync();
        }

        var testFile = GetFileFromPath("TestData/OctopusTestData.csv");
        BlazorApp.Models.Dtos.CsvImportResult result;
        await using (var db = _fixture.CreateDbContext())
        {
            var uploader = new CsvUploadService(new OctopusService(_fixture, new LoggerService(_fixture)), new NoOpHostedShopService(), _fixture);
            result = await uploader.UploadCsvAsync(testFile, "octopus.csv");
        }

        Assert.Single(result.Updated);
        Assert.True(result.Updated[0].StatusFlipped);
        Assert.Equal(ProductStatus.Available, result.Updated[0].PreviousStatus);
        Assert.Equal(ProductStatus.SoldOut, result.Updated[0].NewStatus);

        await using (var db = _fixture.CreateDbContext())
        {
            var entries = db.LogEntries
                .Where(e => e.Kind == LogEntryKind.StatusChanged && e.OctopusId == 14500)
                .ToList();
            Assert.Single(entries);
        }
    }

    // Phase 7: defaults for newly created products match placeholder contract.
    [Fact]
    public async Task CreateMissingProducts_SetsExpectedDefaults()
    {
        var row = new OctopusService.OctopusCsvRow(99001, "Ny øl", 42);

        await using (var db = _fixture.CreateDbContext())
        {
            var service = new OctopusService(_fixture, new LoggerService(_fixture));
            await service.CreateMissingProducts([row]);
        }

        await using (var db = _fixture.CreateDbContext())
        {
            var saved = await db.Products.FindAsync(99001);
            Assert.NotNull(saved);
            Assert.False(saved.InUse);
            Assert.Equal("Ny øl", saved.OctopusTitle);
            Assert.Equal(42, saved.Available);
            Assert.Equal("", saved.WebTitle);
            Assert.Equal("", saved.PdfTitle);
            Assert.Equal("", saved.Category);
            Assert.Equal(0, saved.WebId);
            Assert.Equal(0, saved.Str);
            Assert.Equal(0, saved.Alcohol);
            Assert.Equal(0, saved.PricePrUnit);
            Assert.Equal(0, saved.VariantId1);
            Assert.Equal(0, saved.VariantId2);
        }
    }

    // ParseCsv: duplicate OctopusId keeps the last row and emits a warning.
    [Fact]
    public async Task ParseCsv_DuplicateOctopusId_KeepsLastAndWarns()
    {
        var csv = BuildCsv(
            (42, "Første", 10),
            (42, "Sidste", 99));

        List<OctopusService.OctopusCsvRow> rows;
        List<string> warnings;
        await using (var db = _fixture.CreateDbContext())
        {
            var service = new OctopusService(_fixture, new LoggerService(_fixture));
            (rows, _, warnings) = await service.ParseCsv(csv);
        }

        Assert.Single(rows);
        Assert.Equal(99, rows[0].Available);
        Assert.Equal("Sidste", rows[0].OctopusTitle);
        Assert.Single(warnings);
        Assert.Contains("Duplicate OctopusId 42", warnings[0]);
    }

    // Draft beer: a DraftBeers row's OctopusId must land in ExistingDraftBeers,
    // not Fresh — otherwise the import flow would try to insert a duplicate.
    [Fact]
    public async Task PartitionByExistence_ClassifiesDraftBeersCorrectly()
    {
        await using (var db = _fixture.CreateDbContext())
        {
            await db.Products.AddAsync(CreateSeedProduct(100, available: 50));
            await db.DraftBeers.AddAsync(CreateSeedDraftBeer(200));
            await db.SaveChangesAsync();
        }

        var rows = new List<OctopusService.OctopusCsvRow>
        {
            new(100, "Eksisterende produkt", 12),
            new(200, "Eksisterende fadøl", 5),
            new(99001, "Helt ny vare", 1),
        };

        var service = new OctopusService(_fixture, new LoggerService(_fixture));
        var partitioned = await service.PartitionByExistence(rows);

        Assert.Single(partitioned.ExistingProducts);
        Assert.Equal(100, partitioned.ExistingProducts[0].OctopusId);
        Assert.Single(partitioned.ExistingDraftBeers);
        Assert.Equal(200, partitioned.ExistingDraftBeers[0].OctopusId);
        Assert.Single(partitioned.Fresh);
        Assert.Equal(99001, partitioned.Fresh[0].OctopusId);
    }

    // Draft beer: Available updates from CSV must hit DraftBeers.Available.
    [Fact]
    public async Task ApplyUpdatesToExistingDraftBeers_UpdatesAvailable()
    {
        await using (var db = _fixture.CreateDbContext())
        {
            await db.DraftBeers.AddAsync(CreateSeedDraftBeer(200, available: 10));
            await db.SaveChangesAsync();
        }

        var rows = new List<OctopusService.OctopusCsvRow> { new(200, "Eksisterende fadøl", 0) };
        var service = new OctopusService(_fixture, new LoggerService(_fixture));
        var changes = await service.ApplyUpdatesToExistingDraftBeers(rows);

        Assert.Single(changes);
        Assert.Equal(PendingProductKind.DraftBeer, changes[0].Kind);
        Assert.Equal(10, changes[0].PreviousAvailable);
        Assert.Equal(0, changes[0].NewAvailable);
        Assert.Equal(ProductStatus.Available, changes[0].PreviousStatus);
        Assert.Equal(ProductStatus.SoldOut, changes[0].NewStatus);

        await using (var db = _fixture.CreateDbContext())
        {
            var saved = await db.DraftBeers.FindAsync(200);
            Assert.NotNull(saved);
            Assert.Equal(0, saved.Available);
        }
    }

    // Draft beer: a status flip on a draft beer should generate a log entry
    // tagged with the [Fadøl] prefix so it's distinguishable in the audit log.
    [Fact]
    public async Task ApplyUpdatesToExistingDraftBeers_StatusFlip_LogsWithFadoelPrefix()
    {
        await using (var db = _fixture.CreateDbContext())
        {
            await db.DraftBeers.AddAsync(CreateSeedDraftBeer(200, available: 5, pdfTitle: "Westmalle Tripel"));
            await db.SaveChangesAsync();
        }

        var rows = new List<OctopusService.OctopusCsvRow> { new(200, "irrelevant", 0) };
        var service = new OctopusService(_fixture, new LoggerService(_fixture));
        await service.ApplyUpdatesToExistingDraftBeers(rows);

        await using (var db = _fixture.CreateDbContext())
        {
            var entries = db.LogEntries
                .Where(e => e.Kind == LogEntryKind.StatusChanged && e.OctopusId == 200)
                .ToList();
            Assert.Single(entries);
            Assert.StartsWith("[Fadøl]", entries[0].ProductName);
            Assert.Equal(ProductStatus.Available, entries[0].PreviousStatus);
            Assert.Equal(ProductStatus.SoldOut, entries[0].NewStatus);
        }
    }

    // Draft beer: CommitNewDraftBeersAsync inserts into DraftBeers (not Products)
    // and returns the created rows tagged with Kind=DraftBeer.
    [Fact]
    public async Task CommitNewDraftBeersAsync_AddsRowsAndLogsCreation()
    {
        var beer = new BlazorApp.Models.DraftBeer
        {
            OctopusId = 555,
            WebId = 0,
            WebTitle = "Westmalle 20L",
            PdfTitle = "Westmalle Tripel",
            OctopusTitle = "Westmalle Tripel",
            Available = 6,
            Str = 20,
            Alcohol = 9.5,
            PricePrUnit = 80,
            Category = "FADØL",
            Kobling = "S",
            Land = "Belgien",
        };

        var service = new OctopusService(_fixture, new LoggerService(_fixture));
        var created = await service.CommitNewDraftBeersAsync([beer]);

        Assert.Single(created);
        Assert.Equal(PendingProductKind.DraftBeer, created[0].Kind);
        Assert.Equal(ProductStatus.Available, created[0].Status);

        await using var db = _fixture.CreateDbContext();
        Assert.True(await db.DraftBeers.AnyAsync(b => b.OctopusId == 555));
        Assert.False(await db.Products.AnyAsync(p => p.OctopusId == 555));

        var logEntry = await db.LogEntries
            .Where(e => e.Kind == LogEntryKind.ProductCreated && e.OctopusId == 555)
            .SingleAsync();
        Assert.StartsWith("[Fadøl]", logEntry.ProductName);
    }

    private static BlazorApp.Models.DraftBeer CreateSeedDraftBeer(
        int octopusId,
        int available = 5,
        string pdfTitle = "Seed Fadøl") => new()
    {
        OctopusId    = octopusId,
        WebId        = 0,
        WebTitle     = "seed",
        PdfTitle     = pdfTitle,
        OctopusTitle = "seed",
        Available    = available,
        Str          = 30,
        Alcohol      = 5.0,
        PricePrUnit  = 30,
        Category     = "FADØL",
        Kobling      = "S",
        Land         = "Danmark",
    };

    private static Product CreateSeedProduct(int octopusId, int available)
    {
        return new Product
        {
            OctopusId = octopusId,
            WebId = 0,
            WebTitle = "seed",
            PdfTitle = "seed",
            OctopusTitle = "seed",
            Available = available,
            KegCollar = 0,
            Str = 0.0,
            Alcohol = 0.0,
            PricePrUnit = 0.0,
            Category = "seed",
            VariantId1 = 0,
            VariantId2 = 0,
        };
    }

    private static MemoryStream BuildCsv(params (int OctopusId, string Title, int Available)[] rows)
    {
        var sb = new StringBuilder();
        sb.AppendLine("Varenr;Varetekst;Tekst 2;Lokation;Leverandør;Beholdning;Prisfaktor;Gennemsnit;Lagerværdi;Genanskaffelse;Lagerværdi;Varegruppe;Undergruppe;Lagergruppe;Sidste Salgsdato;Sidste købsdato;Edbnr;Enhed;Type;Disponibel ;");
        foreach (var (id, title, available) in rows)
        {
            sb.AppendLine($"{id};{title};;;;0;0;0;0;0;0;0;0;0;;;{id};STK ;P;{available}; ");
        }
        return new MemoryStream(Encoding.UTF8.GetBytes(sb.ToString()));
    }

    private FileStream GetFileFromPath(string filePath)
    {
        return File.OpenRead(filePath);
    }

    private class NoOpHostedShopService : IHostedShopService
    {
        public Task<bool> OpdaterLager(int variantId, int antal, int lagerId = 1) => Task.FromResult(true);
    }
}
