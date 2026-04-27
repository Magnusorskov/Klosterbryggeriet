namespace BlazorApp.Services;

public interface IHostedShopService
{
    Task<bool> OpdaterLager(int variantId, int antal, int lagerId = 1);
}
