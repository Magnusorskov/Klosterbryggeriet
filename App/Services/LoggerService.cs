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
        ProductStatus previousStatus,
        ProductStatus newStatus)
    {
        var entry = new LogEntry
        {
            OctopusId = changedProduct.OctopusId,
            ProductName = changedProduct.PdfTitle,
            PreviousStatus = previousStatus,
            NewStatus = newStatus,
        };
        await _context.LogEntries.AddAsync(entry);
        await _context.SaveChangesAsync();
    }

    public async Task<List<LogEntry>> GetLogEntries()
    {
        return await _context.LogEntries.ToListAsync();
    }
}
