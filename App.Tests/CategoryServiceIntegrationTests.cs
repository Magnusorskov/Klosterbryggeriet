using BlazorApp.Models;
using BlazorApp.Services;
using Microsoft.EntityFrameworkCore;

namespace App.Tests;

public class CategoryServiceIntegrationTest : IClassFixture<DatabaseFixture>, IAsyncLifetime
{
    private readonly DatabaseFixture _fixture;

    public CategoryServiceIntegrationTest(DatabaseFixture fixture)
    {
        _fixture = fixture;
    }

    public Task InitializeAsync() => Task.CompletedTask;

    public async Task DisposeAsync()
    {
        await using var db = _fixture.CreateDbContext();
        db.CategoryColumns.RemoveRange(db.CategoryColumns);
        db.Categories.RemoveRange(db.Categories);
        await db.SaveChangesAsync();
    }

    [Fact]
    public async Task GetAllAsync_ReturnsCategoriesOrderedBySortOrder()
    {
        // Arrange
        await using (var db = _fixture.CreateDbContext())
        {
            await db.Categories.AddRangeAsync(
                CreateCategory("Wine", sortOrder: 2),
                CreateCategory("Beer", sortOrder: 1),
                CreateCategory("Soda", sortOrder: 3));
            await db.SaveChangesAsync();
        }

        // Act
        List<Category> result;
        await using (var db = _fixture.CreateDbContext())
        {
            var service = new CategoryService(db);
            result = await service.GetAllAsync();
        }

        // Assert
        Assert.Equal(3, result.Count);
        Assert.Equal("Beer", result[0].Name);
        Assert.Equal("Wine", result[1].Name);
        Assert.Equal("Soda", result[2].Name);
    }

    [Fact]
    public async Task GetAllAsync_EmptyDatabase_ReturnsEmptyList()
    {
        // Act
        List<Category> result;
        await using (var db = _fixture.CreateDbContext())
        {
            var service = new CategoryService(db);
            result = await service.GetAllAsync();
        }

        // Assert
        Assert.Empty(result);
    }

    [Fact]
    public async Task GetAllAsync_IncludesColumnsOrderedBySortOrder()
    {
        // Arrange
        var category = CreateCategory("Beer", sortOrder: 1);
        category.Columns =
        [
            CreateColumn("Alcohol", "Alkohol%", sortOrder: 2),
            CreateColumn("Str", "Str.", sortOrder: 1),
        ];

        await using (var db = _fixture.CreateDbContext())
        {
            await db.Categories.AddAsync(category);
            await db.SaveChangesAsync();
        }

        // Act
        List<Category> result;
        await using (var db = _fixture.CreateDbContext())
        {
            var service = new CategoryService(db);
            result = await service.GetAllAsync();
        }

        // Assert
        var columns = result.Single().Columns.ToList();
        Assert.Equal(2, columns.Count);
        Assert.Equal("Str", columns[0].FieldName);
        Assert.Equal("Alcohol", columns[1].FieldName);
    }

    [Fact]
    public async Task GetByIdAsync_ReturnsCategoryWithColumns()
    {
        // Arrange
        int id;
        var category = CreateCategory("Beer", sortOrder: 1);
        category.Columns = [CreateColumn("Alcohol", "Alkohol%", sortOrder: 1)];

        await using (var db = _fixture.CreateDbContext())
        {
            await db.Categories.AddAsync(category);
            await db.SaveChangesAsync();
            id = category.Id;
        }

        // Act
        Category? result;
        await using (var db = _fixture.CreateDbContext())
        {
            var service = new CategoryService(db);
            result = await service.GetByIdAsync(id);
        }

        // Assert
        Assert.NotNull(result);
        Assert.Equal("Beer", result!.Name);
        Assert.Single(result.Columns);
        Assert.Equal("Alcohol", result.Columns.First().FieldName);
    }

