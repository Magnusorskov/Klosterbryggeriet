using BlazorApp.Models;

namespace BlazorApp.Services;

public class OctopusService
{
    public List<Vare> OctopusCsvToEntities(string filePath)
    {
        var lines = File.ReadAllLines(filePath);
    
        foreach (var line in lines.Skip(1)) // Skip(1) to skip header row
        {
            Console.WriteLine(line);
            // var columns = line.Split(',');
        }
        return new List<Vare>();
    }
}