using BlazorApp.Models;

namespace BlazorApp.Services;

public interface IProductService
{
    Task<List<ProductCategory>> GetCategoriesAsync();
}
