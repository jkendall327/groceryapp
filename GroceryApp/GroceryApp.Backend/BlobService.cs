using Azure.Storage.Blobs;

namespace GroceryApp.Backend
{
    public class BlobService : IBlobService
    {
        private readonly BlobServiceClient _blobServiceClient;

        public BlobService(BlobServiceClient blobServiceClient)
        {
            _blobServiceClient = blobServiceClient;
            // Initialize Blob containers, etc.
        }

        // Implement methods for IBlobService
    }
}
