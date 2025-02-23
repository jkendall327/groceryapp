using System;

namespace GroceryApp.Backend.Models
{
    public class ProductInfo
    {
        public string Id { get; set; }
        public string UserId { get; set; } // Added to associate product with a user
        public string ProductName { get; set; }
        public string NutritionalInfo { get; set; }
        public string ShelfLife { get; set; }
        public string FoodCategory { get; set; }
        public string Unit { get; set; }
        public string Quantity { get; set; }
        public double Confidence { get; set; }
        public DateTime ExpirationDate { get; set; }
        public bool IsUsed { get; set; } = false;
        public bool IsSelected { get; set; } = false;
    }
}
