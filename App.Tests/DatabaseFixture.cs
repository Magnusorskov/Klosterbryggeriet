using BlazorApp.Data;
using Microsoft.EntityFrameworkCore;
using Testcontainers.MySql;

namespace App.Tests;

public class DatabaseFixture : IAsyncLifetime
{
    private readonly MySqlContainer _mysql = new MySqlBuilder()
        .WithImage("mysql:8.0")
        .Build();

    public AppDbContext CreateDbContext()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseMySql(_mysql.GetConnectionString(), ServerVersion.AutoDetect(_mysql.GetConnectionString()))
            .Options;

        return new AppDbContext(options);
    }

    public async Task InitializeAsync()
    {
        await _mysql.StartAsync();

        await using var db = CreateDbContext();
        await db.Database.EnsureCreatedAsync();
    }

    public async Task DisposeAsync()
    {
        await _mysql.DisposeAsync();
    }
}