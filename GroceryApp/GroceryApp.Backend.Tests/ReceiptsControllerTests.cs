using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using GroceryApp.Backend;
using GroceryApp.Backend.Services;
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
        public async Task GetReceipt_ReturnsOk()
        {
            // Arrange
            var receiptId = "valid-receipt-id";
            // Setup the substitute to return a sample ReceiptData when GetReceiptAsync is called
            _cosmosService.GetReceiptAsync(receiptId).Returns(new ReceiptData
            {
                Id = receiptId,
                // Populate other necessary properties
            });

            // Act
            var response = await _client.GetAsync($"/api/receipts/{receiptId}");

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        }

        [Fact]
        public async Task GetReceipt_InvalidId_ReturnsNotFound()
        {
            // Arrange
            var receiptId = "invalid-receipt-id";
            // Setup the substitute to return null when an invalid ID is requested
            _cosmosService.GetReceiptAsync(receiptId).Returns((ReceiptData)null);

            // Act
            var response = await _client.GetAsync($"/api/receipts/{receiptId}");

            // Assert
            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        }

        [Fact]
        public async Task UploadReceipt_ValidFile_ReturnsCreated()
        {
            // Arrange
            var content = new MultipartFormDataContent();
            var fileContent = new ByteArrayContent(new byte[] { /* ... file bytes ... */ });
            fileContent.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("application/pdf");
            content.Add(fileContent, "file", "receipt.pdf");

            // Setup the substitute to handle the upload
            _cosmosService.UploadReceiptAsync(Arg.Any<Stream>(), Arg.Any<string>()).Returns("new-receipt-id");

            // Act
            var response = await _client.PostAsync("/api/receipts/upload", content);

            // Assert
            Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        }

        // 根据需要添加更多测试
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
