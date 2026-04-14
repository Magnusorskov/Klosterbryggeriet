using System.Globalization;
using BlazorApp.Data;
using BlazorApp.Models;
using Microsoft.EntityFrameworkCore;

namespace BlazorApp.Services;

public class OctopusService
{
    private readonly AppDbContext _db;
    private static readonly CultureInfo DaCulture = new("da-DK");

    public OctopusService(AppDbContext db)
    {
        _db = db;
    }

    /// <summary>
    /// Parses a semicolon-delimited CSV file exported from Octopus and maps each row to a <see cref="Product"/> entity.
    /// </summary>
    /// <param name="fileStream">A readable stream containing the Octopus CSV data.</param>
    /// <returns>A list of <see cref="Product"/> entities parsed from the CSV rows.</returns>
    public List<Product> OctopusCsvToEntities(Stream fileStream)
    {
        using var reader = new StreamReader(fileStream);
        var lines = new List<string>();
        while (!reader.EndOfStream)
            lines.Add(reader.ReadLine()!);

        var vareListe = new List<Product>();
        foreach (var line in lines.Skip(1))
        {
            var columns = line.Split(';');
            if (columns.Length < 20) continue;

            //var vare = new Product
            //{
                //Varenr = columns[0].Trim(),
                //Varetekst = columns[1].Trim(),
                //Tekst2 = columns[2].Trim(),
                //Beholdning = ParseDecimal(columns[5]),
                //Disponibel = ParseDecimal(columns[19])
            //};
            //vareListe.Add(vare);
        }

        return vareListe;
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
                OctopusId = columns[0].Trim(),
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
        public required string OctopusId { get; set; }
        public required int Available { get; set; }
    }

}