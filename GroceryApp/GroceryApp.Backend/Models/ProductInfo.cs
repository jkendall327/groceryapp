namespace GroceryApp.Backend.Models
{
    public class ProductInfo
    {
        public string ProductName { get; set; }
        public string NutritionalInfo { get; set; }
        public string ShelfLife { get; set; }
        public string FoodCategory { get; set; }
        public string Unit { get; set; }
        public string Quantity { get; set; }
        public double Confidence { get; set; }
    }
}
