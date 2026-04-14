using BlazorApp.Data;
using BlazorApp.Models;
using BlazorApp.Services;
using Microsoft.EntityFrameworkCore;
using Testcontainers.MySql;

namespace App.Tests;

public class OctopusServiceIntegrationTest : IAsyncLifetime
{
    private readonly MySqlContainer _mysql = new MySqlBuilder()
        .WithImage("mysql:8.0")
        .Build();

    private AppDbContext CreateDbContext()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseMySql(_mysql.GetConnectionString(), ServerVersion.AutoDetect(_mysql.GetConnectionString()))
            .Options;

        return new AppDbContext(options);
    }

    public async Task InitializeAsync()
    {
        await _mysql.StartAsync();

        await using var db = CreateDbContext();
        await db.Database.EnsureCreatedAsync();
    }

    public async Task DisposeAsync()
    {
        await _mysql.DisposeAsync();
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
        
        await using (var db = CreateDbContext())
        {
            await db.Products.AddAsync(product);
            await db.SaveChangesAsync();
        }

        var testFile = GetFileFromPath("TestData/OctopusTestData.csv");
        await using (var db = CreateDbContext())
        {
            var service = new OctopusService(db);
            await service.UpdateAvailableFromOctopusCsv(testFile);
        }

        await using (var db = CreateDbContext())
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
        
        await using (var db = CreateDbContext())
        {
            await db.Products.AddAsync(product);
            await db.SaveChangesAsync();
        }

        var testFile = GetFileFromPath("TestData/OctopusTestData.csv");
        await using (var db = CreateDbContext())
        {
            var service = new OctopusService(db);
            await service.UpdateAvailableFromOctopusCsv(testFile);
        }

        await using (var db = CreateDbContext())
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