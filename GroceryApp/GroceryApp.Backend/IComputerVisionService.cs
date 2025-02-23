namespace GroceryApp.Backend
{
    public interface IComputerVisionService
    {
        Task<string> AnalyzeReceiptAsync(string imageUrl);
    }
}
