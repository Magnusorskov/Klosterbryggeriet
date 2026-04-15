using BlazorApp.Data;
using BlazorApp.Models;
using Microsoft.EntityFrameworkCore;

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
        if (previousValue != newValue)
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

    public async Task<List<LogEntry>> GetLogEntries()
    {
        return await _context.LogEntries.ToListAsync();
    }

    // TODO: Implementer rollback
    public async Task RollBack()
    {
        return;
    }
    
}