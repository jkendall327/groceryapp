using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using GroceryApp.Backend.Models;
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

        [Fact]
        public async Task GetExpiringItems_ReturnsOk_WithExpiringItems()
        {
            // Arrange
            var userId = "test-user";
            var expiringItems = new List<ProductInfo>
            {
                new ProductInfo { Id = "1", Name = "Milk", ExpiryDate = DateTime.UtcNow.AddDays(2) },
                new ProductInfo { Id = "2", Name = "Bread", ExpiryDate = DateTime.UtcNow.AddDays(1) }
            };

            _cosmosService.GetExpiringItemsAsync(userId, Arg.Any<DateTime>()).Returns(expiringItems);

            // Act
            var response = await _client.GetAsync($"/api/receipts/{userId}/expiring-items");

            // Assert
            response.EnsureSuccessStatusCode();
            var responseData = await response.Content.ReadAsAsync<List<ProductInfo>>();
            Assert.Equal(2, responseData.Count);
            Assert.Contains(responseData, item => item.Name == "Milk");
            Assert.Contains(responseData, item => item.Name == "Bread");
        }

        [Fact]
        public async Task GetExpiringItems_ReturnsNoContent_WhenNoExpiringItems()
        {
            // Arrange
            var userId = "test-user";
            var expiringItems = new List<ProductInfo>();

            _cosmosService.GetExpiringItemsAsync(userId, Arg.Any<DateTime>()).Returns(expiringItems);

            // Act
            var response = await _client.GetAsync($"/api/receipts/{userId}/expiring-items");

            // Assert
            Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
        }

        [Fact]
        public async Task GetExpiringItems_ReturnsBadRequest_WhenUserIdIsInvalid()
        {
            // Arrange
            string invalidUserId = "";
            // Optionally, set up the service to handle invalid user IDs if applicable

            // Act
            var response = await _client.GetAsync($"/api/receipts/{invalidUserId}/expiring-items");

            // Assert
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact]
        public async Task GetExpiringItems_ThrowsException_WhenServiceFails()
        {
            // Arrange
            var userId = "test-user";
            _cosmosService.GetExpiringItemsAsync(userId, Arg.Any<DateTime>()).Throws(new Exception("Service failure"));

            // Act
            var response = await _client.GetAsync($"/api/receipts/{userId}/expiring-items");

            // Assert
            Assert.Equal(HttpStatusCode.InternalServerError, response.StatusCode);
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
