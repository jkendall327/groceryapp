using Azure.Storage.Blobs;

namespace GroceryApp.Backend
{
    public class BlobService : IBlobService
    {
        private readonly BlobServiceClient _blobServiceClient;
        private readonly string _containerName = "receipts";

        public BlobService(BlobServiceClient blobServiceClient)
        {
            _blobServiceClient = blobServiceClient;
            InitializeContainer();
        }

        private void InitializeContainer()
        {
            var container = _blobServiceClient.GetBlobContainerClient(_containerName);
            container.CreateIfNotExists();
        }

        public async Task<string> UploadReceiptAsync(Stream fileStream, string fileName)
        {
            var container = _blobServiceClient.GetBlobContainerClient(_containerName);
            var blobClient = container.GetBlobClient(fileName);
            await blobClient.UploadAsync(fileStream, overwrite: true);
            return blobClient.Uri.ToString();
        }
    }
}
