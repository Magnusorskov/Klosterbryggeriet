using BlazorApp.Models;
using Microsoft.EntityFrameworkCore;

namespace BlazorApp.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<Product> Products { get; set; }
}