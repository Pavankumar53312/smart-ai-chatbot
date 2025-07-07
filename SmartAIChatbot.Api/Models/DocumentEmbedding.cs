namespace SmartAIChatbot.Api.Models
{
    public class DocumentEmbedding
    {
        public int Id { get; set; }
        public string FileName { get; set; } = "";
        public string Role { get; set; } = ""; // HR, IT, etc.
        public string Chunk { get; set; } = "";
        public string EmbeddingJson { get; set; } = ""; // vector as stringified JSON
    }

}
