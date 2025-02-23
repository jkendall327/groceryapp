namespace GroceryApp.Backend
{
    public interface IBlobService
    {
        Task<string> UploadReceiptAsync(Stream fileStream, string fileName);
    }
}
