using System.Text;
using BlazorApp.Models;
using BlazorApp.Services;

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
            var service = new OctopusService(db, new LoggerService(db));
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
            var service = new OctopusService(db, new LoggerService(db));
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
            var uploader = new CsvUploadService(new OctopusService(db, new LoggerService(db)));
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
            var uploader = new CsvUploadService(new OctopusService(db, new LoggerService(db)));
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
            var uploader = new CsvUploadService(new OctopusService(db, new LoggerService(db)));
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
            var uploader = new CsvUploadService(new OctopusService(db, new LoggerService(db)));
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
            var service = new OctopusService(db, new LoggerService(db));
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
            var service = new OctopusService(db, new LoggerService(db));
            (rows, _, warnings) = await service.ParseCsv(csv);
        }

        Assert.Single(rows);
        Assert.Equal(99, rows[0].Available);
        Assert.Equal("Sidste", rows[0].OctopusTitle);
        Assert.Single(warnings);
        Assert.Contains("Duplicate OctopusId 42", warnings[0]);
    }

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
}
