namespace GroceryApp.Backend.Models;

public class ReceiptData
{
    public List<ProductInfo> Products { get; set; }
    public bool LowConfidence { get; set; }
}
