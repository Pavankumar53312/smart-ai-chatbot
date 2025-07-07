using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.VisualStudio.TestPlatform.TestHost;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Net;
using System.Text;


namespace SmartAIChatbot.ApiTests.Doc
{
    public class DocumentUploadTests : IClassFixture<WebApplicationFactory<Program>>
    {
        private readonly HttpClient _client;

        public DocumentUploadTests(WebApplicationFactory<Program> factory)
        {
            _client = factory.CreateClient();
        }

        private async Task<string> GetAdminTokenAsync()
        {
            var login = new { email = "admin@company.com", password = "password123" };
            var response = await _client.PostAsJsonAsync("/api/auth/login", login);
            response.EnsureSuccessStatusCode();
            var json = await response.Content.ReadFromJsonAsync<dynamic>();
            return (string)json.token;
        }

        [Fact]
        public async Task UploadDocument_AsAdmin_Succeeds()
        {
            // Arrange
            var token = await GetAdminTokenAsync();
            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

            var content = new MultipartFormDataContent();
            var bytes = Encoding.UTF8.GetBytes("Dummy document content for testing upload.");
            var fileContent = new ByteArrayContent(bytes);
            fileContent.Headers.ContentType = new MediaTypeHeaderValue("text/plain");
            content.Add(fileContent, "file", "Test_HRPolicy.txt");

            // Act
            var response = await _client.PostAsync("/api/documents/upload-to-blob", content);

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        }

        [Fact]
        public async Task UploadDocument_WithoutFile_ReturnsBadRequest()
        {
            var token = await GetAdminTokenAsync();
            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

            var content = new MultipartFormDataContent(); // no file

            var response = await _client.PostAsync("/api/documents/upload-to-blob", content);

            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact]
        public async Task UploadDocument_AsUser_IsForbidden()
        {
            var login = new { email = "hr@company.com", password = "password123" };
            var response = await _client.PostAsJsonAsync("/api/auth/login", login);
            var json = await response.Content.ReadFromJsonAsync<dynamic>();
            string token = json.token;

            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

            var content = new MultipartFormDataContent();
            var bytes = Encoding.UTF8.GetBytes("Unauthorized content");
            var fileContent = new ByteArrayContent(bytes);
            fileContent.Headers.ContentType = new MediaTypeHeaderValue("text/plain");
            content.Add(fileContent, "file", "Unauthorized_HRDoc.txt");

            var result = await _client.PostAsync("/api/documents/upload-to-blob", content);

            Assert.Equal(HttpStatusCode.Forbidden, result.StatusCode);
        }
    }
}
