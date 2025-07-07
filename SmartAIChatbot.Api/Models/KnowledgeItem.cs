namespace SmartAIChatbot.Api.Models
{
    public class KnowledgeItem
    {
        public int Id { get; set; }
        public string Role { get; set; }    // HR / IT / Admin …
        public string Question { get; set; }
        public string Answer { get; set; }
    }

}
