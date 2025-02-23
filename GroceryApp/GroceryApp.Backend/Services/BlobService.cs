using Azure.Storage.Blobs;
using Microsoft.Extensions.Logging;

namespace GroceryApp.Backend;

public class BlobService : IBlobService
{
    private readonly BlobServiceClient _blobServiceClient;
    private readonly string _containerName = "receipts";
    private readonly ILogger<BlobService> _logger;

    public BlobService(BlobServiceClient blobServiceClient, ILogger<BlobService> logger)
    {
        _blobServiceClient = blobServiceClient;
        _logger = logger;
        InitializeContainer();
    }

    private void InitializeContainer()
    {
        _logger.LogInformation("Initializing Blob container: {ContainerName}.", _containerName);
        var container = _blobServiceClient.GetBlobContainerClient(_containerName);
        container.CreateIfNotExists();
        _logger.LogInformation("Blob container '{ContainerName}' initialized.", _containerName);
    }

    public async Task<string> UploadReceiptAsync(Stream fileStream, string fileName)
    {
        _logger.LogInformation("Uploading receipt: {FileName}.", fileName);
        var container = _blobServiceClient.GetBlobContainerClient(_containerName);
        var blobClient = container.GetBlobClient(fileName);
        await blobClient.UploadAsync(fileStream, overwrite: true);
        _logger.LogInformation("Receipt uploaded to {BlobUri}.", blobClient.Uri);
        return blobClient.Uri.ToString();
    }
}
