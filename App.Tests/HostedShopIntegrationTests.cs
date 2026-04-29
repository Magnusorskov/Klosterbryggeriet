using Xunit;
using BlazorApp.Services;
using System.Threading.Tasks;
using DotNetEnv;

namespace App.Tests
{
    public class HostedShopIntegrationTests
    {
        [Fact]
        public async Task Test_ApiConnection_ShouldReturnTrue()
        {
            //Opsæt servicen
           
            var service = new HostedShopService(null);

            // Her tester vi med et korrekt ID bare for at se om der er forbindelse
            bool result = await service.OpdaterLager(1766, 2, 1);

            // 3. Assert
            // Hvis login virker, men ID er forkert, returnerer API'et ofte 'null' eller false.
            // Men vi tjekker her om vi overhovedet får svar uden en Exception.

            //sat til false for at passe
            Assert.True(result); // Forventer true pga. korrekt ID
        }
        [Fact]
        public async Task Test_OpdaterToVarianter_BeggeSkalLykkedes()
        {
            var service = new HostedShopService(null);

            // Indsæt dine to rigtige ID'er her
            int variantId1 = 1225;
            int variantId2 = 1226;

            int testLagerAntal = 7;

            bool resultat1 = await service.OpdaterLager(variantId1, testLagerAntal);
            bool resultat2 = await service.OpdaterLager(variantId2, testLagerAntal);

            Assert.True(resultat1, $"Fejl ved første variant ({variantId1})");
            Assert.True(resultat2, $"Fejl ved anden variant ({variantId2})");
        }
    }
}
