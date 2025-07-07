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

namespace SmartAIChatbot.ApiTests.Chat
{
    public class ChatHistoryTests : IClassFixture<WebApplicationFactory<Program>>
    {
        private readonly HttpClient _client;

        public ChatHistoryTests(WebApplicationFactory<Program> factory)
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
        public async Task GetHistory_AsLoggedInUser_ReturnsSuccess()
        {
            var token = await GetTokenAsync("hr@company.com", "password123");
            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

            var response = await _client.GetAsync("/api/history");

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        }

        [Fact]
        public async Task GetHistory_WithoutToken_ReturnsUnauthorized()
        {
            var response = await _client.GetAsync("/api/history");

            Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
        }

        [Fact]
        public async Task GetHistory_AsAdmin_ReturnsSuccess()
        {
            var token = await GetTokenAsync("admin@company.com", "password123");
            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

            var response = await _client.GetAsync("/api/history");

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        }
    }
}
