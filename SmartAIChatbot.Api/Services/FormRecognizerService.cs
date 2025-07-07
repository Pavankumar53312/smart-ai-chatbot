using Azure;
using Azure.AI.FormRecognizer.DocumentAnalysis;

namespace SmartAIChatbot.Api.Services
{
    public class FormRecognizerService
    {
        private readonly DocumentAnalysisClient _client;

        public FormRecognizerService(IConfiguration config)
        {
            _client = new DocumentAnalysisClient(
                new Uri(config["FormRecognizer:Endpoint"]),
                new AzureKeyCredential(config["FormRecognizer:Key"]));
        }

        public async Task<string> ExtractTextAsync(Stream fileStream, CancellationToken ct = default)
        {
            var operation = await _client.AnalyzeDocumentAsync(WaitUntil.Completed, "prebuilt-read", fileStream, cancellationToken: ct);
            var result = operation.Value;

            var text = string.Join("\n", result.Pages.SelectMany(p => p.Lines.Select(l => l.Content)));
            return text;
        }
    }
}
