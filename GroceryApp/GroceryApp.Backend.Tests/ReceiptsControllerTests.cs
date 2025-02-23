using System.Linq;
using System.Net.Http;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using NSubstitute;
using Xunit;

namespace GroceryApp.Backend.Tests
{
    public class ReceiptsControllerTests : IClassFixture<CustomWebApplicationFactory>
    {
        private readonly HttpClient _client;
        private readonly ICosmosService _cosmosService;

        public ReceiptsControllerTests(CustomWebApplicationFactory factory)
        {
            // Create a substitute for ICosmosService
            _cosmosService = Substitute.For<ICosmosService>();

            // Configure the factory to use the substitute
            factory.WithCosmosService(_cosmosService);
            _client = factory.CreateClient();
        }
    }

    // Custom WebApplicationFactory to inject the substitute ICosmosService
    public class CustomWebApplicationFactory : WebApplicationFactory<Program>
    {
        private ICosmosService _cosmosService;

        public CustomWebApplicationFactory WithCosmosService(ICosmosService cosmosService)
        {
            _cosmosService = cosmosService;
            return this;
        }

        protected override void ConfigureWebHost(IWebHostBuilder builder)
        {
            builder.ConfigureServices(services =>
            {
                // Remove the existing ICosmosService registration
                var descriptor = services.SingleOrDefault(
                    d => d.ServiceType == typeof(ICosmosService));
                if (descriptor != null)
                {
                    services.Remove(descriptor);
                }

                // Register the substitute ICosmosService
                services.AddScoped(_ => _cosmosService);
            });
        }
    }
}
