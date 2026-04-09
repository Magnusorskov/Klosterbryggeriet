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

            var vare = new Product
            {
                Varenr = columns[0].Trim(),
                Varetekst = columns[1].Trim(),
                Tekst2 = columns[2].Trim(),
                Lokation = columns[3].Trim(),
                Leverandor = columns[4].Trim(),
                Beholdning = ParseDecimal(columns[5]),
                Prisfaktor = ParseDecimal(columns[6]),
                Gennemsnit = ParseDecimal(columns[7]),
                Lagervaerdi = ParseDecimal(columns[8]),
                Genanskaffelse = ParseDecimal(columns[9]),
                LagervaerdiGenanskaffelse = ParseDecimal(columns[10]),
                Varegruppe = columns[11].Trim(),
                Undergruppe = columns[12].Trim(),
                Lagergruppe = columns[13].Trim(),
                SidsteSalgsdato = ParseDate(columns[14]),
                SidsteKobsdato = ParseDate(columns[15]),
                Edbnr = columns[16].Trim(),
                Enhed = columns[17].Trim(),
                Type = columns[18].Trim(),
                Disponibel = ParseDecimal(columns[19])
            };
            vareListe.Add(vare);
        }

        return vareListe;
    }

    private static decimal ParseDecimal(string value)
    {
        var trimmed = value.Trim();
        if (string.IsNullOrEmpty(trimmed)) return 0;
        return decimal.Parse(trimmed, NumberStyles.Number, DaCulture);
    }

    private static DateTime? ParseDate(string value)
    {
        var trimmed = value.Trim();
        if (string.IsNullOrEmpty(trimmed)) return null;
        return DateTime.ParseExact(trimmed, "dd-MM-yyyy", CultureInfo.InvariantCulture);
    }
}