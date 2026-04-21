using System.ServiceModel;
using WebService;
using DotNetEnv;

namespace BlazorApp.Services
{
    public class HostedShopService
    {
        private readonly IConfiguration _config;

        public HostedShopService(IConfiguration config)
        {
            _config = config;
        }

        public async Task<bool> OpdaterLager(int variantId, int lagerId, int antal)
        {
            try 
            {

                //Env.Load();

                //string apiUser = Env.GetString("SHOP_API_USER");
                //string apiPass = Env.GetString("SHOP_API_PASS");
                string apiUser = "klosterapi7455@klosterapi.dk";
                string apiPass = "Klosterapi2026";

                if (string.IsNullOrEmpty(apiUser))
                {
                    throw new Exception("Fejl: SHOP_API_USER mangler i .env filen!");
                }

                
                var client = new WebServicePortClient();
                if (client.Endpoint.Binding is HttpBindingBase httpBinding)
                {
                    httpBinding.AllowCookies = true;
                }

                
                await client.Solution_ConnectAsync(apiUser, apiPass);
                await client.Solution_SetLanguageAsync("DK");

                var response = await client.Product_UpdateVariantStockForStockLocationAsync(variantId, lagerId, antal);

                return response != null;
            }
            catch (Exception ex)
            {
                
                Console.WriteLine($"API FEJL: {ex.Message}");
                return false;
            }
        }
    }
}
