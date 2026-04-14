using System.Globalization;
using BlazorApp.Models;

namespace BlazorApp.Services;

public class OctopusService
{
    private static readonly CultureInfo DaCulture = new("da-DK");

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

    private static decimal ParseDecimal(string value)
    {
        var trimmed = value.Trim();
        if (string.IsNullOrEmpty(trimmed)) return 0;
        return decimal.Parse(trimmed, NumberStyles.Number, DaCulture);
    }

}