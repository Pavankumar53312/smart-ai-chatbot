

using Azure;
using Azure.AI.OpenAI;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using SmartAIChatbot.Api.Data;
using SmartAIChatbot.Api.Models;
using Microsoft.EntityFrameworkCore;
using UglyToad.PdfPig;
using DocumentFormat.OpenXml.Packaging;
using System.Text;
using System.Text.Json;
using SmartAIChatbot.Api.Helper;
using Microsoft.ApplicationInsights;
using System.Security.Claims;

namespace SmartAIChatbot.Api.Services
{
    public class ChatService
    {
        private readonly BlobContainerClient _container;
        private readonly OpenAIClient _client;
        private readonly string _deployment;
        private readonly AppDbContext _db;
        private readonly AzureOpenAIService _openAI;
        private readonly TelemetryClient _telemetryClient;
        private readonly IHttpContextAccessor _http;

        public ChatService(IConfiguration config, AppDbContext db, AzureOpenAIService openAI, TelemetryClient telemetryClient, IHttpContextAccessor http)
        {
            _client = new OpenAIClient(
                new Uri(config["AzureOpenAI:Endpoint"]),
                new AzureKeyCredential(config["AzureOpenAI:Key"]));

            _deployment = config["AzureOpenAI:Deployment"];
            _db = db;
            _openAI = openAI;
            _telemetryClient = telemetryClient;
            _container = new BlobContainerClient(
                config["AzureBlob:ConnectionString"],
                config["AzureBlob:ContainerName"]);
            _http = http;
        }

        public async Task<string> AskAsync(string question, string context)
        {
            var options = new ChatCompletionsOptions
            {
                MaxTokens = 400,
                Temperature = 0.3f
            };

            options.Messages.Add(new ChatMessage(ChatRole.System,"You are a helpful enterprise assistant. Answer only from the given context."));
            options.Messages.Add(new ChatMessage(ChatRole.User, $"Context:\n{context}\n\nQuestion:\n{question}"));

            var response = await _client.GetChatCompletionsAsync(_deployment, options);
            return response.Value.Choices[0].Message.Content.Trim();
        }

        public async Task<AskResponse> GetAnswerAsync(
            string question,
            string role,
            CancellationToken ct = default)
        {
            /* ---------- 1. Structured SQL lookup ---------- */
            var sqlAnswer = await SearchSqlAsync(question, role, ct);
            if (!string.IsNullOrWhiteSpace(sqlAnswer))
                return new AskResponse { Answer = sqlAnswer, Source = "SQL" };

            /* ---------- 2. Semantic vector search ---------- */
            role ??= "general";
            var roleLower = role.ToLower();
            var isPrivileged = roleLower is "admin" or "superuser";

            var questionVec = await _openAI.GetEmbeddingAsync(question);

            var candidates = await _db.Embeddings
                .Where(e => isPrivileged
                            || e.Role.Contains(roleLower)
                            || e.Role.Contains("general"))
                .ToListAsync(ct);

            var ranked = candidates.Select(e =>
            {
                var vec = JsonSerializer.Deserialize<List<float>>(e.EmbeddingJson) ?? new();
                return new
                {
                    e.Chunk,
                    Score = VectorMath.CosineSimilarity(vec, questionVec)
                };
            })
            .OrderByDescending(x => x.Score)
            .Take(3)
            .Where(x => x.Score > 0)
            .Select(x => x.Chunk)
            .ToList();

            if (ranked.Any())
            {
                var context = string.Join("\n---\n", ranked);
                var prompt = $"You are an AI assistant helping employees by answering questions based on the following internal IT document excerpts.\r\n\n\n{context}\n\nQ: {question}";
                var answer = await _openAI.GetChatCompletionAsync(prompt);
                return new AskResponse { Answer = answer, Source = "Vector RAG" };
            }

            /* ---------- 3. Blob keyword fallback (optional) ---------- */
            var blobContext = await SearchBlobAsync(question, role, ct);
            if (!string.IsNullOrWhiteSpace(blobContext))
            {
                var prompt = $"Using only this context:\n\n{blobContext}\n\nQ: {question}";
                var answer = await _openAI.GetChatCompletionAsync(prompt);
                return new AskResponse { Answer = answer, Source = "Blob Fallback" };
            }

            var email = _http.HttpContext?
                  .User.FindFirst(ClaimTypes.Email)?.Value
            ?? "unknown";

            /* ---------- 4. Nothing found: log to App Insights ---------- */
            _telemetryClient.TrackEvent("NoAnswerFound", new Dictionary<string, string>
                   {
                        { "Question", question },
                        { "UserEmail", _http.HttpContext?.User.FindFirst(ClaimTypes.Email)?.Value ?? "unknown" },
                        { "Role", role }
            });

            return new AskResponse
            {
                Answer = "Sorry, no relevant information was found.",
                Source = "None"
            };
        }


