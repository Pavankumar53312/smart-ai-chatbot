using Azure;
using Azure.AI.OpenAI;
using Microsoft.Extensions.Configuration;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SmartAIChatbot.Api.Services
{
    public class AzureOpenAIService
    {
        private readonly OpenAIClient _client;
        private readonly string _chatDeployment;
        private readonly string _embeddingDeployment;

        public AzureOpenAIService(IConfiguration cfg)
        {
            _client = new OpenAIClient(
                new Uri(cfg["AzureOpenAI:Endpoint"]),
                new AzureKeyCredential(cfg["AzureOpenAI:Key"]));

            _chatDeployment = cfg["AzureOpenAI:ChatDeployment"];      // e.g. gpt-35-turbo
            _embeddingDeployment = cfg["AzureOpenAI:EmbeddingDeployment"]; // e.g. text-embedding-ada-002
        }

        /* ---------- Chat Completion ---------- */
        public async Task<string> GetChatCompletionAsync(string prompt)
        {
            var opts = new ChatCompletionsOptions
            {
                MaxTokens = 400,
                Temperature = 0.3f
            };
            opts.Messages.Add(new ChatMessage(ChatRole.User, "You are a helpful assistant."));
            opts.Messages.Add(new ChatMessage(ChatRole.User, prompt));

            var resp = await _client.GetChatCompletionsAsync(_chatDeployment, opts);
            return resp.Value.Choices[0].Message.Content.Trim();
        }

        /* ---------- Embedding ---------- */
        public async Task<List<float>> GetEmbeddingAsync(string text)
        {
            var result = await _client.GetEmbeddingsAsync(
                _embeddingDeployment,
                new EmbeddingsOptions(text));

            return result.Value.Data[0].Embedding.ToList();
        }
    }
}