    [Fact]
    public async Task GetByIdAsync_NotFound_ReturnsNull()
    {
        // Act
        Category? result;
        await using (var db = _fixture.CreateDbContext())
        {
            var service = new CategoryService(db);
            result = await service.GetByIdAsync(9999);
        }

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public async Task CreateAsync_FirstCategory_AssignsSortOrderOne()
    {
        // Arrange
        var category = CreateCategory("Beer");

        // Act
        await using (var db = _fixture.CreateDbContext())
        {
            var service = new CategoryService(db);
            await service.CreateAsync(category);
        }

        // Assert
        await using (var db = _fixture.CreateDbContext())
        {
            var stored = await db.Categories.SingleAsync();
            Assert.Equal(1, stored.SortOrder);
            Assert.Equal("Beer", stored.Name);
        }
    }

    [Fact]
    public async Task CreateAsync_AssignsSortOrderAtBottom()
    {
        // Arrange
        await using (var db = _fixture.CreateDbContext())
        {
            await db.Categories.AddRangeAsync(
                CreateCategory("Beer", sortOrder: 1),
                CreateCategory("Wine", sortOrder: 5));
            await db.SaveChangesAsync();
        }

        var newCategory = CreateCategory("Soda");

        // Act
        await using (var db = _fixture.CreateDbContext())
        {
            var service = new CategoryService(db);
            await service.CreateAsync(newCategory);
        }

        // Assert
        Assert.Equal(6, newCategory.SortOrder);
    }

    [Fact]
    public async Task UpdateAsync_UpdatesFieldsAndReplacesColumns()
    {
        // Arrange
        int id;
        var category = CreateCategory("Beer", sortOrder: 1);
        category.PriceLabel = "pr. flaske";
        category.Columns = [CreateColumn("Alcohol", "Alkohol%", sortOrder: 1)];

        await using (var db = _fixture.CreateDbContext())
        {
            await db.Categories.AddAsync(category);
            await db.SaveChangesAsync();
            id = category.Id;
        }

        var update = new Category
        {
            Id = id,
            Name = "Craft Beer",
            PriceLabel = "pr. enhed",
            Columns =
            [
                CreateColumn("Str", "Str.", sortOrder: 1),
                CreateColumn("KegCollar", "Kasse/Kolli", sortOrder: 2),
            ]
        };

        // Act
        await using (var db = _fixture.CreateDbContext())
        {
            var service = new CategoryService(db);
            await service.UpdateAsync(update);
        }

        // Assert
        await using (var db = _fixture.CreateDbContext())
        {
            var stored = await db.Categories
                .Include(c => c.Columns.OrderBy(col => col.SortOrder))
                .SingleAsync(c => c.Id == id);

            Assert.Equal("Craft Beer", stored.Name);
            Assert.Equal("pr. enhed", stored.PriceLabel);
            Assert.Equal(2, stored.Columns.Count);
            Assert.Equal("Str", stored.Columns.ElementAt(0).FieldName);
            Assert.Equal("KegCollar", stored.Columns.ElementAt(1).FieldName);
        }
    }

    [Fact]
    public async Task UpdateAsync_CategoryNotFound_Throws()
    {
        // Arrange
        var missing = new Category
        {
            Id = 9999,
            Name = "Nope",
            PriceLabel = "pr. enhed"
        };

        // Act & Assert
        await using var db = _fixture.CreateDbContext();
        var service = new CategoryService(db);
        await Assert.ThrowsAsync<InvalidOperationException>(() => service.UpdateAsync(missing));
    }

    [Fact]
    public async Task SaveOrderAsync_PersistsNewOrder()
    {
        // Arrange
        var beer = CreateCategory("Beer", sortOrder: 1);
        var wine = CreateCategory("Wine", sortOrder: 2);
        var soda = CreateCategory("Soda", sortOrder: 3);

        await using (var db = _fixture.CreateDbContext())
        {
            await db.Categories.AddRangeAsync(beer, wine, soda);
            await db.SaveChangesAsync();
        }

        // Act
        await using (var db = _fixture.CreateDbContext())
        {
            var service = new CategoryService(db);
            await service.SaveOrderAsync([soda.Id, beer.Id, wine.Id]);
        }

        // Assert
        await using (var db = _fixture.CreateDbContext())
        {
            var stored = await db.Categories.OrderBy(c => c.SortOrder).ToListAsync();
            Assert.Equal("Soda", stored[0].Name);
            Assert.Equal("Beer", stored[1].Name);
            Assert.Equal("Wine", stored[2].Name);
        }
    }

    [Fact]
    public async Task SaveOrderAsync_IgnoresUnknownIds()
    {
        // Arrange
        var beer = CreateCategory("Beer", sortOrder: 1);
        var wine = CreateCategory("Wine", sortOrder: 2);

        await using (var db = _fixture.CreateDbContext())
        {
            await db.Categories.AddRangeAsync(beer, wine);
            await db.SaveChangesAsync();
        }

        // Act
        await using (var db = _fixture.CreateDbContext())
        {
            var service = new CategoryService(db);
            await service.SaveOrderAsync([wine.Id, 9999, beer.Id]);
        }

        // Assert
        await using (var db = _fixture.CreateDbContext())
        {
            var stored = await db.Categories.OrderBy(c => c.SortOrder).ToListAsync();
            Assert.Equal("Wine", stored[0].Name);
            Assert.Equal(1, stored[0].SortOrder);
            Assert.Equal("Beer", stored[1].Name);
            Assert.Equal(3, stored[1].SortOrder);
        }
    }

    [Fact]
    public async Task DeleteAsync_RemovesCategory()
    {
        // Arrange
        int id;
        var category = CreateCategory("Beer", sortOrder: 1);

        await using (var db = _fixture.CreateDbContext())
        {
            await db.Categories.AddAsync(category);
            await db.SaveChangesAsync();
            id = category.Id;
        }

        // Act
        await using (var db = _fixture.CreateDbContext())
        {
            var service = new CategoryService(db);
            await service.DeleteAsync(id);
        }

        // Assert
        await using (var db = _fixture.CreateDbContext())
        {
            Assert.False(await db.Categories.AnyAsync(c => c.Id == id));
        }
    }

    [Fact]
    public async Task DeleteAsync_CategoryNotFound_Throws()
    {
        // Act & Assert
        await using var db = _fixture.CreateDbContext();
        var service = new CategoryService(db);
        await Assert.ThrowsAsync<InvalidOperationException>(() => service.DeleteAsync(9999));
    }

    private static Category CreateCategory(string name, int sortOrder = 0) => new()
    {
        Name = name,
        PriceLabel = "pr. enhed",
        SortOrder = sortOrder
    };

    private static CategoryColumn CreateColumn(string fieldName, string displayLabel, int sortOrder) => new()
    {
        FieldName = fieldName,
        DisplayLabel = displayLabel,
        SortOrder = sortOrder
    };
}