using System.Globalization;
using BlazorApp.Data;
using BlazorApp.Models;
using Microsoft.EntityFrameworkCore;

namespace BlazorApp.Services;

public class OctopusService
{
    private readonly AppDbContext _db;
    private readonly LoggerService _logger;
    private static readonly CultureInfo DaCulture = new("da-DK");

    public OctopusService(AppDbContext db,  LoggerService logger)
    {
        _db = db;
        _logger = logger;
    }

    public OctopusService(AppDbContext db)
    {
        _db = db;
        _logger = new LoggerService(db);
    }

    public async Task UpdateAvailableFromOctopusCsv(Stream fileStream)
    {
        using var reader = new StreamReader(fileStream);
        var content = await reader.ReadToEndAsync();
        var lines = content.Split('\n', StringSplitOptions.RemoveEmptyEntries);

        var pairList = new List<OctopusIdAvailablePair>();
        foreach (var line in lines.Skip(1))
        {
            var columns = line.Split(';');
            if (columns.Length < 20) continue;

            var pair = new OctopusIdAvailablePair
            {
                OctopusId = int.Parse(columns[0]),
                Available = int.Parse(columns[19])
            };

            pairList.Add(pair);
        }

        var ids = pairList.Select(pair => pair.OctopusId).ToList();
        var dbProducts = await _db.Products
            .Where(p => ids.Contains(p.OctopusId))
            .ToListAsync();

        foreach (var dbProduct in dbProducts)
        {
            var pairItem = pairList.First(p => p.OctopusId == dbProduct.OctopusId);
            await _logger.LogProductChange
                (
                    dbProduct,
                    "Available",
                    dbProduct.Available.ToString(),
                    pairItem.Available.ToString(), 
                    "int");
            
            dbProduct.Available = pairItem.Available;
        }

        await _db.SaveChangesAsync();
    }

    private static decimal ParseDecimal(string value)
    {
        var trimmed = value.Trim();
        if (string.IsNullOrEmpty(trimmed)) return 0;
        return decimal.Parse(trimmed, NumberStyles.Number, DaCulture);
    }
    
    // Helper class for storing OctopusId and its availability as a pair for quicker processing
    private class OctopusIdAvailablePair
    {
        public required int OctopusId { get; set; }
        public required int Available { get; set; }
    }

}