        private async Task<string> SearchBlobAsync(string question, string role, CancellationToken ct)
        {
            var blobs = _container.GetBlobsAsync(BlobTraits.None, BlobStates.None, prefix: null, ct);
            var snippets = new List<string>();

            await foreach (var blobItem in blobs)
            {
                var name = blobItem.Name.ToLower();
                if (!(name.EndsWith(".txt") || name.EndsWith(".pdf") || name.EndsWith(".docx")))
                    continue;

                if (!name.Contains(role.ToLower()) && !name.Contains("general"))
                    continue;

                var blobClient = _container.GetBlobClient(blobItem.Name);
                var download = await blobClient.DownloadContentAsync(ct);
                string content = "";

                if (name.EndsWith(".txt"))
                {
                    content = download.Value.Content.ToString();
                }
                else if (name.EndsWith(".pdf"))
                {
                    content = ExtractPdfText(download.Value.Content.ToStream());
                }
                else if (name.EndsWith(".docx"))
                {
                    content = ExtractDocxText(download.Value.Content.ToStream());
                }

                if (content.Contains(question, StringComparison.OrdinalIgnoreCase))
                {
                    snippets.Add(content);
                }
            }

            return string.Join("\n---\n", snippets.Take(2)); // prompt size limit
        }

        private async Task<string?> SearchSqlAsync(string question, string role, CancellationToken ct)
        {
        // Very simple keyword match—replace with full‑text or vector search later
        return await _db.KnowledgeBase            // DbSet<KnowledgeItem>
            .Where(k => k.Role == role)
            .Where(k => question
                .ToLower()
                .Contains(k.Question.ToLower()))
            .Select(k => k.Answer)
            .FirstOrDefaultAsync(ct);
        }

        private string ExtractPdfText(Stream stream)
        {
            using var pdf = PdfDocument.Open(stream);
            var sb = new StringBuilder();
            foreach (var page in pdf.GetPages())
            {
                sb.AppendLine(page.Text);
            }
            return sb.ToString();
        }

        private string ExtractDocxText(Stream stream)
        {
            using var wordDoc = WordprocessingDocument.Open(stream, false);
            var sb = new StringBuilder();
            var body = wordDoc.MainDocumentPart?.Document?.Body;
            if (body != null)
            {
                foreach (var para in body.Elements<DocumentFormat.OpenXml.Wordprocessing.Paragraph>())
                {
                    sb.AppendLine(para.InnerText);
                }
            }
            return sb.ToString();
        }

        public static double CosineSimilarity(IReadOnlyList<float> v1, IReadOnlyList<float> v2)
        {
            double dot = 0, mag1 = 0, mag2 = 0;
            for (int i = 0; i < v1.Count; i++)
            {
                dot += v1[i] * v2[i];
                mag1 += v1[i] * v1[i];
                mag2 += v2[i] * v2[i];
            }
            return dot / (Math.Sqrt(mag1) * Math.Sqrt(mag2) + 1e-8);
        }

    }
}
