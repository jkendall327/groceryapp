using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using GroceryApp.Backend;
using Microsoft.AspNetCore.Mvc.Testing;
using Xunit;

namespace GroceryApp.Backend.Tests
{
    public class ReceiptsControllerTests : IClassFixture<WebApplicationFactory<Program>>
    {
        private readonly HttpClient _client;

        public ReceiptsControllerTests(WebApplicationFactory<Program> factory)
        {
            _client = factory.CreateClient();
        }

        [Fact]
        public async Task GetReceipt_ReturnsOk()
        {
            // Arrange
            var receiptId = "valid-receipt-id";

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

            // Act
            var response = await _client.PostAsync("/api/receipts/upload", content);

            // Assert
            Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        }

        // 根据需要添加更多测试
    }
}
