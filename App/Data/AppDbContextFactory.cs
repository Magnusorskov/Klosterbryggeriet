using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace BlazorApp.Data;

// Used by `dotnet ef migrations add` at design time — no live DB needed
public class AppDbContextFactory : IDesignTimeDbContextFactory<AppDbContext>
{
    public AppDbContext CreateDbContext(string[] args)
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseMySql(
                "Server=localhost;Port=3308;Database=klosterbryggeriet;User=root;Password=rootpassword;CharSet=utf8mb4;",
                new MySqlServerVersion(new Version(8, 0, 0)),
                mySqlOptions => mySqlOptions.EnableRetryOnFailure(
                    maxRetryCount: 5,
                    maxRetryDelay: TimeSpan.FromSeconds(5),
                    errorNumbersToAdd: null))
            .Options;

        return new AppDbContext(options);
    }
}