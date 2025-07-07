using System.Net;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.VisualStudio.TestPlatform.TestHost;
using Xunit;

namespace SmartAIChatbot.ApiTests.Chat;

public class ChatAskTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly HttpClient _client;

    public ChatAskTests(WebApplicationFactory<Program> factory)
    {
        _client = factory.CreateClient();
    }

    private async Task<string> GetJwtTokenAsync()
    {
        var payload = new
        {
            email = "admin@company.com",
            password = "Admin@123"
        };

        var json = JsonSerializer.Serialize(payload);
        var content = new StringContent(json, Encoding.UTF8, "application/json");

        var response = await _client.PostAsync("/api/auth/login", content);
        var result = await response.Content.ReadAsStringAsync();

        using var doc = JsonDocument.Parse(result);
        return doc.RootElement.GetProperty("token").GetString()!;
    }

    [Fact]
    public async Task Ask_WithValidQuestion_ReturnsAnswer()
    {
        var token = await GetJwtTokenAsync();

        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var question = new { question = "What are the leave policies?" };
        var json = JsonSerializer.Serialize(question);
        var content = new StringContent(json, Encoding.UTF8, "application/json");

        var response = await _client.PostAsync("/api/chat/ask", content);

        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var answer = await response.Content.ReadAsStringAsync();
        answer.Should().NotBeNullOrWhiteSpace();
    }
}
