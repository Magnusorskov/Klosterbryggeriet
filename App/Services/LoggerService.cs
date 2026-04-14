using BlazorApp.Data;
using BlazorApp.Models;

namespace BlazorApp.Services;

public class LoggerService
{
   private readonly AppDbContext _context;
   
   public LoggerService(AppDbContext context)
    {
        _context = context;
    }

    public async Task LogProductChange(
        Product changedProduct,
        string columnName,
        string previousValue,
        string newValue,
        string valueType)
    {
        var entry = new LogEntry()
        {
            ColumnName = columnName,
            PreviousValue = previousValue,
            NewValue = newValue,
            ValueType = valueType,
            OctopusId = changedProduct.OctopusId,
            ProductName = changedProduct.PdfTitle,

        };
        await _context.LogEntries.AddAsync(entry);
        await _context.SaveChangesAsync();
    }
    
}