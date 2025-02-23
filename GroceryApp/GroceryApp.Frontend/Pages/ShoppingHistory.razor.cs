using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;

namespace GroceryApp.Frontend.Pages
{
    public partial class ShoppingHistory
    {
        private List<PurchasedItem> shoppingHistory;
        private Dictionary<string, List<PurchasedItem>> groupedHistory;
        private bool loading = true;
        private string errorMessage;

        protected override async Task OnInitializedAsync()
        {
            try
            {
                shoppingHistory = await Http.GetFromJsonAsync<List<PurchasedItem>>("api/shoppinghistory");
                if (shoppingHistory != null)
                {
                    // Grouping by user can be implemented here if user information is available.
                    // For now, assuming a single user.
                    groupedHistory = new Dictionary<string, List<PurchasedItem>>
                    {
                        { "All Purchases", shoppingHistory }
                    };
                }
            }
            catch (Exception ex)
            {
                errorMessage = $"Error fetching shopping history: {ex.Message}";
            }
            finally
            {
                loading = false;
            }
        }

        private async Task ExportToJson()
        {
            try
            {
                var json = JsonSerializer.Serialize(shoppingHistory, new JsonSerializerOptions { WriteIndented = true });
                await JSRuntime.InvokeVoidAsync("downloadFile", "shopping_history.json", "application/json", json);
            }
            catch (Exception ex)
            {
                errorMessage = $"Error exporting to JSON: {ex.Message}";
            }
        }

        private async Task ExportToCsv()
        {
            try
            {
                var csvBuilder = new StringBuilder();
                csvBuilder.AppendLine("ProductName,NutritionalInfo,ShelfLife,FoodCategory,Unit,Quantity,Confidence,ExpirationDate,IsUsed");

                foreach (var item in shoppingHistory)
                {
                    var line = $"{EscapeCsv(item.ProductName)},{EscapeCsv(item.NutritionalInfo)},{EscapeCsv(item.ShelfLife)},{EscapeCsv(item.FoodCategory)},{EscapeCsv(item.Unit)},{EscapeCsv(item.Quantity)},{item.Confidence},{item.ExpirationDate:yyyy-MM-dd},{item.IsUsed}";
                    csvBuilder.AppendLine(line);
                }

                var csv = csvBuilder.ToString();
                await JSRuntime.InvokeVoidAsync("downloadFile", "shopping_history.csv", "text/csv", csv);
            }
            catch (Exception ex)
            {
                errorMessage = $"Error exporting to CSV: {ex.Message}";
            }
        }

        private string EscapeCsv(string value)
        {
            if (value.Contains(",") || value.Contains("\"") || value.Contains("\n"))
            {
                value = value.Replace("\"", "\"\"");
                value = $"\"{value}\"";
            }
            return value;
        }

        public class PurchasedItem
        {
            public string Id { get; set; }
            public string UserId { get; set; } // Ensures data is tied to the authenticated user
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
    }
}
