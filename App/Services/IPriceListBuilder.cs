using BlazorApp.Models;

namespace BlazorApp.Services;

public interface IPriceListBuilder
{
    Task<List<ProductCategory>> GetCategoriesAsync();
}
