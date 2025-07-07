using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.VisualStudio.TestPlatform.TestHost;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace SmartAIChatbot.ApiTests.Auth
{
    public class KnowledgeBaseTests : IClassFixture<WebApplicationFactory<Program>>
    {
        private readonly HttpClient _client;

        public KnowledgeBaseTests(WebApplicationFactory<Program> factory)
        {
            _client = factory.CreateClient();
        }

        private async Task<string> GetTokenAsync(string email, string password)
        {
            var login = new { email, password };
            var response = await _client.PostAsJsonAsync("/api/auth/login", login);
            response.EnsureSuccessStatusCode();
            var json = await response.Content.ReadFromJsonAsync<dynamic>();
            return (string)json.token;
        }

        [Fact]
        public async Task GetKnowledgeItems_AsAdmin_Succeeds()
        {
            var token = await GetTokenAsync("admin@company.com", "password123");
            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

            var response = await _client.GetAsync("/api/knowledge");

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        }

        [Fact]
        public async Task GetKnowledgeItems_AsUser_IsForbidden()
        {
            var token = await GetTokenAsync("hr@company.com", "password123");
            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

            var response = await _client.GetAsync("/api/knowledge");

            Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
        }

        [Fact]
        public async Task AddKnowledgeItem_AsAdmin_Works()
        {
            var token = await GetTokenAsync("admin@company.com", "password123");
            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

            var newItem = new
            {
                role = "finance",
                question = "What is the reimbursement process?",
                answer = "Submit the form to finance@company.com"
            };

            var response = await _client.PostAsJsonAsync("/api/knowledge", newItem);

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        }

        [Fact]
        public async Task AddKnowledgeItem_AsUser_IsForbidden()
        {
            var token = await GetTokenAsync("it@company.com", "password123");
            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

            var newItem = new
            {
                role = "hr",
                question = "How to apply leave?",
                answer = "Use the HR portal"
            };

            var response = await _client.PostAsJsonAsync("/api/knowledge", newItem);

            Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
        }

        [Fact]
        public async Task DeleteKnowledgeItem_AsAdmin_WorksIfExists()
        {
            var token = await GetTokenAsync("admin@company.com", "password123");
            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

            var idToDelete = 1; // Adjust according to seed data

            var response = await _client.DeleteAsync($"/api/knowledge/{idToDelete}");

            // Accept both NoContent or NotFound depending on presence
            Assert.True(
                response.StatusCode == HttpStatusCode.NoContent ||
                response.StatusCode == HttpStatusCode.NotFound);
        }

        [Fact]
        public async Task DeleteKnowledgeItem_AsUser_IsForbidden()
        {
            var token = await GetTokenAsync("hr@company.com", "password123");
            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

            var response = await _client.DeleteAsync("/api/knowledge/1");

            Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
        }
    }
}
