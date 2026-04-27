using BlazorApp.Data;
using BlazorApp.Models;
using Microsoft.EntityFrameworkCore;

namespace BlazorApp.Services;

public class LoggerService
{
    private readonly IDbContextFactory<AppDbContext> _contextFactory;

    public LoggerService(IDbContextFactory<AppDbContext> contextFactory)
    {
        _contextFactory = contextFactory;
    }

    public async Task LogProductChange(
        Product changedProduct,
        ProductStatus previousStatus,
        ProductStatus newStatus)
    {
        await using var context = _contextFactory.CreateDbContext();
        var entry = new LogEntry
        {
            OctopusId = changedProduct.OctopusId,
            ProductName = changedProduct.PdfTitle,
            Kind = LogEntryKind.StatusChanged,
            PreviousStatus = previousStatus,
            NewStatus = newStatus,
        };
        await context.LogEntries.AddAsync(entry);
        await context.SaveChangesAsync();
    }

    public async Task LogProductCreated(Product created)
    {
        await using var context = _contextFactory.CreateDbContext();
        var entry = new LogEntry
        {
            OctopusId = created.OctopusId,
            ProductName = created.OctopusTitle,
            Kind = LogEntryKind.ProductCreated,
            PreviousStatus = null,
            NewStatus = created.Status,
        };
        await context.LogEntries.AddAsync(entry);
        await context.SaveChangesAsync();
    }

    public async Task<List<LogEntry>> GetLogEntries()
    {
        await using var context = _contextFactory.CreateDbContext();
        return await context.LogEntries.ToListAsync();
    }
}
