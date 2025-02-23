using System;

namespace GroceryApp.Backend.Models;

public class PurchasedItem
{
    public string Id { get; set; }
    public string ProductName { get; set; }
    public string NutritionalInfo { get; set; }
    public string ShelfLife { get; set; }
    public string FoodCategory { get; set; }
    public string Unit { get; set; }
    public string Quantity { get; set; }
    public double Confidence { get; set; }
    public DateTime ExpirationDate { get; set; }
    public bool IsUsed { get; set; }
}
