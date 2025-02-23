using Microsoft.Azure.CognitiveServices.Vision.ComputerVision;

namespace GroceryApp.Backend
{
    public class ComputerVisionService : IComputerVisionService
    {
        private readonly ComputerVisionClient _computerVisionClient;

        public ComputerVisionService(ComputerVisionClient computerVisionClient)
        {
            _computerVisionClient = computerVisionClient;
            // Initialize Computer Vision client, etc.
        }

        public async Task<string> AnalyzeReceiptAsync(string imageUrl)
        {
            var ocrResult = await _computerVisionClient.RecognizePrintedTextAsync(true, imageUrl);
            
            if (ocrResult == null || ocrResult.Regions == null)
            {
                return "No text detected.";
            }

            var extractedText = string.Empty;

            foreach (var region in ocrResult.Regions)
            {
                foreach (var line in region.Lines)
                {
                    foreach (var word in line.Words)
                    {
                        extractedText += word.Text + " ";
                    }
                    extractedText += "\n";
                }
            }

            return extractedText.Trim();
        }
    }
}
