using GroceryApp.Backend;
using GroceryApp.Backend.Services;
using Microsoft.Extensions.Logging;
using Microsoft.ApplicationInsights.Extensibility;

var builder = WebApplication.CreateBuilder(args);

// Configure logging
builder.Logging.ClearProviders();
builder.Logging.AddConsole();
builder.Logging.AddDebug();

// Add Application Insights
builder.Services.AddApplicationInsightsTelemetry(builder.Configuration["ApplicationInsights:InstrumentationKey"]);

// Add services to the container.
builder.Services.AddCustomServices(builder.Configuration);

// Register ReceiptService
builder.Services.AddScoped<IReceiptService, ReceiptService>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.Logger.LogInformation("Application is running in Development mode.");
}
else
{
    app.Logger.LogInformation("Application is running in Production mode.");
}

app.UseHttpsRedirection();

// Existing ReceiptsController endpoints are now handled by ReceiptService
app.MapPost("/api/upload", async (IFormFile file, IReceiptService receiptService, ILogger<Program> logger) => await receiptService.UploadReceiptAsync(file, logger))
    .WithName("UploadReceipt")
    .Accepts<IFormFile>("multipart/form-data")
    .Produces(StatusCodes.Status200OK)
    .Produces(StatusCodes.Status400BadRequest);

// Map Health Check endpoint
app.MapHealthChecks("/health");

app.Run();

public partial class Program { }
